using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Payments
{
    public class PaymentEventModel
    {
        public string id { get; set; }
        public string type { get; set; }

        public DateTime? event_date { get; set; }

        public DateTime? creation_date { get; set; }

        public PaymentTransactionModel transaction { get; set; }

        public string verification_code { get; set; }
        
    }


    public class PaymentTransactionModel
    {
        public string id { get; set; }
        public string authorization { get; set; }

        public string operation_type { get; set; }

        public string method { get; set; }

        public string transaction_type { get; set; }

        public string status { get; set; }

        public bool conciliated { get; set; }

        public string creation_date { get; set; }

        public string operation_date { get; set; }

        public string description { get; set; }

        public string error_message { get; set; }
        public string order_id { get; set; }
        public string customer_id { get; set; }
        public string due_date { get; set; }

        public string currency { get; set; }
        public decimal amount { get; set; }

        //payment_method
        //fee
    }
}
