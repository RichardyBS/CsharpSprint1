// Controller de Análise - onde os números fazem sentido (ou não)
// HACK: esse controller tá fazendo muita coisa, depois refatoro... talvez
using Analytics.Service.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Service.Controllers;

[ApiController]
[Route("api/analise")] // mudei pra português, mais brasileiro né
[Authorize] // sem token não rola, segurança first
public class ControladorAnalise : ControllerBase
{
    private readonly AnalyticsDbContext _contexto;
    private readonly ILogger<ControladorAnalise> _logger;

    public ControladorAnalise(AnalyticsDbContext contexto, ILogger<ControladorAnalise> logger)
    {
        _contexto = contexto;
        _logger = logger; // logger que ninguém olha mas tem que ter
    }

    [HttpGet("painel")] // dashboard em português é painel né
    public async Task<IActionResult> ObterPainel()
    {
        // Pegando as datas - matemática básica que até eu entendo
        var hoje = DateTime.Today;
        var ontem = hoje.AddDays(-1);
        var ultimoMes = hoje.AddDays(-30);

        // Montando as métricas - aqui que a mágica acontece
        // TODO: cachear isso aqui, tá muito lento pra consultar toda vez
        var metricas = new
        {
            Hoje = await ObterMetricasDia(hoje),
            Ontem = await ObterMetricasDia(ontem), // pra comparar se melhorou ou piorou
            UltimoMes = await ObterMetricasPeriodo(ultimoMes, hoje),
            VagasOcupadasAgora = await _contexto.OcupacoesVagas
                .CountAsync(o => o.Status == "Ocupada"), // quantas vagas tão ocupadas agora
            TopClientes = await ObterTopClientes(ultimoMes) // os que mais gastam $$$ 
        };

        return Ok(metricas); // retorna tudo bonitinho em JSON
    }

