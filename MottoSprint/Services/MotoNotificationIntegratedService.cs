using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MottoSprint.Data;
using MottoSprint.Hubs;
using MottoSprint.Interfaces;
using MottoSprint.Models;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Services;

/// <summary>
/// Serviço integrado para notificações de motos que conecta com a API Java
/// </summary>
public interface IMotoNotificationIntegratedService
{
    Task<Notification> CreateMotoEntradaNotificationAsync(string placa, int vagaId);
    Task<Notification> CreateMotoSaidaNotificationAsync(string placa, int vagaId);
    Task<Notification> CreateVagaOcupadaNotificationAsync(int vagaId, string placa);
    Task<Notification> CreateVagaLiberadaNotificationAsync(int vagaId, string placa);
    Task<Notification> CreateMotoDefeitoNotificationAsync(string placa);
    Task<IEnumerable<Notification>> GetNotificationsByMotoAsync(string placa);
    Task<IEnumerable<Notification>> GetNotificationsByVagaAsync(int vagaId);
    Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string type);
    Task<bool> MonitorarMovimentacaoMotosAsync();
}

public class MotoNotificationIntegratedService : IMotoNotificationIntegratedService
{
    private readonly MottoSprintDbContext _context;
    private readonly IJavaApiService _javaApiService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<MotoNotificationIntegratedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MotoNotificationIntegratedService(
        MottoSprintDbContext context,
        IJavaApiService javaApiService,
        IHubContext<NotificationHub> hubContext,
        ILogger<MotoNotificationIntegratedService> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _javaApiService = javaApiService;
        _hubContext = hubContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<Notification> CreateMotoEntradaNotificationAsync(string placa, int vagaId)
    {
        var moto = await _javaApiService.GetMotoByPlacaAsync(placa);
        var vaga = await _javaApiService.GetVagaByIdAsync(vagaId);

        var notification = new Notification
        {
            Title = $"Moto Entrada - {placa}",
            Message = $"Moto {placa} ({moto?.Modelo ?? "N/A"}) entrou na vaga {vaga?.Linha}{vaga?.Coluna} (ID: {vagaId})",
            NotificationType = "MOTO_ENTRADA",
            Priority = "NORMAL",
            PlacaMoto = placa,
            VagaId = vagaId,
            CreatedAt = DateTime.UtcNow
        };

        AddHateoasLinks(notification);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Enviar notificação em tempo real via SignalR
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);

        _logger.LogInformation("Notificação de entrada criada para moto {Placa} na vaga {VagaId}", placa, vagaId);

        return notification;
    }

    public async Task<Notification> CreateMotoSaidaNotificationAsync(string placa, int vagaId)
    {
        var moto = await _javaApiService.GetMotoByPlacaAsync(placa);
        var vaga = await _javaApiService.GetVagaByIdAsync(vagaId);

        var notification = new Notification
        {
            Title = $"Moto Saída - {placa}",
            Message = $"Moto {placa} ({moto?.Modelo ?? "N/A"}) saiu da vaga {vaga?.Linha}{vaga?.Coluna} (ID: {vagaId})",
            NotificationType = "MOTO_SAIDA",
            Priority = "NORMAL",
            PlacaMoto = placa,
            VagaId = vagaId,
            CreatedAt = DateTime.UtcNow
        };

        AddHateoasLinks(notification);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Enviar notificação em tempo real via SignalR
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);

        _logger.LogInformation("Notificação de saída criada para moto {Placa} da vaga {VagaId}", placa, vagaId);

