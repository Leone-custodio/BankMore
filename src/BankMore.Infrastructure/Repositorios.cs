using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BankMore.Domain.Entidades;
using BankMore.Domain.Interfaces;
using Dapper;
using Oracle.ManagedDataAccess.Client;
namespace BankMore.Infrastructure.Repositorios
{
    public class BaseRepositorio
    {
        protected readonly string _connectionString;
        public BaseRepositorio(string connectionString) => _connectionString = connectionString;
        protected IDbConnection Conexao => new OracleConnection(_connectionString);
    }
    public class UsuarioRepositorio : BaseRepositorio, IUsuarioRepositorio
    {
        public UsuarioRepositorio(string connectionString) : base(connectionString) { }
        public async Task Inserir(Usuario usuario)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO usuario (idusuario, nome, cpf, celular, endereco, senha, salt) VALUES (:IdUsuario, :Nome, :Cpf, :Celular, :Endereco, :Senha, :Salt)", usuario);
        }
        public async Task<Usuario?> ObterPorId(string id)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM usuario WHERE idusuario = :id", new { id });
        }
        public async Task<Usuario?> ObterPorCpf(string cpf)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM usuario WHERE cpf = :cpf", new { cpf });
        }
    }
    public class AgenciaRepositorio : BaseRepositorio, IAgenciaRepositorio
    {
        public AgenciaRepositorio(string connectionString) : base(connectionString) { }
        public async Task Inserir(Agencia agencia)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO agencia (idagencia, numero, nome) VALUES (:IdAgencia, :Numero, :Nome)", agencia);
        }
        public async Task<Agencia?> ObterPorId(string id)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<Agencia>("SELECT * FROM agencia WHERE idagencia = :id", new { id });
        }
        public async Task<Agencia?> ObterPorNumero(string numero)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<Agencia>("SELECT * FROM agencia WHERE numero = :numero", new { numero });
        }
        public async Task<IEnumerable<Agencia>> ObterTodas()
        {
            using var db = Conexao;
            return await db.QueryAsync<Agencia>("SELECT * FROM agencia");
        }
    }
    public class ContaCorrenteRepositorio : BaseRepositorio, IContaCorrenteRepositorio
    {
        public ContaCorrenteRepositorio(string connectionString) : base(connectionString) { }
        public async Task<ContaCorrente?> ObterPorNumero(int numero)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<ContaCorrente>("SELECT * FROM contacorrente WHERE numero = :numero", new { numero });
        }
        public async Task<ContaCorrente?> ObterPorId(string id)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<ContaCorrente>("SELECT * FROM contacorrente WHERE idcontacorrente = :id", new { id });
        }
        public async Task<IEnumerable<ContaCorrente>> ObterPorUsuario(string idUsuario)
        {
            using var db = Conexao;
            return await db.QueryAsync<ContaCorrente>("SELECT * FROM contacorrente WHERE idusuario = :idUsuario", new { idUsuario });
        }
        public async Task Inserir(ContaCorrente conta)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO contacorrente (idcontacorrente, idusuario, idagencia, numero, nome, ativo, saldo) VALUES (:IdContaCorrente, :IdUsuario, :IdAgencia, :Numero, :Nome, :Ativo, :Saldo)", conta);
        }
        public async Task AtualizarStatus(string id, int ativo)
        {
            using var db = Conexao;
            await db.ExecuteAsync("UPDATE contacorrente SET ativo = :ativo WHERE idcontacorrente = :id", new { id, ativo });
        }
        public async Task AtualizarSaldo(string id, double novoSaldo)
        {
            using var db = Conexao;
            await db.ExecuteAsync("UPDATE contacorrente SET saldo = :novoSaldo WHERE idcontacorrente = :id", new { id, novoSaldo });
        }
    }
    public class MovimentoRepositorio : BaseRepositorio, IMovimentoRepositorio
    {
        public MovimentoRepositorio(string connectionString) : base(connectionString) { }
        public async Task Inserir(Movimento movimento)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) VALUES (:IdMovimento, :IdContaCorrente, :DataMovimento, :TipoMovimento, :Valor)", movimento);
        }
        public async Task<IEnumerable<Movimento>> ObterPorConta(string idContaCorrente)
        {
            using var db = Conexao;
            return await db.QueryAsync<Movimento>("SELECT * FROM movimento WHERE idcontacorrente = :idContaCorrente", new { idContaCorrente });
        }
    }
    public class TransferenciaRepositorio : BaseRepositorio, ITransferenciaRepositorio
    {
        public TransferenciaRepositorio(string connectionString) : base(connectionString) { }
        public async Task Inserir(Transferencia transferencia)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor) VALUES (:IdTransferencia, :IdContaCorrenteOrigem, :IdContaCorrenteDestino, :DataMovimento, :Valor)", transferencia);
        }
    }
    public class IdempotenciaRepositorio : BaseRepositorio, IIdempotenciaRepositorio
    {
        public IdempotenciaRepositorio(string connectionString) : base(connectionString) { }
        public async Task<Idempotencia?> ObterPorChave(string chave)
        {
            using var db = Conexao;
            return await db.QueryFirstOrDefaultAsync<Idempotencia>("SELECT * FROM idempotencia WHERE chave_idempotencia = :chave", new { chave });
        }
        public async Task Inserir(Idempotencia idempotencia)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado) VALUES (:ChaveIdempotencia, :Requisicao, :Resultado)", idempotencia);
        }
    }
    public class TarifaRepositorio : BaseRepositorio, ITarifaRepositorio
    {
        public TarifaRepositorio(string connectionString) : base(connectionString) { }
        public async Task Inserir(Tarifa tarifa)
        {
            using var db = Conexao;
            await db.ExecuteAsync("INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor) VALUES (:IdTarifa, :IdContaCorrente, :DataMovimento, :Valor)", tarifa);
        }
    }
}
