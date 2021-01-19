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

        [Display(Name = "Saldo al Corte")]
        public string CurrentBalance { get; set; }

        [Display(Name = "Total de Retiros")]
        public string TotalAmount { get; set; }

        [Display(Name = "Total de Depósitos")]
        public string TotalDeposits { get; set; }

        [Display(Name = "Saldo Final")]
        public string FinalBalance { get; set; }


        public BankViewModel()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Todos", Value = "-1" });

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

    public class BankCredentialsMV
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string credentialProviderId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public Int64 accountId { get; set; }
        public Int64 banckId { get; set; }
        public string Name { get; set; }
        public string NameSite { get; set; }
        public string siteId { get; set; }
        public bool isTwofa { get; set; }
        public string dateTimeAuthorized { get; set; }
        public string dateTimeRefresh { get; set; }
        public int code { get; set; }
    }

    public class BankTransactionMV
    {
        public Int64 id { get; set; }
        public string transactionId { get; set; }
        public string description { get; set; }
        public string amountR { get; set; }
        public string amountD { get; set; }
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

    //public class BankSite
    //{
    //    public string id_site { get; set; }
    //    public string id_site_type { get; set; }
    //    public string name { get; set; }
    //}

    //public class AllBankSites
    //{
    //    public string id_site_organization { get; set; }
    //    public string id_site_organization_type { get; set; }
    //    public string id_country { get; set; }
    //    public string name { get; set; }        
    //    public List<BankSite> sites { get; set; }        
    //}
}