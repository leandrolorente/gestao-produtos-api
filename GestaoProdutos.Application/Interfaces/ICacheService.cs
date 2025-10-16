namespace GestaoProdutos.Application.Interfaces;

/// <summary>
/// Interface para serviços de cache Redis seguindo as melhores práticas de mercado
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Recupera um objeto do cache usando a chave especificada
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser recuperado</typeparam>
    /// <param name="key">Chave única do cache</param>
    /// <returns>Objeto do tipo T ou null se não encontrado</returns>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Armazena um objeto no cache com expiração opcional
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser armazenado</typeparam>
    /// <param name="key">Chave única do cache</param>
    /// <param name="value">Objeto a ser armazenado</param>
    /// <param name="expiry">Tempo de expiração (padrão: 30 minutos)</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    
    /// <summary>
    /// Remove um item específico do cache
    /// </summary>
    /// <param name="key">Chave do item a ser removido</param>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Remove múltiplos itens do cache baseado em um padrão
    /// </summary>
    /// <param name="pattern">Padrão de busca (ex: "produtos:*")</param>
    Task RemovePatternAsync(string pattern);
    
    /// <summary>
    /// Verifica se uma chave existe no cache
    /// </summary>
    /// <param name="key">Chave a ser verificada</param>
    /// <returns>True se existe, False caso contrário</returns>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// Armazena múltiplos itens no cache de uma só vez
    /// </summary>
    /// <typeparam name="T">Tipo dos objetos</typeparam>
    /// <param name="items">Dicionário com chave-valor dos itens</param>
    /// <param name="expiry">Tempo de expiração</param>
    Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null) where T : class;
    
    /// <summary>
    /// Recupera múltiplos itens do cache
    /// </summary>
    /// <typeparam name="T">Tipo dos objetos</typeparam>
    /// <param name="keys">Lista de chaves</param>
    /// <returns>Dicionário com os itens encontrados</returns>
    Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class;
    
    /// <summary>
    /// Incrementa um contador no cache (útil para estatísticas)
    /// </summary>
    /// <param name="key">Chave do contador</param>
    /// <param name="increment">Valor a incrementar (padrão: 1)</param>
    /// <returns>Novo valor do contador</returns>
    Task<long> IncrementAsync(string key, long increment = 1);
    
    /// <summary>
    /// Define TTL (Time To Live) para uma chave existente
    /// </summary>
    /// <param name="key">Chave do cache</param>
    /// <param name="expiry">Novo tempo de expiração</param>
    Task SetExpiryAsync(string key, TimeSpan expiry);
}
