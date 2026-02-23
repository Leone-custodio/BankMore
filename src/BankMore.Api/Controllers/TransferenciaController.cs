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
    [Route("api/transferencia")]
    [Authorize]
    public class TransferenciaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IContaCorrenteRepositorio _contaRepo;
        public TransferenciaController(IMediator mediator, IContaCorrenteRepositorio contaRepo)
        {
            _mediator = mediator;
            _contaRepo = contaRepo;
        }
        private async Task<string?> ObterIdContaOrigem()
        {
            var idConta = User.FindFirst("IdConta")?.Value;
            if (!string.IsNullOrEmpty(idConta)) return idConta;
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return null;
            var contas = await _contaRepo.ObterPorUsuario(idUsuario);
            return contas.FirstOrDefault()?.IdContaCorrente;
        }
        [HttpPost]
        public async Task<IActionResult> Transferir([FromBody] TransferenciaRequest request)
        {
            var idContaOrigem = await ObterIdContaOrigem();
            if (string.IsNullOrEmpty(idContaOrigem)) return Forbid(); 
            var resultado = await _mediator.Send(new EfetuarTransferenciaCommand(request, idContaOrigem));
            if (resultado is ErroResponse erro) return BadRequest(erro);
            return NoContent(); 
        }
    }
}
