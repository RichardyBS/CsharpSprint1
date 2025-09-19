using Analytics.Service.Data;
using Analytics.Service.Models;
using Shared.Contracts.Events;
using Shared.EventBus;

namespace Analytics.Service.Handlers;

public class VagaOcupadaEventHandler
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<VagaOcupadaEventHandler> _logger;

    public VagaOcupadaEventHandler(AnalyticsDbContext context, ILogger<VagaOcupadaEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(EventoVagaOcupada @event)
    {
        try
        {
            var ocupacao = new OcupacaoVaga
            {
                Id = @event.EventoId,
                VagaId = @event.VagaId,
                CodigoVaga = @event.CodigoVaga,
                ClienteId = @event.ClienteId,
                ClienteNome = @event.ClienteNome,
                ClienteCpf = @event.ClienteCpf,
                DataEntrada = @event.DataEntrada,
                Status = "Ocupada"
            };

            _context.OcupacoesVagas.Add(ocupacao);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ocupação registrada para vaga {CodigoVaga} - Cliente {ClienteNome}", 
                @event.CodigoVaga, @event.ClienteNome);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento VagaOcupadaEvent para vaga {VagaId}", @event.VagaId);
            throw;
        }
    }
}