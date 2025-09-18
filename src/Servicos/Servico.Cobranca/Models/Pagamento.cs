using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Service.Models;

/// <summary>
/// Modelo do pagamento - onde o dinheiro (teoricamente) entra
/// </summary>
public class Pagamento
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // ID único do pagamento

    public Guid FaturaId { get; set; } // qual fatura tá sendo paga

    public Guid ClienteId { get; set; } // quem tá pagando (finalmente!)

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; } // quanto foi pago (nem sempre bate com o valor da fatura)

    [Required]
    [MaxLength(50)]
    public string MetodoPagamento { get; set; } = string.Empty; // Cartao, Pix, Dinheiro - forma de pagamento

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Processando"; // Processando, Aprovado, Rejeitado - ciclo de vida do pagamento

    [MaxLength(100)]
    public string? TransacaoId { get; set; } // ID da transação externa (gateway de pagamento)

    public DateTime DataProcessamento { get; set; } = DateTime.UtcNow; // quando começou a processar

    public DateTime? DataAprovacao { get; set; } // quando foi aprovado (se foi aprovado)

    [MaxLength(1000)]
    public string? Observacoes { get; set; } // comentários sobre o pagamento

    public DadosCartao? DadosCartao { get; set; } // dados do cartão (se pagou com cartão)

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow; // timestamp de criação

    // Navigation property
    public Fatura Fatura { get; set; } = null!;
}

/// <summary>
/// Dados do cartão - informações seguras (mascaradas) do cartão de crédito
/// </summary>
public class DadosCartao
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // ID único dos dados do cartão

    public Guid PagamentoId { get; set; } // FK para o pagamento

    [Required]
    [MaxLength(20)]
    public string NumeroMascarado { get; set; } = string.Empty; // **** **** **** 1234 - só os últimos 4 dígitos

    [Required]
    [MaxLength(50)]
    public string Bandeira { get; set; } = string.Empty; // Visa, Mastercard, Elo, etc - qual bandeira

    [Required]
    [MaxLength(100)]
    public string NomePortador { get; set; } = string.Empty; // nome impresso no cartão

    // Navigation property
    public Pagamento Pagamento { get; set; } = null!;
}