using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Payments
{
    public interface IPaymentServiceProvider
    {
        string MerchantId { get; set; }
        string PublicKey { set; get; }
        string PrivateKey { get; set; }
        string DashboardURL { set; get; }

        PaymentModel CreateBankTransferPayment(PaymentModel payment);
        PaymentModel CreateTDCPayment(PaymentModel payment);
    }
}
