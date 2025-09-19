using Microsoft.AspNetCore.SignalR;
using Notification.Service.Hubs;
using Notification.Service.Models;
using System.Net;
using System.Net.Mail;

namespace Notification.Service.Services;

public interface INotificationService
{
    Task<bool> EnviarNotificacaoAsync(Notificacao notificacao);
    Task<bool> EnviarNotificacaoAsync(Guid clienteId, string titulo, string mensagem, string tipo, Dictionary<string, object>? dados = null);
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo);
    Task<bool> EnviarSmsAsync(string telefone, string mensagem);
    Task<bool> EnviarPushAsync(Guid clienteId, string titulo, string mensagem);
    Task<bool> EnviarInAppAsync(Guid clienteId, string titulo, string mensagem);
}

public class NotificationService : INotificationService
{
    private readonly IRedisCacheService _cacheService;
    private readonly IHubContext<HubNotificacao> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IRedisCacheService cacheService,
        IHubContext<HubNotificacao> hubContext,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _cacheService = cacheService;
        _hubContext = hubContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> EnviarNotificacaoAsync(Notificacao notificacao)
    {
        try
        {
            // Salvar notificação no cache
            await _cacheService.SetNotificacaoAsync(notificacao);

            // Obter configurações do cliente
            var config = await _cacheService.GetConfiguracaoAsync(notificacao.ClienteId);
            
            var sucesso = false;
            notificacao.TentativasEnvio++;

            switch (notificacao.Tipo.ToLower())
            {
                case "email":
                    if (config?.EmailAtivo == true && !string.IsNullOrEmpty(notificacao.Destinatario))
                    {
                        sucesso = await EnviarEmailAsync(notificacao.Destinatario, notificacao.Titulo, notificacao.Mensagem);
                    }
                    break;

                case "sms":
                    if (config?.SmsAtivo == true && !string.IsNullOrEmpty(notificacao.Destinatario))
                    {
                        sucesso = await EnviarSmsAsync(notificacao.Destinatario, notificacao.Mensagem);
                    }
                    break;

                case "push":
                    if (config?.PushAtivo == true)
                    {
                        sucesso = await EnviarPushAsync(notificacao.ClienteId, notificacao.Titulo, notificacao.Mensagem);
                    }
                    break;

                case "inapp":
                    if (config?.InAppAtivo == true)
                    {
                        sucesso = await EnviarInAppAsync(notificacao.ClienteId, notificacao.Titulo, notificacao.Mensagem);
                    }
                    break;
            }

            // Atualizar status da notificação
            if (sucesso)
            {
                notificacao.Status = "Enviada";
                notificacao.EnviadaEm = DateTime.UtcNow;
                notificacao.ErroUltimoEnvio = null;
            }
            else
            {
                notificacao.Status = "Falha";
                notificacao.ErroUltimoEnvio = $"Falha no envio via {notificacao.Tipo}";
            }

            await _cacheService.SetNotificacaoAsync(notificacao);

            _logger.LogInformation("Notificação {NotificacaoId} processada. Sucesso: {Sucesso}", 
                notificacao.Id, sucesso);

            return sucesso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação {NotificacaoId}", notificacao.Id);
            
            notificacao.Status = "Falha";
            notificacao.ErroUltimoEnvio = ex.Message;
            await _cacheService.SetNotificacaoAsync(notificacao);
            
            return false;
        }
    }

    public async Task<bool> EnviarNotificacaoAsync(Guid clienteId, string titulo, string mensagem, string tipo, Dictionary<string, object>? dados = null)
    {
        var notificacao = new Notificacao
        {
            ClienteId = clienteId,
            Titulo = titulo,
            Mensagem = mensagem,
            Tipo = tipo,
            Dados = dados ?? new Dictionary<string, object>()
        };

        return await EnviarNotificacaoAsync(notificacao);
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo)
    {
        try
        {
            var smtpConfig = _configuration.GetSection("Email");
            var host = smtpConfig["SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpConfig["SmtpPort"] ?? "587");
            var username = smtpConfig["Username"] ?? "";
            var password = smtpConfig["Password"] ?? "";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(username, "Sistema de Estacionamento"),
                Subject = assunto,
                Body = corpo,
                IsBodyHtml = true
            };

            message.To.Add(destinatario);

            await client.SendMailAsync(message);
            
            _logger.LogInformation("Email enviado com sucesso para {Destinatario}", destinatario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {Destinatario}", destinatario);
            return false;
        }
    }

    public async Task<bool> EnviarSmsAsync(string telefone, string mensagem)
    {
        try
        {
            // Simulação de envio de SMS
            // Em produção, integraria com serviços como Twilio, AWS SNS, etc.
            
            await Task.Delay(500); // Simular latência
            
            _logger.LogInformation("SMS simulado enviado para {Telefone}: {Mensagem}", telefone, mensagem);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar SMS para {Telefone}", telefone);
            return false;
        }
    }

    public async Task<bool> EnviarPushAsync(Guid clienteId, string titulo, string mensagem)
    {
        try
        {
            // Simulação de push notification
            // Em produção, integraria com Firebase Cloud Messaging, Apple Push Notification, etc.
            
            await Task.Delay(200); // Simular latência
            
            _logger.LogInformation("Push notification simulado enviado para cliente {ClienteId}: {Titulo}", 
                clienteId, titulo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar push notification para cliente {ClienteId}", clienteId);
            return false;
        }
    }

    public async Task<bool> EnviarInAppAsync(Guid clienteId, string titulo, string mensagem)
    {
        try
        {
            // Enviar notificação em tempo real via SignalR
            await _hubContext.Clients.Group($"Cliente_{clienteId}")
                .SendAsync("ReceberNotificacao", new
                {
                    Titulo = titulo,
                    Mensagem = mensagem,
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("Notificação in-app enviada para cliente {ClienteId}: {Titulo}", 
                clienteId, titulo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação in-app para cliente {ClienteId}", clienteId);
            return false;
        }
    }
}