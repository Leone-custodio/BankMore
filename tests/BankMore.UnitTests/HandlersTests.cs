using System;
using System.Threading;
using System.Threading.Tasks;
using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using BankMore.Domain.Entidades;
using BankMore.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;

namespace BankMore.UnitTests
{
    public class HandlersTests
    {
        private readonly Mock<IUsuarioRepositorio> _usuarioRepoMock = new();
        private readonly Mock<IAgenciaRepositorio> _agenciaRepoMock = new();
        private readonly Mock<IContaCorrenteRepositorio> _contaRepoMock = new();
        private readonly Mock<IMovimentoRepositorio> _movRepoMock = new();
        private readonly Mock<ITransferenciaRepositorio> _transRepoMock = new();
        private readonly Mock<IIdempotenciaRepositorio> _idemRepoMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<IDistributedCache> _cacheMock = new();

        [Fact]
        public async Task CriarUsuario_DeveRetornarSucesso_QuandoDadosValidos()
        {
            var handler = new CriarUsuarioHandler(_usuarioRepoMock.Object);
            var request = new UsuarioRequest("Leone", "12345678901", "11999999999", "Rua A", "senha123");
            _usuarioRepoMock.Setup(r => r.ObterPorCpf(It.IsAny<string>())).ReturnsAsync((Usuario)null);

            var resultado = await handler.Handle(new CriarUsuarioCommand(request), CancellationToken.None);

            resultado.Should().NotBeNull();
            _usuarioRepoMock.Verify(r => r.Inserir(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task CriarAgencia_DeveRetornarSucesso_QuandoDadosValidos()
        {
            var handler = new CriarAgenciaHandler(_agenciaRepoMock.Object);
            var request = new AgenciaRequest("001", "Agência Central");

            var resultado = await handler.Handle(new CriarAgenciaCommand(request), CancellationToken.None);

            resultado.Should().NotBeNull();
            _agenciaRepoMock.Verify(r => r.Inserir(It.IsAny<Agencia>()), Times.Once);
        }

        [Fact]
        public async Task EfetuarMovimentacao_DeveRetornarErro_QuandoSaldoInsuficiente()
        {
            var handler = new EfetuarMovimentacaoHandler(_contaRepoMock.Object, _movRepoMock.Object, _idemRepoMock.Object, _cacheMock.Object);
            var conta = new ContaCorrente { IdContaCorrente = "1", Saldo = 100, Ativo = 1 };
            _contaRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync(conta);
            var request = new MovimentacaoRequest(null, 200, "D");

            var resultado = await handler.Handle(new EfetuarMovimentacaoCommand(request, "1", "req-1"), CancellationToken.None);

            resultado.Should().BeOfType<ErroResponse>();
            ((ErroResponse)resultado).TipoFalha.Should().Be("INSUFFICIENT_FUNDS");
        }

        [Fact]
        public async Task EfetuarTransferencia_DeveRetornarErro_QuandoAgenciasDiferentes()
        {
            var handler = new EfetuarTransferenciaHandler(_contaRepoMock.Object, _movRepoMock.Object, _transRepoMock.Object, _idemRepoMock.Object, _mediatorMock.Object, _cacheMock.Object);
            var origem = new ContaCorrente { IdContaCorrente = "1", IdAgencia = "A", Saldo = 1000, Ativo = 1 };
            var destino = new ContaCorrente { IdContaCorrente = "2", IdAgencia = "B", Ativo = 1 };
            _contaRepoMock.Setup(r => r.ObterPorId("1")).ReturnsAsync(origem);
            _contaRepoMock.Setup(r => r.ObterPorNumero(It.IsAny<int>())).ReturnsAsync(destino);
            var request = new TransferenciaRequest(222, 100);

            var resultado = await handler.Handle(new EfetuarTransferenciaCommand(request, "1"), CancellationToken.None);

            resultado.Should().BeOfType<ErroResponse>();
            ((ErroResponse)resultado).TipoFalha.Should().Be("INVALID_AGENCY_TRANSFER");
        }

        [Fact]
        public async Task ObterSaldo_DeveRetornarSaldo_QuandoContaExiste()
        {
            var handler = new ObterSaldoHandler(_contaRepoMock.Object, _cacheMock.Object);
            var conta = new ContaCorrente { Numero = 123, Saldo = 500, Nome = "Leone" };
            _contaRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync(conta);
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[])null);

            var resultado = await handler.Handle(new ObterSaldoQuery("1"), CancellationToken.None);

            resultado.Should().NotBeNull();
            resultado.ToString().Should().Contain("500");
        }
    }
}
