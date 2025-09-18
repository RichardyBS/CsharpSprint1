// Serviço de Cobrança - onde o dinheiro entra (ou deveria entrar) 💰
// HACK: esse serviço tá meio gambiarra, mas funciona... na maioria das vezes
using Billing.Service.Data;
using Billing.Service.Handlers;
using Billing.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using Shared.EventBus;
using System.Text;

var construtorApp = WebApplication.CreateBuilder(args);

// Adicionando serviços no container - DI que funciona de vez em quando
construtorApp.Services.AddControllers(); // controllers básicos
construtorApp.Services.AddEndpointsApiExplorer(); // pra documentação que ninguém lê
construtorApp.Services.AddSwaggerGen(); // swagger é vida

// Oracle Database - agora usando banco da FIAP
var stringConexao = construtorApp.Configuration.GetConnectionString("ConexaoPadrao");
construtorApp.Services.AddDbContext<BillingDbContext>(opcoes =>
    opcoes.UseOracle(stringConexao));

// Serviços customizados - DI que funciona (às vezes)
construtorApp.Services.AddScoped<IFaturamentoService, FaturamentoService>();

// JWT Authentication - segurança que ninguém entende direito
var configuracoesJwt = construtorApp.Configuration.GetSection("ConfiguracoesJwt");
var chaveSecreta = Encoding.ASCII.GetBytes(configuracoesJwt["ChaveSecreta"]!);
// WARN: se a chave secreta vazar, tamo ferrado

construtorApp.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // valida quem emitiu
            ValidateAudience = true, // valida pra quem é
            ValidateLifetime = true, // valida se não expirou
            ValidateIssuerSigningKey = true, // valida a assinatura
            ValidIssuer = configuracoesJwt["Emissor"] ?? "CobrancaEstacionamentoAPI",
            ValidAudience = configuracoesJwt["Audiencia"] ?? "ClientesCobranca",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta))
        };
    });

// Event Bus com RabbitMQ - mensageria que funciona (quando o rabbit não cai)
// TODO: adicionar retry policy quando der erro (vai dar erro, sempre dá)
construtorApp.Services.Configure<OpcoesRabbitMQ>(construtorApp.Configuration.GetSection("RabbitMQ"));
construtorApp.Services.AddSingleton<IBarramentoEventos, BarramentoEventosRabbitMQ>();

// Handlers de eventos - aqui que escutamos quando liberam vaga
construtorApp.Services.AddScoped<VagaLiberadaEventHandler>(); // handler pra quando vaga é liberada

// Health Checks - pra saber se tá vivo ou morto
construtorApp.Services.AddHealthChecks()
    .AddDbContextCheck<BillingDbContext>(); // verifica se o Oracle tá respondendo

// CORS liberado pra geral - segurança é overrated mesmo né
construtorApp.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("LiberaGeral", politica =>
    {
        politica.AllowAnyOrigin() // qualquer origem
              .AllowAnyMethod() // qualquer método HTTP
              .AllowAnyHeader(); // qualquer header
    });
});

var aplicacao = construtorApp.Build(); // constrói a aplicação

// Configurando o pipeline HTTP - ordem importa, não mexe!
if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger(); // documentação automática
    aplicacao.UseSwaggerUI(); // interface bonitinha do swagger
}

aplicacao.UseHttpsRedirection(); // força HTTPS porque segurança
aplicacao.UseCors("LiberaGeral"); // aplica a política de CORS

aplicacao.UseAuthentication(); // autenticação primeiro
aplicacao.UseAuthorization(); // autorização depois

aplicacao.MapControllers(); // mapeia os controllers
aplicacao.MapHealthChecks("/saude"); // endpoint de health check em português

// Inscrevendo nos eventos - aqui que a coisa fica interessante
// WARN: se der exception aqui, o serviço não sobe
var barramento = aplicacao.Services.GetRequiredService<IBarramentoEventos>();
var provedorServicos = aplicacao.Services;

barramento.Inscrever<EventoVagaLiberada>(async (evento) =>
{
    // Criando escopo pra resolver dependências - DI 101
    using var escopo = provedorServicos.CreateScope();
    var manipulador = escopo.ServiceProvider.GetRequiredService<VagaLiberadaEventHandler>();
    await manipulador.Handle(evento); // processa o evento de vaga liberada
});

// Inicializando o banco - garantindo que tá tudo configurado
// TODO: migrar pra migrations quando tiver paciência
using (var escopo = aplicacao.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<BillingDbContext>();
    await contexto.InitializeAsync(); // inicializa as collections do mongo
}

aplicacao.Run(); // roda a aplicação - aqui que a mágica acontece