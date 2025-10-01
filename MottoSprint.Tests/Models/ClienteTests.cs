using System.ComponentModel.DataAnnotations;
using MottoSprint.Models;
using Xunit;

namespace MottoSprint.Tests.Models;

/// <summary>
/// Testes unitários para o modelo Cliente
/// Valida propriedades, validações de dados e comportamentos de cliente
/// </summary>
public class ClienteTests
{
    [Fact]
    public void Cliente_DeveCriarInstanciaComValoresPadrao()
    {
        // Arrange & Act
        var cliente = new Cliente();

        // Assert
        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal(string.Empty, cliente.Nome);
        Assert.Equal(string.Empty, cliente.Email);
        Assert.Null(cliente.Telefone);
        Assert.Null(cliente.Cpf);
        Assert.True(cliente.Ativo);
        Assert.True(cliente.CreatedAt <= DateTime.UtcNow);
        Assert.True(cliente.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Cliente_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao.silva@email.com";
        var telefone = "11999999999";
        var cpf = "12345678901";

        // Act
        var cliente = new Cliente
        {
            Nome = nome,
            Email = email,
            Telefone = telefone,
            Cpf = cpf,
            Ativo = false
        };

        // Assert
        Assert.Equal(nome, cliente.Nome);
        Assert.Equal(email, cliente.Email);
        Assert.Equal(telefone, cliente.Telefone);
        Assert.Equal(cpf, cliente.Cpf);
        Assert.False(cliente.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void Cliente_DeveValidarNomeObrigatorio(string? nomeInvalido)
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = nomeInvalido,
            Email = "teste@email.com"
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Nome"));
    }

    [Fact]
    public void Cliente_DeveValidarTamanhoMaximoNome()
    {
        // Arrange
        var nomeGrande = new string('A', 256); // Mais de 255 caracteres
        var cliente = new Cliente
        {
            Nome = nomeGrande,
            Email = "teste@email.com"
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Nome"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void Cliente_DeveValidarEmailObrigatorio(string? emailInvalido)
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = emailInvalido
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Cliente_DeveValidarTamanhoMaximoEmail()
    {
        // Arrange
        var emailGrande = new string('a', 250) + "@email.com"; // Mais de 255 caracteres
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = emailGrande
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Cliente_DeveValidarTamanhoMaximoTelefone()
    {
        // Arrange
        var telefoneGrande = new string('9', 21); // Mais de 20 caracteres
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao@email.com",
            Telefone = telefoneGrande
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Telefone"));
    }

    [Fact]
    public void Cliente_DeveValidarTamanhoMaximoCpf()
    {
        // Arrange
        var cpfGrande = new string('1', 12); // Mais de 11 caracteres
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao@email.com",
            Cpf = cpfGrande
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Cpf"));
    }

    [Fact]
    public void Cliente_DevePassarEmValidacaoCompleta()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com",
            Telefone = "11999999999",
            Cpf = "12345678901"
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void Cliente_DevePermitirTelefoneOpcional()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com",
            Telefone = null
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Empty(validationResults);
        Assert.Null(cliente.Telefone);
    }

    [Fact]
    public void Cliente_DevePermitirCpfOpcional()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com",
            Cpf = null
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Empty(validationResults);
        Assert.Null(cliente.Cpf);
    }

    [Theory]
    [InlineData("11999999999")]
    [InlineData("1199999-9999")]
    [InlineData("(11)99999-9999")]
    [InlineData("+5511999999999")]
    public void Cliente_DeveAceitarFormatosValidosTelefone(string telefone)
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com",
            Telefone = telefone
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Empty(validationResults);
        Assert.Equal(telefone, cliente.Telefone);
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("123.456.789-01")]
    [InlineData("12345678900")]
    public void Cliente_DeveAceitarFormatosValidosCpf(string cpf)
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com",
            Cpf = cpf
        };

        // Act
        var validationResults = ValidateModel(cliente);

        // Assert
        Assert.Empty(validationResults);
        Assert.Equal(cpf, cliente.Cpf);
    }

    [Fact]
    public void Cliente_DeveIniciarComoAtivo()
    {
        // Arrange & Act
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com"
        };

        // Assert
        Assert.True(cliente.Ativo);
    }

    [Fact]
    public void Cliente_DevePermitirDesativacao()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com",
            Ativo = true
        };

        // Act
        cliente.Ativo = false;

        // Assert
        Assert.False(cliente.Ativo);
    }

    [Fact]
    public void Cliente_DeveManterTimestampsAtualizados()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = "joao.silva@email.com"
        };
        var createdAt = cliente.CreatedAt;
        var updatedAt = cliente.UpdatedAt;

        // Act
        Thread.Sleep(10); // Pequena pausa para garantir diferença de timestamp
        cliente.UpdatedAt = DateTime.UtcNow;

        // Assert
        Assert.Equal(createdAt, cliente.CreatedAt); // CreatedAt não deve mudar
        Assert.True(cliente.UpdatedAt > updatedAt); // UpdatedAt deve ser mais recente
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}