using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestaoProdutos.Domain.Entities;

/// <summary>
/// Entidade para armazenamento de endereços
/// </summary>
public class EnderecoEntity : BaseEntity
{
    [BsonElement("clienteId")]
    public string ClienteId { get; set; } = string.Empty;
    
    [BsonElement("cep")]
    public string Cep { get; set; } = string.Empty;
    
    [BsonElement("logradouro")]
    public string Logradouro { get; set; } = string.Empty;
    
    [BsonElement("numero")]
    public string Numero { get; set; } = string.Empty;
    
    [BsonElement("complemento")]
    public string Complemento { get; set; } = string.Empty;
    
    [BsonElement("unidade")]
    public string Unidade { get; set; } = string.Empty;
    
    [BsonElement("bairro")]
    public string Bairro { get; set; } = string.Empty;
    
    [BsonElement("localidade")]
    public string Localidade { get; set; } = string.Empty;
    
    [BsonElement("uf")]
    public string Uf { get; set; } = string.Empty;
    
    [BsonElement("estado")]
    public string Estado { get; set; } = string.Empty;
    
    [BsonElement("regiao")]
    public string Regiao { get; set; } = string.Empty;
    
    [BsonElement("referencia")]
    public string? Referencia { get; set; }
    
    [BsonElement("isPrincipal")]
    public bool IsPrincipal { get; set; } = true;
    
    [BsonElement("tipo")]
    public string Tipo { get; set; } = "Residencial"; // Residencial, Comercial, Cobrança, etc.
    
    // Métodos de domínio
    public string EnderecoCompleto => 
        $"{Logradouro}, {Numero}" +
        (!string.IsNullOrWhiteSpace(Complemento) ? $", {Complemento}" : "") +
        $" - {Bairro}, {Localidade}/{Uf}, CEP: {Cep}";
    
    public void AtualizarDados(string cep, string logradouro, string numero, string complemento, 
        string unidade, string bairro, string localidade, string uf, string estado, string regiao)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Unidade = unidade;
        Bairro = bairro;
        Localidade = localidade;
        Uf = uf;
        Estado = estado;
        Regiao = regiao;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool IsValidoCep()
    {
        var cepLimpo = System.Text.RegularExpressions.Regex.Replace(Cep, @"[^\d]", "");
        return cepLimpo.Length == 8 && cepLimpo.All(char.IsDigit);
    }
}
