// Evento de vaga liberada - quando alguém sai do estacionamento
namespace Shared.Contracts.Events;

// Evento que dispara quando uma vaga é liberada - tipo "saiu da vaga"
public record EventoVagaLiberada(
    Guid EventoId, // ID do evento
    DateTime OcorreuEm, // quando aconteceu
    Guid VagaId, // qual vaga foi liberada
    string CodigoVaga, // código da vaga (tipo A1, B2, etc)
    Guid ClienteId, // quem estava ocupando
    DateTime DataSaida, // quando saiu
    TimeSpan TempoOcupacao, // quanto tempo ficou
    decimal ValorCobrado // quanto pagou pela estadia
) : IEventoIntegracao;