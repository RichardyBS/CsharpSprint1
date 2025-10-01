# 🚀 Guia Completo: Do Java às Notificações com HATEOAS

## 📋 Visão Geral

Este guia te mostra o passo a passo completo desde a criação de um endpoint no Java até a implementação do sistema de notificações com HATEOAS no seu projeto C#.

---

## 🎯 PASSO 1: Criar Endpoint no Java (API Externa)

### 1.1 Estrutura do Controller Java

```java
@RestController
@RequestMapping("/api/motos")
@CrossOrigin(origins = "*")
public class MotoController {

    @Autowired
    private MotoService motoService;

    @PostMapping("/moverVaga")
    public ResponseEntity<MotoResponse> moverVaga(@RequestBody MoverVagaRequest request) {
        try {
            // Lógica para mover moto
            Moto moto = motoService.moverParaVaga(request.getPlaca(), request.getIdVaga());
            
            MotoResponse response = new MotoResponse();
            response.setPlaca(moto.getPlaca());
            response.setModelo(moto.getModelo());
            response.setAno(moto.getAno());
            response.setCor(moto.getCor());
            response.setIdVaga(moto.getIdVaga());
            response.setStatus("NORMAL");
            
            return ResponseEntity.ok(response);
        } catch (Exception e) {
            return ResponseEntity.badRequest()
                .body(new ErrorResponse("Erro ao mover moto: " + e.getMessage()));
        }
    }
}
```

### 1.2 DTOs Java

```java
// MoverVagaRequest.java
public class MoverVagaRequest {
    private String placa;
    private Long idVaga;
    
    // getters e setters
}

// MotoResponse.java
public class MotoResponse {
    private String placa;
    private String modelo;
    private Integer ano;
    private String cor;
    private Long idVaga;
    private String status;
    
    // getters e setters
}
```

### 1.3 Configuração CORS no Java

```java
@Configuration
@EnableWebMvc
public class WebConfig implements WebMvcConfigurer {

    @Override
    public void addCorsMappings(CorsRegistry registry) {
        registry.addMapping("/api/**")
                .allowedOrigins("*")
                .allowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                .allowedHeaders("*");
    }
}
```

---

## 🔗 PASSO 2: Configurar Comunicação entre APIs

### 2.1 Configuração no .env (C#)

```env
# API Java Configuration
JAVA_API_BASE_URL=http://52.226.54.155:8080/api
JAVA_API_TIMEOUT=30
JAVA_API_RETRY_COUNT=3
```

### 2.2 Service de Comunicação (C#)

```csharp
public interface IJavaApiService
{
    Task<T> PostAsync<T>(string endpoint, object data);
    Task<T> GetAsync<T>(string endpoint);
}

public class JavaApiService : IJavaApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JavaApiService> _logger;

    public JavaApiService(HttpClient httpClient, IConfiguration configuration, ILogger<JavaApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        var baseUrl = _configuration["JAVA_API_BASE_URL"];
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Enviando POST para Java API: {Endpoint}", endpoint);
            
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao comunicar com Java API: {Endpoint}", endpoint);
            throw;
        }
    }
}
```

---

## 🎮 PASSO 3: Implementar Endpoint C# Compatível

### 3.1 Controller Compatível

