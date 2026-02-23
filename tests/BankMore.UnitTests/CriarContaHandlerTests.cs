using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using BankMore.Domain.Entidades;
using BankMore.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BankMore.UnitTests
{
    public class CriarContaHandlerTests
    {
        private readonly Mock<IContaCorrenteRepositorio> _repositorioMock;
        private readonly Mock<IAgenciaRepositorio> _agenciaRepoMock;
        private readonly Mock<IUsuarioRepositorio> _usuarioRepoMock;
        private readonly CriarContaHandler _handler;

        public CriarContaHandlerTests()
        {
            _repositorioMock = new Mock<IContaCorrenteRepositorio>();
            _agenciaRepoMock = new Mock<IAgenciaRepositorio>();
            _usuarioRepoMock = new Mock<IUsuarioRepositorio>();
            _handler = new CriarContaHandler(_repositorioMock.Object, _agenciaRepoMock.Object, _usuarioRepoMock.Object);
        }

        [Fact]
        public async Task Handle_DeveRetornarErro_QuandoUsuarioNaoEncontrado()
        {
            // Arrange
            _usuarioRepoMock.Setup(r => r.ObterPorId(It.IsAny<string>())).ReturnsAsync((Usuario)null);
            var request = new CadastroContaRequest("id-agencia", "Teste", "12345678901", "senha123");
            var command = new CriarContaCommand(request, "id-usuario-token");

            // Act
            var resultado = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultado.Should().BeOfType<ErroResponse>();
            ((ErroResponse)resultado).TipoFalha.Should().Be("INVALID_USER");
        }

        [Fact]
        public async Task Handle_DeveCriarConta_QuandoDadosValidos()
        {
            // Arrange
            var usuario = new Usuario { IdUsuario = "id-usuario-token", Nome = "Teste" };
            var agencia = new Agencia { IdAgencia = "id-agencia", Numero = "001" };
            
            _usuarioRepoMock.Setup(r => r.ObterPorId("id-usuario-token")).ReturnsAsync(usuario);
            _agenciaRepoMock.Setup(r => r.ObterPorId("id-agencia")).ReturnsAsync(agencia);

            var request = new CadastroContaRequest("id-agencia", "Teste Titular", "12345678901", "senha123");
            var command = new CriarContaCommand(request, "id-usuario-token");

            // Act
            var resultado = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultado.Should().NotBeNull();
            _repositorioMock.Verify(r => r.Inserir(It.IsAny<ContaCorrente>()), Times.Once);
        }
    }
}
