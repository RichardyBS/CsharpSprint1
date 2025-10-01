using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para requisição de criação de moto (compatível com Java API)
/// </summary>
public class MotoRequestDto : Resource
{
    [JsonPropertyName("placa")]
    [Required(ErrorMessage = "A placa é obrigatória")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    [Required(ErrorMessage = "O modelo é obrigatório")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("ano")]
    [Required(ErrorMessage = "O ano é obrigatório")]
    public int Ano { get; set; }

    [JsonPropertyName("cor")]
    [Required(ErrorMessage = "A cor é obrigatória")]
    public string Cor { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [Required(ErrorMessage = "O status da moto é obrigatório")]
    public string Status { get; set; } = "NORMAL";
}

/// <summary>
/// DTO para resposta de moto (compatível com Java API)
/// </summary>
public class MotoResponseDto : Resource
{
    [JsonPropertyName("placa")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("ano")]
    public int Ano { get; set; }

    [JsonPropertyName("cor")]
    public string Cor { get; set; } = string.Empty;

    [JsonPropertyName("idVaga")]
    public long? IdVaga { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("linha")]
    public string? Linha { get; set; }

    [JsonPropertyName("coluna")]
    public string? Coluna { get; set; }
}

/// <summary>
/// DTO para requisição de criação de vaga (compatível com Java API)
/// </summary>
public class VagaRequestDto : Resource
{
    [JsonPropertyName("linha")]
    [Required(ErrorMessage = "A linha em que a vaga estará é obrigatória")]
    public string Linha { get; set; } = string.Empty;

    [JsonPropertyName("coluna")]
    [Required(ErrorMessage = "A coluna em que a vaga estará é obrigatória")]
    public string Coluna { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de vaga (compatível com Java API)
/// </summary>
public class VagaResponseDto : Resource
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("posicao")]
    public string Posicao { get; set; } = string.Empty; // A1, B2, etc.

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // LIVRE, OCUPADA

    [JsonPropertyName("placa")]
    public string? Placa { get; set; }

    [JsonPropertyName("modelo")]
    public string? Modelo { get; set; }

    [JsonPropertyName("ano")]
    public int? Ano { get; set; }

    [JsonPropertyName("cor")]
    public string? Cor { get; set; }

    [JsonPropertyName("statusMoto")]
    public string? StatusMoto { get; set; }
}

/// <summary>
/// DTO para mover moto para vaga (compatível com Java API)
/// </summary>
public class MoverMotoVagaDto : Resource
{
    [JsonPropertyName("placa")]
    [Required(ErrorMessage = "A placa é obrigatória")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("idVaga")]
    [Required(ErrorMessage = "O ID da vaga é obrigatório")]
    public long IdVaga { get; set; }
}

/// <summary>
/// DTO para mover moto para vaga usando linha e coluna (compatível com Java API)
/// </summary>
public class MoverMotoLinhaColuna : Resource
{
    [JsonPropertyName("placa")]
    [Required(ErrorMessage = "A placa é obrigatória")]
    public string Placa { get; set; } = string.Empty;

    [JsonPropertyName("linha")]
    [Required(ErrorMessage = "A linha é obrigatória")]
    public string Linha { get; set; } = string.Empty;

    [JsonPropertyName("coluna")]
    [Required(ErrorMessage = "A coluna é obrigatória")]
    public string Coluna { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de linha com vagas livres (compatível com Java API)
/// </summary>
public class LinhaResponseDto : Resource
{
    [JsonPropertyName("linha")]
    public string Linha { get; set; } = string.Empty;

    [JsonPropertyName("vagasLivres")]
    public List<VagaResponseDto> VagasLivres { get; set; } = new();

    [JsonPropertyName("totalVagas")]
    public int TotalVagas { get; set; }

    [JsonPropertyName("vagasOcupadas")]
    public int VagasOcupadas { get; set; }
}

/// <summary>
/// Enum para status de vaga (compatível com Java API)
/// </summary>
public enum StatusVaga
{
    LIVRE,
    OCUPADA
}