```csharp
[ApiController]
[Route("api/motos")]
[Produces("application/json")]
public class MotoJavaCompatibleController : ControllerBase
{
    private readonly IJavaApiService _javaApiService;
    private readonly IMotoNotificationService _notificationService;
    private readonly ILogger<MotoJavaCompatibleController> _logger;

    [HttpPost("moverVaga")]
    public async Task<ActionResult<MotoResponseDto>> MoverMoto([FromBody] MoverMotoVagaDto request)
    {
        try
        {
            _logger.LogInformation("Movendo moto {Placa} para vaga {IdVaga}", request.Placa, request.IdVaga);

            // 1. Comunicar com Java API
            var javaResponse = await _javaApiService.PostAsync<MotoResponseDto>("/motos/moverVaga", request);

            // 2. Criar notificação
            await _notificationService.ProcessarEntradaMotoAsync(new MotoNotificationDto
            {
                MotoPlaca = request.Placa,
                TipoMovimentacao = "ENTRADA",
                VagaId = request.IdVaga,
                Mensagem = $"Moto {request.Placa} movida para vaga {request.IdVaga}",
                TimestampEvento = DateTime.Now
            });

            // 3. Adicionar HATEOAS
            var response = new MotoResponseDto
            {
                Placa = request.Placa,
                Modelo = javaResponse.Modelo,
                Ano = javaResponse.Ano,
                Cor = javaResponse.Cor,
                IdVaga = request.IdVaga,
                Status = "NORMAL"
            };

            AddHateoasLinks(response);

            return CreatedAtAction(nameof(BuscarMoto), new { placa = request.Placa }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mover moto {Placa}", request.Placa);
            return BadRequest(new { message = "Erro ao mover moto", details = ex.Message });
        }
    }

    private void AddHateoasLinks(MotoResponseDto response)
    {
        response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa = response.Placa }), "GET");
        response.AddLink("update", Url.Action(nameof(EditarMoto), new { placa = response.Placa }), "PUT");
        response.AddLink("delete", Url.Action(nameof(ExcluirMoto), new { placa = response.Placa }), "DELETE");
        response.AddLink("retirar-vaga", Url.Action(nameof(RetirarMoto), new { placa = response.Placa }), "POST");
        response.AddLink("notifications", $"/api/notifications/moto/{response.Placa}", "GET");
    }
}
```

---

## 📢 PASSO 4: Sistema de Notificações

### 4.1 Model de Notificação

```csharp
public class Notification : Resource
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? PlacaMoto { get; set; }
    public long? VagaId { get; set; }
    public string? ClienteId { get; set; }
}
```

### 4.2 Service de Notificações

```csharp
public interface IMotoNotificationService
{
    Task ProcessarEntradaMotoAsync(MotoNotificationDto notification);
    Task ProcessarSaidaMotoAsync(MotoNotificationDto notification);
    Task<IEnumerable<Notification>> GetNotificationsByMotoAsync(string placa);
}

public class MotoNotificationService : IMotoNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MotoNotificationService> _logger;

    public async Task ProcessarEntradaMotoAsync(MotoNotificationDto notification)
    {
        try
        {
            var notificationEntity = new Notification
            {
                Title = "Entrada de Moto",
                Message = notification.Mensagem,
                NotificationType = "ENTRADA",
                PlacaMoto = notification.MotoPlaca,
                VagaId = notification.VagaId,
                CreatedAt = notification.TimestampEvento,
                IsRead = false
            };

            _context.Notifications.Add(notificationEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notificação de entrada criada para moto {Placa}", notification.MotoPlaca);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar entrada da moto {Placa}", notification.MotoPlaca);
            throw;
        }
    }
}
```

---

## 🔗 PASSO 5: Implementar HATEOAS

### 5.1 Classe Base Resource

```csharp
public abstract class Resource
{
    [JsonPropertyName("_links")]
    public Dictionary<string, Link> Links { get; set; } = new();

    public void AddLink(string rel, string href, string method, string? title = null)
    {
        Links[rel] = new Link 
        { 
            Href = href, 
            Method = method, 
            Title = title ?? rel 
        };
    }
}

public class Link
{
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
```

### 5.2 Controller de Notificações com HATEOAS

```csharp
[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetNotifications()
    {
        var notifications = await _notificationService.GetAllNotificationsAsync();
        
        // Adicionar HATEOAS para cada notificação
        foreach (var notification in notifications)
        {
            AddHateoasLinks(notification);
        }

        var response = new
        {
            data = notifications,
            _links = new Dictionary<string, Link>
            {
                ["self"] = new Link { Href = Url.Action(nameof(GetNotifications)), Method = "GET" },
                ["create"] = new Link { Href = Url.Action(nameof(CreateNotification)), Method = "POST" },
                ["moto-entrada"] = new Link { Href = "/api/notifications/moto-entrada", Method = "POST" },
                ["moto-saida"] = new Link { Href = "/api/notifications/moto-saida", Method = "POST" }
            }
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            return NotFound(new { message = "Notificação não encontrada", id });
        }

        AddHateoasLinks(notification);
        return Ok(notification);
    }

    private void AddHateoasLinks(Notification notification)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}/api";

        notification.AddLink("self", $"{baseUrl}/notifications/{notification.Id}", "GET");
        notification.AddLink("update", $"{baseUrl}/notifications/{notification.Id}", "PUT");
        notification.AddLink("delete", $"{baseUrl}/notifications/{notification.Id}", "DELETE");
        notification.AddLink("mark-read", $"{baseUrl}/notifications/{notification.Id}/mark-read", "PATCH");

        if (!string.IsNullOrEmpty(notification.PlacaMoto))
        {
            notification.AddLink("moto-details", $"http://52.226.54.155:8080/api/motos/{notification.PlacaMoto}", "GET");
            notification.AddLink("moto-notifications", $"{baseUrl}/notifications/moto/{notification.PlacaMoto}", "GET");
        }

        if (notification.VagaId.HasValue)
        {
            notification.AddLink("vaga-details", $"{baseUrl}/vagas/{notification.VagaId}", "GET");
        }
    }
}
```

