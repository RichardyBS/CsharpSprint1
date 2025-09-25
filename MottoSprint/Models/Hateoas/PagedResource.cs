using System.Text.Json.Serialization;

namespace MottoSprint.Models.Hateoas;

/// <summary>
/// Representa um recurso paginado com suporte a HATEOAS
/// </summary>
/// <typeparam name="T">Tipo dos itens da página</typeparam>
public class PagedResource<T> : Resource
{
    /// <summary>
    /// Itens da página atual
    /// </summary>
    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; set; } = new List<T>();

    /// <summary>
    /// Informações de paginação
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();

    public PagedResource() { }

    public PagedResource(IEnumerable<T> data, int page, int pageSize, int totalItems)
    {
        Data = data;
        Pagination = new PaginationInfo
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        };
    }
}

/// <summary>
/// Informações de paginação
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// Página atual
    /// </summary>
    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    /// <summary>
    /// Total de itens
    /// </summary>
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    [JsonPropertyName("hasPrevious")]
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    [JsonPropertyName("hasNext")]
    public bool HasNext => CurrentPage < TotalPages;
}