using Microsoft.EntityFrameworkCore;
using MottoSprint.Data;
using MottoSprint.Interfaces;
using MottoSprint.Models;
using MottoSprint.DTOs;
using MottoSprint.Extensions;

namespace MottoSprint.Services;

/// <summary>
/// Implementação do serviço de notificação de movimentação de motos
/// </summary>
public class MotoNotificationService : IMotoNotificationService
{
    private readonly MottoSprintDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IMotoNotificationIntegratedService _motoNotificationIntegratedService;
    private readonly ILogger<MotoNotificationService> _logger;

    public MotoNotificationService(
        MottoSprintDbContext context,
        INotificationService notificationService,
        IMotoNotificationIntegratedService motoNotificationIntegratedService,
        ILogger<MotoNotificationService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _motoNotificationIntegratedService = motoNotificationIntegratedService;
        _logger = logger;
    }

    public async Task<MotoNotification> ProcessarEntradaMotoAsync(FilaEntrada entrada)
    {
        try
        {
            _logger.LogInformation("Processando entrada de moto: {Placa} na vaga {VagaId}", entrada.MotoPlaca, entrada.VagaId);

            // Verificar se a moto existe
            var moto = await ObterMotoAsync(entrada.MotoPlaca);
            if (moto == null)
            {
                throw new InvalidOperationException($"Moto com placa {entrada.MotoPlaca} não encontrada");
            }

            // Verificar se a vaga está disponível
            var vaga = await ObterVagaAsync(entrada.VagaId);
            if (vaga == null)
            {
                throw new InvalidOperationException($"Vaga {entrada.VagaId} não encontrada");
            }

            if (vaga.Ocupada)
            {
                throw new InvalidOperationException($"Vaga {vaga.Numero} já está ocupada");
            }

            // Marcar vaga como ocupada
            vaga.Ocupada = true;
            vaga.UpdatedAt = DateTime.UtcNow;

            // Criar notificação
            var notificacao = new MotoNotification
            {
                ClienteId = entrada.ClienteId,
                MotoPlaca = entrada.MotoPlaca,
                VagaId = entrada.VagaId,
                TipoMovimentacao = "ENTRADA",
                TimestampEvento = entrada.TimestampEntrada,
                Mensagem = $"Moto {moto.Modelo} (placa: {moto.Placa}) entrou na vaga {vaga.Numero} - {vaga.Linha}"
            };

            // Criar log de movimentação
            var log = new LogMovimentacao
            {
                ClienteId = entrada.ClienteId,
                MotoPlaca = entrada.MotoPlaca,
                VagaId = entrada.VagaId,
                TipoMovimentacao = "ENTRADA",
                TimestampEvento = entrada.TimestampEntrada,
                Detalhes = $"Entrada processada - Vaga: {vaga.Numero}, Linha: {vaga.Linha}"
            };

            // Marcar entrada como processada
            entrada.Status = "PROCESSADO";
            entrada.UpdatedAt = DateTime.UtcNow;

            // Salvar no banco
            _context.MotoNotifications.Add(notificacao);
            _context.LogsMovimentacao.Add(log);
            await _context.SaveChangesAsync();

            // Criar notificação persistente no banco Oracle
            await _motoNotificationIntegratedService.CreateMotoEntradaNotificationAsync(entrada.MotoPlaca, entrada.VagaId.GetHashCode());
            await _motoNotificationIntegratedService.CreateVagaOcupadaNotificationAsync(entrada.VagaId.GetHashCode(), entrada.MotoPlaca);

            // Enviar notificação via SignalR se configurado
            var configuracao = await ObterConfiguracaoNotificacaoAsync(entrada.ClienteId);
            if (configuracao?.NotificarEntrada == true)
            {
                await _notificationService.EnviarNotificacaoMotoAsync(notificacao);
            }

            _logger.LogInformation("Entrada processada com sucesso: {NotificacaoId}", notificacao.Id);
            return notificacao;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar entrada de moto: {Placa}", entrada.MotoPlaca);
            
            // Marcar entrada com erro
            entrada.Status = "ERRO";
            entrada.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            throw;
        }
    }

