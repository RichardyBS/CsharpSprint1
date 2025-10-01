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
/// Testes de integração para o NotificationController
/// Valida endpoints CRUD, status codes adequados, paginação e HATEOAS
/// </summary>
public class NotificationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public NotificationControllerTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetNotifications_DeveRetornarListaComHateoas()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar estrutura HATEOAS
        Assert.NotNull(result._links);
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.create);
        Assert.NotNull(result._meta);
    }

    [Fact]
    public async Task GetNotification_ComIdValido_DeveRetornarNotificacao()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var notification = new Notification
        {
            Title = "Teste Notificação",
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Notification>(content);
        
        Assert.Equal(notification.Title, result.Title);
        Assert.Equal(notification.Message, result.Message);
    }

    [Fact]
    public async Task GetNotification_ComIdInvalido_DeveRetornar404()
    {
        // Act
        var response = await _client.GetAsync("/api/notification/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateNotification_ComDadosValidos_DeveRetornar201()
    {
        // Arrange
        var notification = new
        {
            Title = "Nova Notificação",
            Message = "Mensagem da nova notificação",
            NotificationType = "GENERAL",
            Priority = "HIGH"
        };

        var json = JsonConvert.SerializeObject(notification);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notification", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Notification>(responseContent);
        
        Assert.Equal(notification.Title, result.Title);
        Assert.Equal(notification.Message, result.Message);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task CreateNotification_ComDadosInvalidos_DeveRetornar400()
    {
        // Arrange
        var notification = new
        {
            Title = "", // Título vazio - inválido
            Message = "Mensagem válida",
            NotificationType = "GENERAL"
        };

        var json = JsonConvert.SerializeObject(notification);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notification", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_ComIdValido_DeveRetornar200()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var notification = new Notification
        {
            Title = "Teste Notificação",
            Message = "Mensagem de teste",
            NotificationType = "GENERAL",
            Priority = "MEDIUM",
            IsRead = false
        };
        
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PatchAsync($"/api/notification/{notification.Id}/read", null);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verificar se foi marcada como lida
        var updatedNotification = await context.Notifications.FindAsync(notification.Id);
        Assert.True(updatedNotification.IsRead);
        Assert.NotNull(updatedNotification.ReadAt);
    }

    [Fact]
    public async Task GetNotificationsByType_DeveRetornarNotificacoesFiltradas()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var notifications = new[]
        {
            new Notification { Title = "Entrada", Message = "Moto entrou", NotificationType = "MOTO_ENTRADA", Priority = "MEDIUM" },
            new Notification { Title = "Saída", Message = "Moto saiu", NotificationType = "MOTO_SAIDA", Priority = "MEDIUM" },
            new Notification { Title = "Geral", Message = "Notificação geral", NotificationType = "GENERAL", Priority = "LOW" }
        };
        
        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/notification/type/MOTO_ENTRADA");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Notification[]>(content);
        
        Assert.Single(result);
        Assert.Equal("MOTO_ENTRADA", result[0].NotificationType);
    }

    [Fact]
    public async Task GetNotifications_DeveIncluirMetadados()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar metadados
        Assert.NotNull(result._meta);
        Assert.NotNull(result._meta.total);
        Assert.NotNull(result._meta.timestamp);
        Assert.NotNull(result._meta.types);
    }

    [Fact]
    public async Task GetNotifications_DeveIncluirLinksHateoas()
    {
        // Act
        var response = await _client.GetAsync("/api/notification");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        // Verificar links HATEOAS obrigatórios
        Assert.NotNull(result._links.self);
        Assert.NotNull(result._links.create);
        Assert.NotNull(result._links["moto-entrada"]);
        Assert.NotNull(result._links["moto-saida"]);
        Assert.NotNull(result._links["by-type"]);

        // Verificar estrutura dos links
        Assert.NotNull(result._links.self.Href);
        Assert.NotNull(result._links.self.Method);
        Assert.NotNull(result._links.self.Title);
    }

    [Fact]
    public async Task CreateMotoEntradaNotification_DeveRetornar201()
    {
        // Arrange
        var request = new
        {
            PlacaMoto = "ABC1234",
            VagaId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid()
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notification/moto-entrada", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateMotoSaidaNotification_DeveRetornar201()
    {
        // Arrange
        var request = new
        {
            PlacaMoto = "ABC1234",
            VagaId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid()
        };

        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notification/moto-saida", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("MOTO_ENTRADA")]
    [InlineData("MOTO_SAIDA")]
    [InlineData("GENERAL")]
    [InlineData("VAGA_OCUPADA")]
    [InlineData("VAGA_LIBERADA")]
    public async Task GetNotificationsByType_ComTiposValidos_DeveRetornar200(string tipo)
    {
        // Act
        var response = await _client.GetAsync($"/api/notification/type/{tipo}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNotification_ComIdValido_DeveRetornar204()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MottoSprintDbContext>();
        
        var notification = new Notification
        {
            Title = "Teste Delete",
            Message = "Mensagem para deletar",
            NotificationType = "GENERAL",
            Priority = "LOW"
        };
        
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/notification/{notification.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verificar se foi deletada
        var deletedNotification = await context.Notifications.FindAsync(notification.Id);
        Assert.Null(deletedNotification);
    }
}