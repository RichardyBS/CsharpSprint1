// Serviço de Análise - onde a mágica dos dados acontece 📊
// TODO: refatorar isso aqui quando tiver tempo (spoiler: nunca vou ter)
using Analytics.Service.Data;
using Analytics.Service.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Events;
using Shared.EventBus;
using System.Text;

var construtorApp = WebApplication.CreateBuilder(args);

// Adicionando os bagulho no container - DI é vida mesmo
construtorApp.Services.AddControllers();
construtorApp.Services.AddEndpointsApiExplorer();
construtorApp.Services.AddSwaggerGen(); // documentação que ninguém lê mas tem que ter

// Banco PostgreSQL - porque MySQL é mainstream demais
// Configuração do Oracle - agora usando banco da FIAP
construtorApp.Services.AddDbContext<AnalyticsDbContext>(opcoes =>
    opcoes.UseOracle(construtorApp.Configuration.GetConnectionString("ConexaoPadrao")));

// JWT pra não deixar qualquer um mexer nos dados
// HACK: copiado do gateway, se funciona lá vai funcionar aqui né
var configuracoesJwt = construtorApp.Configuration.GetSection("JwtSettings");
var chaveSecreta = configuracoesJwt["SecretKey"] ?? "EstacionamentoSecretKey2025!@#$%^&*()";

construtorApp.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        // Validações do token - copiei do Stack Overflow e funcionou 🤷‍♂️
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // token eterno é coisa de hacker
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuracoesJwt["Issuer"] ?? "EstacionamentoAPI",
            ValidAudience = configuracoesJwt["Audience"] ?? "EstacionamentoClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta))
        };
    });

// Event Bus com RabbitMQ - mensageria é o futuro (ou não)
// NOTE: se o RabbitMQ cair, tudo para de funcionar... ops
construtorApp.Services.Configure<OpcoesRabbitMQ>(construtorApp.Configuration.GetSection("RabbitMQ"));
construtorApp.Services.AddSingleton<IBarramentoEventos, BarramentoEventosRabbitMQ>();

// Handlers dos eventos - aqui que a coisa acontece de verdade
// TODO: adicionar retry policy quando der erro (vai dar erro, sempre dá)
construtorApp.Services.AddScoped<VagaOcupadaEventHandler>();
construtorApp.Services.AddScoped<VagaLiberadaEventHandler>();

// Health checks pra saber se tá vivo ou morto
construtorApp.Services.AddHealthChecks()
    .AddDbContext<AnalyticsDbContext>();

// CORS liberado pra geral - segurança é overrated mesmo né
// FIXME: restringir isso em produção (se um dia chegar lá)
construtorApp.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("LiberaGeral", politica =>
    {
        politica.AllowAnyOrigin()      // qualquer um pode acessar
              .AllowAnyMethod()        // GET, POST, DELETE, tanto faz
              .AllowAnyHeader();       // headers? que headers?
    });
});

var aplicacao = construtorApp.Build();

// Configurando o pipeline - ordem importa, não mexe!
if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger();    // documentação bonitinha
    aplicacao.UseSwaggerUI();  // interface que ninguém usa mas fica bonito
}

aplicacao.UseHttpsRedirection(); // HTTPS obrigatório, HTTP é coisa do passado
aplicacao.UseCors("LiberaGeral"); // CORS que configuramos ali em cima

aplicacao.UseAuthentication(); // primeiro autentica
aplicacao.UseAuthorization();  // depois autoriza (ordem é tudo!)

aplicacao.MapControllers();
aplicacao.MapHealthChecks("/saude"); // mudei pra português, mais brasileiro né

// Inscrevendo nos eventos - aqui que a mágica acontece
// WARN: se der exception aqui, o serviço não sobe
var barramento = aplicacao.Services.GetRequiredService<IBarramentoEventos>();
var provedorServicos = aplicacao.Services;

// Quando uma vaga é ocupada - $$$ começa a contar
barramento.Inscrever<EventoVagaOcupada>(async (evento) =>
{
    using var escopo = provedorServicos.CreateScope();
    var manipulador = escopo.ServiceProvider.GetRequiredService<VagaOcupadaEventHandler>();
    await manipulador.Handle(evento); // se der pau aqui, boa sorte debugando
});

// Quando uma vaga é liberada - $$$ para de contar
barramento.Inscrever<EventoVagaLiberada>(async (evento) =>
{
    using var escopo = provedorServicos.CreateScope();
    var manipulador = escopo.ServiceProvider.GetRequiredService<VagaLiberadaEventHandler>();
    await manipulador.Handle(evento); // mesma coisa aqui
});

// Garantindo que o banco existe - se não existir, cria na marra
// TODO: migrar pra migrations quando tiver paciência
using (var escopo = aplicacao.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    contexto.Database.EnsureCreated(); // força a criação do banco
}

aplicacao.Run(); // BORA ANALISAR ESSES DADOS! 📊🚀