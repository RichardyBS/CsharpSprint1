using System.Text;
using System.Text.Json;
using MottoSprint.Models;

namespace MottoSprint.Services;

/// <summary>
/// Serviço para integração com a API Java
/// </summary>
public interface IJavaApiService
{
    Task<IEnumerable<Moto>> GetMotosAsync();
    Task<Moto?> GetMotoByPlacaAsync(string placa);
    Task<Moto?> CreateMotoAsync(CreateMotoRequest request);
    Task<Moto?> UpdateMotoAsync(string placa, CreateMotoRequest request);
    Task<bool> DeleteMotoAsync(string placa);
    Task<bool> MoverMotoAsync(MoverMotoRequest request);
    
    Task<IEnumerable<Vaga>> GetVagasAsync();
    Task<Vaga?> GetVagaByIdAsync(int id);
    Task<Vaga?> CreateVagaAsync(CreateVagaRequest request);
    Task<Vaga?> UpdateVagaAsync(int id, CreateVagaRequest request);
    Task<bool> DeleteVagaAsync(int id);
    Task<IEnumerable<Vaga>> GetVagasLivresByLinhaAsync(string linha);
}

public class JavaApiService : IJavaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JavaApiService> _logger;
    private readonly string _baseUrl;

    public JavaApiService(HttpClient httpClient, IConfiguration configuration, ILogger<JavaApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = "http://your-java-api-url:8080/api";
        
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    #region Motos

    public async Task<IEnumerable<Moto>> GetMotosAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/motos");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Moto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Moto>();
            }
            return new List<Moto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar motos da API Java");
            return new List<Moto>();
        }
    }

    public async Task<Moto?> GetMotoByPlacaAsync(string placa)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/motos/{placa}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Moto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar moto {Placa} da API Java", placa);
            return null;
        }
    }

    public async Task<Moto?> CreateMotoAsync(CreateMotoRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/motos", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Moto>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar moto na API Java");
            return null;
        }
    }

    public async Task<Moto?> UpdateMotoAsync(string placa, CreateMotoRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/motos/{placa}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Moto>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar moto {Placa} na API Java", placa);
            return null;
        }
    }

    public async Task<bool> DeleteMotoAsync(string placa)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/motos/{placa}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar moto {Placa} na API Java", placa);
            return false;
        }
    }

    public async Task<bool> MoverMotoAsync(MoverMotoRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/motos/moverVaga", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mover moto na API Java");
            return false;
        }
    }

    #endregion

    #region Vagas

    public async Task<IEnumerable<Vaga>> GetVagasAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/vagas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Vaga>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Vaga>();
            }
            return new List<Vaga>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vagas da API Java");
            return new List<Vaga>();
        }
    }

    public async Task<Vaga?> GetVagaByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/vagas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Vaga>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vaga {Id} da API Java", id);
            return null;
        }
    }

    public async Task<Vaga?> CreateVagaAsync(CreateVagaRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/vagas", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Vaga>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar vaga na API Java");
            return null;
        }
    }

    public async Task<Vaga?> UpdateVagaAsync(int id, CreateVagaRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/vagas/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Vaga>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar vaga {Id} na API Java", id);
            return null;
        }
    }

    public async Task<bool> DeleteVagaAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/vagas/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar vaga {Id} na API Java", id);
            return false;
        }
    }

    public async Task<IEnumerable<Vaga>> GetVagasLivresByLinhaAsync(string linha)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/vagas/livres/{linha}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Vaga>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Vaga>();
            }
            return new List<Vaga>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vagas livres da linha {Linha} da API Java", linha);
            return new List<Vaga>();
        }
    }

    #endregion
}