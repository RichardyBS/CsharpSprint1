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

        resource.AddLink("self", "http://localhost:5003/api/parking/1", "GET", "application/json", "Obter vaga específica");
        resource.AddLink("collection", "http://localhost:5003/api/parking", "GET", "application/json", "Listar todas as vagas");
        resource.AddLink("update", "http://localhost:5003/api/parking/1", "PUT", "application/json", "Atualizar vaga");
        resource.AddLink("delete", "http://localhost:5003/api/parking/1", "DELETE", "application/json", "Remover vaga");
        resource.AddLink("occupy", "http://localhost:5003/api/parking/1/occupy", "PATCH", "application/json", "Ocupar vaga");

        return resource;
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

        resource.AddLink("self", "http://localhost:5003/api/notification/1", "GET", "application/json", "Obter notificação específica");
        resource.AddLink("collection", "http://localhost:5003/api/notification", "GET", "application/json", "Listar todas as notificações");
        resource.AddLink("markAsRead", "http://localhost:5003/api/notification/1/read", "PATCH", "application/json", "Marcar como lida");
        resource.AddLink("delete", "http://localhost:5003/api/notification/1", "DELETE", "application/json", "Remover notificação");

        return resource;
    }
}