    [HttpGet("metricas-diarias")]
    public async Task<IActionResult> ObterMetricasDiarias([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        // Se não passou data, pega os últimos 30 dias - padrão razoável
        var inicio = dataInicio ?? DateTime.Today.AddDays(-30);
        var fim = dataFim ?? DateTime.Today;

        // WARN: se o período for muito grande, vai demorar pra caramba
        var metricas = await _contexto.MetricasDiarias
            .Where(m => m.Data >= inicio && m.Data <= fim)
            .OrderBy(m => m.Data) // ordenado por data, óbvio
            .ToListAsync();

        return Ok(metricas); // retorna as métricas do período
    }

    [HttpGet("ocupacoes")]
    public async Task<IActionResult> ObterOcupacoes([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50)
    {
        // Query básica - depois vou otimizar (mentira, nunca vou)
        var consulta = _contexto.OcupacoesVagas.AsQueryable();

        // Filtro por status se foi passado
        if (!string.IsNullOrEmpty(status))
        {
            consulta = consulta.Where(o => o.Status == status);
        }

        // Contando total - necessário pra paginação
        var totalItens = await consulta.CountAsync();
        
        // HACK: limitando pageSize pra não quebrar o servidor
        if (tamanhoPagina > 100) tamanhoPagina = 100;
        
        var ocupacoes = await consulta
            .Skip((pagina - 1) * tamanhoPagina) // pula as páginas anteriores
            .Take(tamanhoPagina) // pega só o que precisa
            .OrderByDescending(o => o.DataEntrada) // mais recentes primeiro
            .ToListAsync();

        // Montando o resultado paginado - padrão da casa
        var resultado = new
        {
            Itens = ocupacoes,
            TotalItens = totalItens,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalPaginas = (int)Math.Ceiling((double)totalItens / tamanhoPagina)
        };

        return Ok(resultado); // retorna paginado bonitinho
    }

    [HttpGet("relatorio-receita")]
    public async Task<IActionResult> ObterRelatorioReceita([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        // Período padrão: últimos 30 dias - quem não passa data não reclama depois
        var inicio = dataInicio ?? DateTime.Today.AddDays(-30);
        var fim = dataFim ?? DateTime.Today;

        // Agrupando por mês - matemática que até o estagiário entende
        // TODO: adicionar cache aqui, essa query é pesada
        var receitas = await _contexto.MetricasDiarias
            .Where(m => m.Data >= inicio && m.Data <= fim)
            .GroupBy(m => new { m.Data.Year, m.Data.Month }) // agrupando por ano/mês
            .Select(g => new
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month,
                ReceitaTotal = g.Sum(m => m.ReceitaTotal), // soma tudo
                TicketMedio = g.Average(m => m.TicketMedio), // média do período
                TotalOcupacoes = g.Sum(m => m.TotalOcupacoes) // quantas vagas foram usadas
            })
            .OrderBy(r => r.Ano) // ordenado cronologicamente
            .ThenBy(r => r.Mes)
            .ToListAsync();

        return Ok(receitas); // relatório de receita pronto pra impressão
    }

    // Método privado pra pegar métricas de um dia específico
    // HACK: deveria estar numa service layer, mas tá aqui mesmo
    private async Task<object> ObterMetricasDia(DateTime data)
    {
        // Buscando todas as ocupações do dia - query simples
        var ocupacoes = await _contexto.OcupacoesVagas
            .Where(o => o.DataEntrada.Date == data.Date)
            .ToListAsync();

        // Fazendo as contas - matemática básica do ensino médio
        var totalOcupacoes = ocupacoes.Count;
        var ocupacoesLiberadas = ocupacoes.Count(o => o.Status == "Liberada");
        var receitaTotal = ocupacoes.Where(o => o.Status == "Liberada").Sum(o => o.ValorCobrado ?? 0);
        var ticketMedio = ocupacoesLiberadas > 0 ? receitaTotal / ocupacoesLiberadas : 0; // evita divisão por zero

        return new
        {
            Data = data,
            TotalOcupacoes = totalOcupacoes,
            OcupacoesLiberadas = ocupacoesLiberadas,
            ReceitaTotal = receitaTotal,
            TicketMedio = ticketMedio // quanto cada cliente gastou em média
        };
    }

    // Métricas de um período - pra quando quiser ver um range de datas
    private async Task<object> ObterMetricasPeriodo(DateTime inicio, DateTime fim)
    {
        // Buscando ocupações do período - pode ser pesado se for muito tempo
        var ocupacoes = await _contexto.OcupacoesVagas
            .Where(o => o.DataEntrada >= inicio && o.DataEntrada <= fim)
            .ToListAsync();

        // Calculando as métricas do período todo
        var totalOcupacoes = ocupacoes.Count;
        var ocupacoesLiberadas = ocupacoes.Count(o => o.Status == "Liberada");
        var receitaTotal = ocupacoes.Where(o => o.Status == "Liberada").Sum(o => o.ValorCobrado ?? 0);
        var ticketMedio = ocupacoesLiberadas > 0 ? receitaTotal / ocupacoesLiberadas : 0;

        return new
        {
            Periodo = $"{inicio:dd/MM/yyyy} - {fim:dd/MM/yyyy}", // formatado bonitinho
            TotalOcupacoes = totalOcupacoes,
            OcupacoesLiberadas = ocupacoesLiberadas,
            ReceitaTotal = receitaTotal,
            TicketMedio = ticketMedio
        };
    }

    // Top clientes que mais gastam - pra saber quem são os VIPs
    // TODO: cachear isso, roda toda hora no dashboard
    private async Task<List<object>> ObterTopClientes(DateTime dataInicio)
    {
        return await _contexto.OcupacoesVagas
            .Where(o => o.DataEntrada >= dataInicio && o.Status == "Liberada") // só os que pagaram
            .GroupBy(o => o.ClienteId) // agrupando por cliente
            .Select(g => new
            {
                ClienteId = g.Key,
                TotalGasto = g.Sum(o => o.ValorCobrado ?? 0), // quanto gastou no total
                TotalOcupacoes = g.Count(), // quantas vezes usou
                TicketMedio = g.Average(o => o.ValorCobrado ?? 0) // média por uso
            })
            .OrderByDescending(c => c.TotalGasto) // os que mais gastam primeiro
            .Take(10) // top 10 é suficiente
            .Cast<object>()
            .ToListAsync();
    }
}