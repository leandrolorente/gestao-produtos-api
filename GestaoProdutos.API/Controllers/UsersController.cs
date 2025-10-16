using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Apenas usuários autenticados podem acessar
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Listar todos os usuários (apenas admins)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUserById(string id)
    {
        try
        {
            var currentUserId = User.FindFirst("id")?.Value;
            var currentUserRole = User.FindFirst("role")?.Value;

            // Usuários podem ver apenas seu próprio perfil, admins podem ver qualquer um
            if (currentUserId != id && currentUserRole != "admin")
            {
                return Forbid("Você só pode visualizar seu próprio perfil");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Criar novo usuário (apenas admins)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserCreateDto userCreateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.CreateUserAsync(userCreateDto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar usuário
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var currentUserId = User.FindFirst("id")?.Value;
            var currentUserRole = User.FindFirst("role")?.Value;

            // Usuários podem editar apenas seu próprio perfil, admins podem editar qualquer um
            if (currentUserId != id && currentUserRole != "admin")
            {
                return Forbid("Você só pode editar seu próprio perfil");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Desativar usuário (soft delete) - apenas admins
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> DeactivateUser(string id)
    {
        try
        {
            var currentUserId = User.FindFirst("id")?.Value;
            
            // Não permitir que admin desative a si mesmo
            if (currentUserId == id)
            {
                return BadRequest(new { message = "Você não pode desativar sua própria conta" });
            }

            var success = await _userService.DeactivateUserAsync(id);
            if (!success)
                return NotFound(new { message = "Usuário não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Reativar usuário - apenas admins
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> ActivateUser(string id)
    {
        try
        {
            var success = await _userService.ActivateUserAsync(id);
            if (!success)
                return NotFound(new { message = "Usuário não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter usuários por departamento
    /// </summary>
    [HttpGet("department/{department}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsersByDepartment(string department)
    {
        try
        {
            var users = await _userService.GetUsersByDepartmentAsync(department);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter usuários por role
    /// </summary>
    [HttpGet("role/{role}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsersByRole(string role)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}
