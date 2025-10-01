using MottoSprint.Extensions;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.DTOs;

/// <summary>
/// DTO para resultados paginados com suporte a HATEOAS
/// </summary>
/// <typeparam name="T">Tipo dos dados paginados</typeparam>
public class PagedResultDto<T> : Resource where T : class
{
    /// <summary>
    /// Lista de dados da página atual
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Número da página atual (baseado em 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de registros
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Construtor
    /// </summary>
    public PagedResultDto()
    {
    }

    /// <summary>
    /// Construtor com dados
    /// </summary>
    public PagedResultDto(List<T> data, int pageNumber, int pageSize, int totalRecords)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
    }
}