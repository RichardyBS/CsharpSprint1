using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Xunit;
using MottoSprint.Models;

namespace MottoSprint.Tests.Models;

/// <summary>
/// Testes para modelos da API Java
/// Garante compatibilidade e validação corretas
/// </summary>
public class JavaApiModelsTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public JavaApiModelsTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Testes Modelo Moto

    [Fact]
    public void Moto_ComDadosValidos_DeveInicializarCorretamente()
    {
        // Arrange & Act
        var moto = new Moto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Assert
        Assert.Equal("ABC1234", moto.Placa);
        Assert.Equal("Honda CB600", moto.Modelo);
        Assert.Equal(2023, moto.Ano);
        Assert.Equal("Azul", moto.Cor);
        Assert.Equal("NORMAL", moto.Status);
    }

    [Fact]
    public void Moto_ComValoresPadrao_DeveInicializarComStringsVazias()
    {
        // Arrange & Act
        var moto = new Moto();

        // Assert
        Assert.Equal(string.Empty, moto.Placa);
        Assert.Equal(string.Empty, moto.Modelo);
        Assert.Equal(0, moto.Ano);
        Assert.Equal(string.Empty, moto.Cor);
        Assert.Equal(string.Empty, moto.Status);
    }

    [Fact]
    public void Moto_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var moto = new Moto
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
    public void Moto_DeserializacaoJson_DevePreencherPropriedades()
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
        var moto = JsonSerializer.Deserialize<Moto>(json, _jsonOptions);

        // Assert
        Assert.NotNull(moto);
        Assert.Equal("ABC1234", moto.Placa);
        Assert.Equal("Honda CB600", moto.Modelo);
        Assert.Equal(2023, moto.Ano);
        Assert.Equal("Azul", moto.Cor);
        Assert.Equal("NORMAL", moto.Status);
    }

    [Theory]
    [InlineData("NORMAL")]
    [InlineData("DEFEITO")]
    public void Moto_StatusValidos_DeveAceitarValoresCorretos(string status)
    {
        // Arrange & Act
        var moto = new Moto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = status
        };

        // Assert
        Assert.Equal(status, moto.Status);
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("XYZ9876")]
    [InlineData("DEF5555")]
    public void Moto_PlacasValidas_DeveAceitarFormatosCorretos(string placa)
    {
        // Arrange & Act
        var moto = new Moto
        {
            Placa = placa,
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Assert
        Assert.Equal(placa, moto.Placa);
    }

    [Theory]
    [InlineData(2020)]
    [InlineData(2023)]
    [InlineData(2024)]
    public void Moto_AnosValidos_DeveAceitarValoresCorretos(int ano)
    {
        // Arrange & Act
        var moto = new Moto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = ano,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Assert
        Assert.Equal(ano, moto.Ano);
    }

    #endregion

    #region Testes Modelo Vaga

    [Fact]
    public void Vaga_ComDadosValidos_DeveInicializarCorretamente()
    {
        // Arrange & Act
        var vaga = new Vaga
        {
            Id = 1,
            Linha = "A",
            Coluna = "1",
            Ocupada = false,
            Moto = null
        };

        // Assert
        Assert.Equal(1, vaga.Id);
        Assert.Equal("A", vaga.Linha);
        Assert.Equal("1", vaga.Coluna);
        Assert.False(vaga.Ocupada);
        Assert.Null(vaga.Moto);
    }

    [Fact]
    public void Vaga_ComValoresPadrao_DeveInicializarCorretamente()
    {
        // Arrange & Act
        var vaga = new Vaga();

        // Assert
        Assert.Equal(0, vaga.Id);
        Assert.Equal(string.Empty, vaga.Linha);
        Assert.Equal(string.Empty, vaga.Coluna);
        Assert.False(vaga.Ocupada);
        Assert.Null(vaga.Moto);
    }

    [Fact]
    public void Vaga_ComMotoOcupada_DeveDefinirOcupadaTrue()
    {
        // Arrange
        var moto = new Moto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        // Act
        var vaga = new Vaga
        {
            Id = 1,
            Linha = "A",
            Coluna = "1",
            Ocupada = true,
            Moto = moto
        };

        // Assert
        Assert.True(vaga.Ocupada);
        Assert.NotNull(vaga.Moto);
        Assert.Equal("ABC1234", vaga.Moto.Placa);
    }

    [Fact]
    public void Vaga_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var vaga = new Vaga
        {
            Id = 1,
            Linha = "A",
            Coluna = "1",
            Ocupada = false
        };

        // Act
        var json = JsonSerializer.Serialize(vaga, _jsonOptions);

        // Assert
        Assert.Contains("\"id\"", json);
        Assert.Contains("\"linha\"", json);
        Assert.Contains("\"coluna\"", json);
        Assert.Contains("\"ocupada\"", json);
        Assert.Contains("\"moto\"", json);
    }

    [Fact]
    public void Vaga_DeserializacaoJson_DevePreencherPropriedades()
    {
        // Arrange
        var json = """
        {
            "id": 1,
            "linha": "A",
            "coluna": "1",
            "ocupada": false,
            "moto": null
        }
        """;

        // Act
        var vaga = JsonSerializer.Deserialize<Vaga>(json, _jsonOptions);

        // Assert
        Assert.NotNull(vaga);
        Assert.Equal(1, vaga.Id);
        Assert.Equal("A", vaga.Linha);
        Assert.Equal("1", vaga.Coluna);
        Assert.False(vaga.Ocupada);
        Assert.Null(vaga.Moto);
    }

    [Fact]
    public void Vaga_ComMotoCompleta_DeveSerializarCorretamente()
    {
        // Arrange
        var vaga = new Vaga
        {
            Id = 1,
            Linha = "A",
            Coluna = "1",
            Ocupada = true,
            Moto = new Moto
            {
                Placa = "ABC1234",
                Modelo = "Honda CB600",
                Ano = 2023,
                Cor = "Azul",
                Status = "NORMAL"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(vaga, _jsonOptions);

        // Assert
        Assert.Contains("\"ocupada\":true", json);
        Assert.Contains("\"placa\":\"ABC1234\"", json);
        Assert.Contains("\"modelo\":\"Honda CB600\"", json);
    }

    [Theory]
    [InlineData("A", "1")]
    [InlineData("B", "2")]
    [InlineData("C", "10")]
    public void Vaga_LinhasColunas_DeveAceitarValoresCorretos(string linha, string coluna)
    {
        // Arrange & Act
        var vaga = new Vaga
        {
            Id = 1,
            Linha = linha,
            Coluna = coluna,
            Ocupada = false
        };

        // Assert
        Assert.Equal(linha, vaga.Linha);
        Assert.Equal(coluna, vaga.Coluna);
    }

    #endregion

    #region Testes MoverMotoRequest

    [Fact]
    public void MoverMotoRequest_ComDadosValidos_DeveInicializarCorretamente()
    {
        // Arrange & Act
        var request = new MoverMotoRequest
        {
            Placa = "ABC1234",
            IdVaga = 1
        };

        // Assert
        Assert.Equal("ABC1234", request.Placa);
        Assert.Equal(1, request.IdVaga);
    }

    [Fact]
    public void MoverMotoRequest_ComValoresPadrao_DeveInicializarComStringVazia()
    {
        // Arrange & Act
        var request = new MoverMotoRequest();

        // Assert
        Assert.Equal(string.Empty, request.Placa);
        Assert.Equal(0, request.IdVaga);
    }

    [Fact]
    public void MoverMotoRequest_ComPlacaVazia_DeveFalharValidacao()
    {
        // Arrange
        var request = new MoverMotoRequest
        {
            Placa = "",
            IdVaga = 1
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("Placa"));
    }

    [Fact]
    public void MoverMotoRequest_ComIdVagaZero_DeveFalharValidacao()
    {
        // Arrange
        var request = new MoverMotoRequest
        {
            Placa = "ABC1234",
            IdVaga = 0
        };

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage!.Contains("ID da vaga"));
    }

    [Fact]
    public void MoverMotoRequest_SerializacaoJson_DeveUsarNomesCorretos()
    {
        // Arrange
        var request = new MoverMotoRequest
        {
            Placa = "ABC1234",
            IdVaga = 1
        };

        // Act
        var json = JsonSerializer.Serialize(request, _jsonOptions);

        // Assert
        Assert.Contains("\"placa\"", json);
        Assert.Contains("\"idVaga\"", json);
    }

    [Fact]
    public void MoverMotoRequest_DeserializacaoJson_DevePreencherPropriedades()
    {
        // Arrange
        var json = """
        {
            "placa": "ABC1234",
            "idVaga": 1
        }
        """;

        // Act
        var request = JsonSerializer.Deserialize<MoverMotoRequest>(json, _jsonOptions);

        // Assert
        Assert.NotNull(request);
        Assert.Equal("ABC1234", request.Placa);
        Assert.Equal(1, request.IdVaga);
    }

    #endregion

    #region Testes de Compatibilidade com API Java

    [Fact]
    public void ModelosJava_DeveSerCompativel_ComFormatoEsperado()
    {
        // Arrange
        var moto = new Moto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        var vaga = new Vaga
        {
            Id = 1,
            Linha = "A",
            Coluna = "1",
            Ocupada = true,
            Moto = moto
        };

        // Act
        var motoJson = JsonSerializer.Serialize(moto, _jsonOptions);
        var vagaJson = JsonSerializer.Serialize(vaga, _jsonOptions);

        // Assert - Verificar estrutura JSON compatível com API Java
        Assert.Contains("\"placa\":\"ABC1234\"", motoJson);
        Assert.Contains("\"modelo\":\"Honda CB600\"", motoJson);
        Assert.Contains("\"ano\":2023", motoJson);
        Assert.Contains("\"cor\":\"Azul\"", motoJson);
        Assert.Contains("\"status\":\"NORMAL\"", motoJson);

        Assert.Contains("\"id\":1", vagaJson);
        Assert.Contains("\"linha\":\"A\"", vagaJson);
        Assert.Contains("\"coluna\":\"1\"", vagaJson);
        Assert.Contains("\"ocupada\":true", vagaJson);
        Assert.Contains("\"moto\":", vagaJson);
    }

    [Fact]
    public void ModelosJava_DeveSuportarCaseInsensitive()
    {
        // Arrange
        var jsonMoto = """
        {
            "PLACA": "ABC1234",
            "MODELO": "Honda CB600",
            "ANO": 2023,
            "COR": "Azul",
            "STATUS": "NORMAL"
        }
        """;

        var jsonVaga = """
        {
            "ID": 1,
            "LINHA": "A",
            "COLUNA": "1",
            "OCUPADA": false,
            "MOTO": null
        }
        """;

        // Act
        var moto = JsonSerializer.Deserialize<Moto>(jsonMoto, _jsonOptions);
        var vaga = JsonSerializer.Deserialize<Vaga>(jsonVaga, _jsonOptions);

        // Assert
        Assert.NotNull(moto);
        Assert.Equal("ABC1234", moto.Placa);
        Assert.Equal("Honda CB600", moto.Modelo);

        Assert.NotNull(vaga);
        Assert.Equal(1, vaga.Id);
        Assert.Equal("A", vaga.Linha);
        Assert.False(vaga.Ocupada);
    }

    [Fact]
    public void ModelosJava_DeveManterConsistencia_EntreSerializacaoDeserializacao()
    {
        // Arrange
        var motoOriginal = new Moto
        {
            Placa = "ABC1234",
            Modelo = "Honda CB600",
            Ano = 2023,
            Cor = "Azul",
            Status = "NORMAL"
        };

        var vagaOriginal = new Vaga
        {
            Id = 1,
            Linha = "A",
            Coluna = "1",
            Ocupada = true,
            Moto = motoOriginal
        };

        // Act
        var motoJson = JsonSerializer.Serialize(motoOriginal, _jsonOptions);
        var motoDeserializada = JsonSerializer.Deserialize<Moto>(motoJson, _jsonOptions);

        var vagaJson = JsonSerializer.Serialize(vagaOriginal, _jsonOptions);
        var vagaDeserializada = JsonSerializer.Deserialize<Vaga>(vagaJson, _jsonOptions);

        // Assert
        Assert.NotNull(motoDeserializada);
        Assert.Equal(motoOriginal.Placa, motoDeserializada.Placa);
        Assert.Equal(motoOriginal.Modelo, motoDeserializada.Modelo);
        Assert.Equal(motoOriginal.Ano, motoDeserializada.Ano);
        Assert.Equal(motoOriginal.Cor, motoDeserializada.Cor);
        Assert.Equal(motoOriginal.Status, motoDeserializada.Status);

        Assert.NotNull(vagaDeserializada);
        Assert.Equal(vagaOriginal.Id, vagaDeserializada.Id);
        Assert.Equal(vagaOriginal.Linha, vagaDeserializada.Linha);
        Assert.Equal(vagaOriginal.Coluna, vagaDeserializada.Coluna);
        Assert.Equal(vagaOriginal.Ocupada, vagaDeserializada.Ocupada);
        Assert.NotNull(vagaDeserializada.Moto);
        Assert.Equal(vagaOriginal.Moto.Placa, vagaDeserializada.Moto.Placa);
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