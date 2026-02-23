using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using BankMore.Domain.Entidades;
using BankMore.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace BankMore.UnitTests
{
    public class ObterSaldoHandlerTests
    {
        private readonly Mock<IContaCorrenteRepositorio> _repositorioMock;
        private readonly Mock<IDistributedCache> _cache;
        private readonly ObterSaldoHandler _handler;

        public ObterSaldoHandlerTests()
        {
            _repositorioMock = new Mock<IContaCorrenteRepositorio>();
            _cache = new Mock<IDistributedCache>();
            _handler = new ObterSaldoHandler(_repositorioMock.Object, _cache.Object);
        }

        [Fact]
        public async Task Handle_DeveRetornarErro_QuandoContaNaoExiste()
        {
            // Arrange
            _repositorioMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync((ContaCorrente)null);
            var query = new ObterSaldoQuery("id-inexistente");

            // Act
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // Assert
            resultado.Should().BeOfType<ErroResponse>();
            ((ErroResponse)resultado).TipoFalha.Should().Be("INVALID_ACCOUNT");
        }

        [Fact]
        public async Task Handle_DeveRetornarSaldo_QuandoContaExiste()
        {
            // Arrange
            var conta = new ContaCorrente 
            { 
                IdContaCorrente = "id-conta", 
                Numero = 123456, 
                Saldo = 1500.50, 
                Nome = "Leone Teste" 
            };
            _repositorioMock.Setup(r => r.ObterPorId("id-conta")).ReturnsAsync(conta);
            var query = new ObterSaldoQuery("id-conta");

            // Act
            var resultado = await _handler.Handle(query, CancellationToken.None);

            // Assert
            resultado.Should().NotBeNull();
            resultado.ToString().Should().Contain("123456");
        }
    }
}