        return notification;
    }

    public async Task<Notification> CreateVagaOcupadaNotificationAsync(int vagaId, string placa)
    {
        var vaga = await _javaApiService.GetVagaByIdAsync(vagaId);

        var notification = new Notification
        {
            Title = $"Vaga Ocupada - {vaga?.Linha}{vaga?.Coluna}",
            Message = $"Vaga {vaga?.Linha}{vaga?.Coluna} (ID: {vagaId}) foi ocupada pela moto {placa}",
            NotificationType = "VAGA_OCUPADA",
            Priority = "LOW",
            PlacaMoto = placa,
            VagaId = vagaId,
            CreatedAt = DateTime.UtcNow
        };

        AddHateoasLinks(notification);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Enviar notificação em tempo real via SignalR
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);

        return notification;
    }

    public async Task<Notification> CreateVagaLiberadaNotificationAsync(int vagaId, string placa)
    {
        var vaga = await _javaApiService.GetVagaByIdAsync(vagaId);

        var notification = new Notification
        {
            Title = $"Vaga Liberada - {vaga?.Linha}{vaga?.Coluna}",
            Message = $"Vaga {vaga?.Linha}{vaga?.Coluna} (ID: {vagaId}) foi liberada pela moto {placa}",
            NotificationType = "VAGA_LIBERADA",
            Priority = "LOW",
            PlacaMoto = placa,
            VagaId = vagaId,
            CreatedAt = DateTime.UtcNow
        };

        AddHateoasLinks(notification);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Enviar notificação em tempo real via SignalR
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);

        return notification;
    }

    public async Task<Notification> CreateMotoDefeitoNotificationAsync(string placa)
    {
        var moto = await _javaApiService.GetMotoByPlacaAsync(placa);

        var notification = new Notification
        {
            Title = $"Moto com Defeito - {placa}",
            Message = $"Moto {placa} ({moto?.Modelo ?? "N/A"}) está com status de DEFEITO e precisa de atenção",
            NotificationType = "MOTO_DEFEITO",
            Priority = "HIGH",
            PlacaMoto = placa,
            CreatedAt = DateTime.UtcNow
        };

        AddHateoasLinks(notification);

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Enviar notificação em tempo real via SignalR
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);

        return notification;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByMotoAsync(string placa)
    {
        var notifications = await _context.Notifications
            .Where(n => n.PlacaMoto == placa)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            AddHateoasLinks(notification);
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByVagaAsync(int vagaId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.VagaId == vagaId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            AddHateoasLinks(notification);
        }

        return notifications;
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string type)
    {
        var notifications = await _context.Notifications
            .Where(n => n.NotificationType == type)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            AddHateoasLinks(notification);
        }

        return notifications;
    }

    public async Task<bool> MonitorarMovimentacaoMotosAsync()
    {
        try
        {
            // Este método pode ser chamado periodicamente para monitorar mudanças
            // na API Java e gerar notificações automaticamente
            var motos = await _javaApiService.GetMotosAsync();
            var vagas = await _javaApiService.GetVagasAsync();

            // Verificar motos com defeito
            foreach (var moto in motos.Where(m => m.Status == "DEFEITO"))
            {
                var existingNotification = await _context.Notifications
                    .Where(n => n.PlacaMoto == moto.Placa && n.NotificationType == "MOTO_DEFEITO")
                    .OrderByDescending(n => n.CreatedAt)
                    .FirstOrDefaultAsync();

                // Só criar nova notificação se não houver uma recente (últimas 24h)
                if (existingNotification == null || existingNotification.CreatedAt < DateTime.UtcNow.AddDays(-1))
                {
                    await CreateMotoDefeitoNotificationAsync(moto.Placa);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao monitorar movimentação de motos");
            return false;
        }
    }

    private void AddHateoasLinks(Notification notification)
    {
        var baseUrl = "https://localhost:7000/api"; // Ajustar conforme necessário

        notification.Links = new Dictionary<string, Link>
        {
            ["self"] = new Link
            {
                Href = $"{baseUrl}/notifications/{notification.Id}",
                Method = "GET",
                Title = "Ver esta notificação"
            },
            ["update"] = new Link
            {
                Href = $"{baseUrl}/notifications/{notification.Id}",
                Method = "PUT",
                Title = "Atualizar esta notificação"
            },
            ["delete"] = new Link
            {
                Href = $"{baseUrl}/notifications/{notification.Id}",
                Method = "DELETE",
                Title = "Deletar esta notificação"
            },
            ["mark-read"] = new Link
            {
                Href = $"{baseUrl}/notifications/{notification.Id}/mark-read",
                Method = "PATCH",
                Title = "Marcar como lida"
            },
            ["all-notifications"] = new Link
            {
                Href = $"{baseUrl}/notifications",
                Method = "GET",
                Title = "Ver todas as notificações"
            }
        };

        // Links específicos baseados no tipo de notificação
        if (!string.IsNullOrEmpty(notification.PlacaMoto))
        {
            notification.Links["moto-details"] = new Link
            {
                Href = $"{baseUrl}/motos/{notification.PlacaMoto}",
                Method = "GET",
                Title = "Ver detalhes da moto"
            };

            notification.Links["moto-notifications"] = new Link
            {
                Href = $"{baseUrl}/notifications/moto/{notification.PlacaMoto}",
                Method = "GET",
                Title = "Ver todas as notificações desta moto"
            };
        }

        if (notification.VagaId.HasValue)
        {
            notification.Links["vaga-details"] = new Link
            {
                Href = $"{baseUrl}/vagas/{notification.VagaId}",
                Method = "GET",
                Title = "Ver detalhes da vaga"
            };

            notification.Links["vaga-notifications"] = new Link
            {
                Href = $"{baseUrl}/notifications/vaga/{notification.VagaId}",
                Method = "GET",
                Title = "Ver todas as notificações desta vaga"
            };
        }

        notification.Links["notifications-by-type"] = new Link
        {
            Href = $"{baseUrl}/notifications/type/{notification.NotificationType}",
            Method = "GET",
            Title = $"Ver todas as notificações do tipo {notification.NotificationType}"
        };
    }
}