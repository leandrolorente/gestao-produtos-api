namespace GestaoProdutos.Infrastructure.Helpers;

/// <summary>
/// Helper para gerar chaves padronizadas do cache Redis
/// Segue as melhores práticas de nomenclatura hierárquica
/// </summary>
public static class CacheKeyHelper
{
    private const string SEPARATOR = ":";
    private const string APP_PREFIX = "gp"; // gestao-produtos
    
    // === DASHBOARD ===
    public static string DashboardMain() => $"{APP_PREFIX}{SEPARATOR}dashboard{SEPARATOR}main";
    public static string DashboardVendas(DateTime? data = null) => 
        $"{APP_PREFIX}{SEPARATOR}dashboard{SEPARATOR}vendas{SEPARATOR}{(data?.ToString("yyyyMMdd") ?? "all")}";
    
    // === PRODUTOS ===
    public static string ProdutosList(int page, int size, string? search = null) => 
        $"{APP_PREFIX}{SEPARATOR}produtos{SEPARATOR}list{SEPARATOR}p{page}{SEPARATOR}s{size}{SEPARATOR}{(search ?? "all")}";
    public static string ProdutoById(string id) => $"{APP_PREFIX}{SEPARATOR}produto{SEPARATOR}{id}";
    public static string ProdutosBaixoEstoque() => $"{APP_PREFIX}{SEPARATOR}produtos{SEPARATOR}baixo-estoque";
    public static string ProdutosByCategoria(string categoria) => $"{APP_PREFIX}{SEPARATOR}produtos{SEPARATOR}cat{SEPARATOR}{categoria}";
    
    // === CLIENTES ===
    public static string ClientesList(int page, int size, string? search = null) => 
        $"{APP_PREFIX}{SEPARATOR}clientes{SEPARATOR}list{SEPARATOR}p{page}{SEPARATOR}s{size}{SEPARATOR}{(search ?? "all")}";
    public static string ClienteById(string id) => $"{APP_PREFIX}{SEPARATOR}cliente{SEPARATOR}{id}";
    public static string ClienteWithEndereco(string id) => $"{APP_PREFIX}{SEPARATOR}cliente{SEPARATOR}endereco{SEPARATOR}{id}";
    
    // === VENDAS ===
    public static string VendasList(int page, int size, DateTime? inicio = null, DateTime? fim = null) => 
        $"{APP_PREFIX}{SEPARATOR}vendas{SEPARATOR}list{SEPARATOR}p{page}{SEPARATOR}s{size}{SEPARATOR}{inicio?.ToString("yyyyMMdd") ?? "all"}{SEPARATOR}{fim?.ToString("yyyyMMdd") ?? "all"}";
    public static string VendaById(string id) => $"{APP_PREFIX}{SEPARATOR}venda{SEPARATOR}{id}";
    public static string VendasByCliente(string clienteId) => $"{APP_PREFIX}{SEPARATOR}vendas{SEPARATOR}cliente{SEPARATOR}{clienteId}";
    
    // === USUÁRIOS ===
    public static string UsuarioById(string id) => $"{APP_PREFIX}{SEPARATOR}usuario{SEPARATOR}{id}";
    public static string UsuarioSession(string id) => $"{APP_PREFIX}{SEPARATOR}session{SEPARATOR}{id}";
    public static string UsuariosList() => $"{APP_PREFIX}{SEPARATOR}usuarios{SEPARATOR}list";
    
    // === ENDEREÇOS ===
    public static string EnderecoById(string id) => $"{APP_PREFIX}{SEPARATOR}endereco{SEPARATOR}{id}";
    public static string ViaCepResult(string cep) => $"{APP_PREFIX}{SEPARATOR}viacep{SEPARATOR}{cep}";
    
    // === CONFIGURAÇÕES ===
    public static string ConfigByKey(string key) => $"{APP_PREFIX}{SEPARATOR}config{SEPARATOR}{key}";
    public static string ConfigsList() => $"{APP_PREFIX}{SEPARATOR}configs{SEPARATOR}list";
    
    // === RELATÓRIOS ===
    public static string RelatorioVendas(DateTime inicio, DateTime fim) => 
        $"{APP_PREFIX}{SEPARATOR}relatorio{SEPARATOR}vendas{SEPARATOR}{inicio:yyyyMMdd}{SEPARATOR}{fim:yyyyMMdd}";
    public static string RelatorioEstoque() => $"{APP_PREFIX}{SEPARATOR}relatorio{SEPARATOR}estoque";
    public static string RelatorioFinanceiro(DateTime inicio, DateTime fim) => 
        $"{APP_PREFIX}{SEPARATOR}relatorio{SEPARATOR}financeiro{SEPARATOR}{inicio:yyyyMMdd}{SEPARATOR}{fim:yyyyMMdd}";
    
    // === BUSCA ===
    public static string SearchProdutos(string termo) => $"{APP_PREFIX}{SEPARATOR}search{SEPARATOR}produtos{SEPARATOR}{termo.ToLower()}";
    public static string SearchClientes(string termo) => $"{APP_PREFIX}{SEPARATOR}search{SEPARATOR}clientes{SEPARATOR}{termo.ToLower()}";
    public static string SearchVendas(string termo) => $"{APP_PREFIX}{SEPARATOR}search{SEPARATOR}vendas{SEPARATOR}{termo.ToLower()}";
    
    // === COUNTERS/ESTATÍSTICAS ===
    public static string CounterLogin() => $"{APP_PREFIX}{SEPARATOR}counter{SEPARATOR}login";
    public static string CounterApiCalls(string endpoint) => $"{APP_PREFIX}{SEPARATOR}counter{SEPARATOR}api{SEPARATOR}{endpoint}";
    public static string CounterVendasDia(DateTime data) => $"{APP_PREFIX}{SEPARATOR}counter{SEPARATOR}vendas{SEPARATOR}{data:yyyyMMdd}";
    
    // === PADRÕES PARA INVALIDAÇÃO ===
    public static string PatternDashboard() => $"{APP_PREFIX}{SEPARATOR}dashboard{SEPARATOR}*";
    public static string PatternProdutos() => $"{APP_PREFIX}{SEPARATOR}produto*";
    public static string PatternClientes() => $"{APP_PREFIX}{SEPARATOR}cliente*";
    public static string PatternVendas() => $"{APP_PREFIX}{SEPARATOR}venda*";
    public static string PatternRelatorios() => $"{APP_PREFIX}{SEPARATOR}relatorio{SEPARATOR}*";
    public static string PatternSearch() => $"{APP_PREFIX}{SEPARATOR}search{SEPARATOR}*";
    
    // === UTILITY METHODS ===
    
    /// <summary>
    /// Gera uma chave personalizada seguindo o padrão hierárquico
    /// </summary>
    /// <param name="module">Módulo (ex: "produto", "cliente")</param>
    /// <param name="operation">Operação (ex: "list", "detail", "search")</param>
    /// <param name="parameters">Parâmetros adicionais</param>
    /// <returns>Chave formatada</returns>
    public static string CustomKey(string module, string operation, params string[] parameters)
    {
        var key = $"{APP_PREFIX}{SEPARATOR}{module}{SEPARATOR}{operation}";
        if (parameters.Length > 0)
        {
            key += SEPARATOR + string.Join(SEPARATOR, parameters);
        }
        return key;
    }
    
    /// <summary>
    /// Gera padrão de invalidação para um módulo específico
    /// </summary>
    /// <param name="module">Nome do módulo</param>
    /// <returns>Padrão para invalidação</returns>
    public static string InvalidationPattern(string module) => $"{APP_PREFIX}{SEPARATOR}{module}*";
}
