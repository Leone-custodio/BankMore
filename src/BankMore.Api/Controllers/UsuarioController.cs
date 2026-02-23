using System.Threading.Tasks;
using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BankMore.Api.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsuarioController(IMediator mediator) => _mediator = mediator;
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] UsuarioRequest request)
        {
            var resultado = await _mediator.Send(new CriarUsuarioCommand(request));
            if (resultado is ErroResponse erro) return BadRequest(erro);
            return Ok(resultado);
        }
    }
}
