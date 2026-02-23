using System.Threading.Tasks;
using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BankMore.Api.Controllers
{
    [ApiController]
    [Route("api/agencia")]
    public class AgenciaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AgenciaController(IMediator mediator) => _mediator = mediator;
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] AgenciaRequest request)
        {
            var resultado = await _mediator.Send(new CriarAgenciaCommand(request));
            return Ok(resultado);
        }
    }
}
