using Analytics.Service.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;

namespace Analytics.Service.Handlers;

public class VagaLiberadaEventHandler
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<VagaLiberadaEventHandler> _logger;

    public VagaLiberadaEventHandler(AnalyticsDbContext context, ILogger<VagaLiberadaEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(EventoVagaLiberada @event)
    {
        try
        {
            var ocupacao = await _context.OcupacoesVagas
                .FirstOrDefaultAsync(o => o.VagaId == @event.VagaId && o.Status == "Ocupada");

            if (ocupacao != null)
            {
                ocupacao.DataSaida = @event.DataSaida;
                ocupacao.TempoOcupacao = @event.TempoOcupacao;
                ocupacao.ValorCobrado = @event.ValorCobrado;
                ocupacao.Status = "Liberada";

                await _context.SaveChangesAsync();

                // Atualizar métricas diárias
                await AtualizarMetricasDiarias(@event.DataSaida.Date);

                _logger.LogInformation("Ocupação finalizada para vaga {CodigoVaga} - Valor: {Valor:C}", 
                    @event.CodigoVaga, @event.ValorCobrado);
            }
            else
            {
                _logger.LogWarning("Ocupação não encontrada para vaga {VagaId}", @event.VagaId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento VagaLiberadaEvent para vaga {VagaId}", @event.VagaId);
            throw;
        }
    }

    private async Task AtualizarMetricasDiarias(DateTime data)
    {
        var metrica = await _context.MetricasDiarias
            .FirstOrDefaultAsync(m => m.Data.Date == data.Date);

        var ocupacoesDia = await _context.OcupacoesVagas
            .Where(o => o.DataEntrada.Date == data.Date && o.Status == "Liberada")
            .ToListAsync();

        if (metrica == null)
        {
            metrica = new Analytics.Service.Models.MetricaDiaria
            {
                Id = Guid.NewGuid(),
                Data = data.Date
            };
            _context.MetricasDiarias.Add(metrica);
        }

        metrica.TotalOcupacoes = ocupacoesDia.Count;
        metrica.ReceitaTotal = ocupacoesDia.Sum(o => o.ValorCobrado ?? 0);
        metrica.TicketMedio = metrica.TotalOcupacoes > 0 ? metrica.ReceitaTotal / metrica.TotalOcupacoes : 0;
        
        if (ocupacoesDia.Any(o => o.TempoOcupacao.HasValue))
        {
            var tempoMedio = ocupacoesDia
                .Where(o => o.TempoOcupacao.HasValue)
                .Average(o => o.TempoOcupacao!.Value.TotalMinutes);
            metrica.TempoMedioOcupacao = TimeSpan.FromMinutes(tempoMedio);
        }

        metrica.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}