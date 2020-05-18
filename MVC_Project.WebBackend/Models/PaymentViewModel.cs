using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class PaymentViewModel
    {
        public string AppKey { get; set; }

        public string AppName { get; set; }
        public string Id { get; set; }

        public string PaymentMethod { get; set; }

        public string OrderId { get; set; }
        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string PaymentCardURL { get; set; }

        public string TokenId { get; set; }

        public string DeviceSessionId { get; set; }

        public string JsonData { get; set; }

        public string BankName { get; set; }

        public string Clabe { get; set; }

        public string Reference { get; set; }

        public string Name { get; set; }

        public string Agreement { get; set; }

        public bool ChargeSuccess { get; set; }

        public string ConfirmationEmail { get; set; }

        //FOR DISPLAY RESULTS
        public string CreationDate { get; set; }
        public string ConfirmationDate { get; set; }
        public string ProviderId { get; set; }
        public string Status { get; set; }
        public string User { get; set; }
        public string FilterInitialDate { get; set; }
        public string FilterEndDate { get; set; }

        //FOR OPENPAY
        public string PublicKey { get; set; }
        public string MerchantId { get; set; }
    }
}