    public async Task<MotoNotification> ProcessarSaidaMotoAsync(FilaSaida saida)
    {
        try
        {
            _logger.LogInformation("Processando saída de moto: {Placa} da vaga {VagaId}", saida.MotoPlaca, saida.VagaId);

            // Verificar se a moto existe
            var moto = await ObterMotoAsync(saida.MotoPlaca);
            if (moto == null)
            {
                throw new InvalidOperationException($"Moto com placa {saida.MotoPlaca} não encontrada");
            }

            // Verificar se a vaga existe e está ocupada
            var vaga = await ObterVagaAsync(saida.VagaId);
            if (vaga == null)
            {
                throw new InvalidOperationException($"Vaga {saida.VagaId} não encontrada");
            }

            if (!vaga.Ocupada)
            {
                throw new InvalidOperationException($"Vaga {vaga.Numero} não está ocupada");
            }

            // Marcar vaga como livre
            vaga.Ocupada = false;
            vaga.UpdatedAt = DateTime.UtcNow;

            // Criar notificação
            var notificacao = new MotoNotification
            {
                ClienteId = saida.ClienteId,
                MotoPlaca = saida.MotoPlaca,
                VagaId = saida.VagaId,
                TipoMovimentacao = "SAIDA",
                TimestampEvento = saida.TimestampSaida,
                Mensagem = $"Moto {moto.Modelo} (placa: {moto.Placa}) saiu da vaga {vaga.Numero} - {vaga.Linha}"
            };

            // Criar log de movimentação
            var log = new LogMovimentacao
            {
                ClienteId = saida.ClienteId,
                MotoPlaca = saida.MotoPlaca,
                VagaId = saida.VagaId,
                TipoMovimentacao = "SAIDA",
                TimestampEvento = saida.TimestampSaida,
                Detalhes = $"Saída processada - Vaga: {vaga.Numero}, Linha: {vaga.Linha}"
            };

            // Marcar saída como processada
            saida.Status = "PROCESSADO";
            saida.UpdatedAt = DateTime.UtcNow;

            // Salvar no banco
            _context.MotoNotifications.Add(notificacao);
            _context.LogsMovimentacao.Add(log);
            await _context.SaveChangesAsync();

            // Criar notificação persistente no banco Oracle
            await _motoNotificationIntegratedService.CreateMotoSaidaNotificationAsync(saida.MotoPlaca, saida.VagaId.GetHashCode());
            await _motoNotificationIntegratedService.CreateVagaLiberadaNotificationAsync(saida.VagaId.GetHashCode(), saida.MotoPlaca);

            // Enviar notificação via SignalR se configurado
            var configuracao = await ObterConfiguracaoNotificacaoAsync(saida.ClienteId);
            if (configuracao?.NotificarSaida == true)
            {
                await _notificationService.EnviarNotificacaoMotoAsync(notificacao);
            }

            _logger.LogInformation("Saída processada com sucesso: {NotificacaoId}", notificacao.Id);
            return notificacao;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar saída de moto: {Placa}", saida.MotoPlaca);
            
            // Marcar saída com erro
            saida.Status = "ERRO";
            saida.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            throw;
        }
    }

    public async Task<List<MotoNotification>> ObterNotificacoesPorClienteAsync(Guid clienteId)
    {
        return await _context.MotoNotifications
            .Where(n => n.ClienteId == clienteId)
            .OrderByDescending(n => n.TimestampEvento)
            .ToListAsync();
    }

    public async Task<List<LogMovimentacao>> ObterLogsMovimentacaoAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var query = _context.LogsMovimentacao.AsQueryable();

        if (dataInicio.HasValue)
            query = query.Where(l => l.TimestampEvento >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(l => l.TimestampEvento <= dataFim.Value);

        return await query
            .OrderByDescending(l => l.TimestampEvento)
            .ToListAsync();
    }

    public async Task<EstatisticasEstacionamento> ObterEstatisticasAsync(DateTime? dataReferencia = null)
    {
        var data = dataReferencia?.Date ?? DateTime.Today;

        var estatisticas = await _context.EstatisticasEstacionamento
            .FirstOrDefaultAsync(e => e.DataReferencia.Date == data);

        if (estatisticas == null)
        {
            // Calcular estatísticas em tempo real
            var totalVagas = await _context.Vagas.CountAsync(v => v.Ativa);
            var vagasOcupadas = await _context.Vagas.CountAsync(v => v.Ativa && v.Ocupada);
            var totalEntradas = await _context.LogsMovimentacao
                .CountAsync(l => l.TimestampEvento.Date == data && l.TipoMovimentacao == "ENTRADA");
            var totalSaidas = await _context.LogsMovimentacao
                .CountAsync(l => l.TimestampEvento.Date == data && l.TipoMovimentacao == "SAIDA");

            estatisticas = new EstatisticasEstacionamento
            {
                DataReferencia = data,
                TotalVagas = totalVagas,
                VagasOcupadas = vagasOcupadas,
                VagasLivres = totalVagas - vagasOcupadas,
                TotalEntradas = totalEntradas,
                TotalSaidas = totalSaidas,
                TempoMedioPermanencia = 0 // Calcular se necessário
            };

            _context.EstatisticasEstacionamento.Add(estatisticas);
            await _context.SaveChangesAsync();
        }

        return estatisticas;
    }

    public async Task<bool> MarcarNotificacaoComoLidaAsync(Guid notificacaoId)
    {
        var notificacao = await _context.MotoNotifications.FindAsync(notificacaoId);
        if (notificacao == null)
            return false;

        notificacao.Lida = true;
        notificacao.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> ObterContadorNotificacoesNaoLidasAsync(Guid clienteId)
    {
        return await _context.MotoNotifications
            .CountAsync(n => n.ClienteId == clienteId && !n.Lida);
    }

    public async Task<ConfiguracaoNotificacao?> ObterConfiguracaoNotificacaoAsync(Guid clienteId)
    {
        return await _context.ConfiguracoesNotificacao
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);
    }

