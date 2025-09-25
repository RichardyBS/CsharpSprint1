using Microsoft.AspNetCore.Mvc;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Extensions;

/// <summary>
/// Extensões para facilitar a implementação de HATEOAS
/// </summary>
public static class HateoasExtensions
{
    /// <summary>
    /// Adiciona links de navegação básicos para um recurso
    /// </summary>
    /// <param name="resource">Recurso a ser enriquecido</param>
    /// <param name="urlHelper">Helper para geração de URLs</param>
    /// <param name="controller">Nome do controller</param>
    /// <param name="id">ID do recurso</param>
    public static void AddSelfLink(this Resource resource, IUrlHelper urlHelper, string controller, object? id = null)
    {
        var action = id != null ? "GetById" : "GetAll";
        var routeValues = id != null ? new { id } : null;
        
        var url = urlHelper.Action(action, controller, routeValues, urlHelper.ActionContext.HttpContext.Request.Scheme);
        if (!string.IsNullOrEmpty(url))
        {
            resource.AddLink("self", url, "GET", "application/json", "Link para este recurso");
        }
    }

    /// <summary>
    /// Adiciona links de CRUD para um recurso
    /// </summary>
    /// <param name="resource">Recurso a ser enriquecido</param>
    /// <param name="urlHelper">Helper para geração de URLs</param>
    /// <param name="controller">Nome do controller</param>
    /// <param name="id">ID do recurso</param>
    public static void AddCrudLinks(this Resource resource, IUrlHelper urlHelper, string controller, object id)
    {
        var scheme = urlHelper.ActionContext.HttpContext.Request.Scheme;

        // Link para obter o recurso
        var getUrl = urlHelper.Action("GetById", controller, new { id }, scheme);
        if (!string.IsNullOrEmpty(getUrl))
        {
            resource.AddLink("self", getUrl, "GET", "application/json", "Obter este recurso");
        }

        // Link para atualizar o recurso
        var updateUrl = urlHelper.Action("Update", controller, new { id }, scheme);
        if (!string.IsNullOrEmpty(updateUrl))
        {
            resource.AddLink("update", updateUrl, "PUT", "application/json", "Atualizar este recurso");
        }

        // Link para deletar o recurso
        var deleteUrl = urlHelper.Action("Delete", controller, new { id }, scheme);
        if (!string.IsNullOrEmpty(deleteUrl))
        {
            resource.AddLink("delete", deleteUrl, "DELETE", null, "Deletar este recurso");
        }

        // Link para listar todos os recursos
        var listUrl = urlHelper.Action("GetAll", controller, null, scheme);
        if (!string.IsNullOrEmpty(listUrl))
        {
            resource.AddLink("collection", listUrl, "GET", "application/json", "Listar todos os recursos");
        }
    }

    /// <summary>
    /// Adiciona links de paginação para recursos paginados
    /// </summary>
    /// <param name="pagedResource">Recurso paginado</param>
    /// <param name="urlHelper">Helper para geração de URLs</param>
    /// <param name="controller">Nome do controller</param>
    /// <param name="routeValues">Valores de rota adicionais</param>
    public static void AddPaginationLinks<T>(this PagedResource<T> pagedResource, IUrlHelper urlHelper, string controller, object? routeValues = null)
    {
        var scheme = urlHelper.ActionContext.HttpContext.Request.Scheme;
        var pagination = pagedResource.Pagination;

        // Link para primeira página
        var firstPageValues = MergeRouteValues(routeValues, new { page = 1, pageSize = pagination.PageSize });
        var firstUrl = urlHelper.Action("GetAll", controller, firstPageValues, scheme);
        if (!string.IsNullOrEmpty(firstUrl))
        {
            pagedResource.AddLink("first", firstUrl, "GET", "application/json", "Primeira página");
        }

        // Link para última página
        var lastPageValues = MergeRouteValues(routeValues, new { page = pagination.TotalPages, pageSize = pagination.PageSize });
        var lastUrl = urlHelper.Action("GetAll", controller, lastPageValues, scheme);
        if (!string.IsNullOrEmpty(lastUrl))
        {
            pagedResource.AddLink("last", lastUrl, "GET", "application/json", "Última página");
        }

        // Link para página anterior
        if (pagination.HasPrevious)
        {
            var prevPageValues = MergeRouteValues(routeValues, new { page = pagination.CurrentPage - 1, pageSize = pagination.PageSize });
            var prevUrl = urlHelper.Action("GetAll", controller, prevPageValues, scheme);
            if (!string.IsNullOrEmpty(prevUrl))
            {
                pagedResource.AddLink("prev", prevUrl, "GET", "application/json", "Página anterior");
            }
        }

        // Link para próxima página
        if (pagination.HasNext)
        {
            var nextPageValues = MergeRouteValues(routeValues, new { page = pagination.CurrentPage + 1, pageSize = pagination.PageSize });
            var nextUrl = urlHelper.Action("GetAll", controller, nextPageValues, scheme);
            if (!string.IsNullOrEmpty(nextUrl))
            {
                pagedResource.AddLink("next", nextUrl, "GET", "application/json", "Próxima página");
            }
        }

        // Link para página atual (self)
        var selfPageValues = MergeRouteValues(routeValues, new { page = pagination.CurrentPage, pageSize = pagination.PageSize });
        var selfUrl = urlHelper.Action("GetAll", controller, selfPageValues, scheme);
        if (!string.IsNullOrEmpty(selfUrl))
        {
            pagedResource.AddLink("self", selfUrl, "GET", "application/json", "Página atual");
        }
    }

    /// <summary>
    /// Adiciona link para criação de novo recurso
    /// </summary>
    /// <param name="resource">Recurso a ser enriquecido</param>
    /// <param name="urlHelper">Helper para geração de URLs</param>
    /// <param name="controller">Nome do controller</param>
    public static void AddCreateLink(this Resource resource, IUrlHelper urlHelper, string controller)
    {
        var scheme = urlHelper.ActionContext.HttpContext.Request.Scheme;
        var createUrl = urlHelper.Action("Create", controller, null, scheme);
        if (!string.IsNullOrEmpty(createUrl))
        {
            resource.AddLink("create", createUrl, "POST", "application/json", "Criar novo recurso");
        }
    }

    /// <summary>
    /// Mescla valores de rota
    /// </summary>
    private static object MergeRouteValues(object? existing, object additional)
    {
        if (existing == null) return additional;

        var existingDict = existing.GetType().GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(existing));

        var additionalDict = additional.GetType().GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(additional));

        foreach (var kvp in additionalDict)
        {
            existingDict[kvp.Key] = kvp.Value;
        }

        return existingDict;
    }
}