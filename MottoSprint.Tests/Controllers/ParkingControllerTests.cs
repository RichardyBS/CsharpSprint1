using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MottoSprint.Data;
using MottoSprint.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Xunit;

namespace MottoSprint.Tests.Controllers;

/// <summary>
/// Testes de integração para o ParkingController
/// Valida endpoints CRUD, paginação, status codes adequados e HATEOAS
/// </summary>
public class ParkingControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ParkingControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove o DbContext existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MottoSprintDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adiciona DbContext em memória para testes
                services.AddDbContext<MottoSprintDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_Parking_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllSpots_DeveRetornarListaPaginadaComHateoas()
    {
        // Arrange
        await SeedParkingSpots();

        // Act
        var response = await _client.GetAsync("/api/parking/spots?page=1&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar estrutura de paginação
        Assert.NotNull(result.data);
        Assert.NotNull(result.pagination);
        Assert.NotNull(result._links);

        // Verificar metadados de paginação
        Assert.Equal(1, (int)result.pagination.currentPage);
        Assert.Equal(5, (int)result.pagination.pageSize);
        Assert.True((int)result.pagination.totalItems >= 0);
    }

    [Fact]
    public async Task GetSpot_ComIdValido_DeveRetornarVagaComHateoas()
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        
        Assert.Equal(spot.SpotNumber, (string)result.spotNumber);
        Assert.False((bool)result.isOccupied);
        
        // Verificar links HATEOAS
        Assert.NotNull(result._links);
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.occupy); // Vaga livre deve ter link para ocupar
    }

    [Fact]
    public async Task GetSpot_ComIdInvalido_DeveRetornar404()
    {
        // Act
        var response = await _client.GetAsync("/api/parking/spots/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateSpot_ComDadosValidos_DeveRetornar201()
    {
        // Arrange
        var spot = new
        {
            SpotNumber = "B001",
            IsOccupied = false
        };

        var json = JsonConvert.SerializeObject(spot);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/parking/spots", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ParkingSpot>(responseContent);
        
        Assert.Equal(spot.SpotNumber, result.SpotNumber);
        Assert.False(result.IsOccupied);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task CreateSpot_ComDadosInvalidos_DeveRetornar400()
    {
        // Arrange
        var spot = new
        {
            SpotNumber = "", // Número vazio - inválido
            IsOccupied = false
        };

        var json = JsonConvert.SerializeObject(spot);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/parking/spots", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailableSpots_DeveRetornarApenasVagasLivres()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spots = new[]
        {
            new ParkingSpot { SpotNumber = "A001", IsOccupied = false },
            new ParkingSpot { SpotNumber = "A002", IsOccupied = true, VehiclePlate = "ABC1234" },
            new ParkingSpot { SpotNumber = "A003", IsOccupied = false }
        };
        
        context.ParkingSpots.AddRange(spots);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/parking/spots/available");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        
        // Deve retornar apenas as 2 vagas livres
        var data = result.data;
        Assert.Equal(2, data.Count);
        
        foreach (var item in data)
        {
            Assert.False((bool)item.isOccupied);
        }
    }

    [Fact]
    public async Task GetOccupiedSpots_DeveRetornarApenasVagasOcupadas()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spots = new[]
        {
            new ParkingSpot { SpotNumber = "A001", IsOccupied = false },
            new ParkingSpot { SpotNumber = "A002", IsOccupied = true, VehiclePlate = "ABC1234" },
            new ParkingSpot { SpotNumber = "A003", IsOccupied = true, VehiclePlate = "XYZ9876" }
        };
        
        context.ParkingSpots.AddRange(spots);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/parking/spots/occupied");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        
        // Deve retornar apenas as 2 vagas ocupadas
        var data = result.data;
        Assert.Equal(2, data.Count);
        
        foreach (var item in data)
        {
            Assert.True((bool)item.isOccupied);
            Assert.NotNull((string)item.vehiclePlate);
        }
    }

    [Fact]
    public async Task OccupySpot_ComVagaLivre_DeveRetornar200()
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

        var request = new
        {
            VehiclePlate = "ABC1234"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PatchAsync($"/api/parking/spots/{spot.Id}/occupy", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verificar se a vaga foi ocupada
        var updatedSpot = await context.ParkingSpots.FindAsync(spot.Id);
        Assert.True(updatedSpot.IsOccupied);
        Assert.Equal("ABC1234", updatedSpot.VehiclePlate);
        Assert.NotNull(updatedSpot.OccupiedAt);
    }

    [Fact]
    public async Task OccupySpot_ComVagaJaOcupada_DeveRetornar400()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = true,
            VehiclePlate = "XYZ9876"
        };
        
        context.ParkingSpots.Add(spot);
        await context.SaveChangesAsync();

        var request = new
        {
            VehiclePlate = "ABC1234"
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PatchAsync($"/api/parking/spots/{spot.Id}/occupy", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FreeSpot_ComVagaOcupada_DeveRetornar200()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = true,
            VehiclePlate = "ABC1234",
            OccupiedAt = DateTime.UtcNow.AddHours(-2)
        };
        
        context.ParkingSpots.Add(spot);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PatchAsync($"/api/parking/spots/{spot.Id}/free", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verificar se a vaga foi liberada
        var updatedSpot = await context.ParkingSpots.FindAsync(spot.Id);
        Assert.False(updatedSpot.IsOccupied);
        Assert.Null(updatedSpot.VehiclePlate);
        Assert.Null(updatedSpot.OccupiedAt);
    }

    [Fact]
    public async Task FreeSpot_ComVagaJaLivre_DeveRetornar400()
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
        var response = await _client.PatchAsync($"/api/parking/spots/{spot.Id}/free", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task GetAllSpots_ComPaginacaoValida_DeveRetornarPaginaCorreta(int page, int pageSize)
    {
        // Arrange
        await SeedParkingSpots(25); // Criar 25 vagas para testar paginação

        // Act
        var response = await _client.GetAsync($"/api/parking/spots?page={page}&pageSize={pageSize}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        Assert.Equal(page, (int)result.pagination.currentPage);
        Assert.Equal(pageSize, (int)result.pagination.pageSize);
        Assert.True((int)result.data.Count <= pageSize);
    }

    [Fact]
    public async Task DeleteSpot_ComIdValido_DeveRetornar204()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spot = new ParkingSpot
        {
            SpotNumber = "DELETE001",
            IsOccupied = false
        };
        
        context.ParkingSpots.Add(spot);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/parking/spots/{spot.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verificar se foi deletada
        var deletedSpot = await context.ParkingSpots.FindAsync(spot.Id);
        Assert.Null(deletedSpot);
    }

    [Fact]
    public async Task GetSpot_VagaOcupada_DeveIncluirLinkParaLiberar()
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
        Assert.Null(result._links.occupy); // Não deve ter link para ocupar
    }

    private async Task SeedParkingSpots(int count = 10)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var spots = new List<ParkingSpot>();
        for (int i = 1; i <= count; i++)
        {
            spots.Add(new ParkingSpot
            {
                SpotNumber = $"A{i:D3}",
                IsOccupied = i % 3 == 0, // Cada terceira vaga ocupada
                VehiclePlate = i % 3 == 0 ? $"ABC{i:D4}" : null
            });
        }
        
        context.ParkingSpots.AddRange(spots);
        await context.SaveChangesAsync();
    }
}