using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MottoSprint.Data;
using MottoSprint.Services;
using MottoSprint.Hubs;

namespace MottoSprint.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona todos os serviços do MottoSprint
    /// </summary>
    public static IServiceCollection AddMottoSprintServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurações
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // Entity Framework
        services.AddMottoSprintDatabase(configuration);

        // HttpClient para integração com API Java
        services.AddHttpClient<IJavaApiService, JavaApiService>();

        // SignalR para notificações em tempo real
        services.AddSignalR();

        // Serviços de negócio
        services.AddScoped<IParkingService, ParkingService>();
        services.AddScoped<IMotoNotificationService, MotoNotificationService>();
        services.AddScoped<MottoSprint.Interfaces.INotificationService, MottoSprint.Services.NotificationService>();
        services.AddScoped<IJavaApiService, JavaApiService>();
        services.AddScoped<IMotoNotificationIntegratedService, MotoNotificationIntegratedService>();

        return services;
    }

    /// <summary>
    /// Adiciona e configura o Entity Framework com Oracle
    /// </summary>
    public static IServiceCollection AddMottoSprintDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() 
            ?? new DatabaseOptions();

        services.AddDbContext<MottoSprintDbContext>(options =>
        {
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"));

            if (databaseOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    /// <summary>
    /// Adiciona configuração de JWT
    /// </summary>
    public static IServiceCollection AddMottoSprintJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        
        // Aqui você pode adicionar a configuração de autenticação JWT
        // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddJwtBearer(options => { ... });

        return services;
    }
}