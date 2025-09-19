using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Shared.Contracts.Configuration;

/// <summary>
/// Configuração centralizada para JWT em todos os microserviços
/// </summary>
public static class JwtConfiguration
{
    public const string DefaultSecretKey = "EstacionamentoSecretKey2025!@#$%^&*()";
    public const string DefaultIssuer = "EstacionamentoAPI";
    public const string DefaultAudience = "EstacionamentoClients";

    /// <summary>
    /// Obtém a chave secreta da configuração
    /// </summary>
    public static string GetSecretKey(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        return jwtSection["SecretKey"] ?? 
               jwtSection["ChaveSecreta"] ?? 
               DefaultSecretKey;
    }

    /// <summary>
    /// Obtém o issuer da configuração
    /// </summary>
    public static string GetIssuer(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        return jwtSection["Issuer"] ?? 
               jwtSection["Emissor"] ?? 
               DefaultIssuer;
    }

    /// <summary>
    /// Obtém a audience da configuração
    /// </summary>
    public static string GetAudience(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        return jwtSection["Audience"] ?? 
               jwtSection["Audiencia"] ?? 
               DefaultAudience;
    }

    /// <summary>
    /// Configura autenticação JWT para um serviço
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = GetSecretKey(configuration);
        var issuer = GetIssuer(configuration);
        var audience = GetAudience(configuration);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        return services;
    }

    /// <summary>
    /// Obtém a chave de assinatura para criação de tokens
    /// </summary>
    public static SymmetricSecurityKey GetSigningKey(IConfiguration configuration)
    {
        var secretKey = GetSecretKey(configuration);
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }
}