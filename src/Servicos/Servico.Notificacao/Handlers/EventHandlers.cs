using Notification.Service.Models;
using Notification.Service.Services;
using Shared.Contracts.Events;

namespace Notification.Service.Handlers;

public class VagaOcupadaEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<VagaOcupadaEventHandler> _logger;

    public VagaOcupadaEventHandler(INotificationService notificationService, ILogger<VagaOcupadaEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(EventoVagaOcupada @event)
    {
        try
        {
            var notificacao = new Notificacao
            {
                ClienteId = @event.ClienteId,
                Titulo = "Vaga Ocupada",
                Mensagem = $"Sua vaga {@event.CodigoVaga} foi ocupada com sucesso às {@event.DataEntrada:HH:mm}.",
                Tipo = "InApp",
                Destinatario = $"{@event.ClienteNome.ToLower().Replace(" ", ".")}@email.com"
            };

            await _notificationService.EnviarNotificacaoAsync(notificacao);

            // Enviar também por email se configurado
            var emailNotificacao = new Notificacao
            {
                ClienteId = @event.ClienteId,
                Titulo = "Confirmação de Ocupação de Vaga",
                Mensagem = $@"
                    <h2>Vaga Ocupada com Sucesso</h2>
                    <p>Olá {@event.ClienteNome},</p>
                    <p>Sua vaga <strong>{@event.CodigoVaga}</strong> foi ocupada com sucesso.</p>
                    <p><strong>Detalhes:</strong></p>
                    <ul>
                        <li>Data/Hora de Entrada: {@event.DataEntrada:dd/MM/yyyy HH:mm}</li>
                        <li>Código da Vaga: {@event.CodigoVaga}</li>
                    </ul>
                    <p>Obrigado por utilizar nosso sistema de estacionamento!</p>
                ",
                Tipo = "Email",
                Destinatario = $"{@event.ClienteNome.ToLower().Replace(" ", ".")}@email.com"
            };

            await _notificationService.EnviarNotificacaoAsync(emailNotificacao);

            _logger.LogInformation("Notificações de vaga ocupada enviadas para cliente {ClienteId}", @event.ClienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento de vaga ocupada {EventId}", @event.EventoId);
        }
    }
}

public class VagaLiberadaEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<VagaLiberadaEventHandler> _logger;

    public VagaLiberadaEventHandler(INotificationService notificationService, ILogger<VagaLiberadaEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(EventoVagaLiberada @event)
    {
        try
        {
            var notificacao = new Notificacao
            {
                ClienteId = @event.ClienteId,
                Titulo = "Vaga Liberada",
                Mensagem = $"Sua vaga {@event.CodigoVaga} foi liberada. Tempo: {@event.TempoOcupacao.TotalHours:F1}h. Valor cobrado: R$ {@event.ValorCobrado:F2}",
                Tipo = "InApp",
                Destinatario = $"cliente{@event.ClienteId}@email.com"
            };

            await _notificationService.EnviarNotificacaoAsync(notificacao);

            // Enviar também por email com detalhes completos
            var emailNotificacao = new Notificacao
            {
                ClienteId = @event.ClienteId,
                Titulo = "Vaga Liberada - Resumo da Utilização",
                Mensagem = $@"
                    <h2>Vaga Liberada</h2>
                    <p>Olá Cliente {@event.ClienteId},</p>
                    <p>Sua vaga <strong>{@event.CodigoVaga}</strong> foi liberada.</p>
                    <p><strong>Resumo da utilização:</strong></p>
                    <ul>
                        <li>Saída: {@event.DataSaida:dd/MM/yyyy HH:mm}</li>
                        <li>Tempo Total: {@event.TempoOcupacao.TotalHours:F1} horas</li>
                        <li>Valor Cobrado: R$ {@event.ValorCobrado:F2}</li>
                    </ul>
                    <p>A fatura foi gerada e enviada para seu email.</p>
                    <p>Obrigado por utilizar nosso sistema de estacionamento!</p>
                ",
                Tipo = "Email",
                Destinatario = $"cliente{@event.ClienteId}@email.com"
            };

            await _notificationService.EnviarNotificacaoAsync(emailNotificacao);

            _logger.LogInformation("Notificações de vaga liberada enviadas para cliente {ClienteId}", @event.ClienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento de vaga liberada {EventId}", @event.EventoId);
        }
    }
}

public class PagamentoProcessadoEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<PagamentoProcessadoEventHandler> _logger;

    public PagamentoProcessadoEventHandler(INotificationService notificationService, ILogger<PagamentoProcessadoEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(EventoPagamentoProcessado @event)
    {
        try
        {
            var notificacao = new Notificacao
            {
                ClienteId = @event.ClienteId,
                Titulo = "Pagamento Processado",
                Mensagem = $"Seu pagamento de R$ {@event.Valor:F2} foi processado com sucesso via {@event.MetodoPagamento}. Status: {@event.Status}",
                Tipo = "InApp",
                Destinatario = $"cliente{@event.ClienteId}@email.com"
            };

            await _notificationService.EnviarNotificacaoAsync(notificacao);

            // Enviar também por email
            var emailNotificacao = new Notificacao
            {
                ClienteId = @event.ClienteId,
                Titulo = "Confirmação de Pagamento",
                Mensagem = $@"
                    <h2>Pagamento Processado com Sucesso</h2>
                    <p>Seu pagamento foi processado com sucesso!</p>
                    <p><strong>Detalhes do pagamento:</strong></p>
                    <ul>
                        <li>Valor: R$ {@event.Valor:F2}</li>
                        <li>Método: {@event.MetodoPagamento}</li>
                        <li>Data/Hora: {@event.OcorreuEm:dd/MM/yyyy HH:mm}</li>
                        <li>ID da Transação: {@event.TransacaoId}</li>
                        <li>Status: {@event.Status}</li>
                        {(!string.IsNullOrEmpty(@event.CodigoAutorizacao) ? $"<li>Código de Autorização: {@event.CodigoAutorizacao}</li>" : "")}
                    </ul>
                    <p>Obrigado por utilizar nosso sistema de estacionamento!</p>
                ",
                Tipo = "Email",
                Destinatario = $"cliente{@event.ClienteId}@email.com"
            };

            await _notificationService.EnviarNotificacaoAsync(emailNotificacao);

            _logger.LogInformation("Notificações de pagamento processado enviadas para cliente {ClienteId}", @event.ClienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento de pagamento processado {EventId}", @event.EventoId);
        }
    }
}