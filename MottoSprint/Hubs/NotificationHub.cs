using Microsoft.AspNetCore.SignalR;

namespace MottoSprint.Hubs;

/// <summary>
/// Hub SignalR para notificações em tempo real
/// </summary>
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Conecta cliente a um grupo específico
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} adicionado ao grupo {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Remove cliente de um grupo específico
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ConnectionId} removido do grupo {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Conecta cliente ao seu grupo pessoal baseado no ID
    /// </summary>
    public async Task JoinClientGroup(string clienteId)
    {
        var groupName = $"Cliente_{clienteId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Cliente {ClienteId} conectado ao grupo pessoal", clienteId);
    }

    /// <summary>
    /// Evento disparado quando cliente se conecta
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Evento disparado quando cliente se desconecta
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "Cliente desconectado com erro: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Envia mensagem para todos os clientes conectados
    /// </summary>
    public async Task SendToAll(string message)
    {
        await Clients.All.SendAsync("ReceberMensagem", Context.ConnectionId, message);
        _logger.LogInformation("Mensagem enviada para todos os clientes por {ConnectionId}", Context.ConnectionId);
    }

    /// <summary>
    /// Envia mensagem para um grupo específico
    /// </summary>
    public async Task SendToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceberMensagem", Context.ConnectionId, message);
        _logger.LogInformation("Mensagem enviada para grupo {GroupName} por {ConnectionId}", groupName, Context.ConnectionId);
    }
}