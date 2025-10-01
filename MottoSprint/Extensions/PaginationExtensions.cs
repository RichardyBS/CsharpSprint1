using Microsoft.EntityFrameworkCore;
using MottoSprint.DTOs;

namespace MottoSprint.Extensions;

/// <summary>
/// Extensões para paginação de dados
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Aplica paginação a uma query
    /// </summary>
    /// <typeparam name="T">Tipo dos dados</typeparam>
    /// <param name="query">Query a ser paginada</param>
    /// <param name="pageNumber">Número da página (baseado em 1)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Resultado paginado</returns>
    public static async Task<PagedResultDto<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query, 
        int pageNumber = 1, 
        int pageSize = 10) where T : class
    {
        // Validar parâmetros
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize)); // Máximo 100 itens por página

        // Contar total de registros
        var totalRecords = await query.CountAsync();

        // Calcular skip
        var skip = (pageNumber - 1) * pageSize;

        // Obter dados da página
        var data = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<T>(data, pageNumber, pageSize, totalRecords);
    }

    /// <summary>
    /// Aplica paginação a uma lista em memória
    /// </summary>
    /// <typeparam name="T">Tipo dos dados</typeparam>
    /// <param name="source">Lista fonte</param>
    /// <param name="pageNumber">Número da página (baseado em 1)</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Resultado paginado</returns>
    public static PagedResultDto<T> ToPaginatedResult<T>(
        this IEnumerable<T> source, 
        int pageNumber = 1, 
        int pageSize = 10) where T : class
    {
        // Validar parâmetros
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var sourceList = source.ToList();
        var totalRecords = sourceList.Count;

        // Calcular skip
        var skip = (pageNumber - 1) * pageSize;

        // Obter dados da página
        var data = sourceList
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<T>(data, pageNumber, pageSize, totalRecords);
    }
}