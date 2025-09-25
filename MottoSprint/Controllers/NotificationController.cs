using Microsoft.AspNetCore.Mvc;
using MottoSprint.Interfaces;
using MottoSprint.Models;
using MottoSprint.Services;

namespace MottoSprint.Controllers;

/// <summary>
/// Controller para gerenciamento de notificações
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Obtém todas as notificações
    /// </summary>
    /// <returns>Lista de notificações</returns>
    /// <response code="200">Lista de notificações retornada com sucesso</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
    {
        var notifications = await _notificationService.GetAllNotificationsAsync();
        return Ok(notifications);
    }

    /// <summary>
    /// Obtém uma notificação específica por ID
    /// </summary>
    /// <param name="id">ID da notificação</param>
    /// <returns>Notificação encontrada</returns>
    /// <response code="200">Notificação encontrada</response>
    /// <response code="404">Notificação não encontrada</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }
        return Ok(notification);
    }

    /// <summary>
    /// Cria uma nova notificação
    /// </summary>
    /// <param name="notification">Dados da notificação a ser criada</param>
    /// <returns>Notificação criada</returns>
    /// <response code="201">Notificação criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost]
    public async Task<ActionResult<Notification>> CreateNotification(Notification notification)
    {
        var createdNotification = await _notificationService.CreateNotificationAsync(notification);
        return CreatedAtAction(nameof(GetNotification), new { id = createdNotification.Id }, createdNotification);
    }

    /// <summary>
    /// Marca uma notificação como lida
    /// </summary>
    /// <param name="id">ID da notificação</param>
    /// <returns>Confirmação da operação</returns>
    /// <response code="204">Notificação marcada como lida</response>
    /// <response code="404">Notificação não encontrada</response>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var success = await _notificationService.MarkAsReadAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Remove uma notificação
    /// </summary>
    /// <param name="id">ID da notificação a ser removida</param>
    /// <returns>Confirmação da remoção</returns>
    /// <response code="204">Notificação removida com sucesso</response>
    /// <response code="404">Notificação não encontrada</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var success = await _notificationService.DeleteNotificationAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Obtém o número de notificações não lidas
    /// </summary>
    /// <returns>Quantidade de notificações não lidas</returns>
    /// <response code="200">Contagem retornada com sucesso</response>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync();
        return Ok(count);
    }
}