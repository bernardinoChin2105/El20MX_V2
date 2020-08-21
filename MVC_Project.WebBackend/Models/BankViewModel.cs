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
        public DateTime RegisterAt { get; set; }

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
}