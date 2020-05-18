using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Payments
{
    public class PaymentModel
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public string OrderId { get; set; }
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public string TransactionType { get; set; }

        public string Status { get; set; }

        public string PaymentCardURL { get; set; }

        public string TokenId { get; set; }

        public string DeviceSessionId { get; set; }
        
        public string ResultData { get; set; }

        public int ResultCode { get; set; }

        public string ResultCategory{ get; set; }

        public bool ChargeSuccess { get; set; }

        public bool Use3DSecure { get; set; }
        
        public string RedirectUrl { get; set; }

        public string Description { get; set; }

        public PaymentMethodModel PaymentMethod { get; set; }
}

    public class PaymentMethodModel
    {
        public string Type { get; set; }
        public string BankName { get; set; }

        public string Clabe { get; set; }

        public string Reference { get; set; }

        public string Name { get; set; }
        public string Agreement { get; set; }

        public string RedirectUrl { get; set; }
    }

        public static class PaymentMethod
    {
         public const string
            CARD = "card",
            BANK_ACCOUNT = "bank_account";
    }

    public static class PaymentStatus
    {
        public const string
           IN_PROGRESS = "in_progress",
           COMPLETED = "completed",
           ERROR = "Error";
    }

    public static class PaymentError
    {
        public const int
           ANTI_FRAUD = 3005,
           REJECTED = 3001
           ;
    }

    public static class PaymentEventStatus
    {
        public const string
           CHARGE_SUCCEEDED = "charge.succeeded";
    }

    public static class PaymentType
    {
        public const string
           CHARGE = "charge";
    }
}
