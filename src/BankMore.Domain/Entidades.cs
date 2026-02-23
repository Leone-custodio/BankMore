using System;
namespace BankMore.Domain.Entidades
{
    public class Usuario
    {
        public string IdUsuario { get; set; } = Guid.NewGuid().ToString();
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
    public class Agencia
    {
        public string IdAgencia { get; set; } = Guid.NewGuid().ToString();
        public string Numero { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }
    public class ContaCorrente
    {
        public string IdContaCorrente { get; set; } = Guid.NewGuid().ToString();
        public string IdUsuario { get; set; } = string.Empty;
        public string IdAgencia { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string Nome { get; set; } = string.Empty; 
        public int Ativo { get; set; } = 1; 
        public double Saldo { get; set; } = 0;
    }
    public class Movimento
    {
        public string IdMovimento { get; set; } = Guid.NewGuid().ToString();
        public string IdContaCorrente { get; set; } = string.Empty;
        public string DataMovimento { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string TipoMovimento { get; set; } = string.Empty; 
        public double Valor { get; set; }
    }
    public class Transferencia
    {
        public string IdTransferencia { get; set; } = Guid.NewGuid().ToString();
        public string IdContaCorrenteOrigem { get; set; } = string.Empty;
        public string IdContaCorrenteDestino { get; set; } = string.Empty;
        public string DataMovimento { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public double Valor { get; set; }
    }
    public class Tarifa
    {
        public string IdTarifa { get; set; } = Guid.NewGuid().ToString();
        public string IdContaCorrente { get; set; } = string.Empty;
        public string DataMovimento { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public double Valor { get; set; }
    }
    public class Idempotencia
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;
        public string Requisicao { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
    }
}
