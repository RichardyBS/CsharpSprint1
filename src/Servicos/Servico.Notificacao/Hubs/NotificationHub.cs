// Hub das notificações - onde os websockets fazem a festa
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Notification.Service.Hubs;

[Authorize] // só entra quem tem token
public class HubNotificacao : Hub
{
    private readonly ILogger<HubNotificacao> _logger;

    public HubNotificacao(ILogger<HubNotificacao> logger)
    {
        _logger = logger; // logger pra debugar quando der ruim
    }

    public override async Task OnConnectedAsync()
    {
        // Pega o ID do cliente do token JWT - tipo RG digital
        var clienteId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(clienteId))
        {
            // Adiciona o cliente ao grupo dele - tipo sala de chat privada
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Cliente_{clienteId}");
            _logger.LogInformation("Cliente {ClienteId} conectado ao hub - entrou na sala!", clienteId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Cliente saindo - tipo "fulano saiu do grupo"
        var clienteId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(clienteId))
        {
            // Remove do grupo - tchau tchau
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Cliente_{clienteId}");
            _logger.LogInformation("Cliente {ClienteId} desconectado do hub - saiu da sala!", clienteId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EntrarNoGrupo(string nomeGrupo)
    {
        // Entra num grupo específico - tipo entrar numa sala temática
        await Groups.AddToGroupAsync(Context.ConnectionId, nomeGrupo);
        _logger.LogInformation("Conexão {ConnectionId} entrou no grupo {GroupName} - bem-vindo!", 
            Context.ConnectionId, nomeGrupo);
    }

    public async Task SairDoGrupo(string nomeGrupo)
    {
        // Sai do grupo - tipo "saiu do grupo do trabalho"
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, nomeGrupo);
        _logger.LogInformation("Conexão {ConnectionId} saiu do grupo {GroupName} - tchau!", 
            Context.ConnectionId, nomeGrupo);
    }

    public async Task MarcarNotificacaoComoLida(string notificacaoId)
    {
        if (Guid.TryParse(notificacaoId, out var id))
        {
            // Marca como lida - tipo "visto" do WhatsApp
            _logger.LogInformation("Notificação {NotificacaoId} marcada como lida - show!", id);
            
            // Avisa o cliente que marcou - feedback instantâneo
            await Clients.Caller.SendAsync("NotificacaoMarcadaComoLida", notificacaoId);
        }
    }
}