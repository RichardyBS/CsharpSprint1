using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Configuration;

/// <summary>
/// Filtro para documentar esquemas HATEOAS no Swagger
/// </summary>
public class HateoasSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Se o tipo herda de Resource, adicionar documentação dos links HATEOAS
        if (typeof(Resource).IsAssignableFrom(context.Type))
        {
            // Verificar se _links já foi adicionado para evitar duplicação
            if (!schema.Properties.ContainsKey("_links"))
            {
                // Adicionar propriedade _links ao esquema de forma simplificada
                schema.Properties.Add("_links", new OpenApiSchema
                {
                    Type = "object",
                    Description = "Links de navegação HATEOAS",
                    AdditionalProperties = new OpenApiSchema
                    {
                        Type = "object",
                        Description = "Link HATEOAS"
                    }
                });
            }
        }

        // Se o tipo é PagedResource, adicionar documentação de paginação
        if (context.Type.IsGenericType && 
            context.Type.GetGenericTypeDefinition() == typeof(PagedResource<>))
        {
            // Verificar se paginationInfo já foi adicionado para evitar duplicação
            if (!schema.Properties.ContainsKey("paginationInfo"))
            {
                schema.Properties.Add("paginationInfo", new OpenApiSchema
            {
                Type = "object",
                Description = "Informações de paginação",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["currentPage"] = new OpenApiSchema 
                    { 
                        Type = "integer", 
                        Description = "Página atual",
                        Example = new Microsoft.OpenApi.Any.OpenApiInteger(1)
                    },
                    ["pageSize"] = new OpenApiSchema 
                    { 
                        Type = "integer", 
                        Description = "Tamanho da página",
                        Example = new Microsoft.OpenApi.Any.OpenApiInteger(10)
                    },
                    ["totalPages"] = new OpenApiSchema 
                    { 
                        Type = "integer", 
                        Description = "Total de páginas",
                        Example = new Microsoft.OpenApi.Any.OpenApiInteger(5)
                    },
                    ["totalItems"] = new OpenApiSchema 
                    { 
                        Type = "integer", 
                        Description = "Total de itens",
                        Example = new Microsoft.OpenApi.Any.OpenApiInteger(50)
                    },
                    ["hasNext"] = new OpenApiSchema 
                    { 
                        Type = "boolean", 
                        Description = "Tem próxima página",
                        Example = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
                    },
                    ["hasPrevious"] = new OpenApiSchema 
                    { 
                        Type = "boolean", 
                        Description = "Tem página anterior",
                        Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false)
                    }
                }
            });
            }
        }
    }
}