using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MottoSprint.Data;
using MottoSprint.Services;

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

        // Serviços de negócio
        services.AddScoped<IParkingService, ParkingService>();
        services.AddScoped<IMotoNotificationService, MotoNotificationService>();
        services.AddScoped<MottoSprint.Interfaces.INotificationService, MottoSprint.Services.NotificationService>();

        return services;
    }

    /// <summary>
    /// Adiciona e configura o Entity Framework
    /// </summary>
    public static IServiceCollection AddMottoSprintDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() 
            ?? new DatabaseOptions();

        services.AddDbContext<MottoSprintDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection") ?? databaseOptions.DefaultConnection,
                sqlOptions =>
                {
                    if (databaseOptions.EnableRetryOnFailure)
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: databaseOptions.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(databaseOptions.MaxRetryDelaySeconds),
                            errorNumbersToAdd: null);
                    }

                    sqlOptions.CommandTimeout(databaseOptions.CommandTimeoutSeconds);
                });

            if (databaseOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Adicionar também o TodoDbContext para compatibilidade
        services.AddDbContext<TodoDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection") ?? databaseOptions.DefaultConnection);
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