namespace Analytics.Service.Models;

public class OcupacaoVaga
{
    public Guid Id { get; set; }
    public Guid VagaId { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public DateTime? DataSaida { get; set; }
    public decimal? ValorCobrado { get; set; }
    public string Status { get; set; } = string.Empty; // "Ocupada", "Liberada"
}