using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Integration;

/// <summary>
/// Testes de integração específicos para validar o cenário de mudança de CPF para CNPJ
/// e vice-versa, garantindo que o tipo de pessoa seja atualizado corretamente.
/// </summary>
public class ClienteCpfCnpjChangeIntegrationTests
{
    [Fact]
    public void Scenario_EditingClienteFromCpfToCnpj_ShouldChangeTypeToJuridica()
    {
        // Arrange - Cliente inicialmente como Pessoa Física com CPF
        var cliente = new Cliente
        {
            Nome = "João da Silva",
            Email = new Email("joao.silva@email.com"),
            Telefone = "(11) 98765-4321",
            CpfCnpj = new CpfCnpj("12345678901"), // CPF com 11 dígitos
            Tipo = TipoCliente.PessoaFisica,
            Endereco = new Endereco
            {
                Logradouro = "Rua das Flores, 123",
                Cidade = "São Paulo",
                Estado = "SP",
                Cep = "01234-567"
            }
        };

        // Validação inicial - deve ser Pessoa Física
        cliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        cliente.EhPessoaFisica.Should().BeTrue();
        cliente.EhPessoaJuridica.Should().BeFalse();
        cliente.CpfCnpj.EhCpf.Should().BeTrue();
        cliente.CpfCnpj.EhCnpj.Should().BeFalse();

        // Act - Simular edição mudando para CNPJ (empresa)
        cliente.AtualizarInformacoes(
            nome: "Silva Comércio LTDA",
            email: "contato@silvacomercio.com.br",
            telefone: "(11) 3456-7890",
            cpfCnpj: "12345678000195" // CNPJ com 14 dígitos
        );

        // Assert - deve ser Pessoa Jurídica agora
        cliente.Nome.Should().Be("Silva Comércio LTDA");
        cliente.Email!.Valor.Should().Be("contato@silvacomercio.com.br");
        cliente.Telefone.Should().Be("(11) 3456-7890");
        cliente.CpfCnpj!.Valor.Should().Be("12345678000195");
        
        // Verificações críticas do tipo
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.EhPessoaJuridica.Should().BeTrue();
        cliente.EhPessoaFisica.Should().BeFalse();
        cliente.CpfCnpj.EhCnpj.Should().BeTrue();
        cliente.CpfCnpj.EhCpf.Should().BeFalse();
    }

    [Fact]
    public void Scenario_EditingClienteFromCnpjToCpf_ShouldChangeTypeToFisica()
    {
        // Arrange - Cliente inicialmente como Pessoa Jurídica com CNPJ
        var cliente = new Cliente
        {
            Nome = "Tech Solutions LTDA",
            Email = new Email("contato@techsolutions.com.br"),
            Telefone = "(11) 3333-4444",
            CpfCnpj = new CpfCnpj("98765432000155"), // CNPJ com 14 dígitos
            Tipo = TipoCliente.PessoaJuridica,
            Endereco = new Endereco
            {
                Logradouro = "Av. Paulista, 1000",
                Cidade = "São Paulo",
                Estado = "SP",
                Cep = "01310-100"
            }
        };

        // Validação inicial - deve ser Pessoa Jurídica
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.EhPessoaJuridica.Should().BeTrue();
        cliente.EhPessoaFisica.Should().BeFalse();
        cliente.CpfCnpj.EhCnpj.Should().BeTrue();
        cliente.CpfCnpj.EhCpf.Should().BeFalse();

        // Act - Simular edição mudando para CPF (pessoa física)
        cliente.AtualizarInformacoes(
            nome: "Maria Santos",
            email: "maria.santos@email.com",
            telefone: "(11) 99999-8888",
            cpfCnpj: "98765432100" // CPF com 11 dígitos
        );

        // Assert - deve ser Pessoa Física agora
        cliente.Nome.Should().Be("Maria Santos");
        cliente.Email!.Valor.Should().Be("maria.santos@email.com");
        cliente.Telefone.Should().Be("(11) 99999-8888");
        cliente.CpfCnpj!.Valor.Should().Be("98765432100");
        
        // Verificações críticas do tipo
        cliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        cliente.EhPessoaFisica.Should().BeTrue();
        cliente.EhPessoaJuridica.Should().BeFalse();
        cliente.CpfCnpj.EhCpf.Should().BeTrue();
        cliente.CpfCnpj.EhCnpj.Should().BeFalse();
    }

    [Fact]
    public void Scenario_EditingClienteWithFormattedDocuments_ShouldWorkCorrectly()
    {
        // Arrange - Cliente com CPF formatado
        var cliente = new Cliente
        {
            Nome = "Ana Costa",
            Email = new Email("ana@email.com"),
            CpfCnpj = new CpfCnpj("123.456.789-01"), // CPF formatado
            Tipo = TipoCliente.PessoaFisica
        };

        // Act - Mudar para CNPJ formatado
        cliente.AtualizarInformacoes(
            nome: "Costa & Associados LTDA",
            email: "contato@costa.com.br",
            telefone: "(11) 2222-3333",
            cpfCnpj: "12.345.678/0001-95" // CNPJ formatado
        );

        // Assert
        cliente.Nome.Should().Be("Costa & Associados LTDA");
        cliente.CpfCnpj!.Valor.Should().Be("12.345.678/0001-95");
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.CpfCnpj.EhCnpj.Should().BeTrue();
    }

    [Fact]
    public void Scenario_MultipleEdits_ShouldAlwaysHaveCorrectType()
    {
        // Arrange
        var cliente = new Cliente();

        // Act & Assert - Primeira edição: CPF
        cliente.AtualizarInformacoes("João", "joao@email.com", "11999999999", "12345678901");
        cliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        cliente.CpfCnpj!.EhCpf.Should().BeTrue();

        // Act & Assert - Segunda edição: CNPJ
        cliente.AtualizarInformacoes("João LTDA", "contato@joao.com", "1133333333", "12345678000195");
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.CpfCnpj!.EhCnpj.Should().BeTrue();

        // Act & Assert - Terceira edição: CPF novamente
        cliente.AtualizarInformacoes("João Silva", "joao.silva@email.com", "11888888888", "98765432100");
        cliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        cliente.CpfCnpj!.EhCpf.Should().BeTrue();

        // Act & Assert - Quarta edição: CNPJ novamente
        cliente.AtualizarInformacoes("Silva Enterprises", "silva@enterprise.com", "1144444444", "98765432000188");
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.CpfCnpj!.EhCnpj.Should().BeTrue();
    }
}