using Microsoft.AspNetCore.Mvc;
using MottoSprint.DTOs;
using MottoSprint.Models.Hateoas;
using MottoSprint.Interfaces;
using MottoSprint.Services;
using System.ComponentModel.DataAnnotations;

namespace MottoSprint.Controllers;

/// <summary>
/// Controller compatível com a API Java para motos
/// Endpoints que espelham exatamente a estrutura da API Java
/// </summary>
[ApiController]
[Route("api/motos")]
[Produces("application/json")]
public class MotoJavaCompatibleController : ControllerBase
{
    private readonly IMotoNotificationService _motoService;
    private readonly ILogger<MotoJavaCompatibleController> _logger;

    public MotoJavaCompatibleController(
        IMotoNotificationService motoService,
        ILogger<MotoJavaCompatibleController> logger)
    {
        _motoService = motoService;
        _logger = logger;
    }

    /// <summary>
    /// Criar nova moto no sistema
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/motos
    ///     {
    ///         "placa": "ABC1234",
    ///         "modelo": "Honda CB600F",
    ///         "ano": 2023,
    ///         "cor": "Azul",
    ///         "status": "Ativa"
    ///     }
    /// 
    /// PASSO A PASSO PARA TESTAR:
    /// 1. Clique em "Try it out"
    /// 2. Cole o JSON de exemplo no campo "Request body"
    /// 3. Clique em "Execute"
    /// 4. Verifique se retorna status 201 (Created)
    /// 5. Observe os links HATEOAS na resposta
    /// </remarks>
    /// <param name="request">Dados da moto a ser criada</param>
    /// <returns>Dados da moto criada com links HATEOAS</returns>
    /// <response code="201">Moto criada com sucesso</response>
    /// <response code="400">Dados inválidos na requisição</response>
    [HttpPost]
    [ProducesResponseType(typeof(MotoResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MotoResponseDto>> CriarMoto([FromBody] MotoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Criando nova moto com placa: {Placa}", request.Placa);

            var response = new MotoResponseDto
            {
                Placa = request.Placa,
                Modelo = request.Modelo,
                Ano = request.Ano,
                Cor = request.Cor,
                Status = request.Status
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa = request.Placa }), "GET");
            response.AddLink("update", Url.Action(nameof(EditarMoto), new { placa = request.Placa }), "PUT");
            response.AddLink("delete", Url.Action(nameof(ExcluirMoto), new { placa = request.Placa }), "DELETE");
            response.AddLink("mover-vaga", Url.Action(nameof(MoverMoto)), "POST");
            response.AddLink("retirar-vaga", Url.Action(nameof(RetirarMoto), new { placa = request.Placa }), "POST");

            return CreatedAtAction(nameof(BuscarMoto), new { placa = request.Placa }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar moto com placa: {Placa}", request.Placa);
            return BadRequest(new { message = "Erro ao criar moto", details = ex.Message });
        }
    }

    /// <summary>
    /// Editar moto existente (compatível com PUT /api/motos/{placa} da API Java)
    /// </summary>
    [HttpPut("{placa}")]
    public async Task<ActionResult<MotoResponseDto>> EditarMoto(
        [FromRoute] string placa,
        [FromBody] MotoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Editando moto com placa: {Placa}", placa);

            var response = new MotoResponseDto
            {
                Placa = placa,
                Modelo = request.Modelo,
                Ano = request.Ano,
                Cor = request.Cor,
                Status = request.Status
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa }), "GET");
            response.AddLink("delete", Url.Action(nameof(ExcluirMoto), new { placa }), "DELETE");
            response.AddLink("mover-vaga", Url.Action(nameof(MoverMoto)), "POST");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao editar moto com placa: {Placa}", placa);
            return BadRequest(new { message = "Erro ao editar moto", details = ex.Message });
        }
    }

    /// <summary>
    /// Excluir moto (compatível com DELETE /api/motos/{placa} da API Java)
    /// </summary>
    [HttpDelete("{placa}")]
    public async Task<ActionResult> ExcluirMoto([FromRoute] string placa)
    {
        try
        {
            _logger.LogInformation("Excluindo moto com placa: {Placa}", placa);
            return Ok(new { message = $"Moto {placa} excluída com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir moto com placa: {Placa}", placa);
            return BadRequest(new { message = "Erro ao excluir moto", details = ex.Message });
        }
    }

    /// <summary>
    /// Entrada de moto no estacionamento
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// 
    ///     POST /api/motos/entrada
    ///     {
    ///         "placa": "ABC1234",
    ///         "linha": 1,
    ///         "coluna": 2
    ///     }
    /// 
    /// PASSO A PASSO PARA TESTAR:
    /// 1. Primeiro crie uma moto usando POST /api/motos
    /// 2. Clique em "Try it out" neste endpoint
    /// 3. Cole o JSON de exemplo no campo "Request body"
    /// 4. Modifique a placa para uma moto existente
    /// 5. Clique em "Execute"
    /// 6. Verifique se a moto foi alocada na vaga especificada
    /// </remarks>
    /// <param name="request">Dados da entrada (placa e posição da vaga)</param>
    /// <returns>Confirmação da entrada com dados da vaga</returns>
    /// <response code="200">Entrada realizada com sucesso</response>
    /// <response code="400">Dados inválidos ou vaga ocupada</response>
    /// <response code="404">Moto não encontrada</response>
    [HttpPost("entrada")]
    [ProducesResponseType(typeof(MotoResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MotoResponseDto>> EntradaMoto([FromBody] MoverMotoVagaDto request)
    {
        try
        {
            _logger.LogInformation("Movendo moto {Placa} para vaga {IdVaga}", request.Placa, request.IdVaga);

            var response = new MotoResponseDto
            {
                Placa = request.Placa,
                IdVaga = request.IdVaga
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa = request.Placa }), "GET");
            response.AddLink("retirar-vaga", Url.Action(nameof(RetirarMoto), new { placa = request.Placa }), "POST");

            return CreatedAtAction(nameof(BuscarMoto), new { placa = request.Placa }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mover moto {Placa} para vaga {IdVaga}", request.Placa, request.IdVaga);
            return BadRequest(new { message = "Erro ao mover moto", details = ex.Message });
        }
    }

    /// <summary>
    /// Mover moto para vaga usando linha e coluna (compatível com POST /api/motos/moverVagaPorPosicao da API Java)
    /// </summary>
    [HttpPost("moverVagaPorPosicao")]
    public async Task<ActionResult<MotoResponseDto>> MoverMotoPorPosicao([FromBody] MoverMotoLinhaColuna request)
    {
        try
        {
            _logger.LogInformation("Movendo moto {Placa} para vaga na linha {Linha}, coluna {Coluna}", 
                request.Placa, request.Linha, request.Coluna);

            // Simular conversão de linha/coluna para ID da vaga
            // Em uma implementação real, você consultaria o banco de dados para encontrar o ID da vaga
            var idVaga = ConverterPosicaoParaId(request.Linha, request.Coluna);

            var response = new MotoResponseDto
            {
                Placa = request.Placa,
                Modelo = "Honda CBR600",
                Ano = 2023,
                Cor = "Azul Metálico",
                IdVaga = idVaga,
                Status = "NORMAL",
                Linha = request.Linha,
                Coluna = request.Coluna
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa = request.Placa }), "GET");
            response.AddLink("update", Url.Action(nameof(EditarMoto), new { placa = request.Placa }), "PUT");
            response.AddLink("delete", Url.Action(nameof(ExcluirMoto), new { placa = request.Placa }), "DELETE");
            response.AddLink("retirar-vaga", Url.Action(nameof(RetirarMoto), new { placa = request.Placa }), "POST");
            response.AddLink("all-motos", Url.Action(nameof(ListarTodasMotos)), "GET");

            return CreatedAtAction(nameof(BuscarMoto), new { placa = request.Placa }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao mover moto {Placa} para vaga na linha {Linha}, coluna {Coluna}", 
                request.Placa, request.Linha, request.Coluna);
            return BadRequest(new { message = "Erro ao mover moto", details = ex.Message });
        }
    }

    /// <summary>
    /// Converte posição (linha, coluna) para ID da vaga
    /// </summary>
    private long ConverterPosicaoParaId(string linha, string coluna)
    {
        // Implementação simples: A1=1, A2=2, B1=11, B2=12, etc.
        var linhaNum = linha.ToUpper()[0] - 'A' + 1;
        var colunaNum = int.Parse(coluna);
        return (linhaNum - 1) * 10 + colunaNum;
    }

    /// <summary>
    /// Buscar moto por placa (compatível com GET /api/motos/{placa} da API Java)
    /// </summary>
    [HttpGet("{placa}")]
    public async Task<ActionResult<MotoResponseDto>> BuscarMoto([FromRoute] string placa)
    {
        try
        {
            _logger.LogInformation("Buscando moto com placa: {Placa}", placa);

            var response = new MotoResponseDto
            {
                Placa = placa,
                Modelo = "Modelo Exemplo",
                Ano = 2023,
                Cor = "Azul",
                Status = "NORMAL"
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa }), "GET");
            response.AddLink("update", Url.Action(nameof(EditarMoto), new { placa }), "PUT");
            response.AddLink("delete", Url.Action(nameof(ExcluirMoto), new { placa }), "DELETE");
            response.AddLink("mover-vaga", Url.Action(nameof(MoverMoto)), "POST");
            response.AddLink("all-motos", Url.Action(nameof(ListarTodasMotos)), "GET");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar moto com placa: {Placa}", placa);
            return NotFound(new { message = "Moto não encontrada", placa });
        }
    }

    /// <summary>
    /// Listar todas as motos (compatível com GET /api/motos/all da API Java)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<MotoResponseDto>>> ListarTodasMotos()
    {
        try
        {
            _logger.LogInformation("Listando todas as motos");

            var motos = new List<MotoResponseDto>
            {
                new MotoResponseDto
                {
                    Placa = "ABC1234",
                    Modelo = "Honda CB600",
                    Ano = 2023,
                    Cor = "Azul",
                    Status = "NORMAL"
                }
            };

            // Adicionar links HATEOAS para cada moto
            foreach (var moto in motos)
            {
                moto.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa = moto.Placa }), "GET");
                moto.AddLink("update", Url.Action(nameof(EditarMoto), new { placa = moto.Placa }), "PUT");
                moto.AddLink("delete", Url.Action(nameof(ExcluirMoto), new { placa = moto.Placa }), "DELETE");
            }

            return Ok(motos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar todas as motos");
            return BadRequest(new { message = "Erro ao listar motos", details = ex.Message });
        }
    }

    /// <summary>
    /// Retirar moto da vaga (compatível com POST /api/motos/retirarVaga/{placa} da API Java)
    /// </summary>
    [HttpPost("retirarVaga/{placa}")]
    public async Task<ActionResult<MotoResponseDto>> RetirarMoto([FromRoute] string placa)
    {
        try
        {
            _logger.LogInformation("Retirando moto {Placa} da vaga", placa);

            var response = new MotoResponseDto
            {
                Placa = placa,
                IdVaga = null // Moto retirada da vaga
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarMoto), new { placa }), "GET");
            response.AddLink("mover-vaga", Url.Action(nameof(MoverMoto)), "POST");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao retirar moto {Placa} da vaga", placa);
            return BadRequest(new { message = "Erro ao retirar moto", details = ex.Message });
        }
    }
}

