using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BankMore.Application.DTOs;
using BankMore.Domain.Entidades;
using BankMore.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using BCrypt.Net;

namespace BankMore.Application.Handlers
{
    public static class StringExtensions
    {
        public static string SomenteNumeros(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, @"[^\d]", "");
        }
    }

    public record CriarUsuarioCommand(UsuarioRequest Request) : IRequest<object>;
    public class CriarUsuarioHandler : IRequestHandler<CriarUsuarioCommand, object>
    {
        private readonly IUsuarioRepositorio _repositorio;
        public CriarUsuarioHandler(IUsuarioRepositorio repositorio) => _repositorio = repositorio;
        public async Task<object> Handle(CriarUsuarioCommand command, CancellationToken cancellationToken)
        {
            var cpfLimpo = command.Request.Cpf.SomenteNumeros();
            if (string.IsNullOrEmpty(cpfLimpo) || cpfLimpo.Length != 11)
                return new ErroResponse("CPF inválido. Deve conter 11 dígitos numéricos.", "INVALID_DOCUMENT");
            var existente = await _repositorio.ObterPorCpf(cpfLimpo);
            if (existente != null) return new ErroResponse("CPF já cadastrado", "DUPLICATE_CPF");
            
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hashSenha = BCrypt.Net.BCrypt.HashPassword(command.Request.Senha, salt);

            var usuario = new Usuario
            {
                Nome = command.Request.Nome,
                Cpf = cpfLimpo,
                Celular = command.Request.Celular.SomenteNumeros(),
                Endereco = command.Request.Endereco,
                Senha = hashSenha,
                Salt = salt
            };
            await _repositorio.Inserir(usuario);
            return new { IdUsuario = usuario.IdUsuario, Nome = usuario.Nome };
        }
    }

    public record CriarAgenciaCommand(AgenciaRequest Request) : IRequest<object>;
    public class CriarAgenciaHandler : IRequestHandler<CriarAgenciaCommand, object>
    {
        private readonly IAgenciaRepositorio _repositorio;
        public CriarAgenciaHandler(IAgenciaRepositorio repositorio) => _repositorio = repositorio;
        public async Task<object> Handle(CriarAgenciaCommand command, CancellationToken cancellationToken)
        {
            var agencia = new Agencia { Numero = command.Request.Numero, Nome = command.Request.Nome };
            await _repositorio.Inserir(agencia);
            return new { IdAgencia = agencia.IdAgencia, Numero = agencia.Numero };
        }
    }

    public record CriarContaCommand(CadastroContaRequest Request, string IdUsuarioToken) : IRequest<object>;
    public class CriarContaHandler : IRequestHandler<CriarContaCommand, object>
    {
        private readonly IContaCorrenteRepositorio _repositorio;
        private readonly IAgenciaRepositorio _agenciaRepo;
        private readonly IUsuarioRepositorio _usuarioRepo;
        public CriarContaHandler(IContaCorrenteRepositorio repositorio, IAgenciaRepositorio agenciaRepo, IUsuarioRepositorio usuarioRepo)
        {
            _repositorio = repositorio;
            _agenciaRepo = agenciaRepo;
            _usuarioRepo = usuarioRepo;
        }
        public async Task<object> Handle(CriarContaCommand command, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepo.ObterPorId(command.IdUsuarioToken);
            if (usuario == null) return new ErroResponse("Usuário não encontrado via token", "INVALID_USER");
            var agencia = await _agenciaRepo.ObterPorId(command.Request.IdAgencia);
            if (agencia == null) return new ErroResponse("Agência não encontrada", "INVALID_AGENCY");
            var conta = new ContaCorrente
            {
                IdUsuario = usuario.IdUsuario,
                IdAgencia = agencia.IdAgencia,
                Numero = new Random().Next(100000, 999999),
                Nome = command.Request.NomeTitular,
                Ativo = 1,
                Saldo = 0
            };
            await _repositorio.Inserir(conta);
            return new { NumeroConta = conta.Numero };
        }
    }

    public record CancelarContaCommand(CancelarContaRequest Request, string IdUsuarioToken) : IRequest<object>;
    public class CancelarContaHandler : IRequestHandler<CancelarContaCommand, object>
    {
        private readonly IContaCorrenteRepositorio _contaRepo;
        private readonly IUsuarioRepositorio _usuarioRepo;
        public CancelarContaHandler(IContaCorrenteRepositorio contaRepo, IUsuarioRepositorio usuarioRepo)
        {
            _contaRepo = contaRepo;
            _usuarioRepo = usuarioRepo;
        }
        public async Task<object> Handle(CancelarContaCommand command, CancellationToken cancellationToken)
        {
            var conta = await _contaRepo.ObterPorNumero(command.Request.NumeroConta);
            if (conta == null) return new ErroResponse("Conta não encontrada", "INVALID_ACCOUNT");
            if (conta.IdUsuario != command.IdUsuarioToken)
                return new ErroResponse("Você não tem permissão para cancelar esta conta", "UNAUTHORIZED_ACCESS");
            var usuario = await _usuarioRepo.ObterPorId(command.IdUsuarioToken);
            
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(command.Request.Senha, usuario.Senha))
                return new ErroResponse("Senha incorreta", "USER_UNAUTHORIZED");

            await _contaRepo.AtualizarStatus(conta.IdContaCorrente, 0);
            return true;
        }
    }

    public record EfetuarDepositoCommand(DepositoRequest Request, string IdContaToken) : IRequest<object>;
    public class EfetuarDepositoHandler : IRequestHandler<EfetuarDepositoCommand, object>
    {
        private readonly IMediator _mediator;
        public EfetuarDepositoHandler(IMediator mediator) => _mediator = mediator;
        public async Task<object> Handle(EfetuarDepositoCommand command, CancellationToken cancellationToken)
        {
            var idRequisicao = Guid.NewGuid().ToString();
            return await _mediator.Send(new EfetuarMovimentacaoCommand(
                new MovimentacaoRequest(null, command.Request.Valor, "C"), 
                command.IdContaToken,
                idRequisicao));
        }
    }

    public record EfetuarSaqueCommand(SaqueRequest Request, string IdContaToken) : IRequest<object>;
    public class EfetuarSaqueHandler : IRequestHandler<EfetuarSaqueCommand, object>
    {
        private readonly IMediator _mediator;
        public EfetuarSaqueHandler(IMediator mediator) => _mediator = mediator;
        public async Task<object> Handle(EfetuarSaqueCommand command, CancellationToken cancellationToken)
        {
            var idRequisicao = Guid.NewGuid().ToString();
            return await _mediator.Send(new EfetuarMovimentacaoCommand(
                new MovimentacaoRequest(null, command.Request.Valor, "D"), 
                command.IdContaToken,
                idRequisicao));
        }
    }

    public record EfetuarMovimentacaoCommand(MovimentacaoRequest Request, string IdContaToken, string IdRequisicao) : IRequest<object>;
    public class EfetuarMovimentacaoHandler : IRequestHandler<EfetuarMovimentacaoCommand, object>
    {
        private readonly IContaCorrenteRepositorio _contaRepo;
        private readonly IMovimentoRepositorio _movRepo;
        private readonly IIdempotenciaRepositorio _idemRepo;
        private readonly IDistributedCache _cache;

        public EfetuarMovimentacaoHandler(IContaCorrenteRepositorio contaRepo, IMovimentoRepositorio movRepo, IIdempotenciaRepositorio idemRepo, IDistributedCache cache)
        {
            _contaRepo = contaRepo;
            _movRepo = movRepo;
            _idemRepo = idemRepo;
            _cache = cache;
        }
        public async Task<object> Handle(EfetuarMovimentacaoCommand command, CancellationToken cancellationToken)
        {
            var jaProcessado = await _idemRepo.ObterPorChave(command.IdRequisicao);
            if (jaProcessado != null) return true;
            var conta = await _contaRepo.ObterPorId(command.IdContaToken);
            if (conta == null) return new ErroResponse("Conta não cadastrada ou não autorizada via token", "INVALID_ACCOUNT");
            if (conta.Ativo == 0) return new ErroResponse("Conta inativa", "INACTIVE_ACCOUNT");
            if (command.Request.Valor <= 0) return new ErroResponse("Apenas valores positivos", "INVALID_VALUE");
            var tipo = command.Request.TipoMovimento.ToUpper();
            if (tipo != "C" && tipo != "D") return new ErroResponse("Apenas tipos C ou D", "INVALID_TYPE");
            if (tipo == "D" && conta.Saldo < command.Request.Valor)
                return new ErroResponse("Saldo insuficiente para realizar o saque", "INSUFFICIENT_FUNDS");
            await _movRepo.Inserir(new Movimento 
            { 
                IdContaCorrente = conta.IdContaCorrente, 
                TipoMovimento = tipo, 
                Valor = command.Request.Valor 
            });
            var novoSaldo = tipo == "C" ? conta.Saldo + command.Request.Valor : conta.Saldo - command.Request.Valor;
            await _contaRepo.AtualizarSaldo(conta.IdContaCorrente, novoSaldo);
            await _idemRepo.Inserir(new Idempotencia { ChaveIdempotencia = command.IdRequisicao, Resultado = "SUCCESS" });
            
            await _cache.RemoveAsync($"saldo_{conta.IdContaCorrente}");

            return true;
        }
    }

    public record EfetuarTransferenciaCommand(TransferenciaRequest Request, string IdContaOrigem) : IRequest<object>;
    public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaCommand, object>
    {
        private readonly IContaCorrenteRepositorio _contaRepo;
        private readonly IMovimentoRepositorio _movRepo;
        private readonly ITransferenciaRepositorio _transRepo;
        private readonly IIdempotenciaRepositorio _idemRepo;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;

        public EfetuarTransferenciaHandler(IContaCorrenteRepositorio contaRepo, IMovimentoRepositorio movRepo, ITransferenciaRepositorio transRepo, IIdempotenciaRepositorio idemRepo, IMediator mediator, IDistributedCache cache)
        {
            _contaRepo = contaRepo;
            _movRepo = movRepo;
            _transRepo = transRepo;
            _idemRepo = idemRepo;
            _mediator = mediator;
            _cache = cache;
        }
        public async Task<object> Handle(EfetuarTransferenciaCommand command, CancellationToken cancellationToken)
        {
            var idRequisicao = Guid.NewGuid().ToString();
            var jaProcessado = await _idemRepo.ObterPorChave(idRequisicao);
            if (jaProcessado != null) return true;
            var origem = await _contaRepo.ObterPorId(command.IdContaOrigem);
            var destino = await _contaRepo.ObterPorNumero(command.Request.NumeroContaDestino);
            if (origem == null || destino == null) return new ErroResponse("Conta inválida ou não autorizada via token", "INVALID_ACCOUNT");
            if (origem.Ativo == 0 || destino.Ativo == 0) return new ErroResponse("Conta inativa", "INACTIVE_ACCOUNT");
            if (command.Request.Valor <= 0) return new ErroResponse("Valor inválido", "INVALID_VALUE");
            if (origem.Saldo < command.Request.Valor) return new ErroResponse("Saldo insuficiente", "INSUFFICIENT_FUNDS");
            if (origem.IdAgencia != destino.IdAgencia)
                return new ErroResponse("Transferência permitida apenas entre contas da mesma agência", "INVALID_AGENCY_TRANSFER");
            var debito = await _mediator.Send(new EfetuarMovimentacaoCommand(new MovimentacaoRequest(origem.Numero, command.Request.Valor, "D"), command.IdContaOrigem, idRequisicao + "_D"));
            if (debito is ErroResponse) return debito;
            var credito = await _mediator.Send(new EfetuarMovimentacaoCommand(new MovimentacaoRequest(destino.Numero, command.Request.Valor, "C"), destino.IdContaCorrente, idRequisicao + "_C"));
            if (credito is ErroResponse)
            {
                await _mediator.Send(new EfetuarMovimentacaoCommand(new MovimentacaoRequest(origem.Numero, command.Request.Valor, "C"), command.IdContaOrigem, idRequisicao + "_ESTORNO"));
                return credito;
            }
            await _transRepo.Inserir(new Transferencia 
            { 
                IdContaCorrenteOrigem = origem.IdContaCorrente, 
                IdContaCorrenteDestino = destino.IdContaCorrente, 
                Valor = command.Request.Valor 
            });
            await _idemRepo.Inserir(new Idempotencia { ChaveIdempotencia = idRequisicao, Resultado = "SUCCESS" });
            
            await _cache.RemoveAsync($"saldo_{origem.IdContaCorrente}");
            await _cache.RemoveAsync($"saldo_{destino.IdContaCorrente}");

            return true;
        }
    }

    public record ObterSaldoQuery(string IdConta) : IRequest<object>;
    public class ObterSaldoHandler : IRequestHandler<ObterSaldoQuery, object>
    {
        private readonly IContaCorrenteRepositorio _repositorio;
        private readonly IDistributedCache _cache;

        public ObterSaldoHandler(IContaCorrenteRepositorio repositorio, IDistributedCache cache)
        {
            _repositorio = repositorio;
            _cache = cache;
        }
        public async Task<object> Handle(ObterSaldoQuery query, CancellationToken cancellationToken)
        {
            var cacheKey = $"saldo_{query.IdConta}";
            var cachedSaldo = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedSaldo))
            {
                return JsonSerializer.Deserialize<object>(cachedSaldo);
            }

            var conta = await _repositorio.ObterPorId(query.IdConta);
            if (conta == null) return new ErroResponse("Conta não encontrada", "INVALID_ACCOUNT");
            
            var result = new { NumeroConta = conta.Numero, Saldo = conta.Saldo, NomeTitular = conta.Nome };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return result;
        }
    }
}
