using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Interfaces;

namespace GestaoProdutos.Application.Services;

public class EnderecoService : IEnderecoService
{
    private readonly IUnitOfWork _unitOfWork;

    public EnderecoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EnderecoDto>> GetAllAsync()
    {
        var enderecos = await _unitOfWork.Enderecos.GetAllAsync();
        return enderecos.Select(MapToDto);
    }

    public async Task<EnderecoDto?> GetByIdAsync(string id)
    {
        var endereco = await _unitOfWork.Enderecos.GetByIdAsync(id);
        return endereco != null ? MapToDto(endereco) : null;
    }

    public async Task<EnderecoDto?> GetByCepAsync(string cep)
    {
        var enderecos = await _unitOfWork.Enderecos.GetAllAsync();
        var endereco = enderecos.FirstOrDefault(e => e.Cep == cep);
        return endereco != null ? MapToDto(endereco) : null;
    }

    public async Task<IEnumerable<EnderecoDto>> GetByCidadeAsync(string cidade)
    {
        var enderecos = await _unitOfWork.Enderecos.GetAllAsync();
        var enderecosFiltrados = enderecos.Where(e => e.Localidade.ToLower().Contains(cidade.ToLower()));
        return enderecosFiltrados.Select(MapToDto);
    }

    public async Task<IEnumerable<EnderecoDto>> GetByEstadoAsync(string estado)
    {
        var enderecos = await _unitOfWork.Enderecos.GetAllAsync();
        var enderecosFiltrados = enderecos.Where(e => e.Estado.ToLower().Contains(estado.ToLower()) || e.Uf.ToLower().Contains(estado.ToLower()));
        return enderecosFiltrados.Select(MapToDto);
    }

    public async Task<EnderecoDto> CreateAsync(CreateEnderecoDto createDto)
    {
        var endereco = new EnderecoEntity
        {
            Cep = createDto.Cep,
            Logradouro = createDto.Logradouro,
            Numero = createDto.Numero,
            Complemento = createDto.Complemento ?? string.Empty,
            Unidade = createDto.Unidade,
            Bairro = createDto.Bairro,
            Localidade = createDto.Localidade,
            Uf = createDto.Uf,
            Estado = createDto.Estado,
            Regiao = createDto.Regiao,
            Referencia = createDto.Referencia,
            IsPrincipal = createDto.IsPrincipal,
            Tipo = createDto.Tipo
        };

        var createdEndereco = await _unitOfWork.Enderecos.CreateAsync(endereco);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(createdEndereco);
    }

    public async Task<EnderecoDto?> UpdateAsync(string id, UpdateEnderecoDto updateDto)
    {
        var existingEndereco = await _unitOfWork.Enderecos.GetByIdAsync(id);
        if (existingEndereco == null)
            return null;

        existingEndereco.Cep = updateDto.Cep;
        existingEndereco.Logradouro = updateDto.Logradouro;
        existingEndereco.Numero = updateDto.Numero;
        existingEndereco.Complemento = updateDto.Complemento ?? string.Empty;
        existingEndereco.Unidade = updateDto.Unidade;
        existingEndereco.Bairro = updateDto.Bairro;
        existingEndereco.Localidade = updateDto.Localidade;
        existingEndereco.Uf = updateDto.Uf;
        existingEndereco.Estado = updateDto.Estado;
        existingEndereco.Regiao = updateDto.Regiao;
        existingEndereco.Referencia = updateDto.Referencia;
        existingEndereco.IsPrincipal = updateDto.IsPrincipal;
        existingEndereco.Tipo = updateDto.Tipo;

        await _unitOfWork.Enderecos.UpdateAsync(existingEndereco);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(existingEndereco);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _unitOfWork.Enderecos.DeleteAsync(id);
        if (result)
            await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _unitOfWork.Enderecos.ExistsAsync(id);
    }

    public async Task<IEnumerable<EnderecoDto>> SearchAsync(string termo)
    {
        var enderecos = await _unitOfWork.Enderecos.GetAllAsync();
        var enderecosFiltrados = enderecos.Where(e => 
            e.Logradouro.ToLower().Contains(termo.ToLower()) ||
            e.Bairro.ToLower().Contains(termo.ToLower()) ||
            e.Localidade.ToLower().Contains(termo.ToLower()) ||
            e.Estado.ToLower().Contains(termo.ToLower()) ||
            e.Cep.Contains(termo));
        return enderecosFiltrados.Select(MapToDto);
    }

    private static EnderecoDto MapToDto(EnderecoEntity endereco)
    {
        return new EnderecoDto
        {
            Id = endereco.Id,
            Cep = endereco.Cep,
            Logradouro = endereco.Logradouro,
            Numero = endereco.Numero,
            Complemento = endereco.Complemento,
            Unidade = endereco.Unidade,
            Bairro = endereco.Bairro,
            Localidade = endereco.Localidade,
            Uf = endereco.Uf,
            Estado = endereco.Estado,
            Regiao = endereco.Regiao,
            Referencia = endereco.Referencia,
            IsPrincipal = endereco.IsPrincipal,
            Tipo = endereco.Tipo,
            Ativo = endereco.Ativo,
            DataCriacao = endereco.DataCriacao,
            DataAtualizacao = endereco.DataAtualizacao
        };
    }
}