---

## 🧪 PASSO 6: Exemplos Práticos com Postman

### 6.1 Mover Moto (Gera Notificação)

**POST** `http://localhost:5003/api/motos/moverVaga`

```json
{
  "placa": "ABC1234",
  "idVaga": 15
}
```

**Resposta com HATEOAS:**
```json
{
  "placa": "ABC1234",
  "modelo": "Honda CBR600",
  "ano": 2023,
  "cor": "Azul",
  "idVaga": 15,
  "status": "NORMAL",
  "_links": {
    "self": {
      "href": "/api/motos/ABC1234",
      "method": "GET",
      "title": "self"
    },
    "update": {
      "href": "/api/motos/ABC1234",
      "method": "PUT",
      "title": "update"
    },
    "retirar-vaga": {
      "href": "/api/motos/ABC1234/retirarVaga",
      "method": "POST",
      "title": "retirar-vaga"
    },
    "notifications": {
      "href": "/api/notifications/moto/ABC1234",
      "method": "GET",
      "title": "notifications"
    }
  }
}
```

### 6.2 Ver Notificações da Moto

**GET** `http://localhost:5003/api/notifications/moto/ABC1234`

**Resposta:**
```json
{
  "data": [
    {
      "id": 1,
      "title": "Entrada de Moto",
      "message": "Moto ABC1234 movida para vaga 15",
      "notificationType": "ENTRADA",
      "isRead": false,
      "createdAt": "2024-01-15T10:30:00",
      "placaMoto": "ABC1234",
      "vagaId": 15,
      "_links": {
        "self": {
          "href": "/api/notifications/1",
          "method": "GET"
        },
        "mark-read": {
          "href": "/api/notifications/1/mark-read",
          "method": "PATCH"
        },
        "moto-details": {
          "href": "http://52.226.54.155:8080/api/motos/ABC1234",
          "method": "GET"
        }
      }
    }
  ],
  "_links": {
    "self": {
      "href": "/api/notifications/moto/ABC1234",
      "method": "GET"
    },
    "all": {
      "href": "/api/notifications",
      "method": "GET"
    }
  }
}
```

### 6.3 Listar Todas as Notificações

**GET** `http://localhost:5003/api/notifications`

**Resposta com navegação HATEOAS:**
```json
{
  "data": [...],
  "_links": {
    "self": {
      "href": "/api/notifications",
      "method": "GET"
    },
    "create": {
      "href": "/api/notifications",
      "method": "POST"
    },
    "moto-entrada": {
      "href": "/api/notifications/moto-entrada",
      "method": "POST"
    },
    "moto-saida": {
      "href": "/api/notifications/moto-saida",
      "method": "POST"
    }
  }
}
```

---

## 🎯 Resumo do Fluxo Completo

1. **Java API** recebe requisição de mover moto
2. **C# API** comunica com Java API
3. **Sistema de Notificações** processa o evento
4. **HATEOAS** adiciona links de navegação
5. **Cliente** pode navegar pelos recursos relacionados

### Benefícios do HATEOAS:
- ✅ **Descoberta automática** de ações disponíveis
- ✅ **Navegação intuitiva** entre recursos
- ✅ **Redução de acoplamento** entre cliente e servidor
- ✅ **Evolução da API** sem quebrar clientes existentes

---

## 🚀 Próximos Passos

1. Implementar autenticação JWT
2. Adicionar WebSockets para notificações em tempo real
3. Criar dashboard com estatísticas
4. Implementar cache Redis para performance

Este guia te dá a base completa para integrar Java, C#, notificações e HATEOAS! 🎉