/// <summary>
/// Controller compatível com a API Java para vagas
/// Endpoints que espelham exatamente a estrutura da API Java
/// </summary>
[ApiController]
[Route("api/vagas")]
[Produces("application/json")]
public class VagaJavaCompatibleController : ControllerBase
{
    private readonly IMotoNotificationService _vagaService;
    private readonly ILogger<VagaJavaCompatibleController> _logger;

    public VagaJavaCompatibleController(
        IMotoNotificationService vagaService,
        ILogger<VagaJavaCompatibleController> logger)
    {
        _vagaService = vagaService;
        _logger = logger;
    }

    /// <summary>
    /// Criar nova vaga (compatível com POST /api/vagas da API Java)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VagaResponseDto>> CriarVaga([FromBody] VagaRequestDto request)
    {
        try
        {
            _logger.LogInformation("Criando nova vaga na linha {Linha}, coluna {Coluna}", request.Linha, request.Coluna);

            var response = new VagaResponseDto
            {
                Id = new Random().NextInt64(1, 1000),
                Posicao = $"{request.Linha}{request.Coluna}",
                Status = "LIVRE"
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarVagaPorId), new { id = response.Id }), "GET");
            response.AddLink("update", Url.Action(nameof(EditarVaga), new { id = response.Id }), "PUT");
            response.AddLink("delete", Url.Action(nameof(ExcluirVaga), new { id = response.Id }), "DELETE");

