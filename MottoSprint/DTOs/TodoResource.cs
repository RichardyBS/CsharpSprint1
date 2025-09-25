using System.Text.Json.Serialization;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para Todo com suporte a HATEOAS
/// </summary>
public class TodoResource : Resource
{
    /// <summary>
    /// ID da tarefa
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Título da tarefa
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da tarefa
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Indica se a tarefa está completa
    /// </summary>
    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Data de criação da tarefa
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de conclusão da tarefa
    /// </summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Status da tarefa em texto
    /// </summary>
    [JsonPropertyName("status")]
    public string Status => IsCompleted ? "Concluída" : "Pendente";

    /// <summary>
    /// Tempo desde a criação
    /// </summary>
    [JsonPropertyName("ageInDays")]
    public int AgeInDays => (DateTime.Now - CreatedAt).Days;

    /// <summary>
    /// Duração para conclusão (se concluída)
    /// </summary>
    [JsonPropertyName("completionDuration")]
    public string? CompletionDuration
    {
        get
        {
            if (!IsCompleted || !CompletedAt.HasValue)
                return null;

            var duration = CompletedAt.Value - CreatedAt;
            return $"{duration.Days}d {duration.Hours}h {duration.Minutes}m";
        }
    }
}