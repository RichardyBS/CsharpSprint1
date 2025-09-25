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
                // Adicionar propriedade _links ao esquema
                schema.Properties.Add("_links", new OpenApiSchema
            {
                Type = "object",
                Description = "Links de navegação HATEOAS",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["href"] = new OpenApiSchema 
                        { 
                            Type = "string", 
                            Description = "URL do recurso",
                            Example = new Microsoft.OpenApi.Any.OpenApiString("http://localhost:5000/api/resource/1")
                        },
                        ["rel"] = new OpenApiSchema 
                        { 
                            Type = "string", 
                            Description = "Relação do link",
                            Example = new Microsoft.OpenApi.Any.OpenApiString("self")
                        },
                        ["method"] = new OpenApiSchema 
                        { 
                            Type = "string", 
                            Description = "Método HTTP",
                            Example = new Microsoft.OpenApi.Any.OpenApiString("GET")
                        },
                        ["type"] = new OpenApiSchema 
                        { 
                            Type = "string", 
                            Description = "Tipo de conteúdo",
                            Example = new Microsoft.OpenApi.Any.OpenApiString("application/json")
                        },
                        ["title"] = new OpenApiSchema 
                        { 
                            Type = "string", 
                            Description = "Título descritivo do link",
                            Example = new Microsoft.OpenApi.Any.OpenApiString("Link para este recurso")
                        }
                    }
                },
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["self"] = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["href"] = new Microsoft.OpenApi.Any.OpenApiString("http://localhost:5000/api/resource/1"),
                        ["rel"] = new Microsoft.OpenApi.Any.OpenApiString("self"),
                        ["method"] = new Microsoft.OpenApi.Any.OpenApiString("GET"),
                        ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Link para este recurso")
                    },
                    ["update"] = new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["href"] = new Microsoft.OpenApi.Any.OpenApiString("http://localhost:5000/api/resource/1"),
                        ["rel"] = new Microsoft.OpenApi.Any.OpenApiString("update"),
                        ["method"] = new Microsoft.OpenApi.Any.OpenApiString("PUT"),
                        ["title"] = new Microsoft.OpenApi.Any.OpenApiString("Atualizar este recurso")
                    }
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