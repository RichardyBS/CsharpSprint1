using System.Text.Json.Serialization;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para Notification com suporte a HATEOAS
/// </summary>
public class NotificationResource : Resource
{
    /// <summary>
    /// ID da notificação
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Título da notificação
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem da notificação
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da notificação
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a notificação foi lida
    /// </summary>
    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; }

    /// <summary>
    /// Data de criação da notificação
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de leitura da notificação
    /// </summary>
    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Status da notificação em texto
    /// </summary>
    [JsonPropertyName("status")]
    public string Status => IsRead ? "Lida" : "Não lida";

    /// <summary>
    /// Prioridade baseada no tipo
    /// </summary>
    [JsonPropertyName("priority")]
    public string Priority
    {
        get
        {
            return Type.ToLower() switch
            {
                "error" or "erro" => "Alta",
                "warning" or "aviso" => "Média",
                "info" or "informacao" => "Baixa",
                _ => "Normal"
            };
        }
    }

    /// <summary>
    /// Tempo desde a criação
    /// </summary>
    [JsonPropertyName("timeAgo")]
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - CreatedAt;
            if (timeSpan.TotalMinutes < 1)
                return "Agora mesmo";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes} minutos atrás";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours} horas atrás";
            return $"{(int)timeSpan.TotalDays} dias atrás";
        }
    }
}