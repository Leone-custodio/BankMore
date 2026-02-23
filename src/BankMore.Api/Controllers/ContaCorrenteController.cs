using System.Linq;
using System.Threading.Tasks;
using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using BankMore.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BankMore.Api.Controllers
{
    [ApiController]
    [Route("api/contacorrente")]
    [Authorize]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IContaCorrenteRepositorio _contaRepo;
        public ContaCorrenteController(IMediator mediator, IContaCorrenteRepositorio contaRepo)
        {
            _mediator = mediator;
            _contaRepo = contaRepo;
        }
        private async Task<string?> ObterIdContaOperacional()
        {
            var idConta = User.FindFirst("IdConta")?.Value;
            if (!string.IsNullOrEmpty(idConta)) return idConta;
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return null;
            var contas = await _contaRepo.ObterPorUsuario(idUsuario);
            return contas.FirstOrDefault()?.IdContaCorrente;
        }
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] CadastroContaRequest request)
        {
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();
            var resultado = await _mediator.Send(new CriarContaCommand(request, idUsuario));
            if (resultado is ErroResponse erro) return BadRequest(erro);
            return Ok(resultado);
        }
        [HttpPost("cancelar")]
        public async Task<IActionResult> Cancelar([FromBody] CancelarContaRequest request)
        {
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Forbid();
            var resultado = await _mediator.Send(new CancelarContaCommand(request, idUsuario));
            if (resultado is ErroResponse erro) 
            {
                if (erro.TipoFalha == "USER_UNAUTHORIZED") return Unauthorized(erro);
                return BadRequest(erro);
            }
            return NoContent();
        }
        [HttpGet("saldo")]
        public async Task<IActionResult> ObterSaldo()
        {
            var idConta = await ObterIdContaOperacional();
            if (string.IsNullOrEmpty(idConta)) return BadRequest(new ErroResponse("Usuário não possui conta corrente vinculada", "ACCOUNT_NOT_FOUND"));
            var resultado = await _mediator.Send(new ObterSaldoQuery(idConta));
            if (resultado is ErroResponse erro) return BadRequest(erro);
            return Ok(resultado);
        }
        [HttpPost("deposito")]
        public async Task<IActionResult> Deposito([FromBody] DepositoRequest request)
        {
            var idConta = await ObterIdContaOperacional();
            if (string.IsNullOrEmpty(idConta)) return BadRequest(new ErroResponse("Usuário não possui conta corrente vinculada", "ACCOUNT_NOT_FOUND"));
            var resultado = await _mediator.Send(new EfetuarDepositoCommand(request, idConta));
            if (resultado is ErroResponse erro) return BadRequest(erro);
            return NoContent();
        }
        [HttpPost("saque")]
        public async Task<IActionResult> Saque([FromBody] SaqueRequest request)
        {
            var idConta = await ObterIdContaOperacional();
            if (string.IsNullOrEmpty(idConta)) return BadRequest(new ErroResponse("Usuário não possui conta corrente vinculada", "ACCOUNT_NOT_FOUND"));
            var resultado = await _mediator.Send(new EfetuarSaqueCommand(request, idConta));
            if (resultado is ErroResponse erro) return BadRequest(erro);
            return NoContent();
        }
    }
}
