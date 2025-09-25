using MottoSprint.Models;

namespace MottoSprint.Interfaces;

/// <summary>
/// Interface para serviço de notificações
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envia notificação de movimentação de moto
    /// </summary>
    Task<bool> EnviarNotificacaoMotoAsync(MotoNotification notificacao);

    /// <summary>
    /// Envia notificação genérica para um cliente
    /// </summary>
    Task<bool> EnviarNotificacaoAsync(Guid clienteId, string titulo, string mensagem, string tipo, Dictionary<string, object>? dados = null);

    /// <summary>
    /// Envia email
    /// </summary>
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo);

    /// <summary>
    /// Envia SMS
    /// </summary>
    Task<bool> EnviarSmsAsync(string telefone, string mensagem);

    /// <summary>
    /// Envia notificação push
    /// </summary>
    Task<bool> EnviarPushAsync(Guid clienteId, string titulo, string mensagem);

    /// <summary>
    /// Envia notificação in-app via SignalR
    /// </summary>
    Task<bool> EnviarInAppAsync(Guid clienteId, string titulo, string mensagem);

    /// <summary>
    /// Envia notificação para todos os clientes conectados
    /// </summary>
    Task<bool> EnviarBroadcastAsync(string titulo, string mensagem);

    /// <summary>
    /// Envia notificação para um grupo específico
    /// </summary>
    Task<bool> EnviarParaGrupoAsync(string grupo, string titulo, string mensagem);

    /// <summary>
    /// Obtém todas as notificações
    /// </summary>
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();

    /// <summary>
    /// Obtém notificação por ID
    /// </summary>
    Task<Notification?> GetNotificationByIdAsync(int id);

    /// <summary>
    /// Cria nova notificação
    /// </summary>
    Task<Notification> CreateNotificationAsync(Notification notification);

    /// <summary>
    /// Marca notificação como lida
    /// </summary>
    Task<bool> MarkAsReadAsync(int id);

    /// <summary>
    /// Deleta notificação
    /// </summary>
    Task<bool> DeleteNotificationAsync(int id);

    /// <summary>
    /// Obtém contagem de notificações não lidas
    /// </summary>
    Task<int> GetUnreadCountAsync();
}