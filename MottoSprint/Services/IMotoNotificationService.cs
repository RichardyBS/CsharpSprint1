using MottoSprint.Models;

namespace MottoSprint.Services;

/// <summary>
/// Interface para serviço de notificação de movimentação de motos
/// </summary>
public interface IMotoNotificationService
{
    /// <summary>
    /// Processa entrada de moto no estacionamento
    /// </summary>
    Task<MotoNotification> ProcessarEntradaMotoAsync(FilaEntrada entrada);

    /// <summary>
    /// Processa saída de moto do estacionamento
    /// </summary>
    Task<MotoNotification> ProcessarSaidaMotoAsync(FilaSaida saida);

    /// <summary>
    /// Obtém notificações por cliente
    /// </summary>
    Task<List<MotoNotification>> ObterNotificacoesPorClienteAsync(Guid clienteId);

    /// <summary>
    /// Obtém logs de movimentação
    /// </summary>
    Task<List<LogMovimentacao>> ObterLogsMovimentacaoAsync(DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Obtém estatísticas do estacionamento
    /// </summary>
    Task<EstatisticasEstacionamento> ObterEstatisticasAsync(DateTime? dataReferencia = null);

    /// <summary>
    /// Marca notificação como lida
    /// </summary>
    Task<bool> MarcarNotificacaoComoLidaAsync(Guid notificacaoId);

    /// <summary>
    /// Obtém contador de notificações não lidas
    /// </summary>
    Task<int> ObterContadorNotificacoesNaoLidasAsync(Guid clienteId);

    /// <summary>
    /// Obtém configuração de notificação do cliente
    /// </summary>
    Task<ConfiguracaoNotificacao?> ObterConfiguracaoNotificacaoAsync(Guid clienteId);

    /// <summary>
    /// Atualiza configuração de notificação do cliente
    /// </summary>
    Task<ConfiguracaoNotificacao> AtualizarConfiguracaoNotificacaoAsync(ConfiguracaoNotificacao configuracao);

    /// <summary>
    /// Verifica se vaga está disponível
    /// </summary>
    Task<bool> VerificarVagaDisponivelAsync(Guid vagaId);

    /// <summary>
    /// Obtém informações da vaga
    /// </summary>
    Task<Vaga?> ObterVagaAsync(Guid vagaId);

    /// <summary>
    /// Obtém informações da moto
    /// </summary>
    Task<Moto?> ObterMotoAsync(string placa);

    /// <summary>
    /// Obtém informações do cliente
    /// </summary>
    Task<Cliente?> ObterClienteAsync(Guid clienteId);
}