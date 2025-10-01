using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottoSprint.Data;
using MottoSprint.Models;
using MottoSprint.Models.Hateoas;
using MottoSprint.Services;

namespace MottoSprint.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly MottoSprintDbContext _context;
    private readonly IMotoNotificationIntegratedService _motoNotificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        MottoSprintDbContext context,
        IMotoNotificationIntegratedService motoNotificationService,
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _motoNotificationService = motoNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todas as notificações com paginação
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        [FromQuery] bool? isRead = null)
    {
        try
        {
            var query = _context.Notifications.AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(n => n.NotificationType == type);
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            // Paginação
            var totalCount = await query.CountAsync();
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Adicionar HATEOAS links
            foreach (var notification in notifications)
            {
                AddHateoasLinks(notification);
            }

            // Adicionar links de navegação
            var response = new
            {
                Data = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Links = new Dictionary<string, Link>
                {
                    ["self"] = new Link { Href = Url.Action(nameof(GetNotifications), new { page, pageSize, type, isRead })!, Method = "GET" },
                    ["create"] = new Link { Href = Url.Action(nameof(CreateNotification))!, Method = "POST" }
                }
            };

            if (page > 1)
            {
                response.Links["prev"] = new Link { Href = Url.Action(nameof(GetNotifications), new { page = page - 1, pageSize, type, isRead })!, Method = "GET" };
            }

            if (page < response.TotalPages)
            {
                response.Links["next"] = new Link { Href = Url.Action(nameof(GetNotifications), new { page = page + 1, pageSize, type, isRead })!, Method = "GET" };
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter notificação por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(int id)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound(new { message = $"Notificação {id} não encontrada" });
            }

            AddHateoasLinks(notification);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificação {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Criar nova notificação
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Notification>> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        try
        {
            var notification = new Notification
            {
                Title = request.Title,
                Message = request.Message,
                NotificationType = request.NotificationType ?? "GENERAL",
                Priority = request.Priority ?? "NORMAL",
                PlacaMoto = request.PlacaMoto,
                VagaId = request.VagaId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            AddHateoasLinks(notification);

            return CreatedAtAction(
                nameof(GetNotification),
                new { id = notification.Id },
                notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar notificação");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Marcar notificação como lida
    /// </summary>
    [HttpPatch("{id}/read")]
    public async Task<ActionResult<Notification>> MarkAsRead(int id)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound(new { message = $"Notificação {id} não encontrada" });
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            AddHateoasLinks(notification);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação {Id} como lida", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter notificações por placa da moto
    /// </summary>
    [HttpGet("moto/{placa}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByMoto(string placa)
    {
        try
        {
            var notifications = await _motoNotificationService.GetNotificationsByMotoAsync(placa);
            
            foreach (var notification in notifications)
            {
                AddHateoasLinks(notification);
            }

            return Ok(new
            {
                Data = notifications,
                Links = new Dictionary<string, Link>
                {
                    ["self"] = new Link { Href = Url.Action(nameof(GetNotificationsByMoto), new { placa })!, Method = "GET" },
                    ["all"] = new Link { Href = Url.Action(nameof(GetNotifications))!, Method = "GET" }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações da moto {Placa}", placa);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter notificações por vaga
    /// </summary>
    [HttpGet("vaga/{vagaId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByVaga(int vagaId)
    {
        try
        {
            var notifications = await _motoNotificationService.GetNotificationsByVagaAsync(vagaId);
            
            foreach (var notification in notifications)
            {
                AddHateoasLinks(notification);
            }

            return Ok(new
            {
                Data = notifications,
                Links = new Dictionary<string, Link>
                {
                    ["self"] = new Link { Href = Url.Action(nameof(GetNotificationsByVaga), new { vagaId })!, Method = "GET" },
                    ["all"] = new Link { Href = Url.Action(nameof(GetNotifications))!, Method = "GET" }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações da vaga {VagaId}", vagaId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter notificações por tipo
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByType(string type)
    {
        try
        {
            var notifications = await _motoNotificationService.GetNotificationsByTypeAsync(type);
            
            foreach (var notification in notifications)
            {
                AddHateoasLinks(notification);
            }

            return Ok(new
            {
                Data = notifications,
                Links = new Dictionary<string, Link>
                {
                    ["self"] = new Link { Href = Url.Action(nameof(GetNotificationsByType), new { type })!, Method = "GET" },
                    ["all"] = new Link { Href = Url.Action(nameof(GetNotifications))!, Method = "GET" }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter notificações do tipo {Type}", type);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    private void AddHateoasLinks(Notification notification)
    {
        notification.Links = new Dictionary<string, Link>
        {
            ["self"] = new Link { Href = Url.Action(nameof(GetNotification), new { id = notification.Id })!, Method = "GET" },
            ["update"] = new Link { Href = Url.Action(nameof(GetNotification), new { id = notification.Id })!, Method = "PUT" },
            ["mark-read"] = new Link { Href = Url.Action(nameof(MarkAsRead), new { id = notification.Id })!, Method = "PATCH" },
            ["all"] = new Link { Href = Url.Action(nameof(GetNotifications))!, Method = "GET" }
        };

        if (!string.IsNullOrEmpty(notification.PlacaMoto))
        {
            notification.Links["moto-notifications"] = new Link 
            { 
                Href = Url.Action(nameof(GetNotificationsByMoto), new { placa = notification.PlacaMoto })!, 
                Method = "GET" 
            };
        }

        if (notification.VagaId.HasValue)
        {
            notification.Links["vaga-notifications"] = new Link 
            { 
                Href = Url.Action(nameof(GetNotificationsByVaga), new { vagaId = notification.VagaId })!, 
                Method = "GET" 
            };
        }
    }
}

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? NotificationType { get; set; }
    public string? Priority { get; set; }
    public string? PlacaMoto { get; set; }
    public int? VagaId { get; set; }
}