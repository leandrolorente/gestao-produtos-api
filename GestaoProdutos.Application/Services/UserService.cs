using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace GestaoProdutos.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _jwtSecret;

    public UserService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtSecret = configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret não configurado");
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        return usuarios.Select(MapToUserResponseDto);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        return usuario != null ? MapToUserResponseDto(usuario) : null;
    }

    public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        var usuario = usuarios.FirstOrDefault(u => u.Email.Valor == email);
        return usuario != null ? MapToUserResponseDto(usuario) : null;
    }

    public async Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string role)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            return Enumerable.Empty<UserResponseDto>();

        var filteredUsers = usuarios.Where(u => u.Role == userRole);
        return filteredUsers.Select(MapToUserResponseDto);
    }

    public async Task<IEnumerable<UserResponseDto>> GetUsersByDepartmentAsync(string department)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        var filteredUsers = usuarios.Where(u => 
            u.Departamento.Equals(department, StringComparison.OrdinalIgnoreCase) && u.Ativo);
        return filteredUsers.Select(MapToUserResponseDto);
    }

    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
    {
        // Verificar se email já existe
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
        if (usuarios.Any(u => u.Email.Valor == dto.Email))
            throw new InvalidOperationException("Email já está em uso");

        // Parse role
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var userRole))
            userRole = UserRole.User; // Default

        // Criar usuário
        var usuario = new Usuario
        {
            Nome = dto.Name,
            Email = new Email(dto.Email),
            Role = userRole,
            Avatar = dto.Avatar,
            Departamento = dto.Department,
            SenhaHash = CriptografarSenha(dto.Password),
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        await _unitOfWork.Usuarios.CreateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserResponseDto(usuario);
    }

    public async Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null)
            throw new InvalidOperationException("Usuário não encontrado");

        // Verificar se email já existe (se está sendo alterado)
        if (usuario.Email.Valor != dto.Email)
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            if (usuarios.Any(u => u.Email.Valor == dto.Email && u.Id != id))
                throw new InvalidOperationException("Email já está em uso");
        }

        // Parse role
        if (!Enum.TryParse<UserRole>(dto.Role, true, out var userRole))
            userRole = usuario.Role; // Manter role atual se inválida

        // Atualizar dados
        usuario.Nome = dto.Name;
        usuario.Email = new Email(dto.Email);
        usuario.Role = userRole;
        usuario.Avatar = dto.Avatar;
        usuario.Departamento = dto.Department;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserResponseDto(usuario);
    }

    public async Task<bool> DeactivateUserAsync(string id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return false;

        usuario.Ativo = false;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateUserAsync(string id)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
        if (usuario == null) return false;

        usuario.Ativo = true;
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _unitOfWork.Usuarios.UpdateAsync(usuario);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private UserResponseDto MapToUserResponseDto(Usuario usuario)
    {
        return new UserResponseDto
        {
            Id = usuario.Id,
            Name = usuario.Nome,
            Email = usuario.Email.Valor,
            Avatar = usuario.Avatar,
            Department = usuario.Departamento,
            Role = usuario.Role.ToString().ToLowerInvariant(),
            CreatedAt = usuario.DataCriacao,
            UpdatedAt = usuario.DataAtualizacao,
            IsActive = usuario.Ativo
        };
    }

    private string CriptografarSenha(string senha)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha + _jwtSecret));
        return Convert.ToBase64String(hashedBytes);
    }
}