using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using MottoSprint.DTOs;
using MottoSprint.Models;
using MottoSprint.Services;
using Moq;

namespace MottoSprint.Tests.Controllers;

/// <summary>
/// Testes de integração para o JavaCompatibleController
/// Garante compatibilidade com a API Java
/// </summary>
public class JavaCompatibleControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public JavaCompatibleControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Testes de Criação de Moto

    [Fact]
    public async Task CriarMoto_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var motoRequest = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var motoResponse = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);
        
        Assert.NotNull(motoResponse);
        Assert.Equal(motoRequest.Placa, motoResponse.Placa);
        Assert.Equal(motoRequest.Modelo, motoResponse.Modelo);
        Assert.Equal(motoRequest.Ano, motoResponse.Ano);
        Assert.Equal(motoRequest.Cor, motoResponse.Cor);
        Assert.Equal(motoRequest.Status, motoResponse.Status);
        
        // Verificar links HATEOAS
        Assert.NotNull(motoResponse.Links);
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "self");
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "update");
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "delete");
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "mover-vaga");
    }

    [Fact]
    public async Task CriarMoto_ComPlacaVazia_DeveRetornarBadRequest()
    {
        // Arrange
        var motoRequest = new MotoRequestDto
        {
            Placa = "",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CriarMoto_ComModeloVazio_DeveRetornarBadRequest()
    {
        // Arrange
        var motoRequest = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CriarMoto_ComAnoInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        var motoRequest = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 1800, // Ano inválido
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Testes de Busca de Moto

    [Fact]
    public async Task BuscarMoto_ComPlacaValida_DeveRetornarOk()
    {
        // Arrange
        var placa = "ABC1234";

        // Act
        var response = await _client.GetAsync($"/api/motos/{placa}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var motoResponse = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);
        
        Assert.NotNull(motoResponse);
        Assert.Equal(placa, motoResponse.Placa);
        
        // Verificar links HATEOAS
        Assert.NotNull(motoResponse.Links);
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "self");
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "update");
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "delete");
    }

    [Fact]
    public async Task BuscarMoto_ComPlacaInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var placa = "INEXISTENTE";

        // Act
        var response = await _client.GetAsync($"/api/motos/{placa}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Testes de Edição de Moto

    [Fact]
    public async Task EditarMoto_ComDadosValidos_DeveRetornarOk()
    {
        // Arrange
        var placa = "ABC1234";
        var motoRequest = new MotoRequestDto
        {
            Placa = placa,
            Modelo = "Honda CB650",
            Ano = 2024,
            Cor = "Vermelho",
            Status = "NORMAL"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/motos/{placa}", motoRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var motoResponse = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);
        
        Assert.NotNull(motoResponse);
        Assert.Equal(placa, motoResponse.Placa);
        Assert.Equal(motoRequest.Modelo, motoResponse.Modelo);
        Assert.Equal(motoRequest.Ano, motoResponse.Ano);
        Assert.Equal(motoRequest.Cor, motoResponse.Cor);
    }

    #endregion

    #region Testes de Exclusão de Moto

    [Fact]
    public async Task ExcluirMoto_ComPlacaValida_DeveRetornarNoContent()
    {
        // Arrange
        var placa = "ABC1234";

        // Act
        var response = await _client.DeleteAsync($"/api/motos/{placa}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ExcluirMoto_ComPlacaInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var placa = "INEXISTENTE";

        // Act
        var response = await _client.DeleteAsync($"/api/motos/{placa}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Testes de Movimentação de Moto

    [Fact]
    public async Task MoverMoto_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var moverRequest = new MoverMotoVagaDto
        {
            Placa = "ABC1234",
            IdVaga = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos/moverVaga", moverRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var motoResponse = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);
        
        Assert.NotNull(motoResponse);
        Assert.Equal(moverRequest.Placa, motoResponse.Placa);
        Assert.Equal(moverRequest.IdVaga, motoResponse.IdVaga);
        
        // Verificar links HATEOAS
        Assert.NotNull(motoResponse.Links);
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "self");
        Assert.Contains(motoResponse.Links, l => l.Value.Rel == "retirar-vaga");
    }

    [Fact]
    public async Task MoverMoto_ComPlacaVazia_DeveRetornarBadRequest()
    {
        // Arrange
        var moverRequest = new MoverMotoVagaDto
        {
            Placa = "",
            IdVaga = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos/moverVaga", moverRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RetirarMoto_ComPlacaValida_DeveRetornarOk()
    {
        // Arrange
        var placa = "ABC1234";

        // Act
        var response = await _client.PostAsync($"/api/motos/{placa}/retirarVaga", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var motoResponse = JsonSerializer.Deserialize<MotoResponseDto>(content, _jsonOptions);
        
        Assert.NotNull(motoResponse);
        Assert.Equal(placa, motoResponse.Placa);
        Assert.Null(motoResponse.IdVaga);
    }

    #endregion

    #region Testes de Listagem

    [Fact]
    public async Task ListarTodasMotos_DeveRetornarOk()
    {
        // Act
        var response = await _client.GetAsync("/api/motos/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var motos = JsonSerializer.Deserialize<List<MotoResponseDto>>(content, _jsonOptions);
        
        Assert.NotNull(motos);
        
        // Verificar que cada moto tem links HATEOAS
        foreach (var moto in motos)
        {
            Assert.NotNull(moto.Links);
            Assert.Contains(moto.Links, l => l.Value.Rel == "self");
            Assert.Contains(moto.Links, l => l.Value.Rel == "update");
            Assert.Contains(moto.Links, l => l.Value.Rel == "delete");
        }
    }

    #endregion

    #region Testes de Compatibilidade com API Java

    [Fact]
    public async Task VerificarFormatoJsonCompativel_ComAPIJava()
    {
        // Arrange
        var motoRequest = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Verificar que o JSON contém as propriedades esperadas pela API Java
        Assert.Contains("\"placa\"", content);
        Assert.Contains("\"modelo\"", content);
        Assert.Contains("\"ano\"", content);
        Assert.Contains("\"cor\"", content);
        Assert.Contains("\"status\"", content);
    }

    [Fact]
    public async Task VerificarStatusValidos_CompatibilidadeJava()
    {
        // Arrange - Testar status válidos da API Java
        var statusValidos = new[] { "NORMAL", "DEFEITO" };

        foreach (var status in statusValidos)
        {
            var motoRequest = new MotoRequestDto
            {
                Placa = $"TEST{status}",
                Modelo = "Honda CB600",
                Ano = 2023,
                Cor = "Azul",
                Status = status
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    [Fact]
    public async Task VerificarFormatoPlaca_CompatibilidadeJava()
    {
        // Arrange - Testar formatos de placa válidos
        var placasValidas = new[] { "ABC1234", "XYZ9876", "DEF5555" };

        foreach (var placa in placasValidas)
        {
            var motoRequest = new MotoRequestDto
            {
                Placa = placa,
                Modelo = "Honda CB600",
                Ano = 2023,
                Cor = "Azul",
                Status = "NORMAL"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/motos", motoRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    #endregion

    #region Testes de Headers e Content-Type

    [Fact]
    public async Task VerificarContentType_ApplicationJson()
    {
        // Act
        var response = await _client.GetAsync("/api/motos/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task VerificarAcceptHeader_ApplicationJson()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Accept", "application/json");

        // Act
        var response = await _client.GetAsync("/api/motos/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}