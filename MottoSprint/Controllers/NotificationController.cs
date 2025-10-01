using Microsoft.AspNetCore.Mvc;
using MottoSprint.Interfaces;
using MottoSprint.Models;
using MottoSprint.Models.Hateoas;
using MottoSprint.Services;

namespace MottoSprint.Controllers;

/// <summary>
/// Controller para gerenciamento de notificações com suporte completo a HATEOAS
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IMotoNotificationIntegratedService _motoNotificationService;
    private readonly IJavaApiService _javaApiService;

    public NotificationController(
        INotificationService notificationService,
        IMotoNotificationIntegratedService motoNotificationService,
        IJavaApiService javaApiService)
    {
        _notificationService = notificationService;
        _motoNotificationService = motoNotificationService;
        _javaApiService = javaApiService;
    }

    /// <summary>
    /// Listar todas as notificações do sistema
    /// </summary>
    /// <remarks>
    /// PASSO A PASSO PARA TESTAR:
    /// 1. Clique em "Try it out"
    /// 2. (Opcional) Configure os filtros:
    ///    - page: número da página (padrão: 1)
    ///    - size: itens por página (padrão: 10)
    ///    - isRead: true/false para filtrar lidas/não lidas
    /// 3. Clique em "Execute"
    /// 4. Observe a lista de notificações retornada
    /// 5. Verifique os links HATEOAS para navegação
    /// 
    /// TIPOS DE NOTIFICAÇÃO QUE VOCÊ VERÁ:
    /// - MOTO_ENTRADA: Quando uma moto entra no estacionamento
    /// - MOTO_SAIDA: Quando uma moto sai do estacionamento
    /// - VAGA_OCUPADA: Quando uma vaga é ocupada
    /// - VAGA_LIBERADA: Quando uma vaga é liberada
    /// - GENERAL: Notificações gerais do sistema
    /// 
    /// DICA: Se não houver notificações, primeiro faça uma entrada/saída de moto!
    /// 
    /// **Exemplo de uso:**
    /// ```
    /// GET /api/notification
    /// ```
    /// </remarks>
    /// <returns>Lista de notificações com navegação HATEOAS e metadados</returns>
    /// <response code="200">Lista de notificações retornada com sucesso, incluindo links de navegação e metadados</response>
    [HttpGet]
    public async Task<ActionResult<object>> GetNotifications()
    {
        var notifications = await _notificationService.GetAllNotificationsAsync();
        
        var response = new
        {
            notifications = notifications,
            _links = new Dictionary<string, Link>
            {
                ["self"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "GET", Title = "Lista de notificações" },
                ["create"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "POST", Title = "Criar nova notificação" },
                ["moto-entrada"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/moto-entrada", Method = "POST", Title = "Criar notificação de entrada de moto" },
                ["moto-saida"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/moto-saida", Method = "POST", Title = "Criar notificação de saída de moto" },
                ["by-type"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/type/{{type}}", Method = "GET", Title = "Filtrar por tipo" },
                ["monitor"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/monitor", Method = "POST", Title = "Monitorar movimentações" }
            },
            _meta = new
            {
                total = notifications.Count(),
                timestamp = DateTime.UtcNow,
                types = new[] { "MOTO_ENTRADA", "MOTO_SAIDA", "VAGA_OCUPADA", "VAGA_LIBERADA", "MOTO_DEFEITO", "GENERAL" }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtém uma notificação específica por ID com links HATEOAS
    /// </summary>
    /// <param name="id">ID da notificação</param>
    /// <returns>Notificação encontrada com navegação HATEOAS</returns>
    /// <response code="200">Notificação encontrada</response>
    /// <response code="404">Notificação não encontrada</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<Notification>> GetNotification(int id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            return NotFound(new { message = "Notificação não encontrada", id });
        }

        AddHateoasLinks(notification);
        return Ok(notification);
    }

    /// <summary>
    /// Cria uma nova notificação no sistema
    /// </summary>
    /// <remarks>
    /// Cria uma nova notificação com os dados fornecidos. A notificação será automaticamente
    /// processada e disponibilizada para consulta.
    /// 
    /// **Campos obrigatórios:**
    /// - Title: Título da notificação
    /// - Message: Mensagem detalhada
    /// - Type: Tipo da notificação (MOTO_ENTRADA, MOTO_SAIDA, etc.)
    /// 
    /// **Campos opcionais:**
    /// - IsRead: Status de leitura (padrão: false)
    /// - CreatedAt: Data de criação (padrão: agora)
    /// 
    /// **Exemplo de payload:**
    /// ```json
    /// {
    ///   "title": "Nova entrada detectada",
    ///   "message": "Moto ABC-1234 entrou na vaga A001",
    ///   "type": "MOTO_ENTRADA"
    /// }
    /// ```
    /// </remarks>
    /// <param name="notification">Dados da notificação a ser criada</param>
    /// <returns>Notificação criada com ID gerado e links HATEOAS</returns>
    /// <response code="201">Notificação criada com sucesso, retorna a notificação com ID gerado</response>
    /// <response code="400">Dados inválidos fornecidos no payload</response>
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
    [HttpPatch("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var success = await _notificationService.MarkAsReadAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Notificação não encontrada", id });
        }
        return NoContent();
    }

    /// <summary>
    /// Cria notificação de entrada de moto
    /// </summary>
    /// <param name="request">Dados da movimentação</param>
    /// <returns>Notificação criada</returns>
    [HttpPost("moto-entrada")]
    public async Task<ActionResult<Notification>> CreateMotoEntradaNotification([FromBody] MoverMotoRequest request)
    {
        var notification = await _motoNotificationService.CreateMotoEntradaNotificationAsync(request.Placa, request.IdVaga);
        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
    }

    /// <summary>
    /// Cria notificação de saída de moto
    /// </summary>
    /// <param name="request">Dados da movimentação</param>
    /// <returns>Notificação criada</returns>
    [HttpPost("moto-saida")]
    public async Task<ActionResult<Notification>> CreateMotoSaidaNotification([FromBody] MoverMotoRequest request)
    {
        var notification = await _motoNotificationService.CreateMotoSaidaNotificationAsync(request.Placa, request.IdVaga);
        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
    }

    /// <summary>
    /// Obtém notificações por moto específica
    /// </summary>
    /// <param name="placa">Placa da moto</param>
    /// <returns>Lista de notificações da moto</returns>
    [HttpGet("moto/{placa}")]
    public async Task<ActionResult<object>> GetNotificationsByMoto(string placa)
    {
        var notifications = await _motoNotificationService.GetNotificationsByMotoAsync(placa);
        var moto = await _javaApiService.GetMotoByPlacaAsync(placa);

        var response = new
        {
            placa,
            moto,
            notifications,
            _links = new Dictionary<string, Link>
            {
                ["self"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/moto/{placa}", Method = "GET", Title = "Notificações desta moto" },
                ["moto-details"] = new Link { Href = $"http://52.226.54.155:8080/api/motos/{placa}", Method = "GET", Title = "Detalhes da moto na API Java" },
                ["all-notifications"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "GET", Title = "Todas as notificações" }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtém notificações por vaga específica
    /// </summary>
    /// <param name="vagaId">ID da vaga</param>
    /// <returns>Lista de notificações da vaga</returns>
    [HttpGet("vaga/{vagaId}")]
    public async Task<ActionResult<object>> GetNotificationsByVaga(int vagaId)
    {
        var notifications = await _motoNotificationService.GetNotificationsByVagaAsync(vagaId);
        var vaga = await _javaApiService.GetVagaByIdAsync(vagaId);

        var response = new
        {
            vagaId,
            vaga,
            notifications,
            _links = new Dictionary<string, Link>
            {
                ["self"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/vaga/{vagaId}", Method = "GET", Title = "Notificações desta vaga" },
                ["vaga-details"] = new Link { Href = $"http://52.226.54.155:8080/api/vagas/{vagaId}", Method = "GET", Title = "Detalhes da vaga na API Java" },
                ["all-notifications"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "GET", Title = "Todas as notificações" }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtém notificações por tipo
    /// </summary>
    /// <param name="type">Tipo de notificação</param>
    /// <returns>Lista de notificações do tipo especificado</returns>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<object>> GetNotificationsByType(string type)
    {
        var notifications = await _motoNotificationService.GetNotificationsByTypeAsync(type);

        var response = new
        {
            type,
            notifications,
            _links = new Dictionary<string, Link>
            {
                ["self"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/type/{type}", Method = "GET", Title = $"Notificações do tipo {type}" },
                ["all-notifications"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "GET", Title = "Todas as notificações" },
                ["all-types"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "GET", Title = "Ver todos os tipos disponíveis" }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Monitora movimentações e gera notificações automaticamente
    /// </summary>
    /// <returns>Status da operação</returns>
    [HttpPost("monitor")]
    public async Task<ActionResult<object>> MonitorMovimentacoes()
    {
        var success = await _motoNotificationService.MonitorarMovimentacaoMotosAsync();

        var response = new
        {
            success,
            message = success ? "Monitoramento executado com sucesso" : "Erro durante o monitoramento",
            timestamp = DateTime.UtcNow,
            _links = new Dictionary<string, Link>
            {
                ["self"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications/monitor", Method = "POST", Title = "Executar monitoramento" },
                ["notifications"] = new Link { Href = $"{Request.Scheme}://{Request.Host}/api/notifications", Method = "GET", Title = "Ver notificações" }
            }
        };

        return Ok(response);
    }

    private void AddHateoasLinks(Notification notification)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}/api";

        notification.Links = new Dictionary<string, Link>
        {
            ["self"] = new Link { Href = $"{baseUrl}/notifications/{notification.Id}", Method = "GET", Title = "Ver esta notificação" },
            ["update"] = new Link { Href = $"{baseUrl}/notifications/{notification.Id}", Method = "PUT", Title = "Atualizar esta notificação" },
            ["delete"] = new Link { Href = $"{baseUrl}/notifications/{notification.Id}", Method = "DELETE", Title = "Deletar esta notificação" },
            ["mark-read"] = new Link { Href = $"{baseUrl}/notifications/{notification.Id}/mark-read", Method = "PATCH", Title = "Marcar como lida" },
            ["all-notifications"] = new Link { Href = $"{baseUrl}/notifications", Method = "GET", Title = "Ver todas as notificações" }
        };

        // Links específicos baseados no tipo de notificação
        if (!string.IsNullOrEmpty(notification.PlacaMoto))
        {
            notification.Links["moto-details"] = new Link { Href = $"http://52.226.54.155:8080/api/motos/{notification.PlacaMoto}", Method = "GET", Title = "Ver detalhes da moto" };
            notification.Links["moto-notifications"] = new Link { Href = $"{baseUrl}/notifications/moto/{notification.PlacaMoto}", Method = "GET", Title = "Ver todas as notificações desta moto" };
        }

        if (notification.VagaId.HasValue)
        {
            notification.Links["vaga-details"] = new Link { Href = $"http://52.226.54.155:8080/api/vagas/{notification.VagaId}", Method = "GET", Title = "Ver detalhes da vaga" };
            notification.Links["vaga-notifications"] = new Link { Href = $"{baseUrl}/notifications/vaga/{notification.VagaId}", Method = "GET", Title = "Ver todas as notificações desta vaga" };
        }

        notification.Links["notifications-by-type"] = new Link { Href = $"{baseUrl}/notifications/type/{notification.NotificationType}", Method = "GET", Title = $"Ver todas as notificações do tipo {notification.NotificationType}" };
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