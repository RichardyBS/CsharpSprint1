// Portão da API - Gateway dos parça 🚪
// TODO: depois vou melhorar isso aqui, tá meio gambiarra mas funciona né
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Shared.Contracts.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var construtorApp = WebApplication.CreateBuilder(args);

// Adicionando os bagulho no container (DI é vida)
construtorApp.Services.AddControllers();
construtorApp.Services.AddEndpointsApiExplorer();
construtorApp.Services.AddSwaggerGen(); // swagger é amor ❤️

// Autenticação JWT - configuração centralizada
construtorApp.Services.AddJwtAuthentication(construtorApp.Configuration);

// Health Checks - pra saber se tá vivo ou morto
construtorApp.Services.AddHealthChecks();

// Ocelot - o cara que faz a mágica do gateway acontecer
// NOTA: se der pau aqui, é porque o ocelot.json tá zoado
construtorApp.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
construtorApp.Services.AddOcelot();

// CORS liberado pra geral - depois vou restringir isso (talvez)
construtorApp.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("LiberaGeral", politica =>
    {
        politica.AllowAnyOrigin()      // qualquer origem pode acessar
              .AllowAnyMethod()        // GET, POST, PUT, DELETE, whatever
              .AllowAnyHeader();       // headers liberados
    });
});

var aplicacao = construtorApp.Build();

// Configurando o pipeline HTTP - a ordem importa aqui galera!
if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseSwagger();    // documentação automática, que maravilha
    aplicacao.UseSwaggerUI();  // interface bonitinha do swagger
}

aplicacao.UseHttpsRedirection(); // força HTTPS porque HTTP é coisa do passado
aplicacao.UseCors("LiberaGeral"); // CORS que configuramos ali em cima

aplicacao.UseAuthentication(); // primeiro autentica
aplicacao.UseAuthorization();  // depois autoriza (ordem importa!)

// Endpoint pra saber se tá funcionando - tipo um "tô vivo"
aplicacao.MapHealthChecks("/saude");

// Endpoint de login - gambiarra básica mas funciona
// TODO: depois implementar direito com banco de dados e hash da senha
aplicacao.MapPost("/api/auth/entrar", (PedidoLogin pedido, IConfiguration config) =>
{
    // Autenticação básica - credenciais vêm da configuração
    // TODO: implementar autenticação com banco de dados e hash da senha
    var adminUser = config["Auth:AdminUser"] ?? "admin";
    var adminPassword = config["Auth:AdminPassword"] ?? "EstacionamentoAdmin2025!";
    
    if (pedido.NomeUsuario == adminUser && pedido.Senha == adminPassword)
    {
        var manipuladorToken = new JwtSecurityTokenHandler();
        var chaveAssinatura = JwtConfiguration.GetSigningKey(config);
        var descricaoToken = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("nomeUsuario", pedido.NomeUsuario),
                new Claim("papel", "admin") // todo mundo é admin por enquanto 😅
            }),
            Expires = DateTime.UtcNow.AddHours(24), // token válido por 24h - generoso né
            Issuer = JwtConfiguration.GetIssuer(config),
            Audience = JwtConfiguration.GetAudience(config),
            SigningCredentials = new SigningCredentials(chaveAssinatura, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = manipuladorToken.CreateToken(descricaoToken);
        var tokenString = manipuladorToken.WriteToken(token);

        return Results.Ok(new { Token = tokenString, ExpiraEm = descricaoToken.Expires });
    }

    return Results.Unauthorized(); // vai tomar no... ops, não autorizado!
});

// Ocelot é o coração do gateway - sem ele não rola
// FIXME: às vezes demora pra carregar, investigar depois
await aplicacao.UseOcelot();

aplicacao.Run(); // BORA RODAR ESSA BAGAÇA! 🚀

// Record pra receber os dados de login - C# 9 é vida ❤️
public record PedidoLogin(string NomeUsuario, string Senha);