using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

/// <summary>
/// Interface para serviço de integração com ViaCEP
/// </summary>
public interface IViaCepService
{
    /// <summary>
    /// Busca informações de endereço pelo CEP
    /// </summary>
    /// <param name="cep">CEP para consulta (formato: 12345678 ou 12345-678)</param>
    /// <returns>Dados do endereço ou null se não encontrado</returns>
    Task<ViaCepResponseDto?> BuscarEnderecoPorCepAsync(string cep);
}