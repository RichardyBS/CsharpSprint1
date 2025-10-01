using MottoSprint.Extensions;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para notificação de moto com suporte a HATEOAS
/// </summary>
public class MotoNotificationDto : Resource
{
    /// <summary>
    /// ID da notificação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID do cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string? ClienteNome { get; set; }

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
    /// Mensagem da notificação
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Indica se foi lida
    /// </summary>
    public bool Lida { get; set; }

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}