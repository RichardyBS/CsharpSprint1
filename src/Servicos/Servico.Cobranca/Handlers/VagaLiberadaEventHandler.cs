using Billing.Service.Models;
using Billing.Service.Services;
using Shared.Contracts.Events;

namespace Billing.Service.Handlers;

public class VagaLiberadaEventHandler
{
    private readonly IFaturamentoService _faturamentoService;
    private readonly ILogger<VagaLiberadaEventHandler> _logger;

    public VagaLiberadaEventHandler(IFaturamentoService faturamentoService, ILogger<VagaLiberadaEventHandler> logger)
    {
        _faturamentoService = faturamentoService;
        _logger = logger;
    }

    public async Task Handle(EventoVagaLiberada @event)
    {
        try
        {
            _logger.LogInformation("Processando liberação de vaga {VagaId} para cliente {ClienteId}", 
                @event.VagaId, @event.ClienteId);

            // Calcular tempo de ocupação e valor
            var tempoOcupacao = @event.DataSaida - @event.DataEntrada;
            var valorHora = 5.0m; // Valor fixo por hora (em produção viria de configuração)
            var horasOcupacao = (decimal)Math.Ceiling(tempoOcupacao.TotalHours);
            var valorTotal = horasOcupacao * valorHora;

            // Criar item da fatura
            var itemFatura = new ItemFatura
            {
                OcupacaoId = @event.Id,
                VagaId = @event.VagaId,
                CodigoVaga = @event.CodigoVaga,
                DataEntrada = @event.DataEntrada,
                DataSaida = @event.DataSaida,
                TempoOcupacao = tempoOcupacao,
                ValorHora = valorHora,
                ValorTotal = valorTotal,
                Descricao = $"Ocupação da vaga {@event.CodigoVaga} - {tempoOcupacao.TotalHours:F2}h"
            };

            // Gerar fatura
            var fatura = await _faturamentoService.GerarFaturaAsync(@event.ClienteId, new List<ItemFatura> { itemFatura });

            _logger.LogInformation("Fatura {NumeroFatura} gerada para ocupação {OcupacaoId}", 
                fatura.NumeroFatura, @event.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar liberação de vaga {VagaId}", @event.VagaId);
            throw;
        }
    }
}