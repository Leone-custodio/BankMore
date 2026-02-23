using System.Collections.Generic;
using System.Threading.Tasks;
using BankMore.Domain.Entidades;
namespace BankMore.Domain.Interfaces
{
    public interface IUsuarioRepositorio
    {
        Task Inserir(Usuario usuario);
        Task<Usuario?> ObterPorId(string id);
        Task<Usuario?> ObterPorCpf(string cpf);
    }
    public interface IAgenciaRepositorio
    {
        Task Inserir(Agencia agencia);
        Task<Agencia?> ObterPorId(string id);
        Task<Agencia?> ObterPorNumero(string numero);
        Task<IEnumerable<Agencia>> ObterTodas();
    }
    public interface IContaCorrenteRepositorio
    {
        Task<ContaCorrente?> ObterPorNumero(int numero);
        Task<ContaCorrente?> ObterPorId(string id);
        Task<IEnumerable<ContaCorrente>> ObterPorUsuario(string idUsuario);
        Task Inserir(ContaCorrente conta);
        Task AtualizarStatus(string id, int ativo);
        Task AtualizarSaldo(string id, double novoSaldo);
    }
    public interface IMovimentoRepositorio
    {
        Task Inserir(Movimento movimento);
        Task<IEnumerable<Movimento>> ObterPorConta(string idContaCorrente);
    }
    public interface ITransferenciaRepositorio
    {
        Task Inserir(Transferencia transferencia);
    }
    public interface ITarifaRepositorio
    {
        Task Inserir(Tarifa tarifa);
    }
    public interface IIdempotenciaRepositorio
    {
        Task<Idempotencia?> ObterPorChave(string chave);
        Task Inserir(Idempotencia idempotencia);
    }
}
