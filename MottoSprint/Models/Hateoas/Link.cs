using System.Text.Json.Serialization;

namespace MottoSprint.Models.Hateoas;

/// <summary>
/// Representa um link HATEOAS para navegação na API
/// </summary>
public class Link
{
    /// <summary>
    /// URL do link
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Relação do link (self, next, prev, etc.)
    /// </summary>
    [JsonPropertyName("rel")]
    public string Rel { get; set; } = string.Empty;

    /// <summary>
    /// Método HTTP para o link
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Tipo de conteúdo aceito
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Título descritivo do link
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    public Link() { }

    public Link(string href, string rel, string method = "GET", string? type = null, string? title = null)
    {
        Href = href;
        Rel = rel;
        Method = method;
        Type = type;
        Title = title;
    }
}