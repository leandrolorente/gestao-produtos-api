using System.Text.RegularExpressions;

namespace GestaoProdutos.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }
    
    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email não pode ser vazio");
            
        if (!IsValid(valor))
            throw new ArgumentException("Email inválido");
            
        Valor = valor.ToLowerInvariant();
    }
    
    private static bool IsValid(string email)
    {
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    
    public static implicit operator string(Email email) => email.Valor;
    public static implicit operator Email(string valor) => new(valor);
}

public record CpfCnpj
{
    public string Valor { get; }
    public bool EhCpf => Valor.Replace(".", "").Replace("-", "").Replace("/", "").Length == 11;
    public bool EhCnpj => !EhCpf;
    
    public CpfCnpj(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("CPF/CNPJ não pode ser vazio");
            
        var apenasNumeros = Regex.Replace(valor, @"[^\d]", "");
        
        if (apenasNumeros.Length != 11 && apenasNumeros.Length != 14)
            throw new ArgumentException("CPF/CNPJ deve ter 11 ou 14 dígitos");
            
        Valor = valor;
    }
    
    public static implicit operator string(CpfCnpj cpfCnpj) => cpfCnpj.Valor;
    public static implicit operator CpfCnpj(string valor) => new(valor);
}

public record Endereco
{
    public string Logradouro { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string? Complemento { get; init; }
    
    public string EnderecoCompleto =>
        $"{Logradouro}, {Cidade}/{Estado} - {Cep}";
}
