using Notification.Service.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Notification.Service.Services;

public interface IRedisCacheService
{
    Task<bool> SetNotificacaoAsync(Notificacao notificacao, TimeSpan? expiry = null);
    Task<Notificacao?> GetNotificacaoAsync(Guid id);
    Task<List<Notificacao>> GetNotificacoesPorClienteAsync(Guid clienteId);
    Task<bool> DeleteNotificacaoAsync(Guid id);
    Task<bool> SetConfiguracaoAsync(ConfiguracaoNotificacao config);
    Task<ConfiguracaoNotificacao?> GetConfiguracaoAsync(Guid clienteId);
    Task<long> GetCountNotificacoesNaoLidasAsync(Guid clienteId);
    Task<bool> MarcarComoLidaAsync(Guid notificacaoId);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<bool> SetNotificacaoAsync(Notificacao notificacao, TimeSpan? expiry = null)
    {
        try
        {
            var key = $"notificacao:{notificacao.Id}";
            var clienteKey = $"cliente:{notificacao.ClienteId}:notificacoes";
            
            var json = JsonSerializer.Serialize(notificacao, _jsonOptions);
            
            // Salvar notificação individual
            var result = await _database.StringSetAsync(key, json, expiry);
            
            // Adicionar à lista do cliente
            await _database.ListLeftPushAsync(clienteKey, notificacao.Id.ToString());
            
            // Manter apenas as últimas 100 notificações por cliente
            await _database.ListTrimAsync(clienteKey, 0, 99);
            
            // Incrementar contador de não lidas se for nova notificação
            if (notificacao.Status == "Pendente")
            {
                var contadorKey = $"cliente:{notificacao.ClienteId}:nao_lidas";
                await _database.StringIncrementAsync(contadorKey);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar notificação {NotificacaoId} no Redis", notificacao.Id);
            return false;
        }
    }

    public async Task<Notificacao?> GetNotificacaoAsync(Guid id)
    {
        try
        {
            var key = $"notificacao:{id}";
            var json = await _database.StringGetAsync(key);
            
            if (!json.HasValue)
                return null;
                
            return JsonSerializer.Deserialize<Notificacao>(json!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificação {NotificacaoId} no Redis", id);
            return null;
        }
    }

    public async Task<List<Notificacao>> GetNotificacoesPorClienteAsync(Guid clienteId)
    {
        try
        {
            var clienteKey = $"cliente:{clienteId}:notificacoes";
            var notificacaoIds = await _database.ListRangeAsync(clienteKey, 0, 49); // Últimas 50
            
            var notificacoes = new List<Notificacao>();
            
            foreach (var id in notificacaoIds)
            {
                if (Guid.TryParse(id, out var notificacaoId))
                {
                    var notificacao = await GetNotificacaoAsync(notificacaoId);
                    if (notificacao != null)
                    {
                        notificacoes.Add(notificacao);
                    }
                }
            }
            
            return notificacoes.OrderByDescending(n => n.CriadaEm).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar notificações do cliente {ClienteId} no Redis", clienteId);
            return new List<Notificacao>();
        }
    }

    public async Task<bool> DeleteNotificacaoAsync(Guid id)
    {
        try
        {
            var key = $"notificacao:{id}";
            return await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar notificação {NotificacaoId} no Redis", id);
            return false;
        }
    }

    public async Task<bool> SetConfiguracaoAsync(ConfiguracaoNotificacao config)
    {
        try
        {
            var key = $"config:{config.ClienteId}";
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            return await _database.StringSetAsync(key, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configuração do cliente {ClienteId} no Redis", config.ClienteId);
            return false;
        }
    }

    public async Task<ConfiguracaoNotificacao?> GetConfiguracaoAsync(Guid clienteId)
    {
        try
        {
            var key = $"config:{clienteId}";
            var json = await _database.StringGetAsync(key);
            
            if (!json.HasValue)
                return null;
                
            return JsonSerializer.Deserialize<ConfiguracaoNotificacao>(json!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar configuração do cliente {ClienteId} no Redis", clienteId);
            return null;
        }
    }

    public async Task<long> GetCountNotificacoesNaoLidasAsync(Guid clienteId)
    {
        try
        {
            var contadorKey = $"cliente:{clienteId}:nao_lidas";
            var count = await _database.StringGetAsync(contadorKey);
            return count.HasValue ? (long)count : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contador de não lidas do cliente {ClienteId}", clienteId);
            return 0;
        }
    }

    public async Task<bool> MarcarComoLidaAsync(Guid notificacaoId)
    {
        try
        {
            var notificacao = await GetNotificacaoAsync(notificacaoId);
            if (notificacao == null)
                return false;
                
            // Decrementar contador se estava como não lida
            if (notificacao.Status == "Pendente")
            {
                var contadorKey = $"cliente:{notificacao.ClienteId}:nao_lidas";
                await _database.StringDecrementAsync(contadorKey);
            }
            
            // Atualizar status da notificação
            notificacao.Status = "Lida";
            return await SetNotificacaoAsync(notificacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação {NotificacaoId} como lida", notificacaoId);
            return false;
        }
    }
}