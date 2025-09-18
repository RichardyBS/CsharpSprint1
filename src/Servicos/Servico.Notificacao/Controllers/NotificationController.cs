// Controller das notificações - onde os avisos voam que nem WhatsApp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Service.Models;
using Notification.Service.Services;

namespace Notification.Service.Controllers;

[ApiController]
[Route("api/notificacoes")] // rota brasileirizada
[Authorize] // só entra quem tem token
public class ControladorNotificacao : ControllerBase
{
    private readonly IRedisCacheService _servicoCache;
    private readonly INotificationService _servicoNotificacao;
    private readonly ILogger<ControladorNotificacao> _logger;

    public ControladorNotificacao(
        IRedisCacheService servicoCache,
        INotificationService servicoNotificacao,
        ILogger<ControladorNotificacao> logger)
    {
        _servicoCache = servicoCache; // cache que salva a vida
        _servicoNotificacao = servicoNotificacao; // serviço que manda as notificações
        _logger = logger; // logger pra debugar quando der ruim
    }

    [HttpGet("{clienteId:guid}")]
    public async Task<IActionResult> ObterNotificacoes(Guid clienteId)
    {
        // Busca as notificações do cliente - tipo inbox do email
        var notificacoes = await _servicoCache.GetNotificacoesPorClienteAsync(clienteId);
        var naoLidas = await _servicoCache.GetCountNotificacoesNaoLidasAsync(clienteId);

        return Ok(new
        {
            Notificacoes = notificacoes, // lista das notificações
            TotalNaoLidas = naoLidas // quantas ainda não foram lidas
        });
    }

    [HttpGet("{clienteId:guid}/nao-lidas")]
    public async Task<IActionResult> ObterContadorNaoLidas(Guid clienteId)
    {
        // Conta quantas notificações não foram lidas - tipo badge do WhatsApp
        var contador = await _servicoCache.GetCountNotificacoesNaoLidasAsync(clienteId);
        return Ok(new { Contador = contador });
    }

    [HttpPut("{notificacaoId:guid}/marcar-lida")]
    public async Task<IActionResult> MarcarComoLida(Guid notificacaoId)
    {
        // Marca a notificação como lida - tipo "visto" do WhatsApp
        await _servicoCache.MarcarComoLidaAsync(notificacaoId);
        return Ok(new { Mensagem = "Notificação marcada como lida - show de bola!" });
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> EnviarNotificacao([FromBody] SolicitacaoEnviarNotificacao solicitacao)
    {
        try
        {
            // Envia a notificação - aqui que a mágica acontece
            await _servicoNotificacao.EnviarNotificacaoAsync(
                solicitacao.ClienteId,
                solicitacao.Titulo,
                solicitacao.Mensagem,
                solicitacao.Tipo,
                solicitacao.Dados);

            return Ok(new { Mensagem = "Notificação enviada com sucesso - voou!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação - deu ruim");
            return StatusCode(500, new { Mensagem = "Erro interno do servidor - algo quebrou" });
        }
    }

    [HttpGet("configuracao/{clienteId:guid}")]
    public async Task<IActionResult> ObterConfiguracao(Guid clienteId)
    {
        // Pega as configurações do cliente - tipo preferências do usuário
        var configuracao = await _servicoCache.GetConfiguracaoAsync(clienteId);
        return Ok(configuracao);
    }

    [HttpPost("configuracao")]
    public async Task<IActionResult> SalvarConfiguracao([FromBody] ConfiguracaoNotificacao configuracao)
    {
        // Salva as configurações - tipo "salvar preferências"
        await _servicoCache.SetConfiguracaoAsync(configuracao);
        return Ok(new { Mensagem = "Configuração salva com sucesso - tudo certo!" });
    }

    [HttpDelete("{notificacaoId:guid}")]
    public async Task<IActionResult> DeletarNotificacao(Guid notificacaoId)
    {
        // Deleta a notificação - tipo "apagar conversa"
        await _servicoCache.DeleteNotificacaoAsync(notificacaoId);
        return Ok(new { Mensagem = "Notificação deletada com sucesso - sumiu!" });
    }

    [HttpPost("teste")]
    public async Task<IActionResult> TestarNotificacao([FromBody] SolicitacaoTesteNotificacao solicitacao)
    {
        try
        {
            // Envia uma notificação de teste - pra ver se tá funcionando
            var notificacaoTeste = new Notificacao
            {
                Id = Guid.NewGuid(),
                ClienteId = solicitacao.ClienteId,
                Titulo = "Teste de Notificação - Oi, funciona!",
                Mensagem = solicitacao.Mensagem,
                Tipo = "teste",
                CriadaEm = DateTime.UtcNow,
                Lida = false
            };
            
            await _servicoNotificacao.EnviarNotificacaoAsync(notificacaoTeste);

            return Ok(new { Mensagem = "Notificação de teste enviada com sucesso - chegou!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar notificação de teste - falhou no teste");
            return StatusCode(500, new { Mensagem = "Erro interno do servidor - teste deu ruim" });
        }
    }
}

// Classe pra solicitar envio de notificação - tipo formulário de contato
public class SolicitacaoEnviarNotificacao
{
    public Guid ClienteId { get; set; } // quem vai receber
    public string Titulo { get; set; } = string.Empty; // assunto da parada
    public string Mensagem { get; set; } = string.Empty; // o que vai falar
    public string Tipo { get; set; } = string.Empty; // que tipo de notificação
    public Dictionary<string, object>? Dados { get; set; } // dados extras se precisar
}

// Classe pra testar notificação - tipo "enviar mensagem de teste"
public class SolicitacaoTesteNotificacao
{
    public Guid ClienteId { get; set; } // quem vai receber o teste
    public string Mensagem { get; set; } = string.Empty; // mensagem do teste
}