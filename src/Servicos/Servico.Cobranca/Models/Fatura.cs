using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Service.Models;

/// <summary>
/// Modelo da fatura - onde o cliente descobre quanto deve pagar
/// </summary>
public class Fatura
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // ID único - porque GUID é vida

    public Guid ClienteId { get; set; } // quem vai pagar (ou tentar não pagar)

    [Required]
    [MaxLength(200)]
    public string ClienteNome { get; set; } = string.Empty; // nome do coitado

    [Required]
    [MaxLength(200)]
    public string ClienteEmail { get; set; } = string.Empty; // email pra mandar cobrança

    [Required]
    [MaxLength(50)]
    public string NumeroFatura { get; set; } = string.Empty; // número sequencial da fatura

    public DateTime DataEmissao { get; set; } = DateTime.UtcNow; // quando foi gerada

    public DateTime DataVencimento { get; set; } // prazo pra pagar (que ninguém respeita)

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; } // quanto o cliente deve (sempre mais do que esperava)

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pendente"; // Pendente, Paga, Vencida, Cancelada - ciclo da vida de uma fatura

    public List<ItemFatura> Itens { get; set; } = new(); // detalhamento do que tá sendo cobrado

    public DateTime? DataPagamento { get; set; } // quando foi paga (se foi paga)

    [MaxLength(100)]
    public string? MetodoPagamento { get; set; } // como foi pago - cartão, pix, promessa...

    [MaxLength(1000)]
    public string? Observacoes { get; set; } // comentários extras - geralmente reclamações

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow; // timestamp de criação

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow; // última atualização
}

/// <summary>
/// Item da fatura - cada período de estacionamento vira uma linha na conta
/// </summary>
public class ItemFatura
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // ID único do item

    public Guid FaturaId { get; set; } // FK para a fatura

    public Guid OcupacaoId { get; set; } // ID da ocupação que gerou essa cobrança

    public Guid VagaId { get; set; } // qual vaga foi ocupada

    [Required]
    [MaxLength(20)]
    public string CodigoVaga { get; set; } = string.Empty; // código amigável da vaga (A1, B2, etc)

    public DateTime DataEntrada { get; set; } // quando chegou (e começou a pagar)

    public DateTime DataSaida { get; set; } // quando saiu (e parou de pagar)

    public TimeSpan TempoOcupacao { get; set; } // quanto tempo ficou (sempre mais do que planejava)

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorHora { get; set; } // preço por hora (que sempre sobe)

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; } // total desse item (matemática que dói no bolso)

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; } = string.Empty; // descrição do período - ex: "Estacionamento vaga A1 - 2h30min"

    // Navigation property
    public Fatura Fatura { get; set; } = null!;
}