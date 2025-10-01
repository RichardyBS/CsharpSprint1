using System.ComponentModel.DataAnnotations;
using MottoSprint.Models;
using Xunit;

namespace MottoSprint.Tests.Models;

/// <summary>
/// Testes unitários para o modelo ParkingSpot
/// Valida propriedades, validações de dados e comportamentos de vaga de estacionamento
/// </summary>
public class ParkingSpotTests
{
    [Fact]
    public void ParkingSpot_DeveCriarInstanciaComValoresPadrao()
    {
        // Arrange & Act
        var parkingSpot = new ParkingSpot();

        // Assert
        Assert.Equal(0, parkingSpot.Id);
        Assert.Equal(string.Empty, parkingSpot.SpotNumber);
        Assert.False(parkingSpot.IsOccupied);
        Assert.True(parkingSpot.CreatedAt <= DateTime.UtcNow);
        Assert.Null(parkingSpot.OccupiedAt);
        Assert.Null(parkingSpot.VehiclePlate);
    }

    [Fact]
    public void ParkingSpot_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var spotNumber = "A001";
        var vehiclePlate = "ABC1234";
        var occupiedAt = DateTime.UtcNow;

        // Act
        var parkingSpot = new ParkingSpot
        {
            Id = 1,
            SpotNumber = spotNumber,
            IsOccupied = true,
            VehiclePlate = vehiclePlate,
            OccupiedAt = occupiedAt
        };

        // Assert
        Assert.Equal(1, parkingSpot.Id);
        Assert.Equal(spotNumber, parkingSpot.SpotNumber);
        Assert.True(parkingSpot.IsOccupied);
        Assert.Equal(vehiclePlate, parkingSpot.VehiclePlate);
        Assert.Equal(occupiedAt, parkingSpot.OccupiedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null!)]
    public void ParkingSpot_DeveValidarSpotNumberObrigatorio(string? spotNumberInvalido)
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = spotNumberInvalido
        };

        // Act
        var validationResults = ValidateModel(parkingSpot);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("SpotNumber"));
    }

    [Fact]
    public void ParkingSpot_DeveValidarTamanhoMaximoSpotNumber()
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = "NUMEROVAGAMUITOLONGO" // Mais de 10 caracteres
        };

        // Act
        var validationResults = ValidateModel(parkingSpot);

        // Assert
        Assert.Contains(validationResults, v => v.MemberNames.Contains("SpotNumber"));
    }

    [Fact]
    public void ParkingSpot_DevePassarEmValidacaoCompleta()
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = "A001"
        };

        // Act
        var validationResults = ValidateModel(parkingSpot);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void ParkingSpot_DeveOcuparVagaCorretamente()
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = "A001"
        };
        var vehiclePlate = "ABC1234";
        var occupiedTime = DateTime.UtcNow;

        // Act
        parkingSpot.IsOccupied = true;
        parkingSpot.VehiclePlate = vehiclePlate;
        parkingSpot.OccupiedAt = occupiedTime;

        // Assert
        Assert.True(parkingSpot.IsOccupied);
        Assert.Equal(vehiclePlate, parkingSpot.VehiclePlate);
        Assert.Equal(occupiedTime, parkingSpot.OccupiedAt);
    }

    [Fact]
    public void ParkingSpot_DeveDesocuparVagaCorretamente()
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = true,
            VehiclePlate = "ABC1234",
            OccupiedAt = DateTime.UtcNow.AddHours(-2)
        };

        // Act
        parkingSpot.IsOccupied = false;
        parkingSpot.VehiclePlate = null;
        parkingSpot.OccupiedAt = null;

        // Assert
        Assert.False(parkingSpot.IsOccupied);
        Assert.Null(parkingSpot.VehiclePlate);
        Assert.Null(parkingSpot.OccupiedAt);
    }

    [Theory]
    [InlineData("A001")]
    [InlineData("B123")]
    [InlineData("C999")]
    [InlineData("1")]
    [InlineData("999")]
    public void ParkingSpot_DeveAceitarFormatosValidosDeNumeroVaga(string spotNumber)
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = spotNumber
        };

        // Act
        var validationResults = ValidateModel(parkingSpot);

        // Assert
        Assert.Empty(validationResults);
        Assert.Equal(spotNumber, parkingSpot.SpotNumber);
    }

    [Fact]
    public void ParkingSpot_DeveManterHistoricoTempoOcupacao()
    {
        // Arrange
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = "A001"
        };
        var startTime = DateTime.UtcNow.AddHours(-3);

        // Act
        parkingSpot.IsOccupied = true;
        parkingSpot.VehiclePlate = "ABC1234";
        parkingSpot.OccupiedAt = startTime;

        // Assert
        Assert.True(parkingSpot.IsOccupied);
        Assert.Equal(startTime, parkingSpot.OccupiedAt);
        
        // Verificar que o tempo de ocupação pode ser calculado
        var occupationTime = DateTime.UtcNow - parkingSpot.OccupiedAt.Value;
        Assert.True(occupationTime.TotalHours >= 3);
    }

    [Fact]
    public void ParkingSpot_DevePermitirVagaVaziaSemPlaca()
    {
        // Arrange & Act
        var parkingSpot = new ParkingSpot
        {
            SpotNumber = "A001",
            IsOccupied = false,
            VehiclePlate = null,
            OccupiedAt = null
        };

        // Assert
        Assert.False(parkingSpot.IsOccupied);
        Assert.Null(parkingSpot.VehiclePlate);
        Assert.Null(parkingSpot.OccupiedAt);
        
        var validationResults = ValidateModel(parkingSpot);
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