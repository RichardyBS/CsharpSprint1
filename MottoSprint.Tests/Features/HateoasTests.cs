using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MottoSprint.Data;
using MottoSprint.Models;
using Newtonsoft.Json;
using System.Net;
using Xunit;

namespace MottoSprint.Tests.Features;

/// <summary>
/// Testes específicos para validação de HATEOAS (Hypermedia as the Engine of Application State)
/// Verifica se todos os endpoints retornam links adequados para navegação e ações disponíveis
/// </summary>
public class HateoasTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HateoasTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MottoSprintDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<MottoSprintDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_Hateoas_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task NotificationEndpoint_DeveIncluirLinksHateoasObrigatorios()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar presença de links HATEOAS obrigatórios
        Assert.NotNull(result._links);
        
        // Links básicos de navegação
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.create);
        
        // Links específicos do domínio
        Assert.NotNull(result._links["moto-entrada"]);
        Assert.NotNull(result._links["moto-saida"]);
        Assert.NotNull(result._links["by-type"]);
        Assert.NotNull(result._links.monitor);

        // Verificar estrutura dos links
        ValidateLinkStructure(result._links.self);
        ValidateLinkStructure(result._links.create);
    }

    [Fact]
    public async Task ParkingEndpoint_DeveIncluirLinksHateoasObrigatorios()
    {
        // Act
        var response = await _client.GetAsync("/api/parking/spots");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar presença de links HATEOAS
        Assert.NotNull(result._links);
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.create);
        
        // Verificar links de paginação se aplicável
        if (result.pagination != null && (int)result.pagination.totalPages > 1)
        {
            Assert.NotNull(result._links.first);
            Assert.NotNull(result._links.last);
        }
    }

    [Fact]
    public async Task ParkingSpot_VagaLivre_DeveIncluirLinkParaOcupar()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = false
        };
        
        context.ParkingSpots.Add(spot);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/parking/spots/{spot.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Vaga livre deve ter link para ocupar
        Assert.NotNull(result._links.occupy);
        Assert.Equal("PATCH", (string)result._links.occupy.Method);
        Assert.Contains("occupy", (string)result._links.occupy.Href);
        
        // Não deve ter link para liberar
        Assert.Null(result._links.free);
    }

    [Fact]
    public async Task ParkingSpot_VagaOcupada_DeveIncluirLinkParaLiberar()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = true,
            VehiclePlate = "ABC1234"
        };
        
        context.ParkingSpots.Add(spot);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/parking/spots/{spot.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Vaga ocupada deve ter link para liberar
        Assert.NotNull(result._links.free);
        Assert.Equal("PATCH", (string)result._links.free.Method);
        Assert.Contains("free", (string)result._links.free.Href);
        
        // Não deve ter link para ocupar
        Assert.Null(result._links.occupy);
    }

    [Fact]
    public async Task Notification_Individual_DeveIncluirLinksCrud()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var notification = new Notification
        {
            Title = "Teste HATEOAS",
            Message = "Mensagem de teste",
            NotificationType = "GENERAL",
            Priority = "MEDIUM"
        };
        
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/notification/{notification.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar links CRUD básicos
        Assert.NotNull(result._links);
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.update);
        Assert.NotNull(result._links.delete);
        
        // Verificar link específico para marcar como lida
        if (!(bool)result.IsRead)
        {
            Assert.NotNull(result._links["mark-read"]);
        }
    }

    [Fact]
    public async Task LinksHateoas_DevemTerEstruturaPadronizada()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar cada link na estrutura
        foreach (var linkProperty in result._links)
        {
            var link = linkProperty.Value;
            
            // Cada link deve ter pelo menos Href e Method
            Assert.NotNull(link.Href);
            Assert.NotNull(link.Method);
            
            // Href deve ser uma URL válida
            Assert.True(Uri.IsWellFormedUriString((string)link.Href, UriKind.Absolute));
            
            // Method deve ser um verbo HTTP válido
            var method = (string)link.Method;
            Assert.Contains(method, new[] { "GET", "POST", "PUT", "PATCH", "DELETE" });
        }
    }

    [Fact]
    public async Task LinksHateoas_DevemIncluirTitulos()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar se os links principais têm títulos descritivos
        Assert.NotNull(result._links.self.Title);
        Assert.NotNull(result._links.create.Title);
        Assert.NotNull(result._links["moto-entrada"].Title);
        Assert.NotNull(result._links["moto-saida"].Title);
        
        // Títulos devem ser strings não vazias
        Assert.False(string.IsNullOrWhiteSpace((string)result._links.self.Title));
        Assert.False(string.IsNullOrWhiteSpace((string)result._links.create.Title));
    }

    [Fact]
    public async Task LinksHateoas_DevemIncluirContentType()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = false
        };
        
        context.ParkingSpots.Add(spot);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/parking/spots/{spot.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Links que requerem body devem especificar Content-Type
        if (result._links.occupy != null)
        {
            Assert.Equal("application/json", (string)result._links.occupy.ContentType);
        }
        
        if (result._links.update != null)
        {
            Assert.Equal("application/json", (string)result._links.update.ContentType);
        }
    }

    [Theory]
    [InlineData("/api/notification")]
    [InlineData("/api/parking/spots")]
    [InlineData("/api/parking/spots/available")]
    [InlineData("/api/parking/spots/occupied")]
    public async Task EndpointsLista_DevemIncluirLinksNavegacao(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            // Todos os endpoints de lista devem ter pelo menos self e create
            Assert.NotNull(result._links);
            Assert.NotNull(result._links.self);
            
            // Verificar se o link self aponta para o endpoint correto
            var selfHref = (string)result._links.self.Href;
            Assert.Contains(endpoint.TrimStart('/'), selfHref);
        }
    }

    [Fact]
    public async Task LinksHateoas_DevemSerConsistentesComBaseUrl()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Todos os links devem usar a mesma base URL
        var baseUrl = $"{_client.BaseAddress?.Scheme}://{_client.BaseAddress?.Authority}";
        
        foreach (var linkProperty in result._links)
        {
            var href = (string)linkProperty.Value.Href;
            Assert.StartsWith(baseUrl, href);
        }
    }

    [Fact]
    public async Task MetadadosHateoas_DevemIncluirInformacoesUteis()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar metadados úteis
        Assert.NotNull(result._meta);
        Assert.NotNull(result._meta.total);
        Assert.NotNull(result._meta.timestamp);
        
        // Para notificações, deve incluir tipos disponíveis
        Assert.NotNull(result._meta.types);
        Assert.True(((Newtonsoft.Json.Linq.JArray)result._meta.types).Count > 0);
    }

    private static void ValidateLinkStructure(dynamic link)
    {
        Assert.NotNull(link.Href);
        Assert.NotNull(link.Method);
        
        // Href deve ser uma URL válida
        Assert.True(Uri.IsWellFormedUriString((string)link.Href, UriKind.Absolute));
        
        // Method deve ser um verbo HTTP válido
        var method = (string)link.Method;
        Assert.Contains(method, new[] { "GET", "POST", "PUT", "PATCH", "DELETE" });
    }
}