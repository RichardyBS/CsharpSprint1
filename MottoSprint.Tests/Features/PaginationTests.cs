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
/// Testes específicos para validação de paginação em endpoints de listagem
/// Verifica se a paginação funciona corretamente com diferentes tamanhos de página e navegação
/// </summary>
public class PaginationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PaginationTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("TestDatabase_Pagination_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ParkingSpots_DevemSuportarPaginacaoBasica()
    {
        // Arrange
        await SeedParkingSpots(25); // Criar 25 vagas para testar paginação

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar estrutura de paginação
        Assert.NotNull(result.data);
        Assert.NotNull(result.pagination);
        
        // Verificar metadados de paginação
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.Equal(10, (int)result.pagination.pageSize);
        Assert.Equal(25, (int)result.pagination.totalItems);
        Assert.Equal(3, (int)result.pagination.totalPages);
        
        // Verificar que retornou exatamente 10 itens
        Assert.Equal(10, ((Newtonsoft.Json.Linq.JArray)result.data).Count);
    }

    [Fact]
    public async Task ParkingSpots_PaginaInvalida_DeveRetornarPrimeiraPagina()
    {
        // Arrange
        await SeedParkingSpots(15);

        // Act - Tentar acessar página que não existe
        var response = await _client.GetAsync("/api/parking/spots?page=10&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Deve retornar a primeira página quando página solicitada não existe
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.True(((Newtonsoft.Json.Linq.JArray)result.data).Count > 0);
    }

    [Fact]
    public async Task ParkingSpots_PageSizeZero_DeveUsarTamanhoPadrao()
    {
        // Arrange
        await SeedParkingSpots(30);

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=1&pageSize=0");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Deve usar tamanho padrão (provavelmente 10 ou 20)
        Assert.True((int)result.pagination.pageSize > 0);
        Assert.True(((Newtonsoft.Json.Linq.JArray)result.data).Count > 0);
    }

    [Fact]
    public async Task ParkingSpots_PageSizeMuitoGrande_DeveLimitarTamanho()
    {
        // Arrange
        await SeedParkingSpots(50);

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=1&pageSize=1000");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Deve limitar o tamanho da página (máximo 100 por exemplo)
        Assert.True((int)result.pagination.pageSize <= 100);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 5)]
    [InlineData(3, 5)]
    public async Task ParkingSpots_NavegacaoEntrePaginas_DeveRetornarDadosCorretos(int page, int pageSize)
    {
        // Arrange
        await SeedParkingSpots(20);

        // Act
        var response = await _client.GetAsync($"/api/parking/spots?page={page}&pageSize={pageSize}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar que a página atual está correta
        Assert.Equal(page, (int)result.pagination.currentPage);
        Assert.Equal(pageSize, (int)result.pagination.pageSize);
        
        // Verificar que o número de itens está correto
        var expectedItems = Math.Min(pageSize, 20 - (page - 1) * pageSize);
        if (expectedItems > 0)
        {
            Assert.Equal(expectedItems, ((Newtonsoft.Json.Linq.JArray)result.data).Count);
        }
    }

    [Fact]
    public async Task ParkingSpots_LinksNavegacao_DevemEstarPresentes()
    {
        // Arrange
        await SeedParkingSpots(30);

        // Act - Página do meio
        var response = await _client.GetAsync("/api/parking/spots?page=2&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar links de navegação HATEOAS
        Assert.NotNull(result._links);
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.first);
        Assert.NotNull(result._links.last);
        Assert.NotNull(result._links.prev);
        Assert.NotNull(result._links.next);
        
        // Verificar URLs dos links
        Assert.Contains("page=2", (string)result._links.self.Href);
        Assert.Contains("page=1", (string)result._links.first.Href);
        Assert.Contains("page=1", (string)result._links.prev.Href);
        Assert.Contains("page=3", (string)result._links.next.Href);
    }

    [Fact]
    public async Task ParkingSpots_PrimeiraPagina_NaoDeveIncluirLinkPrevious()
    {
        // Arrange
        await SeedParkingSpots(20);

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Primeira página não deve ter link "prev"
        Assert.Null(result._links.prev);
        
        // Mas deve ter outros links
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.first);
        Assert.NotNull(result._links.last);
        Assert.NotNull(result._links.next);
    }

    [Fact]
    public async Task ParkingSpots_UltimaPagina_NaoDeveIncluirLinkNext()
    {
        // Arrange
        await SeedParkingSpots(25);

        // Act - Última página (página 3 com pageSize 10)
        var response = await _client.GetAsync("/api/parking/spots?page=3&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Última página não deve ter link "next"
        Assert.Null(result._links.next);
        
        // Mas deve ter outros links
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.first);
        Assert.NotNull(result._links.last);
        Assert.NotNull(result._links.prev);
    }

    [Fact]
    public async Task ParkingSpots_UnicaPagina_DeveTerApenasLinksBasicos()
    {
        // Arrange
        await SeedParkingSpots(5); // Apenas 5 itens

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Com apenas uma página, não deve ter links prev/next
        Assert.Null(result._links.prev);
        Assert.Null(result._links.next);
        
        // Mas deve ter self, first e last
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.first);
        Assert.NotNull(result._links.last);
        
        // first e last devem apontar para a mesma página
        Assert.Equal(result._links.first.Href, result._links.last.Href);
    }

    [Fact]
    public async Task Notifications_DevemSuportarPaginacao()
    {
        // Arrange
        await SeedNotifications(15);

        // Act
        var response = await _client.GetAsync("/api/notification?page=1&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar estrutura de paginação
        Assert.NotNull(result.data);
        Assert.NotNull(result.pagination);
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.Equal(5, (int)result.pagination.pageSize);
        Assert.Equal(15, (int)result.pagination.totalItems);
        Assert.Equal(3, (int)result.pagination.totalPages);
    }

    [Fact]
    public async Task ParkingSpots_Available_DevemSuportarPaginacao()
    {
        // Arrange
        await SeedParkingSpots(20, onlyAvailable: true);

        // Act
        var response = await _client.GetAsync("/api/parking/spots/available?page=1&pageSize=8");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar paginação para vagas disponíveis
        Assert.NotNull(result.pagination);
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.Equal(8, (int)result.pagination.pageSize);
        Assert.True((int)result.pagination.totalItems > 0);
        
        // Verificar que todas as vagas retornadas estão disponíveis
        foreach (var spot in result.data)
        {
            Assert.False((bool)spot.IsOccupied);
        }
    }

    [Fact]
    public async Task ParkingSpots_Occupied_DevemSuportarPaginacao()
    {
        // Arrange
        await SeedParkingSpots(15, onlyOccupied: true);

        // Act
        var response = await _client.GetAsync("/api/parking/spots/occupied?page=1&pageSize=6");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar paginação para vagas ocupadas
        Assert.NotNull(result.pagination);
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.Equal(6, (int)result.pagination.pageSize);
        
        // Verificar que todas as vagas retornadas estão ocupadas
        foreach (var spot in result.data)
        {
            Assert.True((bool)spot.IsOccupied);
        }
    }

    [Fact]
    public async Task Paginacao_ParametrosInvalidos_DeveRetornarBadRequest()
    {
        // Act & Assert - Página negativa
        var response1 = await _client.GetAsync("/api/parking/spots?page=-1&pageSize=10");
        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);

        // Act & Assert - PageSize negativo
        var response2 = await _client.GetAsync("/api/parking/spots?page=1&pageSize=-5");
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task Paginacao_SemParametros_DeveUsarValoresPadrao()
    {
        // Arrange
        await SeedParkingSpots(30);

        // Act
        var response = await _client.GetAsync("/api/parking/spots");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Deve usar valores padrão
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.True((int)result.pagination.pageSize > 0);
        Assert.True(((Newtonsoft.Json.Linq.JArray)result.data).Count > 0);
    }

    [Fact]
    public async Task Paginacao_MetadadosCompletos_DevemEstarPresentes()
    {
        // Arrange
        await SeedParkingSpots(25);

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=2&pageSize=7");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        var pagination = result.pagination;
        
        // Verificar todos os metadados de paginação
        Assert.NotNull(pagination.currentPage);
        Assert.NotNull(pagination.pageSize);
        Assert.NotNull(pagination.totalItems);
        Assert.NotNull(pagination.totalPages);
        Assert.NotNull(pagination.hasNext);
        Assert.NotNull(pagination.hasPrevious);
        
        // Verificar valores calculados
        Assert.Equal(2, (int)pagination.currentPage);
        Assert.Equal(7, (int)pagination.pageSize);
        Assert.Equal(25, (int)pagination.totalItems);
        Assert.Equal(4, (int)pagination.totalPages); // 25 / 7 = 4 páginas
        Assert.True((bool)pagination.hasNext);
        Assert.True((bool)pagination.hasPrevious);
    }

    private async Task SeedParkingSpots(int count, bool onlyAvailable = false, bool onlyOccupied = false)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();

        for (int i = 1; i <= count; i++)
        {
            var isOccupied = onlyOccupied || (!onlyAvailable && i % 3 == 0);
            
            var spot = new ParkingSpot
            {
                SpotNumber = $"A{i:D3}",
                IsOccupied = isOccupied,
                VehiclePlate = isOccupied ? $"ABC{i:D4}" : null,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };

            context.ParkingSpots.Add(spot);
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedNotifications(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();

        var types = new[] { "ENTRY", "EXIT", "GENERAL", "WARNING" };
        var priorities = new[] { "LOW", "MEDIUM", "HIGH", "URGENT" };

        for (int i = 1; i <= count; i++)
        {
            var notification = new Notification
            {
                Title = $"Notificação {i}",
                Message = $"Mensagem de teste {i}",
                NotificationType = types[i % types.Length],
                Priority = priorities[i % priorities.Length],
                IsRead = i % 4 == 0,
                CreatedAt = DateTime.UtcNow.AddHours(-i)
            };

            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();
    }
}