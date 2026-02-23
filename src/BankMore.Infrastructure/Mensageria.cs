using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Serializer;
namespace BankMore.Infrastructure.Mensageria
{
    public record TransferenciaRealizadaEvent(string IdRequisicao, string IdContaCorrente);
    public record TarifacaoRealizadaEvent(string IdContaCorrente, double Valor);
    public interface IMensageriaService
    {
        Task PublicarTransferencia(TransferenciaRealizadaEvent @event);
        Task PublicarTarifacao(TarifacaoRealizadaEvent @event);
    }
    public class MensageriaService : IMensageriaService
    {
        private readonly IMessageProducer _producer;
        public MensageriaService(IMessageProducer producer) => _producer = producer;
        public async Task PublicarTransferencia(TransferenciaRealizadaEvent @event)
        {
            await _producer.ProduceAsync("transferencias-realizadas", @event);
        }
        public async Task PublicarTarifacao(TarifacaoRealizadaEvent @event)
        {
            await _producer.ProduceAsync("tarifacoes-realizadas", @event);
        }
    }
    public class TarifacaoHandler : IMessageHandler<TransferenciaRealizadaEvent>
    {
        public async Task Handle(IMessageContext context, TransferenciaRealizadaEvent message)
        {
            System.Console.WriteLine($"[Kafka] Processando tarifação para conta: {message.IdContaCorrente}");
        }
    }
}
