// Modelos das notificações - onde a estrutura dos dados mora
using System.Text.Json.Serialization;

namespace Notification.Service.Models;

// Classe da notificação - tipo mensagem estruturada
public class Notificacao
{
    public Guid Id { get; set; } = Guid.NewGuid(); // ID único - tipo CPF da notificação
    public Guid ClienteId { get; set; } // quem vai receber
    public string Titulo { get; set; } = string.Empty; // assunto da parada
    public string Mensagem { get; set; } = string.Empty; // o que vai falar
    public string Tipo { get; set; } = string.Empty; // Email, SMS, Push, InApp - tipo canal de comunicação
    public string Status { get; set; } = "Pendente"; // Pendente, Enviada, Falha - status da entrega
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow; // quando foi criada
    public DateTime? EnviadaEm { get; set; } // quando foi enviada (se foi)
    public string? Destinatario { get; set; } // pra onde vai (email, telefone, etc)
    public Dictionary<string, object> Dados { get; set; } = new(); // dados extras se precisar
    public int TentativasEnvio { get; set; } = 0; // quantas vezes tentou enviar
    public string? ErroUltimoEnvio { get; set; } // se deu ruim, qual foi o erro
}

// Template de notificação - tipo modelo pré-pronto
public class NotificacaoTemplate
{
    public string Nome { get; set; } = string.Empty; // nome do template
    public string Tipo { get; set; } = string.Empty; // que tipo de notificação
    public string Assunto { get; set; } = string.Empty; // assunto padrão
    public string Corpo { get; set; } = string.Empty; // corpo da mensagem com placeholders
    public Dictionary<string, string> Variaveis { get; set; } = new(); // variáveis que podem ser substituídas
}

// Configuração de notificação do cliente - tipo preferências do usuário
public class ConfiguracaoNotificacao
{
    public Guid ClienteId { get; set; } // de quem é a configuração
    public bool EmailAtivo { get; set; } = true; // quer receber email?
    public bool SmsAtivo { get; set; } = false; // quer receber SMS? (custa caro)
    public bool PushAtivo { get; set; } = true; // quer receber push?
    public bool InAppAtivo { get; set; } = true; // quer receber no app?
    public string? EmailPreferido { get; set; } // email principal
    public string? TelefonePreferido { get; set; } // telefone principal
    public List<string> TiposNotificacao { get; set; } = new(); // que tipos quer receber
}