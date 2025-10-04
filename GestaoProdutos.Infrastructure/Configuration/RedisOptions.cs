namespace GestaoProdutos.Infrastructure.Configuration;

/// <summary>
/// Configurações do Redis seguindo as melhores práticas
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";
    
    /// <summary>
    /// String de conexão do Redis
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
    
    /// <summary>
    /// Nome da instância (usado como prefixo nas chaves)
    /// </summary>
    public string InstanceName { get; set; } = "GestaoProdutos";
    
    /// <summary>
    /// Timeout de conexão em segundos
    /// </summary>
    public int ConnectTimeout { get; set; } = 30;
    
    /// <summary>
    /// Timeout de sincronização em segundos
    /// </summary>
    public int SyncTimeout { get; set; } = 30;
    
    /// <summary>
    /// Número de tentativas de reconexão
    /// </summary>
    public int ConnectRetry { get; set; } = 3;
    
    /// <summary>
    /// Habilita SSL/TLS (para produção)
    /// </summary>
    public bool Ssl { get; set; } = false;
    
    /// <summary>
    /// Senha de autenticação do Redis
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// Índice da database (0-15)
    /// </summary>
    public int Database { get; set; } = 0;
    
    /// <summary>
    /// TTL padrão para itens do cache em minutos
    /// </summary>
    public int DefaultTtlMinutes { get; set; } = 30;
    
    /// <summary>
    /// Configurações específicas por tipo de cache
    /// </summary>
    public CacheSettings CacheSettings { get; set; } = new();
}

/// <summary>
/// Configurações específicas de TTL por tipo de dados
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// TTL para dados do dashboard em minutos
    /// </summary>
    public int DashboardTtlMinutes { get; set; } = 5;
    
    /// <summary>
    /// TTL para listagem de produtos em minutos
    /// </summary>
    public int ProductListTtlMinutes { get; set; } = 10;
    
    /// <summary>
    /// TTL para dados de clientes em minutos
    /// </summary>
    public int ClienteTtlMinutes { get; set; } = 30;
    
    /// <summary>
    /// TTL para sessões de usuário em minutos
    /// </summary>
    public int SessionTtlMinutes { get; set; } = 480; // 8 horas
    
    /// <summary>
    /// TTL para configurações do sistema em minutos
    /// </summary>
    public int ConfigTtlMinutes { get; set; } = 60;
    
    /// <summary>
    /// TTL para relatórios em minutos
    /// </summary>
    public int ReportTtlMinutes { get; set; } = 360; // 6 horas
    
    /// <summary>
    /// TTL para resultados de busca em minutos
    /// </summary>
    public int SearchTtlMinutes { get; set; } = 15;
    
    /// <summary>
    /// TTL para dados de endereço (ViaCEP) em horas
    /// </summary>
    public int AddressTtlHours { get; set; } = 24;
}