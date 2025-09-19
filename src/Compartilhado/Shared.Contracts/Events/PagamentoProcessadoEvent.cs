// Evento de pagamento processado - quando o dinheiro rola (ou não)
namespace Shared.Contracts.Events;

// Evento que dispara quando um pagamento foi processado - tipo "dinheiro caiu na conta"
public record EventoPagamentoProcessado(
    Guid EventoId, // ID do evento
    DateTime OcorreuEm, // quando aconteceu
    Guid TransacaoId, // ID da transação - tipo comprovante
    Guid ClienteId, // quem pagou
    decimal Valor, // quanto foi
    string MetodoPagamento, // como pagou (cartão, pix, etc)
    string Status, // se deu certo ou não
    string? CodigoAutorizacao // código da operadora (se tiver)
) : IEventoIntegracao;