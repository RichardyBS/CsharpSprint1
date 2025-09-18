namespace Analytics.Service.Models;

public class MetricaDiaria
{
    public Guid Id { get; set; }
    public DateTime Data { get; set; }
    public int TotalOcupacoes { get; set; }
    public int VagasOcupadas { get; set; }
    public int VagasLivres { get; set; }
    public decimal ReceitaTotal { get; set; }
    public TimeSpan TempoMedioOcupacao { get; set; }
    public int PicoOcupacao { get; set; }
    public TimeSpan HorarioPico { get; set; }
    public decimal TicketMedio { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}