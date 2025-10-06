using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.ValueObjects;
using GestaoProdutos.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Xunit;
using FluentAssertions;

namespace GestaoProdutos.Tests.Unit.Entities;

public class ClienteSerializationTests
{
    [Fact]
    public void Cliente_Should_Serialize_And_Deserialize_Correctly()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Nome = "João Silva",
            Email = new Email("joao@email.com"),
            Telefone = "(18) 99999-9999",
            CpfCnpj = new CpfCnpj("12345678901"),
            EnderecoId = ObjectId.GenerateNewId().ToString(),
            Tipo = TipoCliente.PessoaFisica,
            Observacoes = "Cliente teste",
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        // Act - Serialize to BSON and back
        var bsonDocument = cliente.ToBsonDocument();
        var deserializedCliente = BsonSerializer.Deserialize<Cliente>(bsonDocument);
        
        // Assert - Check if all properties are preserved
        deserializedCliente.Nome.Should().Be("João Silva");
        deserializedCliente.Email?.Valor.Should().Be("joao@email.com");
        deserializedCliente.Telefone.Should().Be("(18) 99999-9999");
        deserializedCliente.CpfCnpj?.Valor.Should().Be("12345678901");
        deserializedCliente.EnderecoId.Should().Be(cliente.EnderecoId);
        deserializedCliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        deserializedCliente.Observacoes.Should().Be("Cliente teste");
        deserializedCliente.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Cliente_Should_Handle_Legacy_Document_With_EnderecoId()
    {
        // Arrange - Create a BSON document without legacy endereco field (only EnderecoId)
        var legacyBsonDocument = new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["Nome"] = "João Silva Legacy",
            ["Email"] = new BsonDocument { ["Valor"] = "joao.legacy@email.com" },
            ["Telefone"] = "(18) 88888-8888",
            ["CpfCnpj"] = new BsonDocument { ["Valor"] = "98765432109" },
            ["enderecoId"] = "endereco_legacy_id", // New EnderecoId field
            ["Tipo"] = (int)TipoCliente.PessoaFisica,
            ["Ativo"] = true,
            ["DataCriacao"] = DateTime.UtcNow,
            ["DataAtualizacao"] = DateTime.UtcNow
        };

        // Act - Should not throw exception when deserializing
        var deserializationAction = () => BsonSerializer.Deserialize<Cliente>(legacyBsonDocument);

        // Assert - Should successfully deserialize ignoring legacy field
        deserializationAction.Should().NotThrow();
        
        var cliente = deserializationAction();
        cliente.Nome.Should().Be("João Silva Legacy");
        cliente.Email?.Valor.Should().Be("joao.legacy@email.com");
        cliente.Telefone.Should().Be("(18) 88888-8888");
        cliente.CpfCnpj?.Valor.Should().Be("98765432109");
        cliente.EnderecoId.Should().Be("endereco_legacy_id"); // EnderecoId was successfully deserialized
    }
}