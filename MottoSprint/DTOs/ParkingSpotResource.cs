using System.Text.Json.Serialization;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para ParkingSpot com suporte a HATEOAS
/// </summary>
public class ParkingSpotResource : Resource
{
    /// <summary>
    /// ID da vaga
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Número da vaga
    /// </summary>
    [JsonPropertyName("spotNumber")]
    public string SpotNumber { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a vaga está ocupada
    /// </summary>
    [JsonPropertyName("isOccupied")]
    public bool IsOccupied { get; set; }

    /// <summary>
    /// Data de criação da vaga
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de ocupação da vaga
    /// </summary>
    [JsonPropertyName("occupiedAt")]
    public DateTime? OccupiedAt { get; set; }

    /// <summary>
    /// Placa do veículo (se ocupada)
    /// </summary>
    [JsonPropertyName("vehiclePlate")]
    public string? VehiclePlate { get; set; }

    /// <summary>
    /// Status da vaga em texto
    /// </summary>
    [JsonPropertyName("status")]
    public string Status => IsOccupied ? "Ocupada" : "Disponível";

    /// <summary>
    /// Tempo de ocupação (se ocupada)
    /// </summary>
    [JsonPropertyName("occupationDuration")]
    public string? OccupationDuration
    {
        get
        {
            if (!IsOccupied || !OccupiedAt.HasValue)
                return null;

            var duration = DateTime.Now - OccupiedAt.Value;
            return $"{duration.Days}d {duration.Hours}h {duration.Minutes}m";
        }
    }
}