using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller responsável pela autenticação e autorização de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza login no sistema
    /// </summary>
    /// <param name="loginDto">Dados de login (email e senha)</param>
    /// <returns>Token JWT e dados do usuário</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Solicita reset de senha via email
    /// </summary>
    /// <param name="forgotPasswordDto">Email do usuário</param>
    /// <returns>Confirmação do envio</returns>
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            
            // Sempre retorna sucesso por segurança (não revela se email existe)
            return Ok(new { message = "Se o email estiver cadastrado, você receberá as instruções para redefinir a senha." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Redefine a senha com token de reset
    /// </summary>
    /// <param name="resetPasswordDto">Dados para reset de senha</param>
    /// <returns>Confirmação do reset</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.ResetPasswordAsync(resetPasswordDto);
            
            if (!success)
                return BadRequest(new { message = "Token inválido ou expirado." });

            return Ok(new { message = "Senha redefinida com sucesso." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Altera a senha do usuário logado
    /// </summary>
    /// <param name="changePasswordDto">Senha atual e nova senha</param>
    /// <returns>Confirmação da alteração</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido." });

            var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            
            if (!success)
                return BadRequest(new { message = "Não foi possível alterar a senha." });

            return Ok(new { message = "Senha alterada com sucesso." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Retorna informações do usuário logado
    /// </summary>
    /// <returns>Dados do usuário logado</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var user = await _authService.GetCurrentUserAsync(userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Valida se o token JWT é válido
    /// </summary>
    /// <returns>Status da validação</returns>
    [HttpPost("validate-token")]
    [Authorize]
    public async Task<ActionResult> ValidateToken()
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var isValid = await _authService.ValidateTokenAsync(token);
                return Ok(new { message = "Token válido.", valid = isValid });
            }
            
            return BadRequest(new { message = "Token não fornecido.", valid = false });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Realiza logout do sistema
    /// </summary>
    /// <param name="logoutDto">Dados do usuário para logout</param>
    /// <returns>Confirmação do logout</returns>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        try
        {
            // Validar se os dados foram enviados
            if (string.IsNullOrEmpty(logoutDto.UserId))
            {
                return BadRequest(new { message = "UserId é obrigatório para logout" });
            }

            // Realizar logout com os dados enviados pelo frontend
            var success = await _authService.LogoutAsync(logoutDto);
            
            if (!success)
            {
                return BadRequest(new { message = "Não foi possível realizar logout. Usuário não encontrado ou inativo." });
            }

            return Ok(new 
            { 
                message = "Logout realizado com sucesso",
                userId = logoutDto.UserId,
                timestamp = DateTime.UtcNow,
                instructions = "Token removido com sucesso"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Verifica se o token atual ainda é válido
    /// </summary>
    /// <returns>Status do token</returns>
    [HttpGet("verify-token")]
    [Authorize]
    public ActionResult VerifyToken()
    {
        try
        {
            var userId = User.FindFirst("id")?.Value;
            var userName = User.FindFirst("name")?.Value;
            var userRole = User.FindFirst("role")?.Value;
            var exp = User.FindFirst("exp")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            // Converter exp para data legível
            DateTime? expirationDate = null;
            if (long.TryParse(exp, out long expUnix))
            {
                expirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).DateTime;
            }

            return Ok(new 
            { 
                message = "Token válido",
                userId = userId,
                userName = userName,
                userRole = userRole,
                expiresAt = expirationDate,
                isValid = true,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Configura o primeiro administrador do sistema (apenas quando não há usuários)
    /// </summary>
    /// <param name="createDto">Dados do primeiro administrador</param>
    /// <returns>Dados do usuário criado</returns>
    [HttpPost("setup-first-admin")]
    public async Task<ActionResult<UserResponseDto>> SetupFirstAdmin([FromBody] UserCreateDto createDto)
    {
        try
        {
            // Inject UserService para verificar se já existe usuário
            var userService = HttpContext.RequestServices.GetRequiredService<IUserService>();
            
            // Verificar se já existe algum usuário no sistema
            var existingUsers = await userService.GetAllUsersAsync();
            if (existingUsers.Any())
            {
                return BadRequest(new { message = "Sistema já possui usuários. Use o endpoint de criação normal." });
            }

            // Forçar role como Admin para o primeiro usuário
            createDto = createDto with { Role = "admin" };
            
            var result = await userService.CreateUserAsync(createDto);
            return Created("", result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }
}