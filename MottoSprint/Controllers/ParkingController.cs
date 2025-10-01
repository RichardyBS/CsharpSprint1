using Microsoft.AspNetCore.Mvc;
using MottoSprint.Models;
using MottoSprint.Services;
using MottoSprint.DTOs;
using MottoSprint.Extensions;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Controllers;

/// <summary>
/// Controller para gerenciamento de vagas de estacionamento
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParkingController : ControllerBase
{
    private readonly IParkingService _parkingService;

    public ParkingController(IParkingService parkingService)
    {
        _parkingService = parkingService;
    }

    /// <summary>
    /// Listar todas as vagas do estacionamento
    /// </summary>
    /// <remarks>
    /// PASSO A PASSO PARA TESTAR:
    /// 1. Clique em "Try it out"
    /// 2. (Opcional) Configure a paginação:
    ///    - page: número da página (padrão: 1)
    ///    - pageSize: vagas por página (padrão: 10)
    /// 3. Clique em "Execute"
    /// 4. Observe a lista de vagas retornada
    /// 5. Verifique o status de cada vaga (ocupada/livre)
    /// 6. Use os links HATEOAS para navegar entre páginas
    /// 
    /// O QUE VOCÊ VERÁ:
    /// - spotNumber: Identificação da vaga (ex: "A001", "B002")
    /// - isOccupied: true se ocupada, false se livre
    /// - createdAt: Data de criação da vaga
    /// - links: Ações disponíveis para cada vaga
    /// 
    /// DICA: Este é um bom endpoint para começar os testes!
    /// </remarks>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <returns>Lista paginada de vagas com links de navegação</returns>
    /// <response code="200">Lista de vagas retornada com sucesso</response>
    [HttpGet("spots")]
    [ProducesResponseType(typeof(PagedResource<ParkingSpotResource>), 200)]
    public async Task<ActionResult<PagedResource<ParkingSpotResource>>> GetAllSpots([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var spots = await _parkingService.GetAllSpotsAsync();
        var totalItems = spots.Count();
        var pagedSpots = spots.Skip((page - 1) * pageSize).Take(pageSize);
        
        var spotResources = pagedSpots.Select(spot => MapToResource(spot)).ToList();
        
        var pagedResource = new PagedResource<ParkingSpotResource>(spotResources, page, pageSize, totalItems);
        pagedResource.AddPaginationLinks(Url, "Parking");
        pagedResource.AddCreateLink(Url, "Parking");
        
        return Ok(pagedResource);
    }

    /// <summary>
    /// Obtém uma vaga específica por ID com links HATEOAS
    /// </summary>
    /// <param name="id">ID da vaga</param>
    /// <returns>Vaga com links de navegação e ações disponíveis</returns>
    /// <response code="200">Vaga encontrada</response>
    /// <response code="404">Vaga não encontrada</response>
    [HttpGet("spots/{id}")]
    public async Task<ActionResult<ParkingSpotResource>> GetSpot(int id)
    {
        var spot = await _parkingService.GetSpotByIdAsync(id);
        if (spot == null)
        {
            return NotFound();
        }
        
        var resource = MapToResource(spot);
        resource.AddCrudLinks(Url, "Parking", id);
        
        // Links específicos para ações de estacionamento
        if (spot.IsOccupied)
        {
            var freeUrl = Url.Action("FreeSpot", "Parking", new { id }, Request.Scheme);
            if (!string.IsNullOrEmpty(freeUrl))
            {
                resource.AddLink("free", freeUrl, "PATCH", "application/json", "Liberar vaga");
            }
        }
        else
        {
            var occupyUrl = Url.Action("OccupySpot", "Parking", new { id }, Request.Scheme);
            if (!string.IsNullOrEmpty(occupyUrl))
            {
                resource.AddLink("occupy", occupyUrl, "PATCH", "application/json", "Ocupar vaga");
            }
        }
        
        return Ok(resource);
    }

    /// <summary>
    /// Obtém todas as vagas disponíveis com paginação
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <returns>Lista paginada de vagas disponíveis</returns>
    /// <response code="200">Lista de vagas disponíveis retornada com sucesso</response>
    [HttpGet("spots/available")]
    public async Task<ActionResult<PagedResource<ParkingSpotResource>>> GetAvailableSpots([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var spots = await _parkingService.GetAvailableSpotsAsync();
        var totalItems = spots.Count();
        var pagedSpots = spots.Skip((page - 1) * pageSize).Take(pageSize);
        
        var spotResources = pagedSpots.Select(spot => MapToResource(spot)).ToList();
        
        var pagedResource = new PagedResource<ParkingSpotResource>(spotResources, page, pageSize, totalItems);
        pagedResource.AddPaginationLinks(Url, "Parking", new { status = "available" });
        
        // Link para todas as vagas
        var allSpotsUrl = Url.Action("GetAllSpots", "Parking", null, Request.Scheme);
        if (!string.IsNullOrEmpty(allSpotsUrl))
        {
            pagedResource.AddLink("all", allSpotsUrl, "GET", "application/json", "Todas as vagas");
        }
        
        return Ok(pagedResource);
    }

    /// <summary>
    /// Obtém todas as vagas ocupadas com paginação
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <returns>Lista paginada de vagas ocupadas</returns>
    /// <response code="200">Lista de vagas ocupadas retornada com sucesso</response>
    [HttpGet("spots/occupied")]
    public async Task<ActionResult<PagedResource<ParkingSpotResource>>> GetOccupiedSpots([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var spots = await _parkingService.GetOccupiedSpotsAsync();
        var totalItems = spots.Count();
        var pagedSpots = spots.Skip((page - 1) * pageSize).Take(pageSize);
        
        var spotResources = pagedSpots.Select(spot => MapToResource(spot)).ToList();
        
        var pagedResource = new PagedResource<ParkingSpotResource>(spotResources, page, pageSize, totalItems);
        pagedResource.AddPaginationLinks(Url, "Parking", new { status = "occupied" });
        
        // Link para todas as vagas
        var allSpotsUrl = Url.Action("GetAllSpots", "Parking", null, Request.Scheme);
        if (!string.IsNullOrEmpty(allSpotsUrl))
        {
            pagedResource.AddLink("all", allSpotsUrl, "GET", "application/json", "Todas as vagas");
        }
        
        return Ok(pagedResource);
    }

    /// <summary>
    /// Cria uma nova vaga de estacionamento
    /// </summary>
    /// <param name="spot">Dados da vaga a ser criada</param>
    /// <returns>Vaga criada com links HATEOAS</returns>
    /// <response code="201">Vaga criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost("spots")]
    public async Task<ActionResult<ParkingSpotResource>> CreateSpot(ParkingSpot spot)
    {
        var createdSpot = await _parkingService.CreateSpotAsync(spot);
        var resource = MapToResource(createdSpot);
        resource.AddCrudLinks(Url, "Parking", createdSpot.Id);
        
        return CreatedAtAction(nameof(GetSpot), new { id = createdSpot.Id }, resource);
    }

    /// <summary>
    /// Ocupa uma vaga de estacionamento
    /// </summary>
    /// <param name="id">ID da vaga</param>
    /// <param name="vehiclePlate">Placa do veículo</param>
    /// <returns>Vaga atualizada com links HATEOAS</returns>
    /// <response code="200">Vaga ocupada com sucesso</response>
    /// <response code="400">Vaga não encontrada ou já ocupada</response>
    [HttpPatch("spots/{id}/occupy")]
    public async Task<ActionResult<ParkingSpotResource>> OccupySpot(int id, [FromBody] string vehiclePlate)
    {
        var success = await _parkingService.OccupySpotAsync(id, vehiclePlate);
        if (!success)
        {
            return BadRequest("Vaga não encontrada ou já ocupada");
        }
        
        var updatedSpot = await _parkingService.GetSpotByIdAsync(id);
        if (updatedSpot != null)
        {
            var resource = MapToResource(updatedSpot);
            resource.AddCrudLinks(Url, "Parking", id);
            
            // Adicionar link para liberar a vaga
            var freeUrl = Url.Action("FreeSpot", "Parking", new { id }, Request.Scheme);
            if (!string.IsNullOrEmpty(freeUrl))
            {
                resource.AddLink("free", freeUrl, "PATCH", "application/json", "Liberar vaga");
            }
            
            return Ok(resource);
        }
        
        return NoContent();
    }

    /// <summary>
    /// Libera uma vaga de estacionamento
    /// </summary>
    /// <param name="id">ID da vaga</param>
    /// <returns>Vaga atualizada com links HATEOAS</returns>
    /// <response code="200">Vaga liberada com sucesso</response>
    /// <response code="400">Vaga não encontrada ou já livre</response>
    [HttpPatch("spots/{id}/free")]
    public async Task<ActionResult<ParkingSpotResource>> FreeSpot(int id)
    {
        var success = await _parkingService.FreeSpotAsync(id);
        if (!success)
        {
            return BadRequest("Vaga não encontrada ou já livre");
        }
        
        var updatedSpot = await _parkingService.GetSpotByIdAsync(id);
        if (updatedSpot != null)
        {
            var resource = MapToResource(updatedSpot);
            resource.AddCrudLinks(Url, "Parking", id);
            
            // Adicionar link para ocupar a vaga
            var occupyUrl = Url.Action("OccupySpot", "Parking", new { id }, Request.Scheme);
            if (!string.IsNullOrEmpty(occupyUrl))
            {
                resource.AddLink("occupy", occupyUrl, "PATCH", "application/json", "Ocupar vaga");
            }
            
            return Ok(resource);
        }
        
        return NoContent();
    }

    /// <summary>
    /// Mapeia um ParkingSpot para ParkingSpotResource
    /// </summary>
    private ParkingSpotResource MapToResource(ParkingSpot spot)
    {
        return new ParkingSpotResource
        {
            Id = spot.Id,
            SpotNumber = spot.SpotNumber,
            IsOccupied = spot.IsOccupied,
            CreatedAt = spot.CreatedAt,
            OccupiedAt = spot.OccupiedAt,
            VehiclePlate = spot.VehiclePlate
        };
    }
}