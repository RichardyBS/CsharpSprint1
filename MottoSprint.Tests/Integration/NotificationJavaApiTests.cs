using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using MottoSprint.Models;
using MottoSprint.Services;
using MottoSprint.DTOs;

namespace MottoSprint.Tests.Integration;

/// <summary>
/// Testes de integração para notificações comunicando com a API Java
/// Valida a comunicação end-to-end entre as APIs
/// </summary>
public class NotificationJavaApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public NotificationJavaApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Testes de Notificação com Java API

    [Fact]
    public async Task ProcessarNotificacao_ComMotoValida_DeveComunicarComJavaApi()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockJavaApiResponse = new
        {
            placa = "ABC1234",
            modelo = "Honda CB600",
            ano = 2023,
            cor = "Azul",
            status = "NORMAL"
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("ABC1234")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockJavaApiResponse), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(httpClient);
            });
        }).CreateClient();

        var notificacao = new MotoNotificationDto
        {
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Moto ABC1234 entrou no estacionamento",
            TimestampEvento = DateTime.Now
        };

        var json = JsonSerializer.Serialize(notificacao);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/notifications", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MotoNotificationDto>(responseContent, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal("ABC1234", result.MotoPlaca);
        Assert.Equal("ENTRADA", result.TipoMovimentacao);
        Assert.Contains("self", result.Links.Keys);
    }

    [Fact]
    public async Task ProcessarNotificacao_ComMotoInexistente_DeveRetornarErro()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("XYZ9999")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Moto não encontrada", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(httpClient);
            });
        }).CreateClient();

        var notificacao = new MotoNotificationDto
        {
            MotoPlaca = "XYZ9999",
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Moto XYZ9999 tentou entrar no estacionamento",
            TimestampEvento = DateTime.Now
        };

        var json = JsonSerializer.Serialize(notificacao);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/notifications", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessarNotificacao_ComMovimentacao_DeveAtualizarVagaNaJavaApi()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        // Mock para buscar moto
        var mockMotoResponse = new
        {
            placa = "ABC1234",
            modelo = "Honda CB600",
            ano = 2023,
            cor = "Azul",
            status = "NORMAL"
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("motos/ABC1234")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockMotoResponse), Encoding.UTF8, "application/json")
            });

        // Mock para mover moto
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains("motos/mover")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Moto movida com sucesso", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(httpClient);
            });
        }).CreateClient();

        var notificacao = new MotoNotificationDto
        {
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "MOVIMENTACAO",
            Mensagem = "Moto ABC1234 foi movida para vaga A1",
            TimestampEvento = DateTime.Now
        };

        var json = JsonSerializer.Serialize(notificacao);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/notifications", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Verificar se a chamada para mover moto foi feita
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Put && 
                req.RequestUri!.ToString().Contains("motos/mover")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ProcessarNotificacao_ComSaida_DeveLiberarVagaNaJavaApi()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        // Mock para buscar moto
        var mockMotoResponse = new
        {
            placa = "ABC1234",
            modelo = "Honda CB600",
            ano = 2023,
            cor = "Azul",
            status = "NORMAL"
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.ToString().Contains("motos/ABC1234")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockMotoResponse), Encoding.UTF8, "application/json")
            });

        // Mock para remover moto da vaga
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains("motos/remover-vaga")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Moto removida da vaga com sucesso", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(httpClient);
            });
        }).CreateClient();

        var notificacao = new MotoNotificationDto
        {
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "SAIDA",
            Mensagem = "Moto ABC1234 saiu do estacionamento",
            TimestampEvento = DateTime.Now
        };

        var json = JsonSerializer.Serialize(notificacao);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/notifications", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Verificar se a chamada para remover moto da vaga foi feita
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Put && 
                req.RequestUri!.ToString().Contains("motos/remover-vaga")),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Testes de Comunicação com Java API

    [Fact]
    public async Task JavaApiService_DeveRetornarDadosCorretos()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockResponse = new
        {
            placa = "ABC1234",
            modelo = "Honda CB600",
            ano = 2023,
            cor = "Azul",
            status = "NORMAL"
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockResponse), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var logger = new Mock<ILogger<JavaApiService>>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var javaApiService = new JavaApiService(httpClient, configuration.Object, logger.Object);

        // Act
        var result = await javaApiService.GetMotoByPlacaAsync("ABC1234");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC1234", result.Placa);
        Assert.Equal("Honda CB600", result.Modelo);
        Assert.Equal(2023, result.Ano);
        Assert.Equal("Azul", result.Cor);
        Assert.Equal("NORMAL", result.Status);
    }

    [Fact]
    public async Task JavaApiService_ComErroDeRede_DeveTratarExcecao()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Erro de conexão"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var logger = new Mock<ILogger<JavaApiService>>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var javaApiService = new JavaApiService(httpClient, configuration.Object, logger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => javaApiService.GetMotoByPlacaAsync("ABC1234"));
        
        Assert.Contains("Erro de conexão", exception.Message);
    }

    [Fact]
    public async Task JavaApiService_ComTimeout_DeveTratarCorretamente()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/"),
            Timeout = TimeSpan.FromSeconds(1)
        };

        var logger = new Mock<ILogger<JavaApiService>>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var javaApiService = new JavaApiService(httpClient, configuration.Object, logger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(
            () => javaApiService.GetMotoByPlacaAsync("ABC1234"));
        
        Assert.Contains("Timeout", exception.Message);
    }

    #endregion

    #region Testes de Validação de Dados

    [Theory]
    [InlineData("ABC1234", "ENTRADA", true)]
    [InlineData("XYZ9876", "SAIDA", true)]
    [InlineData("DEF5555", "MOVIMENTACAO", true)]
    [InlineData("", "ENTRADA", false)]
    [InlineData("ABC1234", "", false)]
    [InlineData("ABC1234", "TIPO_INVALIDO", false)]
    public async Task ValidarNotificacao_ComDiferentesDados_DeveRetornarResultadoCorreto(
        string placa, string tipo, bool esperado)
    {
        // Arrange
        var notificacao = new MotoNotificationDto
        {
            MotoPlaca = placa,
            TipoMovimentacao = tipo,
            Mensagem = $"Teste para {placa}",
            TimestampEvento = DateTime.Now
        };

        var json = JsonSerializer.Serialize(notificacao);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notifications/validate", content);

        // Assert
        if (esperado)
        {
            Assert.True(response.IsSuccessStatusCode);
        }
        else
        {
            Assert.False(response.IsSuccessStatusCode);
        }
    }

    [Fact]
    public async Task ProcessarNotificacao_ComDadosCompletos_DeveIncluirHateoas()
    {
        // Arrange
        var notificacao = new MotoNotificationDto
        {
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Moto ABC1234 entrou no estacionamento",
            TimestampEvento = DateTime.Now
        };

        var json = JsonSerializer.Serialize(notificacao);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notifications", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MotoNotificationDto>(responseContent, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Links);
        Assert.True(result.Links.Count > 0);
        Assert.Contains("self", result.Links.Keys);
    }

    #endregion

    #region Testes de Performance e Resiliência

    [Fact]
    public async Task ProcessarMultiplasNotificacoes_DeveManterPerformance()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var notificacao = new MotoNotificationDto
            {
                MotoPlaca = $"ABC{i:D4}",
                TipoMovimentacao = "ENTRADA",
                Mensagem = $"Moto ABC{i:D4} entrou no estacionamento",
                TimestampEvento = DateTime.Now
            };

            var json = JsonSerializer.Serialize(notificacao);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            tasks.Add(_client.PostAsync("/api/notifications", content));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Menos de 5 segundos
        Assert.All(responses, response => 
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest));
    }

    [Fact]
    public async Task JavaApiService_ComFalhaTemporaria_DeveImplementarRetry()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var callCount = 0;

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 2)
                {
                    return Task.FromResult(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError
                    });
                }
                
                var mockResponse = new
                {
                    placa = "ABC1234",
                    modelo = "Honda CB600",
                    ano = 2023,
                    cor = "Azul",
                    status = "NORMAL"
                };

                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(mockResponse), Encoding.UTF8, "application/json")
                });
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/")
        };

        var logger = new Mock<ILogger<JavaApiService>>();
        var configuration = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        var javaApiService = new JavaApiService(httpClient, configuration.Object, logger.Object);

        // Act
        var result = await javaApiService.GetMotoByPlacaAsync("ABC1234");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC1234", result.Placa);
        Assert.Equal(3, callCount); // Verificar que houve retry
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}