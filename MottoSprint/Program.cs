using MottoSprint.Configuration;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using MottoSprint.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Configurar para sempre usar a porta 5003
builder.WebHost.UseUrls("http://localhost:5003");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MottoSprint API - Sistema de Estacionamento",
        Version = "v1.0.0",
        Description = @"
# MottoSprint - API de Estacionamento

**Desenvolvido por: Richardy Borges Santana - RM: 557883**

---

## GUIA RAPIDO PARA TESTES

### ORDEM RECOMENDADA DE TESTES:
1. **GET /api/parking/spots** - Ver vagas disponíveis
2. **POST /api/motos** - Criar uma moto
3. **GET /api/motos/{placa}** - Buscar a moto criada
4. **POST /api/motos/entrada** - Fazer entrada no estacionamento
5. **GET /api/notification** - Ver notificações geradas
6. **POST /api/motos/retirarVaga/{placa}** - Fazer saída

### COMO TESTAR CADA ENDPOINT:
- Clique em qualquer endpoint abaixo
- Clique no botão ""Try it out""
- Preencha os dados (use os exemplos fornecidos)
- Clique em ""Execute""
- Observe a resposta e os links HATEOAS

### DADOS DE EXEMPLO PARA TESTES:

**Criar Moto:**
```json
{
  ""placa"": ""ABC1234"",
  ""modelo"": ""Honda CB600F"",
  ""ano"": 2023,
  ""cor"": ""Azul"",
  ""status"": ""Ativa""
}
```

**Entrada no Estacionamento:**
```json
{
  ""placa"": ""ABC1234"",
  ""linha"": 1,
  ""coluna"": 2
}
```

### FUNCIONALIDADES PRINCIPAIS:
- **Gerenciamento de Motos**: CRUD completo
- **Sistema de Vagas**: Controle de ocupação
- **Notificações**: Alertas automáticos
- **HATEOAS**: Links dinâmicos em todas as respostas
- **Paginação**: Navegação inteligente

### OBSERVAÇÕES IMPORTANTES:
- Todas as respostas incluem links HATEOAS
- A API usa SQLite em memória (dados são perdidos ao reiniciar)
- Endpoints marcados com 🔒 requerem autenticação
- Use os códigos de status HTTP para validar os testes

---

**Challenge FIAP 2025 - Sistema MottoSprint**",
        Contact = new OpenApiContact
        {
            Name = "Equipe MottoSprint - Challenge FIAP 2025",
            Email = "contato@mottosprint.com",
            Url = new Uri("https://github.com/mottosprint/api")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        },
        TermsOfService = new Uri("https://mottosprint.com/terms")
    });

    // Incluir comentários XML para documentação automática
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Configurar esquemas HATEOAS
    c.SchemaFilter<HateoasSchemaFilter>();

    // Configurar exemplos de resposta HATEOAS
    c.ExampleFilters();

    // Configurar tags personalizadas
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);

    // Configurar segurança JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"
# 🔐 Autenticação JWT Bearer Token

## 📝 Como Usar

Para acessar endpoints protegidos, inclua o token JWT no header Authorization:

```
Authorization: Bearer {seu_token_jwt}
```

## 🚀 Passo a Passo

### 1️⃣ **Obter Token**
- Faça login através do endpoint de autenticação
- O sistema retornará um token JWT válido
- Copie o token completo (sem o prefixo 'Bearer')

### 2️⃣ **Usar Token**
- Cole o token no campo 'Value' acima
- Clique em 'Authorize' 
- O token será automaticamente incluído nas requisições

### 3️⃣ **Testar Endpoints**
- Endpoints com 🔒 requerem autenticação
- Endpoints sem 🔒 são públicos

## 💡 Exemplos Práticos

**Via cURL:**
```bash
curl -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...' \
     http://localhost:5003/api/notification
```

**Via JavaScript:**
```javascript
fetch('http://localhost:5003/api/notification', {
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  }
})
```

## ⚠️ **Importante**
- Tokens têm validade limitada
- Mantenha seu token seguro
- Não compartilhe tokens em logs ou URLs",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
            Array.Empty<string>()
        }
    });

    // Configurar servidores
    c.AddServer(new OpenApiServer
    {
        Url = "http://localhost:5003",
        Description = "Servidor de Desenvolvimento Local"
    });

    c.AddServer(new OpenApiServer
    {
        Url = "https://mottosprint-api.azurewebsites.net",
        Description = "Servidor de Produção (Azure)"
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
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MottoSprint API v1.0.0");
    c.RoutePrefix = "swagger"; // Swagger em /swagger
    
    // Configurações de interface
    c.DocumentTitle = "MottoSprint API - Sistema de Estacionamento Inteligente";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelExpandDepth(2);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.EnableValidator();
    
    // Configurar tema e aparência
    c.InjectStylesheet("/swagger-ui/custom.css");
    c.InjectJavascript("/swagger-ui/custom.js");
    
    // Configurações de autenticação
    c.OAuthClientId("mottosprint-swagger");
    c.OAuthAppName("MottoSprint API");
    c.OAuthUsePkce();
    
    // Configurar exemplos e try-it-out
    c.ConfigObject.AdditionalItems.Add("syntaxHighlight", new Dictionary<string, object>
    {
        ["activated"] = true,
        ["theme"] = "agate"
    });
    
    // Personalizar cabeçalho
    c.HeadContent = @"
        <style>
            .swagger-ui .topbar { background-color: #1976d2; }
            .swagger-ui .topbar .download-url-wrapper { display: none; }
            .swagger-ui .info .title { color: #1976d2; }
            .swagger-ui .scheme-container { background: #f8f9fa; padding: 15px; border-radius: 5px; }
        </style>
        <script>
            window.onload = function() {
                console.log('MottoSprint API Documentation Loaded');
            };
        </script>";
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Mapear o hub SignalR
app.MapHub<MottoSprint.Hubs.NotificationHub>("/notificationHub");

app.Run();

// Classe Program para permitir acesso nos testes de integração
public partial class Program { }