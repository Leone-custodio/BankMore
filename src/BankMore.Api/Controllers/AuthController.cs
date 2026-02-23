using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BankMore.Application.DTOs;
using BankMore.Application.Handlers;
using BankMore.Domain.Entidades;
using BankMore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace BankMore.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IContaCorrenteRepositorio _contaRepo;
        public AuthController(IUsuarioRepositorio usuarioRepo, IContaCorrenteRepositorio contaRepo)
        {
            _usuarioRepo = usuarioRepo;
            _contaRepo = contaRepo;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Usuario? usuario = null;
            if (!string.IsNullOrEmpty(request.Cpf))
            {
                var cpfLimpo = request.Cpf.SomenteNumeros();
                usuario = await _usuarioRepo.ObterPorCpf(cpfLimpo);
                if (usuario == null)
                    return Unauthorized(new ErroResponse("Usuário não encontrado com o CPF informado", "USER_NOT_FOUND"));
            }
            else if (request.NumeroConta.HasValue)
            {
                var conta = await _contaRepo.ObterPorNumero(request.NumeroConta.Value);
                if (conta == null)
                    return Unauthorized(new ErroResponse("Número de conta não encontrado no sistema", "ACCOUNT_NOT_FOUND"));
                usuario = await _usuarioRepo.ObterPorId(conta.IdUsuario);
            }
            else
            {
                return BadRequest(new ErroResponse("Informe o CPF ou o Número da Conta para realizar o login", "MISSING_CREDENTIALS"));
            }
            
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
                return Unauthorized(new ErroResponse("Senha incorreta ou credenciais inválidas", "USER_UNAUTHORIZED"));

            var contas = await _contaRepo.ObterPorUsuario(usuario.IdUsuario);
            var contaPrincipal = contas.FirstOrDefault();
            var idConta = contaPrincipal?.IdContaCorrente ?? "";
            var numeroConta = contaPrincipal?.Numero.ToString() ?? "";
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ChaveSuperSecretaBankMore2026!@#$");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim("IdUsuario", usuario.IdUsuario),
                    new Claim("NomeUsuario", usuario.Nome),
                    new Claim("CpfUsuario", usuario.Cpf),
                    new Claim("IdConta", idConta),
                    new Claim("NumeroConta", numeroConta)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new LoginResponse(tokenHandler.WriteToken(token), usuario.Nome));
        }
        [HttpPost("selecionar-conta")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> SelecionarConta([FromBody] int numeroConta)
        {
            var idUsuario = User.FindFirst("IdUsuario")?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();
            var conta = await _contaRepo.ObterPorNumero(numeroConta);
            if (conta == null || conta.IdUsuario != idUsuario)
                return BadRequest(new ErroResponse("Conta não pertence ao usuário ou não existe", "INVALID_ACCOUNT"));
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ChaveSuperSecretaBankMore2026!@#$");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim("IdUsuario", idUsuario),
                    new Claim("IdConta", conta.IdContaCorrente),
                    new Claim("NumeroConta", conta.Numero.ToString()),
                    new Claim("IdAgencia", conta.IdAgencia)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { TokenOperacional = tokenHandler.WriteToken(token) });
        }
    }
}
