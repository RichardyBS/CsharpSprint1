using MottoSprint.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using MottoSprint.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MottoSprint API",
        Version = "v1",
        Description = "API REST para gerenciamento de estacionamento com suporte completo a HATEOAS (Hypermedia as the Engine of Application State). " +
                     "Esta API fornece navegação intuitiva através de links hipermídia, permitindo descoberta automática de recursos e ações disponíveis.",
        Contact = new OpenApiContact
        {
            Name = "Equipe MottoSprint",
            Email = "contato@mottosprint.com",
            Url = new Uri("https://mottosprint.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Incluir comentários XML para documentação automática
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurar esquemas HATEOAS
    c.SchemaFilter<HateoasSchemaFilter>();

    // Configurar exemplos de resposta HATEOAS
    c.ExampleFilters();

    // Configurar segurança JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Registrar exemplos de resposta HATEOAS
builder.Services.AddSwaggerExamplesFromAssemblyOf<ParkingSpotResourceExample>();

// Add MottoSprint services (includes EF, business services, and configurations)
builder.Services.AddMottoSprintServices(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MottoSprint API v1");
    c.RoutePrefix = "swagger"; // Swagger em /swagger
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();