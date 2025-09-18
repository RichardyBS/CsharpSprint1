// Portão da API - Gateway dos parça 🚪
// TODO: depois vou melhorar isso aqui, tá meio gambiarra mas funciona né
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var construtorApp = WebApplication.CreateBuilder(args);

// Adicionando os bagulho no container (DI é vida)
construtorApp.Services.AddControllers();
construtorApp.Services.AddEndpointsApiExplorer();
construtorApp.Services.AddSwaggerGen(); // swagger é amor ❤️

// Autenticação JWT - porque segurança é importante né galera
// FIXME: essa chave secreta tá hardcoded, depois mudo pra variável de ambiente
var configuracoesJwt = construtorApp.Configuration.GetSection("ConfiguracoesJwt");
var chaveSecreta = configuracoesJwt["ChaveSecreta"] ?? "EstacionamentoChaveSecreta2025!@#MuitoSegura$%^&*()"; // senha123 era muito óbvio kkkk

construtorApp.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        // Configurações do JWT - copiei do Stack Overflow e funcionou 🤷‍♂️
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // token não pode ser eterno né
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuracoesJwt["Emissor"] ?? "PortaoEstacionamentoAPI",
            ValidAudience = configuracoesJwt["Audiencia"] ?? "ClientesEstacionamento",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta))
        };
    });

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
aplicacao.MapPost("/api/auth/entrar", async (PedidoLogin pedido) =>
{
    // Autenticação super segura kkkk (NOT!)
    // HACK: usuário e senha hardcoded, depois vou fazer direito... talvez
    if (pedido.NomeUsuario == "admin" && pedido.Senha == "password") // senha123 era muito óbvio
    {
        var manipuladorToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var chave = Encoding.UTF8.GetBytes(chaveSecreta);
        var descricaoToken = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("nomeUsuario", pedido.NomeUsuario),
                new System.Security.Claims.Claim("papel", "admin") // todo mundo é admin por enquanto 😅
            }),
            Expires = DateTime.UtcNow.AddHours(24), // token válido por 24h - generoso né
            Issuer = configuracoesJwt["Emissor"] ?? "PortaoEstacionamentoAPI",
            Audience = configuracoesJwt["Audiencia"] ?? "ClientesEstacionamento",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256Signature)
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