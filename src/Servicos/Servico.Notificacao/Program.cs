// Notification Service - onde as notificações voam que nem pombo correio
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Notification.Service.Handlers;
using Notification.Service.Hubs;
using Notification.Service.Services;
using Shared.Contracts.Events;
using Shared.EventBus;
using StackExchange.Redis;
using System.Text;

var construtorApp = WebApplication.CreateBuilder(args);

// Adicionando serviços no container - DI que funciona na base do "se correr o bicho pega"
construtorApp.Services.AddControllers(); // controllers básicos
construtorApp.Services.AddEndpointsApiExplorer(); // explorador de endpoints
construtorApp.Services.AddSwaggerGen(); // swagger pra documentar (que ninguém lê)

// Redis - cache que salva a vida (quando não tá fora do ar)
var stringConexaoRedis = construtorApp.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
construtorApp.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(stringConexaoRedis)); // conecta no redis local

// Serviços maneiros - onde a mágica das notificações acontece
construtorApp.Services.AddScoped<IRedisCacheService, RedisCacheService>(); // cache service que é top
construtorApp.Services.AddScoped<INotificationService, NotificationService>(); // serviço de notificação raiz

// SignalR - websockets que funcionam (na teoria)
construtorApp.Services.AddSignalR(); // real-time que é sucesso

// JWT Authentication - segurança que é show de bola
var configuracoesJwt = construtorApp.Configuration.GetSection("JwtSettings");
var chaveSecreta = configuracoesJwt["SecretKey"] ?? "EstacionamentoSecretKey2025!@#$%^&*()";
// WARN: essa chave aí não pode vazar, senão é GG

construtorApp.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // valida quem emitiu
            ValidateAudience = true, // valida pra quem é
            ValidateLifetime = true, // valida se não expirou
            ValidateIssuerSigningKey = true, // valida a assinatura
            ValidIssuer = configuracoesJwt["Issuer"] ?? "EstacionamentoAPI",
            ValidAudience = configuracoesJwt["Audience"] ?? "EstacionamentoClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta))
        };

        // Configuração JWT pro SignalR - gambiarra que funciona
        opcoes.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexto =>
            {
                var tokenAcesso = contexto.Request.Query["access_token"];
                var caminho = contexto.HttpContext.Request.Path;
                
                // Se tem token e tá acessando o hub, beleza
                if (!string.IsNullOrEmpty(tokenAcesso) && caminho.StartsWithSegments("/notificationHub"))
                {
                    contexto.Token = tokenAcesso; // seta o token no contexto
                }
                
                return Task.CompletedTask; // sucesso total
            }
        };
    });

// Event Bus com RabbitMQ - mensageria que é massa
construtorApp.Services.Configure<OpcoesRabbitMQ>(construtorApp.Configuration.GetSection("RabbitMQ"));
construtorApp.Services.AddSingleton<IBarramentoEventos, BarramentoEventosRabbitMQ>(); // barramento de eventos raiz

// Handlers dos eventos - onde a coisa acontece de verdade
construtorApp.Services.AddScoped<VagaOcupadaEventHandler>(); // quando alguém ocupa uma vaga
construtorApp.Services.AddScoped<VagaLiberadaEventHandler>(); // quando alguém libera uma vaga
construtorApp.Services.AddScoped<PagamentoProcessadoEventHandler>(); // quando o pagamento rola

// Health Checks - pra saber se tá vivo ou morto
construtorApp.Services.AddHealthChecks(); // checa se o serviço tá respondendo

// CORS liberado geral - porque vida é curta pra ficar configurando CORS
construtorApp.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("LiberaGeral", politica =>
    {
        politica.AllowAnyOrigin() // qualquer origem
              .AllowAnyMethod() // qualquer método
              .AllowAnyHeader(); // qualquer header
    });
});

var aplicacao = construtorApp.Build(); // constrói a aplicação

// Configurando o pipeline HTTP - onde as requisições passam
if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger(); // swagger só em dev
    aplicacao.UseSwaggerUI(); // interface do swagger
}

aplicacao.UseHttpsRedirection(); // força HTTPS - segurança first
aplicacao.UseCors("LiberaGeral"); // aplica a política CORS

aplicacao.UseAuthentication(); // autenticação JWT
aplicacao.UseAuthorization(); // autorização baseada em roles

aplicacao.MapControllers(); // mapeia os controllers
aplicacao.MapHealthChecks("/saude"); // endpoint de health check

// Mapeia o Hub do SignalR - onde a mágica do real-time acontece
aplicacao.MapHub<HubNotificacao>("/hubNotificacao"); // hub das notificações

// Inscrevendo nos eventos - aqui que a coisa fica interessante
// TEMPORÁRIO: Comentado para teste sem RabbitMQ
/*
var barramento = aplicacao.Services.GetRequiredService<IBarramentoEventos>();
var provedorServicos = aplicacao.Services;

// Quando uma vaga é ocupada - manda notificação pro cliente
barramento.Inscrever<EventoVagaOcupada>(async (evento) =>
{
    using var escopo = provedorServicos.CreateScope();
    var manipulador = escopo.ServiceProvider.GetRequiredService<VagaOcupadaEventHandler>();
    await manipulador.Handle(evento); // processa o evento
});

// Quando uma vaga é liberada - avisa que tem vaga livre
barramento.Inscrever<EventoVagaLiberada>(async (evento) =>
{
    using var escopo = provedorServicos.CreateScope();
    var manipulador = escopo.ServiceProvider.GetRequiredService<VagaLiberadaEventHandler>();
    await manipulador.Handle(evento); // processa o evento
});

// Quando um pagamento é processado - notifica o resultado
barramento.Inscrever<EventoPagamentoProcessado>(async (evento) =>
{
    using var escopo = provedorServicos.CreateScope();
    var manipulador = escopo.ServiceProvider.GetRequiredService<PagamentoProcessadoEventHandler>();
    await manipulador.Handle(evento); // processa o evento
});
*/

aplicacao.Run(); // roda a aplicação - que comece o show!