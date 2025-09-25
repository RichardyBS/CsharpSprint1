using Swashbuckle.AspNetCore.Filters;
using MottoSprint.DTOs;
using MottoSprint.Models.Hateoas;

namespace MottoSprint.Swagger;

/// <summary>
/// Exemplos de resposta HATEOAS para documentação do Swagger
/// </summary>
public class ParkingSpotResourceExample : IExamplesProvider<ParkingSpotResource>
{
    public ParkingSpotResource GetExamples()
    {
        var resource = new ParkingSpotResource
        {
            Id = 1,
            SpotNumber = "A001",
            IsOccupied = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        resource.AddLink("self", "https://localhost:5000/api/parking/1", "GET", "application/json", "Obter vaga específica");
        resource.AddLink("collection", "https://localhost:5000/api/parking", "GET", "application/json", "Listar todas as vagas");
        resource.AddLink("update", "https://localhost:5000/api/parking/1", "PUT", "application/json", "Atualizar vaga");
        resource.AddLink("delete", "https://localhost:5000/api/parking/1", "DELETE", "application/json", "Remover vaga");
        resource.AddLink("occupy", "https://localhost:5000/api/parking/1/occupy", "PATCH", "application/json", "Ocupar vaga");

        return resource;
    }
}

public class TodoResourceExample : IExamplesProvider<TodoResource>
{
    public TodoResource GetExamples()
    {
        var resource = new TodoResource
        {
            Id = 1,
            Title = "Implementar HATEOAS",
            Description = "Adicionar links de navegação REST à API",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow,
            CompletedAt = null
        };

        resource.AddLink("self", "https://localhost:5000/api/todo/1", "GET", "application/json", "Obter tarefa específica");
        resource.AddLink("collection", "https://localhost:5000/api/todo", "GET", "application/json", "Listar todas as tarefas");
        resource.AddLink("update", "https://localhost:5000/api/todo/1", "PUT", "application/json", "Atualizar tarefa");
        resource.AddLink("delete", "https://localhost:5000/api/todo/1", "DELETE", "application/json", "Remover tarefa");
        resource.AddLink("complete", "https://localhost:5000/api/todo/1/complete", "PATCH", "application/json", "Marcar como concluída");

        return resource;
    }
}

public class PagedTodoResourceExample : IExamplesProvider<PagedResource<TodoResource>>
{
    public PagedResource<TodoResource> GetExamples()
    {
        var todos = new List<TodoResource>
        {
            new TodoResource
            {
                Id = 1,
                Title = "Implementar HATEOAS",
                Description = "Adicionar links de navegação REST à API",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                CompletedAt = null
            },
            new TodoResource
            {
                Id = 2,
                Title = "Documentar API",
                Description = "Criar documentação completa da API",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        // Adicionar links HATEOAS para cada item
        foreach (var todo in todos)
        {
            todo.AddLink("self", $"https://localhost:5000/api/todo/{todo.Id}", "GET", "application/json", "Obter tarefa específica");
            todo.AddLink("update", $"https://localhost:5000/api/todo/{todo.Id}", "PUT", "application/json", "Atualizar tarefa");
            todo.AddLink("delete", $"https://localhost:5000/api/todo/{todo.Id}", "DELETE", "application/json", "Remover tarefa");
            
            if (!todo.IsCompleted)
            {
                todo.AddLink("complete", $"https://localhost:5000/api/todo/{todo.Id}/complete", "PATCH", "application/json", "Marcar como concluída");
            }
        }

        var pagedResource = new PagedResource<TodoResource>(todos, 1, 10, 25);
        
        // Adicionar links de paginação
        pagedResource.AddLink("self", "https://localhost:5000/api/todo?page=1&pageSize=10", "GET", "application/json", "Página atual");
        pagedResource.AddLink("first", "https://localhost:5000/api/todo?page=1&pageSize=10", "GET", "application/json", "Primeira página");
        pagedResource.AddLink("last", "https://localhost:5000/api/todo?page=3&pageSize=10", "GET", "application/json", "Última página");
        pagedResource.AddLink("next", "https://localhost:5000/api/todo?page=2&pageSize=10", "GET", "application/json", "Próxima página");
        pagedResource.AddLink("create", "https://localhost:5000/api/todo", "POST", "application/json", "Criar nova tarefa");

        return pagedResource;
    }
}

public class NotificationResourceExample : IExamplesProvider<NotificationResource>
{
    public NotificationResource GetExamples()
    {
        var resource = new NotificationResource
        {
            Id = 1,
            Title = "Nova vaga disponível",
            Message = "A vaga A001 está agora disponível para uso",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            Type = "ParkingSpot"
        };

        resource.AddLink("self", "https://localhost:5000/api/notification/1", "GET", "application/json", "Obter notificação específica");
        resource.AddLink("collection", "https://localhost:5000/api/notification", "GET", "application/json", "Listar todas as notificações");
        resource.AddLink("markAsRead", "https://localhost:5000/api/notification/1/read", "PATCH", "application/json", "Marcar como lida");
        resource.AddLink("delete", "https://localhost:5000/api/notification/1", "DELETE", "application/json", "Remover notificação");

        return resource;
    }
}