using System.ComponentModel.DataAnnotations;

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
}