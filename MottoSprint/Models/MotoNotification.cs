using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottoSprint.Models;

/// <summary>
/// Modelo principal para notificações de movimentação de motos
/// </summary>
[Table("TB_NOTIFICACAO_MOTO")]
public class MotoNotification
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("CLIENTE_ID")]
    public Guid ClienteId { get; set; }

    [Required]
    [StringLength(10)]
    [Column("MOTO_PLACA")]
    public string MotoPlaca { get; set; } = string.Empty;

    [Required]
    [Column("VAGA_ID")]
    public Guid VagaId { get; set; }

    [Required]
    [StringLength(20)]
    [Column("TIPO_MOVIMENTACAO")]
    public string TipoMovimentacao { get; set; } = string.Empty; // ENTRADA, SAIDA

    [Required]
    [Column("TIMESTAMP_EVENTO")]
    public DateTime TimestampEvento { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("MENSAGEM")]
    public string Mensagem { get; set; } = string.Empty;

    [Column("LIDA")]
    public bool Lida { get; set; } = false;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para fila de entrada de motos
/// </summary>
[Table("TB_FILA_ENTRADA")]
public class FilaEntrada
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("CLIENTE_ID")]
    public Guid ClienteId { get; set; }

    [Required]
    [StringLength(10)]
    [Column("MOTO_PLACA")]
    public string MotoPlaca { get; set; } = string.Empty;

    [Required]
    [Column("VAGA_ID")]
    public Guid VagaId { get; set; }

    [Required]
    [Column("TIMESTAMP_ENTRADA")]
    public DateTime TimestampEntrada { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    [Column("STATUS")]
    public string Status { get; set; } = "PENDENTE"; // PENDENTE, PROCESSADO, ERRO

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para fila de saída de motos
/// </summary>
[Table("TB_FILA_SAIDA")]
public class FilaSaida
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("CLIENTE_ID")]
    public Guid ClienteId { get; set; }

    [Required]
    [StringLength(10)]
    [Column("MOTO_PLACA")]
    public string MotoPlaca { get; set; } = string.Empty;

    [Required]
    [Column("VAGA_ID")]
    public Guid VagaId { get; set; }

    [Required]
    [Column("TIMESTAMP_SAIDA")]
    public DateTime TimestampSaida { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    [Column("STATUS")]
    public string Status { get; set; } = "PENDENTE"; // PENDENTE, PROCESSADO, ERRO

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para logs de movimentação
/// </summary>
[Table("TB_LOG_MOVIMENTACAO")]
public class LogMovimentacao
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("CLIENTE_ID")]
    public Guid ClienteId { get; set; }

    [Required]
    [StringLength(10)]
    [Column("MOTO_PLACA")]
    public string MotoPlaca { get; set; } = string.Empty;

    [Required]
    [Column("VAGA_ID")]
    public Guid VagaId { get; set; }

    [Required]
    [StringLength(20)]
    [Column("TIPO_MOVIMENTACAO")]
    public string TipoMovimentacao { get; set; } = string.Empty; // ENTRADA, SAIDA

    [Required]
    [Column("TIMESTAMP_EVENTO")]
    public DateTime TimestampEvento { get; set; } = DateTime.UtcNow;

    [Column("DETALHES")]
    public string? Detalhes { get; set; }

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para configurações de notificação por cliente
/// </summary>
[Table("TB_CONFIGURACAO_NOTIFICACAO")]
public class ConfiguracaoNotificacao
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("CLIENTE_ID")]
    public Guid ClienteId { get; set; }

    [Column("NOTIFICAR_ENTRADA")]
    public bool NotificarEntrada { get; set; } = true;

    [Column("NOTIFICAR_SAIDA")]
    public bool NotificarSaida { get; set; } = true;

    [StringLength(255)]
    [Column("EMAIL_NOTIFICACAO")]
    public string? EmailNotificacao { get; set; }

    [StringLength(20)]
    [Column("TELEFONE_NOTIFICACAO")]
    public string? TelefoneNotificacao { get; set; }

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Modelo para clientes
/// </summary>
[Table("TB_CLIENTE")]
public class Cliente
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(255)]
    [Column("NOME")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("EMAIL")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("TELEFONE")]
    public string? Telefone { get; set; }

    [StringLength(11)]
    [Column("CPF")]
    public string? Cpf { get; set; }

    [Column("ATIVO")]
    public bool Ativo { get; set; } = true;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}



/// <summary>
/// Modelo para estatísticas de estacionamento
/// </summary>
[Table("TB_ESTATISTICAS_ESTACIONAMENTO")]
public class EstatisticasEstacionamento
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("DATA_REFERENCIA")]
    public DateTime DataReferencia { get; set; }

    [Column("TOTAL_VAGAS")]
    public int TotalVagas { get; set; } = 0;

    [Column("VAGAS_OCUPADAS")]
    public int VagasOcupadas { get; set; } = 0;

    [Column("VAGAS_LIVRES")]
    public int VagasLivres { get; set; } = 0;

    [Column("TOTAL_ENTRADAS")]
    public int TotalEntradas { get; set; } = 0;

    [Column("TOTAL_SAIDAS")]
    public int TotalSaidas { get; set; } = 0;

    [Column("TEMPO_MEDIO_PERMANENCIA")]
    public decimal TempoMedioPermanencia { get; set; } = 0;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}