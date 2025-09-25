using System.Text.Json.Serialization;

namespace MottoSprint.Models.Hateoas;

/// <summary>
/// Classe base para recursos que suportam HATEOAS
/// </summary>
public abstract class Resource
{
    /// <summary>
    /// Links de navegação HATEOAS
    /// </summary>
    [JsonPropertyName("_links")]
    public Dictionary<string, Link> Links { get; set; } = new();

    /// <summary>
    /// Adiciona um link ao recurso
    /// </summary>
    /// <param name="rel">Relação do link</param>
    /// <param name="href">URL do link</param>
    /// <param name="method">Método HTTP</param>
    /// <param name="type">Tipo de conteúdo</param>
    /// <param name="title">Título do link</param>
    public void AddLink(string rel, string href, string method = "GET", string? type = null, string? title = null)
    {
        Links[rel] = new Link(href, rel, method, type, title);
    }

    /// <summary>
    /// Remove um link do recurso
    /// </summary>
    /// <param name="rel">Relação do link a ser removido</param>
    public void RemoveLink(string rel)
    {
        Links.Remove(rel);
    }

    /// <summary>
    /// Verifica se o recurso possui um link específico
    /// </summary>
    /// <param name="rel">Relação do link</param>
    /// <returns>True se o link existe</returns>
    public bool HasLink(string rel)
    {
        return Links.ContainsKey(rel);
    }

    /// <summary>
    /// Obtém um link específico
    /// </summary>
    /// <param name="rel">Relação do link</param>
    /// <returns>Link ou null se não encontrado</returns>
    public Link? GetLink(string rel)
    {
        return Links.TryGetValue(rel, out var link) ? link : null;
    }
}