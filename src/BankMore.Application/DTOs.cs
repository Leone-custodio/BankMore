using System;
namespace BankMore.Application.DTOs
{
    public record UsuarioRequest(string Nome, string Cpf, string Celular, string Endereco, string Senha);
    public record AgenciaRequest(string Numero, string Nome);
    public record CadastroContaRequest(string IdAgencia, string NomeTitular, string Cpf, string Senha);
    public record LoginRequest(string? Cpf, int? NumeroConta, string Senha);
    public record LoginResponse(string Token, string NomeUsuario);
    public record CancelarContaRequest(string Senha, int NumeroConta);
    public record DepositoRequest(double Valor);
    public record SaqueRequest(double Valor);
    public record MovimentacaoRequest(int? NumeroConta, double Valor, string TipoMovimento);
    public record TransferenciaRequest(int NumeroContaDestino, double Valor);
    public record SaldoResponse(int NumeroConta, string NomeTitular, string DataHoraConsulta, double ValorSaldoAtual);
    public record ErroResponse(string Mensagem, string TipoFalha);
    public record TransferenciaKafkaMessage(string IdRequisicao, string IdContaLogada);
    public record TarifacaoKafkaMessage(string IdContaCorrente, double ValorTarifado);
}
