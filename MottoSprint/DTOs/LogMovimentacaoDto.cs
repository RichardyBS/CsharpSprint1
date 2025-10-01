using MottoSprint.Extensions;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para log de movimentação com suporte a HATEOAS
/// </summary>
public class LogMovimentacaoDto : Resource
{
    /// <summary>
    /// ID do log
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID do cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Placa da moto
    /// </summary>
    public string MotoPlaca { get; set; } = string.Empty;

    /// <summary>
    /// ID da vaga
    /// </summary>
    public Guid VagaId { get; set; }

    /// <summary>
    /// Tipo de movimentação (ENTRADA/SAIDA)
    /// </summary>
    public string TipoMovimentacao { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp do evento
    /// </summary>
    public DateTime TimestampEvento { get; set; }

    /// <summary>
    /// Detalhes da movimentação
    /// </summary>
    public string Detalhes { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; }
}