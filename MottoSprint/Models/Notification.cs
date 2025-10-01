using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Models;

public class Notification
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mensagem é obrigatória")]
    [StringLength(1000, ErrorMessage = "Mensagem deve ter no máximo 1000 caracteres")]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    // Propriedades para integração com sistema de motos/vagas
    public string? PlacaMoto { get; set; }
    public int? VagaId { get; set; }
    public string NotificationType { get; set; } = "GENERAL"; // MOTO_ENTRADA, MOTO_SAIDA, VAGA_OCUPADA, VAGA_LIBERADA, GENERAL
    public string Priority { get; set; } = "NORMAL"; // LOW, NORMAL, HIGH, URGENT
    
    // HATEOAS Links
    [NotMapped]
    [JsonPropertyName("_links")]
    public Dictionary<string, Link> Links { get; set; } = new();
}