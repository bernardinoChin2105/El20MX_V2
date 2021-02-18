using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.ContaLink
{
    public class ContaLinkModels
    {
        public class BankTransaction
        {
            public Int64 id { get; set; } 
            public string bank { get; set; } //(Banco) - Banco / Caja / Tarjeta donde se realiza el movimiento bancario
            public string date { get; set; } //(Fecha) - Fecha del movimiento bancario(YYYY-MM-DD) 
            public decimal deposit { get; set; } //(Deposito) - En caso de ser un depósito a la cuenta, el monto depositado
            public string description { get; set; } //(Descripción) - Descripción del movimiento bancario
            public string reference { get; set; } //(Referencia) - Referencia del movimiento bancario
            public decimal withdrawal { get; set; } //(Retiro) - En caso de ser un retiro de la cuenta, el monto retirado
        }     
        
        public class ResponseBankTransaction
        {
            public string message { get; set; } 
            public Int32 status { get; set; }
            public BankTransaction transaction_bank { get; set; }
        }
    }
}
