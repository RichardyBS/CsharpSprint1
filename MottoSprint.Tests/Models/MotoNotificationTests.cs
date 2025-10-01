using System.ComponentModel.DataAnnotations;
using MottoSprint.Models;
using Xunit;

namespace MottoSprint.Tests.Models;

/// <summary>
/// Testes unitários para o modelo MotoNotification
/// Valida propriedades, validações de dados e comportamentos esperados
/// </summary>
public class MotoNotificationTests
{
    [Fact]
    public void MotoNotification_DeveCriarInstanciaComValoresPadrao()
    {
        // Arrange & Act
        var notification = new MotoNotification();

        // Assert
        Assert.NotEqual(Guid.Empty, notification.Id);
        Assert.Equal(string.Empty, notification.MotoPlaca);
        Assert.Equal(string.Empty, notification.TipoMovimentacao);
        Assert.Equal(string.Empty, notification.Mensagem);
        Assert.False(notification.Lida);
        Assert.True(notification.TimestampEvento <= DateTime.UtcNow);
        Assert.True(notification.CreatedAt <= DateTime.UtcNow);
        Assert.True(notification.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MotoNotification_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var vagaId = Guid.NewGuid();
        var placa = "ABC1234";
        var tipoMovimentacao = "ENTRADA";
        var mensagem = "Moto estacionada com sucesso";
        var timestamp = DateTime.UtcNow;

        // Act
        var notification = new MotoNotification
        {
            ClienteId = clienteId,
            VagaId = vagaId,
            MotoPlaca = placa,
            TipoMovimentacao = tipoMovimentacao,
            Mensagem = mensagem,
            TimestampEvento = timestamp,
            Lida = true
        };

        // Assert
        Assert.Equal(clienteId, notification.ClienteId);
        Assert.Equal(vagaId, notification.VagaId);
        Assert.Equal(placa, notification.MotoPlaca);
        Assert.Equal(tipoMovimentacao, notification.TipoMovimentacao);
        Assert.Equal(mensagem, notification.Mensagem);
        Assert.Equal(timestamp, notification.TimestampEvento);
        Assert.True(notification.Lida);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void MotoNotification_DeveValidarMotoPlacaObrigatoria(string? placaInvalida)
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.NewGuid(),
            MotoPlaca = placaInvalida,
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Teste"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("MotoPlaca"));
    }

    [Fact]
    public void MotoNotification_DeveValidarTamanhoMaximoMotoPlaca()
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.NewGuid(),
            MotoPlaca = "PLACAMUITOLONGA", // Mais de 10 caracteres
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Teste"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("MotoPlaca"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void MotoNotification_DeveValidarTipoMovimentacaoObrigatorio(string? tipoInvalido)
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.NewGuid(),
            MotoPlaca = "ABC1234",
            TipoMovimentacao = tipoInvalido,
            Mensagem = "Teste"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("TipoMovimentacao"));
    }

    [Fact]
    public void MotoNotification_DeveValidarTamanhoMaximoTipoMovimentacao()
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.NewGuid(),
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "TIPOMOVIMENTACAOMUITOLONGO", // Mais de 20 caracteres
            Mensagem = "Teste"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("TipoMovimentacao"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void MotoNotification_DeveValidarMensagemObrigatoria(string? mensagemInvalida)
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.NewGuid(),
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "ENTRADA",
            Mensagem = mensagemInvalida
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Mensagem"));
    }

    [Fact]
    public void MotoNotification_DeveValidarClienteIdObrigatorio()
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.Empty,
            VagaId = Guid.NewGuid(),
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Teste"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("ClienteId"));
    }

    [Fact]
    public void MotoNotification_DeveValidarVagaIdObrigatorio()
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.Empty,
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Teste"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("VagaId"));
    }

    [Fact]
    public void MotoNotification_DevePassarEmValidacaoCompleta()
    {
        // Arrange
        var notification = new MotoNotification
        {
            ClienteId = Guid.NewGuid(),
            VagaId = Guid.NewGuid(),
            MotoPlaca = "ABC1234",
            TipoMovimentacao = "ENTRADA",
            Mensagem = "Moto estacionada com sucesso"
        };

        // Act
        var validationResults = ValidateModel(notification);

        // Assert
        Assert.Empty(validationResults);
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}