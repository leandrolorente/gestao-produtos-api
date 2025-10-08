using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

public interface IEnderecoService
{
    Task<IEnumerable<EnderecoDto>> GetAllAsync();
    Task<EnderecoDto?> GetByIdAsync(string id);
    Task<EnderecoDto?> GetByCepAsync(string cep);
    Task<IEnumerable<EnderecoDto>> GetByCidadeAsync(string cidade);
    Task<IEnumerable<EnderecoDto>> GetByEstadoAsync(string estado);
    Task<EnderecoDto> CreateAsync(CreateEnderecoDto createDto);
    Task<EnderecoDto?> UpdateAsync(string id, UpdateEnderecoDto updateDto);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<EnderecoDto>> SearchAsync(string termo);
}
