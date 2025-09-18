// Controller de Cobrança - onde o dinheiro é cobrado (ou deveria ser) 💸
// HACK: esse controller tem mais bugs que feature, mas funciona
using Billing.Service.Models;
using Billing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Service.Controllers;

[ApiController]
[Route("api/cobranca")] // mudei pra português, mais brasileiro
[Authorize] // sem token não rola, dinheiro é coisa séria
public class ControladorCobranca : ControllerBase
{
    private readonly IFaturamentoService _servicoFaturamento;
    private readonly ILogger<ControladorCobranca> _logger;

    public ControladorCobranca(IFaturamentoService servicoFaturamento, ILogger<ControladorCobranca> logger)
    {
        _servicoFaturamento = servicoFaturamento;
        _logger = logger; // logger que ninguém olha mas tem que ter
    }

    [HttpGet("faturas/{clienteId:guid}")]
    public async Task<IActionResult> ObterFaturasPorCliente(Guid clienteId)
    {
        // Buscando faturas do cliente - espero que ele tenha pago
        var faturas = await _servicoFaturamento.ObterFaturasPorClienteAsync(clienteId);
        return Ok(faturas); // retorna as faturas do cliente
    }

    [HttpGet("fatura/{faturaId:guid}")]
    public async Task<IActionResult> ObterFatura(Guid faturaId)
    {
        // Buscando fatura específica - tomara que exista
        var fatura = await _servicoFaturamento.ObterFaturaAsync(faturaId);
        if (fatura == null)
        {
            return NotFound("Fatura não encontrada"); // 404 quando não acha
        }

        return Ok(fatura); // retorna a fatura encontrada
    }

    [HttpPost("fatura/{faturaId:guid}/pagamento")]
    public async Task<IActionResult> ProcessarPagamento(Guid faturaId, [FromBody] SolicitacaoProcessarPagamento solicitacao)
    {
        // Primeiro verifica se a fatura existe - básico né
        var fatura = await _servicoFaturamento.ObterFaturaAsync(faturaId);
        if (fatura == null)
        {
            return NotFound("Fatura não encontrada"); // sem fatura, sem pagamento
        }

        // Verifica se tá pendente - não pode pagar fatura já paga
        if (fatura.Status != "Pendente")
        {
            return BadRequest("Fatura não está pendente de pagamento"); // já foi paga ou cancelada
        }

        // Montando o objeto de pagamento - aqui que a coisa fica séria
        // TODO: validar dados do cartão antes de processar
        var pagamento = new Pagamento
        {
            Valor = solicitacao.Valor, // quanto vai pagar
            MetodoPagamento = solicitacao.MetodoPagamento, // cartão, pix, dinheiro...
            TransacaoId = solicitacao.TransacaoId, // ID da transação no gateway
            Observacoes = solicitacao.Observacoes, // comentários do cliente
            DadosCartao = solicitacao.DadosCartao != null ? new DadosCartao
            {
                NumeroMascarado = solicitacao.DadosCartao.NumeroMascarado, // **** **** **** 1234
                Bandeira = solicitacao.DadosCartao.Bandeira, // visa, master, etc
                NomePortador = solicitacao.DadosCartao.NomePortador // nome no cartão
            } : null
        };

        // Processando o pagamento - aqui que reza pra dar certo
        var sucesso = await _servicoFaturamento.ProcessarPagamentoAsync(faturaId, pagamento);

        if (sucesso)
        {
            // Deu certo! 🎉
            return Ok(new { Sucesso = true, Mensagem = "Pagamento processado com sucesso", PagamentoId = pagamento.Id });
        }

        // Deu ruim... 😢
        return BadRequest(new { Sucesso = false, Mensagem = "Pagamento rejeitado" });
    }

    [HttpGet("relatorio-financeiro")]
    public async Task<IActionResult> ObterRelatorioFinanceiro([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        // Relatório financeiro - onde os números (não) batem
        // TODO: implementar agregações do MongoDB quando tiver tempo (spoiler: nunca vou ter)
        
        var inicio = dataInicio ?? DateTime.Today.AddDays(-30); // padrão: últimos 30 dias
        var fim = dataFim ?? DateTime.Today.AddDays(1); // até hoje

        // Aqui seria implementada a lógica real com agregações do MongoDB
        // Por enquanto é só um mock pra não quebrar a API
        var relatorio = new
        {
            Periodo = new { Inicio = inicio, Fim = fim },
            TotalFaturado = 0m, // TODO: calcular via agregação no mongo
            TotalRecebido = 0m,  // TODO: somar pagamentos aprovados
            FaturasPendentes = 0, // TODO: contar faturas em aberto
            Mensagem = "Relatório financeiro - implementação completa requer agregações MongoDB (e paciência)"
        };

        return Ok(relatorio); // retorna o relatório mock
    }
}

// Classes de request - DTOs pra receber dados do frontend
public class SolicitacaoProcessarPagamento
{
    public decimal Valor { get; set; } // quanto vai pagar
    public string MetodoPagamento { get; set; } = string.Empty; // forma de pagamento
    public string? TransacaoId { get; set; } // ID da transação externa
    public string? Observacoes { get; set; } // comentários opcionais
    public SolicitacaoDadosCartao? DadosCartao { get; set; } // dados do cartão se for cartão
}

public class SolicitacaoDadosCartao
{
    public string NumeroMascarado { get; set; } = string.Empty; // **** **** **** 1234
    public string Bandeira { get; set; } = string.Empty; // visa, master, elo...
    public string NomePortador { get; set; } = string.Empty; // nome impresso no cartão
}