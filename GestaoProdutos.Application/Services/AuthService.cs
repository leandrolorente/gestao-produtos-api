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

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Verificar se email já existe
            var usuariosExistentes = await _unitOfWork.Usuarios.GetAllAsync();
            if (usuariosExistentes.Any(u => u.Email.Valor == registerDto.Email))
                throw new InvalidOperationException("Email já está em uso");

            // Criar novo usuário
            var usuario = new Usuario
            {
                Nome = registerDto.Name,
                Email = new Email(registerDto.Email),
                Role = UserRole.User, // Usuário padrão
                Avatar = registerDto.Avatar,
                Departamento = registerDto.Department,
                SenhaHash = CriptografarSenha(registerDto.Password),
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            await _unitOfWork.Usuarios.CreateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return MapToUserDto(usuario);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro durante o registro: {ex.Message}");
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
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email.Valor),
            new Claim(ClaimTypes.Role, usuario.Role.ToString()),
            new Claim("Department", usuario.Departamento)
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
            IsActive = usuario.Ativo
        };
    }
}