    public async Task<ConfiguracaoNotificacao> AtualizarConfiguracaoNotificacaoAsync(ConfiguracaoNotificacao configuracao)
    {
        var existente = await ObterConfiguracaoNotificacaoAsync(configuracao.ClienteId);
        
        if (existente == null)
        {
            _context.ConfiguracoesNotificacao.Add(configuracao);
        }
        else
        {
            existente.NotificarEntrada = configuracao.NotificarEntrada;
            existente.NotificarSaida = configuracao.NotificarSaida;
            existente.EmailNotificacao = configuracao.EmailNotificacao;
            existente.TelefoneNotificacao = configuracao.TelefoneNotificacao;
            existente.UpdatedAt = DateTime.UtcNow;
            configuracao = existente;
        }

        await _context.SaveChangesAsync();
        return configuracao;
    }

    public async Task<bool> VerificarVagaDisponivelAsync(Guid vagaId)
    {
        var vaga = await _context.Vagas.FindAsync(vagaId);
        return vaga != null && vaga.Ativa && !vaga.Ocupada;
    }

    public async Task<VagaDb?> ObterVagaAsync(Guid vagaId)
    {
        return await _context.Vagas.FindAsync(vagaId);
    }

    public async Task<MotoDb?> ObterMotoAsync(string placa)
    {
        return await _context.Motos
            .Include(m => m.Cliente)
            .FirstOrDefaultAsync(m => m.Placa == placa);
    }

    public async Task<Cliente?> ObterClienteAsync(Guid clienteId)
    {
        return await _context.Clientes.FindAsync(clienteId);
    }

    public async Task<MotoNotification?> ObterNotificacaoPorIdAsync(Guid id)
    {
        return await _context.MotoNotifications
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<LogMovimentacao>> ObterLogsPorMotoAsync(string placa, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var query = _context.LogsMovimentacao
            .Where(l => l.MotoPlaca == placa);

        if (dataInicio.HasValue)
            query = query.Where(l => l.TimestampEvento >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(l => l.TimestampEvento <= dataFim.Value);

        return await query
            .OrderByDescending(l => l.TimestampEvento)
            .ToListAsync();
    }

    public async Task<PagedResultDto<LogMovimentacaoDto>> ObterLogsMovimentacaoPaginadoAsync(
        int pageNumber = 1, 
        int pageSize = 10, 
        DateTime? dataInicio = null, 
        DateTime? dataFim = null)
    {
        var query = _context.LogsMovimentacao.AsQueryable();

        if (dataInicio.HasValue)
            query = query.Where(l => l.TimestampEvento >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(l => l.TimestampEvento <= dataFim.Value);

        var totalCount = await query.CountAsync();
        
        var logs = await query
            .OrderByDescending(l => l.TimestampEvento)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LogMovimentacaoDto
            {
                Id = l.Id,
                MotoPlaca = l.MotoPlaca,
                VagaId = l.VagaId,
                TipoMovimentacao = l.TipoMovimentacao,
                TimestampEvento = l.TimestampEvento,
                ClienteId = l.ClienteId,
                Detalhes = l.Detalhes ?? string.Empty
            })
            .ToListAsync();

        return logs.ToPaginatedResult(pageNumber, pageSize);
    }

    public async Task<PagedResultDto<LogMovimentacaoDto>> ObterLogsPorMotoPaginadoAsync(
        string placa, 
        int pageNumber = 1, 
        int pageSize = 10, 
        DateTime? dataInicio = null, 
        DateTime? dataFim = null)
    {
        var query = _context.LogsMovimentacao
            .Where(l => l.MotoPlaca == placa);

        if (dataInicio.HasValue)
            query = query.Where(l => l.TimestampEvento >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(l => l.TimestampEvento <= dataFim.Value);

        var totalCount = await query.CountAsync();
        
        var logs = await query
            .OrderByDescending(l => l.TimestampEvento)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LogMovimentacaoDto
            {
                Id = l.Id,
                MotoPlaca = l.MotoPlaca,
                VagaId = l.VagaId,
                TipoMovimentacao = l.TipoMovimentacao,
                TimestampEvento = l.TimestampEvento,
                ClienteId = l.ClienteId,
                Detalhes = l.Detalhes ?? string.Empty
            })
            .ToListAsync();

        return logs.ToPaginatedResult(pageNumber, pageSize);
    }

    public async Task<PagedResultDto<MotoNotificationDto>> ObterNotificacoesPorClientePaginadoAsync(
        Guid clienteId, 
        int pageNumber = 1, 
        int pageSize = 10)
    {
        var query = _context.MotoNotifications
            .Where(n => n.ClienteId == clienteId);

        var totalCount = await query.CountAsync();
        
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new MotoNotificationDto
            {
                Id = n.Id,
                ClienteId = n.ClienteId,
                MotoPlaca = n.MotoPlaca,
                VagaId = n.VagaId,
                TipoMovimentacao = n.TipoMovimentacao,
                TimestampEvento = n.TimestampEvento,
                Mensagem = n.Mensagem,
                Lida = n.Lida,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            })
            .ToListAsync();

        return notifications.ToPaginatedResult(pageNumber, pageSize);
    }
}