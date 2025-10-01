using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using MottoSprint.Models;
using MottoSprint.Services;

namespace MottoSprint.Tests.Integration;

/// <summary>
/// Testes de integração para o JavaApiService
/// Simula chamadas para a API Java usando mocks
/// </summary>
public class JavaApiIntegrationTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<JavaApiService>> _loggerMock;
    private readonly JavaApiService _javaApiService;
    private readonly JsonSerializerOptions _jsonOptions;

    public JavaApiIntegrationTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://52.226.54.155:8080/api")
        };
        _loggerMock = new Mock<ILogger<JavaApiService>>();
        
        var configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        _javaApiService = new JavaApiService(_httpClient, configurationMock.Object, _loggerMock.Object);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Testes GetMotosAsync

    [Fact]
    public async Task GetMotosAsync_ComSucesso_DeveRetornarListaMotos()
    {
        // Arrange
        var motosEsperadas = new List<Moto>
        {
            new Moto { Placa = "ABC1234", Modelo = "Honda CB600", Ano = 2023, Cor = "Azul", Status = "NORMAL" },
            new Moto { Placa = "XYZ9876", Modelo = "Yamaha MT-07", Ano = 2022, Cor = "Preto", Status = "NORMAL" }
        };

        var jsonResponse = JsonSerializer.Serialize(motosEsperadas, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/motos")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetMotosAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count());
        
        var motos = resultado.ToList();
        Assert.Equal("ABC1234", motos[0].Placa);
        Assert.Equal("Honda CB600", motos[0].Modelo);
        Assert.Equal("XYZ9876", motos[1].Placa);
        Assert.Equal("Yamaha MT-07", motos[1].Modelo);
    }

    [Fact]
    public async Task GetMotosAsync_ComErro_DeveRetornarListaVazia()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetMotosAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
    }

    #endregion

    #region Testes GetMotoByPlacaAsync

    [Fact]
    public async Task GetMotoByPlacaAsync_ComPlacaExistente_DeveRetornarMoto()
    {
        // Arrange
        var placa = "ABC1234";
        var motoEsperada = new Moto 
        { 
            Placa = placa, 
            Modelo = "Honda CB600", 
            Ano = 2023, 
            Cor = "Azul", 
            Status = "NORMAL" 
        };

        var jsonResponse = JsonSerializer.Serialize(motoEsperada, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/motos/{placa}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetMotoByPlacaAsync(placa);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(placa, resultado.Placa);
        Assert.Equal("Honda CB600", resultado.Modelo);
        Assert.Equal(2023, resultado.Ano);
        Assert.Equal("Azul", resultado.Cor);
        Assert.Equal("NORMAL", resultado.Status);
    }

    [Fact]
    public async Task GetMotoByPlacaAsync_ComPlacaInexistente_DeveRetornarNull()
    {
        // Arrange
        var placa = "INEXISTENTE";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/motos/{placa}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetMotoByPlacaAsync(placa);

        // Assert
        Assert.Null(resultado);
    }

    #endregion

    #region Testes CreateMotoAsync

    [Fact]
    public async Task CreateMotoAsync_ComDadosValidos_DeveRetornarMoto()
    {
        // Arrange
        var createRequest = new CreateMotoRequest
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul"
        };

        var motoEsperada = new Moto 
        { 
            Placa = createRequest.Placa, 
            Modelo = createRequest.Modelo, 
            Ano = createRequest.Ano, 
            Cor = createRequest.Cor, 
            Status = "NORMAL" 
        };

        var jsonResponse = JsonSerializer.Serialize(motoEsperada, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("/motos")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.CreateMotoAsync(createRequest);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(createRequest.Placa, resultado.Placa);
        Assert.Equal(createRequest.Modelo, resultado.Modelo);
        Assert.Equal(createRequest.Ano, resultado.Ano);
        Assert.Equal(createRequest.Cor, resultado.Cor);
        Assert.Equal("NORMAL", resultado.Status);
    }

    [Fact]
    public async Task CreateMotoAsync_ComErro_DeveRetornarNull()
    {
        // Arrange
        var createRequest = new CreateMotoRequest
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.CreateMotoAsync(createRequest);

        // Assert
        Assert.Null(resultado);
    }

    #endregion

    #region Testes UpdateMotoAsync

    [Fact]
    public async Task UpdateMotoAsync_ComDadosValidos_DeveRetornarMotoAtualizada()
    {
        // Arrange
        var placa = "ABC1234";
        var updateRequest = new CreateMotoRequest
        {
            Placa = placa,
            Modelo = "Honda CB650",
            Ano = 2024,
            Cor = "Vermelho"
        };

        var motoAtualizada = new Moto 
        { 
            Placa = placa, 
            Modelo = updateRequest.Modelo, 
            Ano = updateRequest.Ano, 
            Cor = updateRequest.Cor, 
            Status = "NORMAL" 
        };

        var jsonResponse = JsonSerializer.Serialize(motoAtualizada, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Put && 
                    req.RequestUri!.ToString().Contains($"/motos/{placa}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.UpdateMotoAsync(placa, updateRequest);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(placa, resultado.Placa);
        Assert.Equal("Honda CB650", resultado.Modelo);
        Assert.Equal(2024, resultado.Ano);
        Assert.Equal("Vermelho", resultado.Cor);
    }

    #endregion

    #region Testes DeleteMotoAsync

    [Fact]
    public async Task DeleteMotoAsync_ComPlacaExistente_DeveRetornarTrue()
    {
        // Arrange
        var placa = "ABC1234";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Delete && 
                    req.RequestUri!.ToString().Contains($"/motos/{placa}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.DeleteMotoAsync(placa);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task DeleteMotoAsync_ComPlacaInexistente_DeveRetornarFalse()
    {
        // Arrange
        var placa = "INEXISTENTE";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.DeleteMotoAsync(placa);

        // Assert
        Assert.False(resultado);
    }

    #endregion

    #region Testes MoverMotoAsync

    [Fact]
    public async Task MoverMotoAsync_ComDadosValidos_DeveRetornarTrue()
    {
        // Arrange
        var moverRequest = new MoverMotoRequest
        {
            Placa = "ABC1234",
            IdVaga = 1
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString().Contains("/motos/mover")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.MoverMotoAsync(moverRequest);

        // Assert
        Assert.True(resultado);
    }

    #endregion

    #region Testes GetVagasAsync

    [Fact]
    public async Task GetVagasAsync_ComSucesso_DeveRetornarListaVagas()
    {
        // Arrange
        var vagasEsperadas = new List<Vaga>
        {
            new Vaga { Id = 1, Linha = "A", Coluna = "1", Ocupada = false },
            new Vaga { Id = 2, Linha = "A", Coluna = "2", Ocupada = true, 
                      Moto = new Moto { Placa = "ABC1234", Modelo = "Honda CB600", Ano = 2023, Cor = "Azul", Status = "NORMAL" } }
        };

        var jsonResponse = JsonSerializer.Serialize(vagasEsperadas, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/vagas")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetVagasAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count());
        
        var vagas = resultado.ToList();
        Assert.Equal(1, vagas[0].Id);
        Assert.Equal("A", vagas[0].Linha);
        Assert.False(vagas[0].Ocupada);
        
        Assert.Equal(2, vagas[1].Id);
        Assert.True(vagas[1].Ocupada);
        Assert.NotNull(vagas[1].Moto);
        Assert.Equal("ABC1234", vagas[1].Moto.Placa);
    }

    #endregion

    #region Testes GetVagaByIdAsync

    [Fact]
    public async Task GetVagaByIdAsync_ComIdExistente_DeveRetornarVaga()
    {
        // Arrange
        var id = 1;
        var vagaEsperada = new Vaga 
        { 
            Id = id, 
            Linha = "A", 
            Coluna = "1", 
            Ocupada = false 
        };

        var jsonResponse = JsonSerializer.Serialize(vagaEsperada, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/vagas/{id}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetVagaByIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado.Id);
        Assert.Equal("A", resultado.Linha);
        Assert.Equal("1", resultado.Coluna);
        Assert.False(resultado.Ocupada);
    }

    #endregion

    #region Testes GetVagasLivresByLinhaAsync

    [Fact]
    public async Task GetVagasLivresByLinhaAsync_ComLinhaExistente_DeveRetornarVagasLivres()
    {
        // Arrange
        var linha = "A";
        var vagasLivres = new List<Vaga>
        {
            new Vaga { Id = 1, Linha = linha, Coluna = "1", Ocupada = false },
            new Vaga { Id = 3, Linha = linha, Coluna = "3", Ocupada = false }
        };

        var jsonResponse = JsonSerializer.Serialize(vagasLivres, _jsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/vagas/linha/{linha}/livres")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var resultado = await _javaApiService.GetVagasLivresByLinhaAsync(linha);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count());
        Assert.All(resultado, v => Assert.Equal(linha, v.Linha));
        Assert.All(resultado, v => Assert.False(v.Ocupada));
    }

    #endregion

    #region Testes de Tratamento de Exceções

    [Fact]
    public async Task GetMotosAsync_ComExcecao_DeveLogarErroERetornarListaVazia()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Erro de rede"));

        // Act
        var resultado = await _javaApiService.GetMotosAsync();

        // Assert
        Assert.NotNull(resultado);
        Assert.Empty(resultado);
        
        // Verificar se o erro foi logado
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao buscar motos da API Java")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMotoByPlacaAsync_ComExcecao_DeveLogarErroERetornarNull()
    {
        // Arrange
        var placa = "ABC1234";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Erro de rede"));

        // Act
        var resultado = await _javaApiService.GetMotoByPlacaAsync(placa);

        // Assert
        Assert.Null(resultado);
        
        // Verificar se o erro foi logado
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Erro ao buscar moto {placa} da API Java")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}