using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MottoSprint.Models;

/// <summary>
/// Modelo para motos no banco de dados local
/// </summary>
[Table("TB_MOTO")]
public class MotoDb
{
    [Key]
    [StringLength(10)]
    [Column("PLACA")]
    public string Placa { get; set; } = string.Empty;

    [Required]
    [Column("CLIENTE_ID")]
    public Guid ClienteId { get; set; }

    [StringLength(100)]
    [Column("MODELO")]
    public string? Modelo { get; set; }

    [StringLength(50)]
    [Column("COR")]
    public string? Cor { get; set; }

    [Column("ANO")]
    public int? Ano { get; set; }

    [StringLength(20)]
    [Column("STATUS")]
    public string Status { get; set; } = "NORMAL"; // NORMAL ou DEFEITO

    [Column("ATIVA")]
    public bool Ativa { get; set; } = true;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("ClienteId")]
    public virtual Cliente? Cliente { get; set; }
}

/// <summary>
/// Modelo para vagas no banco de dados local
/// </summary>
[Table("TB_VAGA")]
public class VagaDb
{
    [Key]
    [Column("ID")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(10)]
    [Column("NUMERO")]
    public string Numero { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    [Column("LINHA")]
    public string Linha { get; set; } = string.Empty;

    [StringLength(10)]
    [Column("COLUNA")]
    public string Coluna { get; set; } = string.Empty;

    [Column("OCUPADA")]
    public bool Ocupada { get; set; } = false;

    [Column("ATIVA")]
    public bool Ativa { get; set; } = true;

    [StringLength(10)]
    [Column("PLACA_MOTO")]
    public string? PlacaMoto { get; set; }

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("PlacaMoto")]
    public virtual MotoDb? Moto { get; set; }
}