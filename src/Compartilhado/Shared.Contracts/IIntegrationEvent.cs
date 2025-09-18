// Interface dos eventos de integração - tipo contrato dos eventos que voam entre serviços
namespace Shared.Contracts;

// Interface que todo evento de integração deve implementar - tipo "regra do jogo"
public interface IEventoIntegracao
{
    Guid EventoId { get; } // ID único do evento - tipo RG do evento
    DateTime OcorreuEm { get; } // quando o evento aconteceu - timestamp da parada
}