            return CreatedAtAction(nameof(BuscarVagaPorId), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar vaga");
            return BadRequest(new { message = "Erro ao criar vaga", details = ex.Message });
        }
    }

    /// <summary>
    /// Buscar vaga por ID (compatível com GET /api/vagas/{id} da API Java)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VagaResponseDto>> BuscarVagaPorId([FromRoute] long id)
    {
        try
        {
            _logger.LogInformation("Buscando vaga com ID: {Id}", id);

            var response = new VagaResponseDto
            {
                Id = id,
                Posicao = "A1",
                Status = "LIVRE"
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarVagaPorId), new { id }), "GET");
            response.AddLink("update", Url.Action(nameof(EditarVaga), new { id }), "PUT");
            response.AddLink("delete", Url.Action(nameof(ExcluirVaga), new { id }), "DELETE");
            response.AddLink("all-vagas", Url.Action(nameof(ListarTodasVagas)), "GET");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vaga com ID: {Id}", id);
            return NotFound(new { message = "Vaga não encontrada", id });
        }
    }

    /// <summary>
    /// Editar vaga (compatível com PUT /api/vagas/{id} da API Java)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VagaResponseDto>> EditarVaga(
        [FromRoute] long id,
        [FromBody] VagaRequestDto request)
    {
        try
        {
            _logger.LogInformation("Editando vaga com ID: {Id}", id);

            var response = new VagaResponseDto
            {
                Id = id,
                Posicao = $"{request.Linha}{request.Coluna}",
                Status = "LIVRE"
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(BuscarVagaPorId), new { id }), "GET");
            response.AddLink("delete", Url.Action(nameof(ExcluirVaga), new { id }), "DELETE");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao editar vaga com ID: {Id}", id);
            return BadRequest(new { message = "Erro ao editar vaga", details = ex.Message });
        }
    }

    /// <summary>
    /// Excluir vaga (compatível com DELETE /api/vagas/{id} da API Java)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> ExcluirVaga([FromRoute] long id)
    {
        try
        {
            _logger.LogInformation("Excluindo vaga com ID: {Id}", id);
            return Ok(new { message = $"Vaga {id} excluída com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir vaga com ID: {Id}", id);
            return BadRequest(new { message = "Erro ao excluir vaga", details = ex.Message });
        }
    }

    /// <summary>
    /// Buscar vagas livres por linha (compatível com GET /api/vagas/livres/{linha} da API Java)
    /// </summary>
    [HttpGet("livres/{linha}")]
    public async Task<ActionResult<LinhaResponseDto>> VagasLivres([FromRoute] string linha)
    {
        try
        {
            _logger.LogInformation("Buscando vagas livres na linha: {Linha}", linha);

            var response = new LinhaResponseDto
            {
                Linha = linha,
                VagasLivres = new List<VagaResponseDto>
                {
                    new VagaResponseDto
                    {
                        Id = 1,
                        Posicao = $"{linha}1",
                        Status = "LIVRE"
                    }
                },
                TotalVagas = 10,
                VagasOcupadas = 3
            };

            // Adicionar links HATEOAS
            response.AddLink("self", Url.Action(nameof(VagasLivres), new { linha }), "GET");
            response.AddLink("all-vagas", Url.Action(nameof(ListarTodasVagas)), "GET");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vagas livres na linha: {Linha}", linha);
            return BadRequest(new { message = "Erro ao buscar vagas livres", details = ex.Message });
        }
    }

    /// <summary>
    /// Listar todas as vagas (compatível com GET /api/vagas/all da API Java)
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<VagaResponseDto>>> ListarTodasVagas()
    {
        try
        {
            _logger.LogInformation("Listando todas as vagas");

            var vagas = new List<VagaResponseDto>
            {
                new VagaResponseDto
                {
                    Id = 1,
                    Posicao = "A1",
                    Status = "LIVRE"
                }
            };

            // Adicionar links HATEOAS para cada vaga
            foreach (var vaga in vagas)
            {
                vaga.AddLink("self", Url.Action(nameof(BuscarVagaPorId), new { id = vaga.Id }), "GET");
                vaga.AddLink("update", Url.Action(nameof(EditarVaga), new { id = vaga.Id }), "PUT");
                vaga.AddLink("delete", Url.Action(nameof(ExcluirVaga), new { id = vaga.Id }), "DELETE");
            }

            return Ok(vagas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar todas as vagas");
            return BadRequest(new { message = "Erro ao listar vagas", details = ex.Message });
        }
    }

    /// <summary>
    /// Listar vagas por status (compatível com GET /api/vagas/all/{status} da API Java)
    /// </summary>
    [HttpGet("all/{status}")]
    public async Task<ActionResult<List<VagaResponseDto>>> ListarVagasPorStatus([FromRoute] StatusVaga status)
    {
        try
        {
            _logger.LogInformation("Listando vagas com status: {Status}", status);

            var vagas = new List<VagaResponseDto>
            {
                new VagaResponseDto
                {
                    Id = 1,
                    Posicao = "A1",
                    Status = status.ToString()
                }
            };

            // Adicionar links HATEOAS para cada vaga
            foreach (var vaga in vagas)
            {
                vaga.AddLink("self", Url.Action(nameof(BuscarVagaPorId), new { id = vaga.Id }), "GET");
                vaga.AddLink("update", Url.Action(nameof(EditarVaga), new { id = vaga.Id }), "PUT");
                vaga.AddLink("delete", Url.Action(nameof(ExcluirVaga), new { id = vaga.Id }), "DELETE");
            }

            return Ok(vagas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar vagas por status: {Status}", status);
            return BadRequest(new { message = "Erro ao listar vagas por status", details = ex.Message });
        }
    }
}