namespace GestaoProdutos.Application.DTOs;

public record VendasStatsDto
{
    public int TotalVendas { get; init; }
    public int VendasHoje { get; init; }
    public decimal FaturamentoMes { get; init; }
    public decimal TicketMedio { get; init; }
    public int VendasPendentes { get; init; }
    public IEnumerable<TopClienteDto> TopClientes { get; init; } = new List<TopClienteDto>();
    public IEnumerable<VendasPorMesDto> VendasPorMes { get; init; } = new List<VendasPorMesDto>();
}