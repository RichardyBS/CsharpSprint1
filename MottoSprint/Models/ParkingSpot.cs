using System.ComponentModel.DataAnnotations;

namespace MottoSprint.Models;

public class ParkingSpot
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Número da vaga é obrigatório")]
    [StringLength(10, ErrorMessage = "Número da vaga deve ter no máximo 10 caracteres")]
    public string SpotNumber { get; set; } = string.Empty;

    public bool IsOccupied { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? OccupiedAt { get; set; }

    public string? VehiclePlate { get; set; }
}