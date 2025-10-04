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
        try
        {
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            var clientesDto = new List<ClienteDto>();
            
            foreach (var cliente in clientes)
            {
                // Log para debug
                Console.WriteLine($"Cliente: {cliente.Id}, Nome: {cliente.Nome}, Email: {cliente.Email?.Valor}, Telefone: {cliente.Telefone}, EnderecoId: {cliente.EnderecoId}");
                clientesDto.Add(await MapToDtoAsync(cliente));
            }
            
            return clientesDto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro em GetAllClientesAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<ClienteDto?> GetClienteByIdAsync(string id)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        return cliente != null ? await MapToDtoAsync(cliente) : null;
    }

    public async Task<ClienteDto?> GetClienteByCpfCnpjAsync(string cpfCnpj)
    {
        var cliente = await _unitOfWork.Clientes.GetClientePorCpfCnpjAsync(cpfCnpj);
        return cliente != null ? await MapToDtoAsync(cliente) : null;
    }

    public async Task<IEnumerable<ClienteDto>> GetClientesAtivosPorTipoAsync(TipoCliente tipo)
    {
        var clientes = await _unitOfWork.Clientes.GetClientesAtivosPorTipoAsync(tipo);
        var clientesDto = new List<ClienteDto>();
        
        foreach (var cliente in clientes)
        {
            clientesDto.Add(await MapToDtoAsync(cliente));
        }
        
        return clientesDto;
    }

    public async Task<IEnumerable<ClienteDto>> GetClientesComCompraRecenteAsync(int dias = 30)
    {
        var clientes = await _unitOfWork.Clientes.GetClientesComCompraRecenteAsync(dias);
        var clientesDto = new List<ClienteDto>();
        
        foreach (var cliente in clientes)
        {
            clientesDto.Add(await MapToDtoAsync(cliente));
        }
        
        return clientesDto;
    }

    public async Task<ClienteDto> CreateClienteAsync(CreateClienteDto dto)
    {
        // Validar se CPF/CNPJ já existe
        if (await _unitOfWork.Clientes.CpfCnpjJaExisteAsync(dto.CpfCnpj))
        {
            throw new InvalidOperationException("CPF/CNPJ já cadastrado");
        }

        // Criar o endereço primeiro
        var endereco = new EnderecoEntity
        {
            Cep = dto.Endereco.Cep,
            Logradouro = dto.Endereco.Logradouro,
            Numero = dto.Endereco.Numero,
            Complemento = dto.Endereco.Complemento ?? string.Empty,
            Unidade = dto.Endereco.Unidade,
            Bairro = dto.Endereco.Bairro,
            Localidade = dto.Endereco.Localidade,
            Uf = dto.Endereco.Uf,
            Estado = dto.Endereco.Estado,
            Regiao = dto.Endereco.Regiao,
            Referencia = dto.Endereco.Referencia,
            IsPrincipal = dto.Endereco.IsPrincipal,
            Tipo = dto.Endereco.Tipo
        };

        var enderecoCreated = await _unitOfWork.Enderecos.CreateAsync(endereco);

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Email = new Email(dto.Email),
            Telefone = dto.Telefone,
            CpfCnpj = new CpfCnpj(dto.CpfCnpj),
            EnderecoId = enderecoCreated.Id,
            Tipo = dto.Tipo,
            Observacoes = dto.Observacoes
        };

        await _unitOfWork.Clientes.CreateAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(cliente);
    }

    public async Task<ClienteDto> UpdateClienteAsync(string id, UpdateClienteDto dto)
    {
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null)
        {
            throw new ArgumentException("Cliente não encontrado");
        }

        // Atualizar ou criar endereço se dto.Endereco for fornecido
        if (dto.Endereco != null)
        {
            if (!string.IsNullOrEmpty(cliente.EnderecoId))
            {
                // Cliente já tem endereço - atualizar
                var endereco = await _unitOfWork.Enderecos.GetByIdAsync(cliente.EnderecoId);
                if (endereco != null)
                {
                    Console.WriteLine($"Atualizando endereço existente para cliente {cliente.Nome}");
                    endereco.Cep = dto.Endereco.Cep;
                    endereco.Logradouro = dto.Endereco.Logradouro;
                    endereco.Numero = dto.Endereco.Numero;
                    endereco.Complemento = dto.Endereco.Complemento ?? string.Empty;
                    endereco.Unidade = dto.Endereco.Unidade;
                    endereco.Bairro = dto.Endereco.Bairro;
                    endereco.Localidade = dto.Endereco.Localidade;
                    endereco.Uf = dto.Endereco.Uf;
                    endereco.Estado = dto.Endereco.Estado;
                    endereco.Regiao = dto.Endereco.Regiao;
                    endereco.Referencia = dto.Endereco.Referencia;
                    endereco.IsPrincipal = dto.Endereco.IsPrincipal;
                    endereco.Tipo = dto.Endereco.Tipo;

                    await _unitOfWork.Enderecos.UpdateAsync(endereco);
                }
            }
            else
            {
                // Cliente não tem endereço - criar novo
                Console.WriteLine($"Criando novo endereço para cliente {cliente.Nome}");
                var novoEndereco = new EnderecoEntity
                {
                    Cep = dto.Endereco.Cep,
                    Logradouro = dto.Endereco.Logradouro,
                    Numero = dto.Endereco.Numero,
                    Complemento = dto.Endereco.Complemento ?? string.Empty,
                    Unidade = dto.Endereco.Unidade,
                    Bairro = dto.Endereco.Bairro,
                    Localidade = dto.Endereco.Localidade,
                    Uf = dto.Endereco.Uf,
                    Estado = dto.Endereco.Estado,
                    Regiao = dto.Endereco.Regiao,
                    Referencia = dto.Endereco.Referencia,
                    IsPrincipal = dto.Endereco.IsPrincipal,
                    Tipo = dto.Endereco.Tipo
                };

                await _unitOfWork.Enderecos.CreateAsync(novoEndereco);
                cliente.EnderecoId = novoEndereco.Id;
                Console.WriteLine($"Endereço criado com ID: {novoEndereco.Id} para cliente {cliente.Nome}");
            }
        }

        cliente.AtualizarInformacoes(dto.Nome, dto.Email, dto.Telefone, dto.CpfCnpj);
        cliente.Observacoes = dto.Observacoes;

        await _unitOfWork.Clientes.UpdateAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(cliente);
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

    private async Task<ClienteDto> MapToDtoAsync(Cliente cliente)
    {
        EnderecoDto? enderecoDto = null;
        
        if (!string.IsNullOrEmpty(cliente.EnderecoId))
        {
            Console.WriteLine($"Buscando endereço para cliente {cliente.Nome} com EnderecoId: {cliente.EnderecoId}");
            var endereco = await _unitOfWork.Enderecos.GetByIdAsync(cliente.EnderecoId);
            if (endereco != null)
            {
                Console.WriteLine($"Endereço encontrado: {endereco.Logradouro}, {endereco.Localidade}");
                enderecoDto = new EnderecoDto
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
            else
            {
                Console.WriteLine($"Endereço não encontrado para EnderecoId: {cliente.EnderecoId}");
            }
        }
        else
        {
            Console.WriteLine($"Cliente {cliente.Nome} não possui EnderecoId");
        }

        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email?.Valor ?? string.Empty,
            Telefone = cliente.Telefone,
            CpfCnpj = cliente.CpfCnpj?.Valor ?? string.Empty,
            Endereco = enderecoDto,
            Tipo = cliente.Tipo == TipoCliente.PessoaFisica ? "Pessoa Física" : "Pessoa Jurídica",
            Ativo = cliente.Ativo,
            DataCadastro = cliente.DataCriacao,
            UltimaCompra = cliente.UltimaCompra,
            Observacoes = cliente.Observacoes
        };
    }
}