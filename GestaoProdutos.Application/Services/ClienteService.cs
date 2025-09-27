using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllClientesAsync()
    {
        var clientes = await _unitOfWork.Clientes.GetAllAsync();
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto?> GetClienteByIdAsync(string id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        return cliente != null ? MapToDto(cliente) : null;
    }

    public async Task<ClienteDto?> GetClienteByCpfCnpjAsync(string cpfCnpj)
    {
        var cliente = await _unitOfWork.Clientes.GetClientePorCpfCnpjAsync(cpfCnpj);
        return cliente != null ? MapToDto(cliente) : null;
    }

    public async Task<IEnumerable<ClienteDto>> GetClientesAtivosPorTipoAsync(TipoCliente tipo)
    {
        var clientes = await _unitOfWork.Clientes.GetClientesAtivosPorTipoAsync(tipo);
        return clientes.Select(MapToDto);
    }

    public async Task<IEnumerable<ClienteDto>> GetClientesComCompraRecenteAsync(int dias = 30)
    {
        var clientes = await _unitOfWork.Clientes.GetClientesComCompraRecenteAsync(dias);
        return clientes.Select(MapToDto);
    }

    public async Task<ClienteDto> CreateClienteAsync(CreateClienteDto dto)
    {
        // Validar se CPF/CNPJ já existe
        if (await _unitOfWork.Clientes.CpfCnpjJaExisteAsync(dto.CpfCnpj))
        {
            throw new InvalidOperationException("CPF/CNPJ já cadastrado");
        }

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Email = new Email(dto.Email),
            Telefone = dto.Telefone,
            CpfCnpj = new CpfCnpj(dto.CpfCnpj),
            Endereco = new Endereco
            {
                Logradouro = dto.Endereco,
                Cidade = dto.Cidade,
                Estado = dto.Estado,
                Cep = dto.Cep
            },
            Tipo = dto.Tipo,
            Observacoes = dto.Observacoes
        };

        await _unitOfWork.Clientes.CreateAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(cliente);
    }

    public async Task<ClienteDto> UpdateClienteAsync(string id, UpdateClienteDto dto)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null)
        {
            throw new ArgumentException("Cliente não encontrado");
        }

        cliente.AtualizarInformacoes(dto.Nome, dto.Email, dto.Telefone);
        cliente.Endereco = new Endereco
        {
            Logradouro = dto.Endereco,
            Cidade = dto.Cidade,
            Estado = dto.Estado,
            Cep = dto.Cep
        };
        cliente.Observacoes = dto.Observacoes;

        await _unitOfWork.Clientes.UpdateAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(cliente);
    }

    public async Task<bool> DeleteClienteAsync(string id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return false;

        cliente.Ativo = false;
        cliente.DataAtualizacao = DateTime.UtcNow;

        await _unitOfWork.Clientes.UpdateAsync(cliente);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ToggleStatusClienteAsync(string id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return false;

        cliente.Ativo = !cliente.Ativo;
        cliente.DataAtualizacao = DateTime.UtcNow;

        await _unitOfWork.Clientes.UpdateAsync(cliente);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> RegistrarCompraAsync(string id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null) return false;

        cliente.AtualizarUltimaCompra();
        await _unitOfWork.Clientes.UpdateAsync(cliente);
        return await _unitOfWork.SaveChangesAsync();
    }

    private static ClienteDto MapToDto(Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email.Valor,
            Telefone = cliente.Telefone,
            CpfCnpj = cliente.CpfCnpj.Valor,
            Endereco = cliente.Endereco.Logradouro,
            Cidade = cliente.Endereco.Cidade,
            Estado = cliente.Endereco.Estado,
            Cep = cliente.Endereco.Cep,
            Tipo = cliente.Tipo == TipoCliente.PessoaFisica ? "Pessoa Física" : "Pessoa Jurídica",
            Ativo = cliente.Ativo,
            DataCadastro = cliente.DataCriacao,
            UltimaCompra = cliente.UltimaCompra,
            Observacoes = cliente.Observacoes
        };
    }
}