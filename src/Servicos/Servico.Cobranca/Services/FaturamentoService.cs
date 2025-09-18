using Billing.Service.Data;
using Billing.Service.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using Shared.EventBus;

namespace Billing.Service.Services;

public interface IFaturamentoService
{
    Task<Fatura> GerarFaturaAsync(Guid clienteId, List<ItemFatura> itens);
    Task<Fatura?> ObterFaturaAsync(Guid faturaId);
    Task<List<Fatura>> ObterFaturasPorClienteAsync(Guid clienteId);
    Task<bool> ProcessarPagamentoAsync(Guid faturaId, Pagamento pagamento);
    Task<string> GerarNumeroFaturaAsync();
}

public class FaturamentoService : IFaturamentoService
{
    private readonly BillingDbContext _context;
    private readonly IBarramentoEventos _eventBus;
    private readonly ILogger<FaturamentoService> _logger;

    public FaturamentoService(BillingDbContext context, IBarramentoEventos eventBus, ILogger<FaturamentoService> logger)
    {
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Fatura> GerarFaturaAsync(Guid clienteId, List<ItemFatura> itens)
    {
        var fatura = new Fatura
        {
            ClienteId = clienteId,
            NumeroFatura = await GerarNumeroFaturaAsync(),
            DataEmissao = DateTime.UtcNow,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Itens = itens,
            ValorTotal = itens.Sum(i => i.ValorTotal)
        };

        // Configurar FaturaId nos itens
        foreach (var item in itens)
        {
            item.FaturaId = fatura.Id;
        }

        _context.Faturas.Add(fatura);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Fatura {NumeroFatura} gerada para cliente {ClienteId}", 
            fatura.NumeroFatura, clienteId);

        return fatura;
    }

    public async Task<Fatura?> ObterFaturaAsync(Guid faturaId)
    {
        return await _context.Faturas
            .Include(f => f.Itens)
            .FirstOrDefaultAsync(f => f.Id == faturaId);
    }

    public async Task<List<Fatura>> ObterFaturasPorClienteAsync(Guid clienteId)
    {
        return await _context.Faturas
            .Include(f => f.Itens)
            .Where(f => f.ClienteId == clienteId)
            .OrderByDescending(f => f.DataEmissao)
            .ToListAsync();
    }

    public async Task<bool> ProcessarPagamentoAsync(Guid faturaId, Pagamento pagamento)
    {
        var fatura = await ObterFaturaAsync(faturaId);
        if (fatura == null)
        {
            _logger.LogWarning("Fatura {FaturaId} não encontrada", faturaId);
            return false;
        }

        if (fatura.Status != "Pendente")
        {
            _logger.LogWarning("Fatura {FaturaId} não está pendente. Status atual: {Status}", 
                faturaId, fatura.Status);
            return false;
        }

        // Simular processamento do pagamento
        pagamento.FaturaId = faturaId;
        pagamento.ClienteId = fatura.ClienteId;
        
        // Simular aprovação (em produção seria integração com gateway de pagamento)
        await Task.Delay(1000); // Simular latência
        
        var aprovado = SimularProcessamentoPagamento(pagamento);
        
        if (aprovado)
        {
            pagamento.Status = "Aprovado";
            pagamento.DataAprovacao = DateTime.UtcNow;
            
            // Atualizar fatura
            fatura.Status = "Paga";
            fatura.DataPagamento = DateTime.UtcNow;
            fatura.MetodoPagamento = pagamento.MetodoPagamento;
            fatura.AtualizadoEm = DateTime.UtcNow;

            _context.Faturas.Update(fatura);
            
            // Publicar evento de pagamento processado
            var evento = new EventoPagamentoProcessado(
                EventoId: Guid.NewGuid(),
                OcorreuEm: DateTime.UtcNow,
                TransacaoId: Guid.NewGuid(),
                ClienteId: fatura.ClienteId,
                Valor: pagamento.Valor,
                MetodoPagamento: pagamento.MetodoPagamento,
                Status: "Aprovado",
                CodigoAutorizacao: null
            );

            await _eventBus.PublicarAsync(evento);
            
            _logger.LogInformation("Pagamento aprovado para fatura {FaturaId}", faturaId);
        }
        else
        {
            pagamento.Status = "Rejeitado";
            pagamento.Observacoes = "Pagamento rejeitado pelo processador";
            
            _logger.LogWarning("Pagamento rejeitado para fatura {FaturaId}", faturaId);
        }

        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();
        return aprovado;
    }

    public async Task<string> GerarNumeroFaturaAsync()
    {
        var ano = DateTime.Now.Year;
        var mes = DateTime.Now.Month;
        
        var ultimaFatura = await _context.Faturas
            .Find(f => f.NumeroFatura.StartsWith($"{ano:0000}{mes:00}"))
            .SortByDescending(f => f.NumeroFatura)
            .FirstOrDefaultAsync();

        var proximoNumero = 1;
        if (ultimaFatura != null)
        {
            var ultimoNumero = ultimaFatura.NumeroFatura.Substring(6);
            if (int.TryParse(ultimoNumero, out var numero))
            {
                proximoNumero = numero + 1;
            }
        }

        return $"{ano:0000}{mes:00}{proximoNumero:000000}";
    }

    private bool SimularProcessamentoPagamento(Pagamento pagamento)
    {
        // Simular taxa de aprovação de 90%
        var random = new Random();
        return random.NextDouble() > 0.1;
    }
}