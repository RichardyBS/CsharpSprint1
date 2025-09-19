// Implementação do barramento de eventos com RabbitMQ - onde a mensageria acontece
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace Shared.EventBus;

// Barramento de eventos usando RabbitMQ - tipo "correios turbinado"
public class BarramentoEventosRabbitMQ : IBarramentoEventos, IDisposable
{
    private readonly IConnection _conexao;
    private readonly IModel _canal;
    private readonly ILogger<BarramentoEventosRabbitMQ> _logger;
    private readonly Dictionary<string, Func<string, Task>> _manipuladores = new();
    private readonly string _nomeExchange = "estacionamento_eventos"; // nome do exchange

    public BarramentoEventosRabbitMQ(IOptions<OpcoesRabbitMQ> opcoes, ILogger<BarramentoEventosRabbitMQ> logger)
    {
        _logger = logger; // logger pra debugar quando der ruim
        
        // Configuração da fábrica de conexões - tipo "configurar o correio"
        var fabrica = new ConnectionFactory
        {
            HostName = opcoes.Value.NomeHost,
            Port = opcoes.Value.Porta,
            UserName = opcoes.Value.NomeUsuario,
            Password = opcoes.Value.Senha
        };

        // Cria conexão e canal - tipo "abrir linha telefônica"
        _conexao = fabrica.CreateConnection();
        _canal = _conexao.CreateModel();
        
        // Declara o exchange - tipo "criar caixa postal central"
        _canal.ExchangeDeclare(_nomeExchange, ExchangeType.Topic, durable: true);
    }

    public async Task PublicarAsync<T>(T evento, CancellationToken tokenCancelamento = default) where T : IEventoIntegracao
    {
        // Pega o nome do evento - tipo "assunto da carta"
        var nomeEvento = typeof(T).Name;
        var mensagem = JsonSerializer.Serialize(evento); // serializa pra JSON
        var corpo = Encoding.UTF8.GetBytes(mensagem); // converte pra bytes

        // Propriedades da mensagem - tipo "envelope da carta"
        var propriedades = _canal.CreateBasicProperties();
        propriedades.Persistent = true; // persiste no disco - não perde se der pau
        propriedades.MessageId = evento.EventoId.ToString(); // ID da mensagem
        propriedades.Timestamp = new AmqpTimestamp(((DateTimeOffset)evento.OcorreuEm).ToUnixTimeSeconds());

        // Publica a mensagem - tipo "mandar carta"
        _canal.BasicPublish(
            exchange: _nomeExchange,
            routingKey: nomeEvento,
            basicProperties: propriedades,
            body: corpo);

        _logger.LogInformation("Evento {EventName} publicado com ID {EventId} - voou!", nomeEvento, evento.EventoId);
        
        await Task.CompletedTask;
    }

    public void Inscrever<T>(Func<T, Task> manipulador) where T : IEventoIntegracao
    {
        // Pega o nome do evento - tipo "assunto que interessa"
        var nomeEvento = typeof(T).Name;
        var nomeFila = $"{nomeEvento}_fila"; // nome da fila

        // Declara a fila - tipo "criar caixa de entrada"
        _canal.QueueDeclare(nomeFila, durable: true, exclusive: false, autoDelete: false);
        _canal.QueueBind(nomeFila, _nomeExchange, nomeEvento); // conecta fila ao exchange

        // Cria o consumidor - tipo "carteiro que fica de olho na caixa"
        var consumidor = new EventingBasicConsumer(_canal);
        
        consumidor.Received += async (modelo, argumentos) =>
        {
            try
            {
                // Pega o corpo da mensagem - tipo "abrir carta"
                var corpo = argumentos.Body.ToArray();
                var mensagem = Encoding.UTF8.GetString(corpo);
                var evento = JsonSerializer.Deserialize<T>(mensagem);

                if (evento != null)
                {
                    // Processa o evento - aqui que a mágica acontece
                    await manipulador(evento);
                    _canal.BasicAck(argumentos.DeliveryTag, false); // confirma que processou
                    _logger.LogInformation("Evento {EventName} processado com ID {EventId} - show!", nomeEvento, evento.EventoId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento {EventName} - deu ruim", nomeEvento);
                _canal.BasicNack(argumentos.DeliveryTag, false, true); // rejeita e recoloca na fila
            }
        };

        // Inicia o consumo - tipo "começar a receber cartas"
        _canal.BasicConsume(nomeFila, autoAck: false, consumidor);
        
        // Guarda o manipulador - tipo "agenda de contatos"
        _manipuladores[nomeEvento] = async (mensagem) =>
        {
            var evento = JsonSerializer.Deserialize<T>(mensagem);
            if (evento != null)
                await manipulador(evento);
        };
    }

    public void Desinscrever<T>() where T : IEventoIntegracao
    {
        // Remove a inscrição - tipo "cancelar assinatura"
        var nomeEvento = typeof(T).Name;
        _manipuladores.Remove(nomeEvento);
    }

    public void Dispose()
    {
        // Fecha tudo - tipo "fechar a loja"
        _canal?.Close();
        _conexao?.Close();
    }
}

// Configurações do RabbitMQ - tipo "manual de instruções"
public class OpcoesRabbitMQ
{
    public string NomeHost { get; set; } = "localhost"; // onde tá rodando o RabbitMQ
    public int Porta { get; set; } = 5672; // porta padrão
    public string NomeUsuario { get; set; } = "guest"; // usuário padrão - deve vir da configuração
    public string Senha { get; set; } = "guest"; // senha padrão - deve vir da configuração
    
    // Método para configurar com variáveis de ambiente
    public static OpcoesRabbitMQ CriarDaConfiguracao()
    {
        return new OpcoesRabbitMQ
        {
            NomeHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Porta = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var porta) ? porta : 5672,
            NomeUsuario = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Senha = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };
    }
}