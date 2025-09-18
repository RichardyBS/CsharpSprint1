// Evento de vaga ocupada - quando alguém entra no estacionamento
namespace Shared.Contracts.Events;

// Evento que dispara quando uma vaga é ocupada - tipo "chegou alguém"
public record EventoVagaOcupada(
    Guid EventoId, // ID do evento
    DateTime OcorreuEm, // quando aconteceu
    int VagaId, // qual vaga foi ocupada
    string CodigoVaga, // código da vaga (tipo A1, B2, etc)
    int ClienteId, // quem ocupou
    string ClienteNome, // nome do cliente
    string ClienteCpf, // CPF do cliente
    DateTime DataEntrada // quando entrou
) : IEventoIntegracao;