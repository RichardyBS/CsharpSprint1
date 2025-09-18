// Interface do barramento de eventos - onde os eventos voam entre serviços
using Shared.Contracts;

namespace Shared.EventBus;

// Interface do barramento de eventos - tipo "correios dos eventos"
public interface IBarramentoEventos
{
    // Publica um evento - tipo "mandar carta"
    Task PublicarAsync<T>(T evento, CancellationToken tokenCancelamento = default) where T : IEventoIntegracao;
    
    // Se inscreve pra receber eventos - tipo "assinar newsletter"
    void Inscrever<T>(Func<T, Task> manipulador) where T : IEventoIntegracao;
    
    // Cancela a inscrição - tipo "unsubscribe"
    void Desinscrever<T>() where T : IEventoIntegracao;
}