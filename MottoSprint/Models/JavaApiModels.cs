using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MottoSprint.Models;

/// <summary>
/// Modelo para representar uma moto da API Java
/// </summary>
public class Moto
{
    [JsonPropertyName("placa")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("ano")]
    public int Ano { get; set; }

    [JsonPropertyName("cor")]
    public string Cor { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // NORMAL ou DEFEITO
}

/// <summary>
/// Modelo para representar uma vaga da API Java
/// </summary>
public class Vaga
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("linha")]
    public string Linha { get; set; } = string.Empty;

    [JsonPropertyName("coluna")]
    public string Coluna { get; set; } = string.Empty;

    [JsonPropertyName("ocupada")]
    public bool Ocupada { get; set; }

    [JsonPropertyName("moto")]
    public Moto? Moto { get; set; }
}

/// <summary>
/// Modelo para mover moto para vaga
/// </summary>
public class MoverMotoRequest
{
    [JsonPropertyName("placa")]
    [Required(ErrorMessage = "Placa é obrigatória")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("idVaga")]
    [Required(ErrorMessage = "ID da vaga é obrigatório")]
    public int IdVaga { get; set; }
}

/// <summary>
/// Modelo para criar nova moto
/// </summary>
public class CreateMotoRequest
{
    [JsonPropertyName("placa")]
    [Required(ErrorMessage = "Placa é obrigatória")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    [Required(ErrorMessage = "Modelo é obrigatório")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("ano")]
    [Required(ErrorMessage = "Ano é obrigatório")]
    public int Ano { get; set; }

    [JsonPropertyName("cor")]
    [Required(ErrorMessage = "Cor é obrigatória")]
    public string Cor { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [Required(ErrorMessage = "Status é obrigatório")]
    public string Status { get; set; } = "NORMAL"; // NORMAL ou DEFEITO
}

/// <summary>
/// Modelo para criar nova vaga
/// </summary>
public class CreateVagaRequest
{
    [JsonPropertyName("linha")]
    [Required(ErrorMessage = "Linha é obrigatória")]
    public string Linha { get; set; } = string.Empty;

    [JsonPropertyName("coluna")]
    [Required(ErrorMessage = "Coluna é obrigatória")]
    public string Coluna { get; set; } = string.Empty;
}