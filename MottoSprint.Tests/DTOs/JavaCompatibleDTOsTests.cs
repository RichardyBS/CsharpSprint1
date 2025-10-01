using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Xunit;
using MottoSprint.DTOs;

namespace MottoSprint.Tests.DTOs;

/// <summary>
/// Testes para DTOs compatíveis com a API Java
/// Garante validação e serialização corretas
/// </summary>
public class JavaCompatibleDTOsTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public JavaCompatibleDTOsTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Testes MotoRequestDto

    [Fact]
    public void MotoRequestDto_ComDadosValidos_DevePassarValidacao()
    {
        // Arrange
        var moto = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var validationResults = ValidateModel(moto);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void MotoRequestDto_ComPlacaVazia_DeveFalharValidacao()
    {
        // Arrange
        var moto = new MotoRequestDto
        {
            Placa = "",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var validationResults = ValidateModel(moto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("placa"));
    }

    [Fact]
    public void MotoRequestDto_ComModeloVazio_DeveFalharValidacao()
    {
        // Arrange
        var moto = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var validationResults = ValidateModel(moto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("modelo"));
    }

    [Fact]
    public void MotoRequestDto_ComCorVazia_DeveFalharValidacao()
    {
        // Arrange
        var moto = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "",
            Status = "NORMAL"
        };

        // Act
        var validationResults = ValidateModel(moto);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("cor"));
    }

    [Fact]
    public void MotoRequestDto_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var moto = new MotoRequestDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var json = JsonSerializer.Serialize(moto, _jsonOptions);

        // Assert
        Assert.Contains("\"placa\"", json);
        Assert.Contains("\"modelo\"", json);
        Assert.Contains("\"ano\"", json);
        Assert.Contains("\"cor\"", json);
        Assert.Contains("\"status\"", json);
    }

    [Fact]
    public void MotoRequestDto_DeserializacaoJson_DevePreencherPropriedades()
    {
        // Arrange
        var json = """
        {
            "placa": "ABC1234",
            "modelo": "Honda CB600",
            "ano": 2023,
            "cor": "Azul",
            "status": "NORMAL"
        }
        """;

        // Act
        var moto = JsonSerializer.Deserialize<MotoRequestDto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(moto);
        Assert.Equal("ABC1234", moto.Placa);
        Assert.Equal("Honda CB600", moto.Modelo);
        Assert.Equal(2023, moto.Ano);
        Assert.Equal("Azul", moto.Cor);
        Assert.Equal("NORMAL", moto.Status);
    }

    #endregion

    #region Testes MotoResponseDto

    [Fact]
    public void MotoResponseDto_SerializacaoJson_DeveIncluirTodasPropriedades()
    {
        // Arrange
        var moto = new MotoResponseDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL",
            IdVaga = 1,
            Linha = "A",
            Coluna = "1"
        };

        // Act
        var json = JsonSerializer.Serialize(moto, _jsonOptions);

        // Assert
        Assert.Contains("\"placa\"", json);
        Assert.Contains("\"modelo\"", json);
        Assert.Contains("\"ano\"", json);
        Assert.Contains("\"cor\"", json);
        Assert.Contains("\"status\"", json);
        Assert.Contains("\"idVaga\"", json);
        Assert.Contains("\"linha\"", json);
        Assert.Contains("\"coluna\"", json);
    }

    [Fact]
    public void MotoResponseDto_ComIdVagaNulo_DeveSerializarCorretamente()
    {
        // Arrange
        var moto = new MotoResponseDto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL",
            IdVaga = null
        };

        // Act
        var json = JsonSerializer.Serialize(moto, _jsonOptions);

        // Assert
        Assert.Contains("\"idVaga\":null", json);
    }

    #endregion

    #region Testes VagaRequestDto

    [Fact]
    public void VagaRequestDto_ComDadosValidos_DevePassarValidacao()
    {
        // Arrange
        var vaga = new VagaRequestDto
        {
            Linha = "A",
            Coluna = "1"
        };

        // Act
        var validationResults = ValidateModel(vaga);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void VagaRequestDto_ComLinhaVazia_DeveFalharValidacao()
    {
        // Arrange
        var vaga = new VagaRequestDto
        {
            Linha = "",
            Coluna = "1"
        };

        // Act
        var validationResults = ValidateModel(vaga);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("linha"));
    }

    [Fact]
    public void VagaRequestDto_ComColunaVazia_DeveFalharValidacao()
    {
        // Arrange
        var vaga = new VagaRequestDto
        {
            Linha = "A",
            Coluna = ""
        };

        // Act
        var validationResults = ValidateModel(vaga);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("coluna"));
    }

    [Fact]
    public void VagaRequestDto_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var vaga = new VagaRequestDto
        {
            Linha = "A",
            Coluna = "1"
        };

        // Act
        var json = JsonSerializer.Serialize(vaga, _jsonOptions);

        // Assert
        Assert.Contains("\"linha\"", json);
        Assert.Contains("\"coluna\"", json);
    }

    #endregion

    #region Testes VagaResponseDto

    [Fact]
    public void VagaResponseDto_SerializacaoCompleta_DeveIncluirTodasPropriedades()
    {
        // Arrange
        var vaga = new VagaResponseDto
        {
            Id = 1,
            Posicao = "A1",
            Status = "OCUPADA",
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            StatusMoto = "NORMAL"
        };

        // Act
        var json = JsonSerializer.Serialize(vaga, _jsonOptions);

        // Assert
        Assert.Contains("\"id\"", json);
        Assert.Contains("\"posicao\"", json);
        Assert.Contains("\"status\"", json);
        Assert.Contains("\"placa\"", json);
        Assert.Contains("\"modelo\"", json);
        Assert.Contains("\"ano\"", json);
        Assert.Contains("\"cor\"", json);
        Assert.Contains("\"statusMoto\"", json);
    }

    [Fact]
    public void VagaResponseDto_VagaLivre_DeveTerPropriedadesMotoNulas()
    {
        // Arrange
        var vaga = new VagaResponseDto
        {
            Id = 1,
            Posicao = "A1",
            Status = "LIVRE",
            Placa = null,
            Modelo = null,
            Ano = null,
            Cor = null,
            StatusMoto = null
        };

        // Act
        var json = JsonSerializer.Serialize(vaga, _jsonOptions);

        // Assert
        Assert.Contains("\"placa\":null", json);
        Assert.Contains("\"modelo\":null", json);
        Assert.Contains("\"ano\":null", json);
        Assert.Contains("\"cor\":null", json);
        Assert.Contains("\"statusMoto\":null", json);
    }

    #endregion

    #region Testes MoverMotoVagaDto

    [Fact]
    public void MoverMotoVagaDto_ComDadosValidos_DevePassarValidacao()
    {
        // Arrange
        var mover = new MoverMotoVagaDto
        {
            Placa = "ABC1234",
            IdVaga = 1
        };

        // Act
        var validationResults = ValidateModel(mover);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void MoverMotoVagaDto_ComPlacaVazia_DeveFalharValidacao()
    {
        // Arrange
        var mover = new MoverMotoVagaDto
        {
            Placa = "",
            IdVaga = 1
        };

        // Act
        var validationResults = ValidateModel(mover);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("placa"));
    }

    [Fact]
    public void MoverMotoVagaDto_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var mover = new MoverMotoVagaDto
        {
            Placa = "ABC1234",
            IdVaga = 1
        };

        // Act
        var json = JsonSerializer.Serialize(mover, _jsonOptions);

        // Assert
        Assert.Contains("\"placa\"", json);
        Assert.Contains("\"idVaga\"", json);
    }

    #endregion

    #region Testes LinhaResponseDto

    [Fact]
    public void LinhaResponseDto_SerializacaoCompleta_DeveIncluirTodasPropriedades()
    {
        // Arrange
        var linha = new LinhaResponseDto
        {
            Linha = "A",
            VagasLivres = new List<VagaResponseDto>
            {
                new VagaResponseDto { Id = 1, Posicao = "A1", Status = "LIVRE" }
            },
            TotalVagas = 10,
            VagasOcupadas = 5
        };

        // Act
        var json = JsonSerializer.Serialize(linha, _jsonOptions);

        // Assert
        Assert.Contains("\"linha\"", json);
        Assert.Contains("\"vagasLivres\"", json);
        Assert.Contains("\"totalVagas\"", json);
        Assert.Contains("\"vagasOcupadas\"", json);
    }

    #endregion

    #region Testes de Enum StatusVaga

    [Fact]
    public void StatusVaga_DeveConterValoresCorretos()
    {
        // Act & Assert
        Assert.True(Enum.IsDefined(typeof(StatusVaga), StatusVaga.LIVRE));
        Assert.True(Enum.IsDefined(typeof(StatusVaga), StatusVaga.OCUPADA));
    }

    [Fact]
    public void StatusVaga_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var statusLivre = StatusVaga.LIVRE;
        var statusOcupada = StatusVaga.OCUPADA;

        // Act
        var jsonLivre = JsonSerializer.Serialize(statusLivre, _jsonOptions);
        var jsonOcupada = JsonSerializer.Serialize(statusOcupada, _jsonOptions);

        // Assert
        Assert.Contains("LIVRE", jsonLivre);
        Assert.Contains("OCUPADA", jsonOcupada);
    }

    #endregion

    #region Métodos Auxiliares

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    #endregion
}