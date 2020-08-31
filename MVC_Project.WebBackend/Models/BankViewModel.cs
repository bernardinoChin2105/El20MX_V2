using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class BankViewModel
    {

        [Display(Name = "Banco")]
        public Int64? BankName { get; set; }
        public SelectList ListBanks { get; set; }

        [Display(Name = "No. Cuenta")]
        public Int64? NumberBankAccount { get; set; }
        public SelectList ListNumberBankAccount { get; set; }

        [Display(Name = "Movimientos")]
        public Int64? Movements { get; set; }
        public SelectList ListMovements { get; set; }

        //Falta las campos de fecha
        [Display(Name = "Fecha")]
        public DateTime? RegisterAt { get; set; }

        public string FilterInitialDate { get; set; }
        public string FilterEndDate { get; set; }

        [Display(Name = "Divisa")]
        public string Currency { get; set; }

        [Display(Name = "Saldo Actual")]
        public string CurrentBalance { get; set; }

        [Display(Name = "Total de Retiros")]
        public string TotalAmount { get; set; }

        [Display(Name = "Total de Depósitos")]
        public string TotalDeposits { get; set; }

        [Display(Name = "Total de Depósitos")]
        public string FinalBalance { get; set; }


        public BankViewModel()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Todos...", Value = "-1" });

            ListBanks = new SelectList(list);
            ListNumberBankAccount = new SelectList(list);
            ListMovements = new SelectList(list);            
        }
    }

    public class BankAccountsVM
    {
        public Int64 id { get; set; }
        //public Guid uuid { get; set; }
        public string accountProviderId { get; set; }
        public string accountProviderType { get; set; }
        public string name { get; set; }
        public string balance { get; set; }
        public string currency { get; set; }
        public string number { get; set; }
        public Int32 isDisable { get; set; }
        public DateTime refreshAt { get; set; }
        public string clabe { get; set; }
        public Int64 bankCredentialId { get; set; }
        //public DateTime createdAt { get; set; }
        //public DateTime modifiedAt { get; set; }
        public string status { get; set; }
    }

    public class BankTransactionMV
    {
        public Int64 id { get; set; }
        public string transactionId { get; set; }
        public string description { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string transactionAt { get; set; }
        public string balance { get; set; }
        public string bankAccountName { get; set; }
        public string number { get; set; }
        public string bankName { get; set; }
        public string refreshAt { get; set; }        
    }

    public class BankTransactionTotalVM
    {
        public string currency { get; set; }
        public string TotalAmount { get; set; }
        public string TotalRetirement { get; set; }
        public string TotalDeposits { get; set; }
        public string TotalFinal { get; set; }
    }
}