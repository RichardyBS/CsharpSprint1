using Microsoft.AspNetCore.Mvc;
using MottoSprint.Models;
using MottoSprint.Services;
using System.ComponentModel.DataAnnotations;

namespace MottoSprint.Controllers;

/// <summary>
/// Controller para gerenciar notificações de movimentação de motos
/// </summary>
[ApiController]
[Route("api/moto-notifications")]
public class MotoNotificationController : ControllerBase
{
    private readonly IMotoNotificationService _motoNotificationService;
    private readonly ILogger<MotoNotificationController> _logger;

    public MotoNotificationController(
        IMotoNotificationService motoNotificationService,
        ILogger<MotoNotificationController> logger)
    {
        _motoNotificationService = motoNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Processa entrada de moto no estacionamento
    /// </summary>
    [HttpPost("entrada")]
    public async Task<IActionResult> ProcessarEntrada([FromBody] FilaEntrada entrada)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Processando entrada de moto: {Placa} -> Vaga {VagaId}", entrada.MotoPlaca, entrada.VagaId);

            var notificacao = await _motoNotificationService.ProcessarEntradaMotoAsync(entrada);
            
            return Ok(new
            {
                success = true,
                message = "Entrada processada com sucesso",
                notificacao = new
                {
                    notificacao.Id,
                    notificacao.ClienteId,
                    notificacao.MotoPlaca,
                    notificacao.VagaId,
                    notificacao.TipoMovimentacao,
                    notificacao.TimestampEvento,
                    notificacao.Mensagem,
                    notificacao.Lida
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar entrada: {Placa}", entrada.MotoPlaca);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao processar entrada: {Placa}", entrada.MotoPlaca);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Processa saída de moto do estacionamento
    /// </summary>
    [HttpPost("saida")]
    public async Task<IActionResult> ProcessarSaida([FromBody] FilaSaida saida)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Processando saída de moto: {Placa} <- Vaga {VagaId}", saida.MotoPlaca, saida.VagaId);

            var notificacao = await _motoNotificationService.ProcessarSaidaMotoAsync(saida);
            
            return Ok(new
            {
                success = true,
                message = "Saída processada com sucesso",
                notificacao = new
                {
                    notificacao.Id,
                    notificacao.ClienteId,
                    notificacao.MotoPlaca,
                    notificacao.VagaId,
                    notificacao.TipoMovimentacao,
                    notificacao.TimestampEvento,
                    notificacao.Mensagem,
                    notificacao.Lida
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar saída: {Placa}", saida.MotoPlaca);
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao processar saída: {Placa}", saida.MotoPlaca);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém notificações de um cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> ObterNotificacoesPorCliente([FromRoute] Guid clienteId)
    {
        try
        {
            var notificacoes = await _motoNotificationService.ObterNotificacoesPorClienteAsync(clienteId);
            
            return Ok(new
            {
                success = true,
                clienteId,
                total = notificacoes.Count,
                notificacoes = notificacoes.Select(n => new
                {
                    n.Id,
                    n.ClienteId,
                    n.MotoPlaca,
                    n.VagaId,
                    n.TipoMovimentacao,
                    n.TimestampEvento,
                    n.Mensagem,
                    n.Lida,
                    n.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações do cliente: {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Marca notificação como lida
    /// </summary>
    [HttpPut("{notificacaoId}/marcar-lida")]
    public async Task<IActionResult> MarcarComoLida([FromRoute] Guid notificacaoId)
    {
        try
        {
            var sucesso = await _motoNotificationService.MarcarNotificacaoComoLidaAsync(notificacaoId);
            
            if (!sucesso)
            {
                return NotFound(new { success = false, message = "Notificação não encontrada" });
            }

            return Ok(new { success = true, message = "Notificação marcada como lida" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação como lida: {NotificacaoId}", notificacaoId);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém contador de notificações não lidas
    /// </summary>
    [HttpGet("cliente/{clienteId}/nao-lidas/contador")]
    public async Task<IActionResult> ObterContadorNaoLidas([FromRoute] Guid clienteId)
    {
        try
        {
            var contador = await _motoNotificationService.ObterContadorNotificacoesNaoLidasAsync(clienteId);
            
            return Ok(new
            {
                success = true,
                clienteId,
                contadorNaoLidas = contador
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contador de notificações não lidas: {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém logs de movimentação
    /// </summary>
    [HttpGet("logs")]
    public async Task<IActionResult> ObterLogsMovimentacao(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        try
        {
            var logs = await _motoNotificationService.ObterLogsMovimentacaoAsync(dataInicio, dataFim);
            
            return Ok(new
            {
                success = true,
                filtros = new { dataInicio, dataFim },
                total = logs.Count,
                logs = logs.Select(l => new
                {
                    l.Id,
                    l.ClienteId,
                    l.MotoPlaca,
                    l.VagaId,
                    l.TipoMovimentacao,
                    l.TimestampEvento,
                    l.Detalhes,
                    l.CreatedAt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter logs de movimentação");
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém estatísticas do estacionamento
    /// </summary>
    [HttpGet("estatisticas")]
    public async Task<IActionResult> ObterEstatisticas([FromQuery] DateTime? dataReferencia)
    {
        try
        {
            var estatisticas = await _motoNotificationService.ObterEstatisticasAsync(dataReferencia);
            
            return Ok(new
            {
                success = true,
                estatisticas = new
                {
                    estatisticas.Id,
                    estatisticas.DataReferencia,
                    estatisticas.TotalVagas,
                    estatisticas.VagasOcupadas,
                    estatisticas.VagasLivres,
                    estatisticas.TotalEntradas,
                    estatisticas.TotalSaidas,
                    estatisticas.TempoMedioPermanencia,
                    TaxaOcupacao = estatisticas.TotalVagas > 0 
                        ? Math.Round((decimal)estatisticas.VagasOcupadas / estatisticas.TotalVagas * 100, 2) 
                        : 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas do estacionamento");
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém configuração de notificação do cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}/configuracao")]
    public async Task<IActionResult> ObterConfiguracaoNotificacao([FromRoute] Guid clienteId)
    {
        try
        {
            var configuracao = await _motoNotificationService.ObterConfiguracaoNotificacaoAsync(clienteId);
            
            if (configuracao == null)
            {
                return NotFound(new { success = false, message = "Configuração não encontrada" });
            }

            return Ok(new
            {
                success = true,
                configuracao = new
                {
                    configuracao.Id,
                    configuracao.ClienteId,
                    configuracao.NotificarEntrada,
                    configuracao.NotificarSaida,
                    configuracao.EmailNotificacao,
                    configuracao.TelefoneNotificacao,
                    configuracao.CreatedAt,
                    configuracao.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração de notificação: {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza configuração de notificação do cliente
    /// </summary>
    [HttpPut("cliente/{clienteId}/configuracao")]
    public async Task<IActionResult> AtualizarConfiguracaoNotificacao(
        [FromRoute] Guid clienteId,
        [FromBody] ConfiguracaoNotificacao configuracao)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            configuracao.ClienteId = clienteId;
            var configuracaoAtualizada = await _motoNotificationService.AtualizarConfiguracaoNotificacaoAsync(configuracao);
            
            return Ok(new
            {
                success = true,
                message = "Configuração atualizada com sucesso",
                configuracao = new
                {
                    configuracaoAtualizada.Id,
                    configuracaoAtualizada.ClienteId,
                    configuracaoAtualizada.NotificarEntrada,
                    configuracaoAtualizada.NotificarSaida,
                    configuracaoAtualizada.EmailNotificacao,
                    configuracaoAtualizada.TelefoneNotificacao,
                    configuracaoAtualizada.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar configuração de notificação: {ClienteId}", clienteId);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se uma vaga está disponível
    /// </summary>
    [HttpGet("vaga/{vagaId}/disponivel")]
    public async Task<IActionResult> VerificarVagaDisponivel([FromRoute] Guid vagaId)
    {
        try
        {
            var disponivel = await _motoNotificationService.VerificarVagaDisponivelAsync(vagaId);
            var vaga = await _motoNotificationService.ObterVagaAsync(vagaId);
            
            return Ok(new
            {
                success = true,
                vagaId,
                disponivel,
                vaga = vaga != null ? new
                {
                    vaga.Id,
                    vaga.Numero,
                    vaga.Linha,
                    vaga.Ocupada,
                    vaga.Ativa
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar disponibilidade da vaga: {VagaId}", vagaId);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }
}