namespace MottoSprint.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// String de conexão padrão
    /// </summary>
    public string DefaultConnection { get; set; } = string.Empty;

    /// <summary>
    /// Timeout para comandos do banco em segundos
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Habilitar logging detalhado do EF Core
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Habilitar retry automático em caso de falha
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>
    /// Número máximo de tentativas de retry
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Delay máximo entre tentativas de retry em segundos
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;
}