using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MottoSprint.Data;
using MottoSprint.Hubs;
using MottoSprint.Interfaces;
using MottoSprint.Models;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace MottoSprint.Services;

/// <summary>
/// Serviço de notificações integrado
/// </summary>
public class NotificationService : INotificationService
{
    private readonly MottoSprintDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        MottoSprintDbContext context,
        IHubContext<NotificationHub> hubContext,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> EnviarNotificacaoMotoAsync(MotoNotification notificacao)
    {
        try
        {
            // Obter configuração do cliente
            var config = await _context.ConfiguracoesNotificacao
                .FirstOrDefaultAsync(c => c.ClienteId == notificacao.ClienteId);

            if (config == null)
            {
                _logger.LogWarning("Configuração de notificação não encontrada para cliente {ClienteId}", notificacao.ClienteId);
                return false;
            }

            var sucesso = false;

            // Verificar se deve notificar baseado no tipo de movimentação
            var deveNotificar = (notificacao.TipoMovimentacao == "ENTRADA" && config.NotificarEntrada) ||
                               (notificacao.TipoMovimentacao == "SAIDA" && config.NotificarSaida);

            if (!deveNotificar)
            {
                _logger.LogInformation("Notificação não enviada - configuração do cliente desabilitada para {Tipo}", notificacao.TipoMovimentacao);
                return true; // Consideramos sucesso pois a configuração está correta
            }

            // Enviar notificação in-app via SignalR
            sucesso = await EnviarInAppAsync(notificacao.ClienteId, "Movimentação de Moto", notificacao.Mensagem);

            // Enviar email se configurado
            if (!string.IsNullOrEmpty(config.EmailNotificacao))
            {
                var emailSucesso = await EnviarEmailAsync(
                    config.EmailNotificacao,
                    $"Movimentação de Moto - {notificacao.TipoMovimentacao}",
                    notificacao.Mensagem);
                
                sucesso = sucesso || emailSucesso;
            }

            // Enviar SMS se configurado
            if (!string.IsNullOrEmpty(config.TelefoneNotificacao))
            {
                var smsSucesso = await EnviarSmsAsync(config.TelefoneNotificacao, notificacao.Mensagem);
                sucesso = sucesso || smsSucesso;
            }

            _logger.LogInformation("Notificação de moto {NotificacaoId} processada. Sucesso: {Sucesso}", 
                notificacao.Id, sucesso);

            return sucesso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de moto {NotificacaoId}", notificacao.Id);
            return false;
        }
    }

    public async Task<bool> EnviarNotificacaoAsync(Guid clienteId, string titulo, string mensagem, string tipo, Dictionary<string, object>? dados = null)
    {
        try
        {
            var notificacao = new
            {
                Id = Guid.NewGuid(),
                ClienteId = clienteId,
                Titulo = titulo,
                Mensagem = mensagem,
                Tipo = tipo,
                Dados = dados,
                Timestamp = DateTime.UtcNow
            };

            // Enviar via SignalR para o cliente específico
            await _hubContext.Clients.Group($"Cliente_{clienteId}")
                .SendAsync("ReceberNotificacao", notificacao);

            _logger.LogInformation("Notificação enviada para cliente {ClienteId}: {Titulo}", clienteId, titulo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para cliente {ClienteId}", clienteId);
            return false;
        }
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "MottoSprint";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
            {
                _logger.LogWarning("Configuração de email não encontrada");
                return false;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUser, fromName),
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
            // Implementação simulada de SMS
            // Em produção, integrar com provedor de SMS (Twilio, AWS SNS, etc.)
            
            var smsProvider = _configuration["SMS:Provider"];
            var smsApiKey = _configuration["SMS:ApiKey"];

            if (string.IsNullOrEmpty(smsProvider) || string.IsNullOrEmpty(smsApiKey))
            {
                _logger.LogWarning("Configuração de SMS não encontrada");
                return false;
            }

            // Simulação de envio de SMS
            await Task.Delay(100); // Simula latência de API

            _logger.LogInformation("SMS enviado com sucesso para {Telefone}: {Mensagem}", telefone, mensagem);
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
            // Implementação simulada de Push Notification
            // Em produção, integrar com Firebase Cloud Messaging, Apple Push Notification, etc.
            
            var pushProvider = _configuration["Push:Provider"];
            var pushApiKey = _configuration["Push:ApiKey"];

            if (string.IsNullOrEmpty(pushProvider) || string.IsNullOrEmpty(pushApiKey))
            {
                _logger.LogWarning("Configuração de push notification não encontrada");
                return false;
            }

            // Simulação de envio de push
            await Task.Delay(100); // Simula latência de API

            _logger.LogInformation("Push notification enviado para cliente {ClienteId}: {Titulo}", clienteId, titulo);
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
            var notificacao = new
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                Mensagem = mensagem,
                Timestamp = DateTime.UtcNow,
                Tipo = "inapp"
            };

            // Enviar para o cliente específico
            await _hubContext.Clients.Group($"Cliente_{clienteId}")
                .SendAsync("ReceberNotificacao", notificacao);

            _logger.LogInformation("Notificação in-app enviada para cliente {ClienteId}: {Titulo}", clienteId, titulo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação in-app para cliente {ClienteId}", clienteId);
            return false;
        }
    }

    public async Task<bool> EnviarBroadcastAsync(string titulo, string mensagem)
    {
        try
        {
            var notificacao = new
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                Mensagem = mensagem,
                Timestamp = DateTime.UtcNow,
                Tipo = "broadcast"
            };

            // Enviar para todos os clientes conectados
            await _hubContext.Clients.All.SendAsync("ReceberNotificacao", notificacao);

            _logger.LogInformation("Broadcast enviado: {Titulo}", titulo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar broadcast: {Titulo}", titulo);
            return false;
        }
    }

    public async Task<bool> EnviarParaGrupoAsync(string grupo, string titulo, string mensagem)
    {
        try
        {
            var notificacao = new
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                Mensagem = mensagem,
                Timestamp = DateTime.UtcNow,
                Tipo = "grupo",
                Grupo = grupo
            };

            // Enviar para o grupo específico
            await _hubContext.Clients.Group(grupo).SendAsync("ReceberNotificacao", notificacao);

            _logger.LogInformation("Notificação enviada para grupo {Grupo}: {Titulo}", grupo, titulo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação para grupo {Grupo}: {Titulo}", grupo, titulo);
            return false;
        }
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetNotificationByIdAsync(int id)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.Notifications
            .CountAsync(n => !n.IsRead);
    }
}