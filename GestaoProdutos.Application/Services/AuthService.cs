using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GestaoProdutos.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly int _jwtExpirationHours;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _jwtSecret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret não configurado");
        _jwtExpirationHours = int.Parse(_configuration["JWT:ExpirationHours"] ?? "24");
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Buscar usuário por email
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Email.Valor == loginDto.Email && u.Ativo);
            
            if (usuario == null)
                throw new UnauthorizedAccessException("Credenciais inválidas");

            // Verificar senha
            if (!VerificarSenha(loginDto.Password, usuario.SenhaHash))
                throw new UnauthorizedAccessException("Credenciais inválidas");

            // Registrar login
            usuario.RegistrarLogin();
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            // Gerar token JWT
            var token = GerarJwtToken(usuario);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtExpirationHours);

            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(usuario)
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro durante o login: {ex.Message}");
        }
    }

    public async Task<bool> LogoutAsync(LogoutDto logoutDto)
    {
        try
        {
            // Validar entrada
            if (string.IsNullOrEmpty(logoutDto.UserId))
                throw new ArgumentException("UserId é obrigatório para logout");

            // Buscar usuário
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(logoutDto.UserId);
            if (usuario == null || !usuario.Ativo)
                return false;

            // Validar token se fornecido (verificação adicional de segurança)
            if (!string.IsNullOrEmpty(logoutDto.Token))
            {
                var isValidToken = await ValidateTokenAsync(logoutDto.Token);
                if (!isValidToken)
                    throw new UnauthorizedAccessException("Token inválido para logout");
            }

            // Registrar logout para auditoria
            usuario.RegistrarLogout();
            
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            // Log para auditoria (opcional)
            Console.WriteLine($"Logout realizado para usuário: {usuario.Nome} ({usuario.Email.Valor}) em {DateTime.UtcNow}");
            
            if (!string.IsNullOrEmpty(logoutDto.DeviceInfo))
            {
                Console.WriteLine($"Device Info: {logoutDto.DeviceInfo}");
            }

            // Nota: Como JWT é stateless, o token continuará válido até expirar
            // O cliente deve remover o token do localStorage/sessionStorage
            // Para invalidação imediata, seria necessário implementar uma blacklist de tokens

            return true;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro durante o logout: {ex.Message}");
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(string userId)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
            if (usuario == null || !usuario.Ativo)
                throw new InvalidOperationException("Usuário não encontrado");

            return MapToUserDto(usuario);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao obter usuário atual: {ex.Message}");
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            // Buscar usuário por email
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Email.Valor == forgotPasswordDto.Email && u.Ativo);
            
            if (usuario == null)
                return false; // Não revelar se o email existe

            // Gerar token de reset (em produção, salvar no banco)
            var resetToken = GerarTokenReset();
            
            // TODO: Implementar envio de email
            // await _emailService.EnviarEmailResetSenhaAsync(usuario.Email.Value, resetToken);
            
            // Por enquanto, apenas loggar o token (remover em produção)
            Console.WriteLine($"Token de reset para {usuario.Email.Valor}: {resetToken}");
            
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao processar solicitação de reset: {ex.Message}");
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            // Em produção, validar o token salvo no banco
            // Por enquanto, aceitar qualquer token para demonstração
            
            // Buscar usuário por email
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Email.Valor == resetPasswordDto.Email && u.Ativo);
            
            if (usuario == null)
                return false;

            // Atualizar senha
            usuario.SenhaHash = CriptografarSenha(resetPasswordDto.NewPassword);
            usuario.DataAtualizacao = DateTime.UtcNow;
            
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao redefinir senha: {ex.Message}");
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);
            if (usuario == null || !usuario.Ativo)
                return false;

            // Verificar senha atual
            if (!VerificarSenha(changePasswordDto.CurrentPassword, usuario.SenhaHash))
                throw new UnauthorizedAccessException("Senha atual incorreta");

            // Atualizar senha
            usuario.SenhaHash = CriptografarSenha(changePasswordDto.NewPassword);
            usuario.DataAtualizacao = DateTime.UtcNow;
            
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao alterar senha: {ex.Message}");
        }
    }

    private string GerarJwtToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("id", usuario.Id),
            new Claim("name", usuario.Nome),
            new Claim("email", usuario.Email.Valor),
            new Claim("role", usuario.Role.ToString().ToLowerInvariant()),
            new Claim("department", usuario.Departamento)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtExpirationHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string CriptografarSenha(string senha)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha + _jwtSecret));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerificarSenha(string senha, string hash)
    {
        var hashVerificacao = CriptografarSenha(senha);
        return hashVerificacao == hash;
    }

    private string GerarTokenReset()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private UserDto MapToUserDto(Usuario usuario)
    {
        return new UserDto
        {
            Id = usuario.Id,
            Name = usuario.Nome,
            Email = usuario.Email.Valor,
            Role = usuario.Role.ToString().ToLowerInvariant(),
            Avatar = usuario.Avatar,
            Department = usuario.Departamento,
            LastLogin = usuario.UltimoLogin,
            LastUpdated = usuario.DataAtualizacao,
            IsActive = usuario.Ativo
        };
    }
}
