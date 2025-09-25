using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottoSprint.Data;
using MottoSprint.Models;
using MottoSprint.DTOs;
using MottoSprint.Extensions;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Controllers;

/// <summary>
/// Controller para gerenciamento de tarefas (Todo)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly MottoSprintDbContext _context;

    public TodoController(MottoSprintDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as tarefas com paginação e links HATEOAS
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <returns>Lista paginada de tarefas com links de navegação</returns>
    /// <response code="200">Lista de tarefas retornada com sucesso</response>
    [HttpGet]
    public async Task<ActionResult<PagedResource<TodoResource>>> GetTodos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalItems = await _context.Todos.CountAsync();
        var todos = await _context.Todos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var todoResources = todos.Select(todo => MapToResource(todo)).ToList();
        
        var pagedResource = new PagedResource<TodoResource>(todoResources, page, pageSize, totalItems);
        pagedResource.AddPaginationLinks(Url, "Todo");
        pagedResource.AddCreateLink(Url, "Todo");
        
        return Ok(pagedResource);
    }

    /// <summary>
    /// Obtém uma tarefa específica por ID com links HATEOAS
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <returns>Tarefa com links de navegação</returns>
    /// <response code="200">Tarefa encontrada</response>
    /// <response code="404">Tarefa não encontrada</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoResource>> GetTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        var resource = MapToResource(todo);
        resource.AddCrudLinks(Url, "Todo", id);
        
        // Links específicos para ações de todo
        if (!todo.IsCompleted)
        {
            var completeUrl = Url.Action("CompleteTodo", "Todo", new { id }, Request.Scheme);
            if (!string.IsNullOrEmpty(completeUrl))
            {
                resource.AddLink("complete", completeUrl, "PATCH", "application/json", "Marcar como concluída");
            }
        }

        return Ok(resource);
    }

    /// <summary>
    /// Cria uma nova tarefa
    /// </summary>
    /// <param name="todo">Dados da tarefa a ser criada</param>
    /// <returns>Tarefa criada com links HATEOAS</returns>
    /// <response code="201">Tarefa criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPost]
    public async Task<ActionResult<TodoResource>> PostTodo(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        var resource = MapToResource(todo);
        resource.AddCrudLinks(Url, "Todo", todo.Id);

        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, resource);
    }

    /// <summary>
    /// Atualiza uma tarefa existente
    /// </summary>
    /// <param name="id">ID da tarefa</param>
    /// <param name="todo">Dados atualizados da tarefa</param>
    /// <returns>Confirmação da atualização</returns>
    /// <response code="204">Tarefa atualizada com sucesso</response>
    /// <response code="400">ID não corresponde ou dados inválidos</response>
    /// <response code="404">Tarefa não encontrada</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodo(int id, Todo todo)
    {
        if (id != todo.Id)
        {
            return BadRequest();
        }

        todo.UpdatedAt = DateTime.UtcNow;
        _context.Entry(todo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TodoExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Remove uma tarefa
    /// </summary>
    /// <param name="id">ID da tarefa a ser removida</param>
    /// <returns>Confirmação da remoção</returns>
    /// <response code="204">Tarefa removida com sucesso</response>
    /// <response code="404">Tarefa não encontrada</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null)
        {
            return NotFound();
        }

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Marca uma tarefa como concluída
    /// </summary>
    /// <param name="id">ID da tarefa a ser marcada como concluída</param>
    /// <returns>Tarefa atualizada com links HATEOAS</returns>
    /// <response code="200">Tarefa marcada como concluída</response>
    /// <response code="404">Tarefa não encontrada</response>
    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<TodoResource>> CompleteTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null)
        {
            return NotFound();
        }

        todo.IsCompleted = true;
        todo.CompletedAt = DateTime.UtcNow;
        todo.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var resource = MapToResource(todo);
        resource.AddCrudLinks(Url, "Todo", id);

        return Ok(resource);
    }

    private bool TodoExists(int id)
    {
        return _context.Todos.Any(e => e.Id == id);
    }

    /// <summary>
    /// Mapeia um Todo para TodoResource
    /// </summary>
    private TodoResource MapToResource(Todo todo)
    {
        return new TodoResource
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            CompletedAt = todo.CompletedAt,
            UpdatedAt = todo.UpdatedAt
        };
    }
}