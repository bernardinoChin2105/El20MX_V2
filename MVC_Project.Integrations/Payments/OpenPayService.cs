using MVC_Project.Integrations.Payments;
using Openpay;
using Openpay.Entities;
using Openpay.Entities.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Payments
{
    public class OpenPayService : IPaymentServiceProvider
    {
        readonly bool IsProductionEnvironment;
        readonly string Agreement;

        public string DashboardURL { set; get; }
        public string MerchantId { set; get; }
        public string PublicKey { set; get; }
        public string PrivateKey { get; set; }

        public OpenPayService()
        {
            IsProductionEnvironment = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["Payments.IsProductionEnvironment"]);
            /*
            DashboardURL = System.Configuration.ConfigurationManager.AppSettings["Payments.DashboardURL"];
            PublicKey = System.Configuration.ConfigurationManager.AppSettings["Payments.PublicKey"];
            PrivateKey = System.Configuration.ConfigurationManager.AppSettings["Payments.OpenpayKey"];
            MerchantId = System.Configuration.ConfigurationManager.AppSettings["Payments.MerchantId"];
            Agreement = System.Configuration.ConfigurationManager.AppSettings["Payments.OpenpayAgreement"];
            */
        }


        public PaymentModel CreateBankTransferPayment(PaymentModel payment)
        {
            OpenpayAPI openpayAPI = new OpenpayAPI(PrivateKey, MerchantId);
            openpayAPI.Production = IsProductionEnvironment;
            try
            {
                Customer customer = openpayAPI.CustomerService.Get(payment.ClientId);

                ChargeRequest request = new ChargeRequest
                {
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    DueDate = payment.DueDate,
                    Method = PaymentMethod.BANK_ACCOUNT,
                    Description = payment.Description,
                    Customer = customer
                };
            
                Charge charge = openpayAPI.ChargeService.Create(request);

                payment.Id = charge.Id;
                payment.DueDate = request.DueDate;
                payment.Status = charge.Status;
                payment.TransactionType = charge.TransactionType;
                payment.PaymentCardURL = DashboardURL + "/spei-pdf/" + MerchantId + "/" + charge.Id;
                payment.ResultData = charge.ToJson();
                payment.ChargeSuccess = true;

                payment.PaymentMethod = new PaymentMethodModel
                {
                    Type = charge.PaymentMethod.Type,
                    BankName = charge.PaymentMethod.BankName,
                    Clabe = charge.PaymentMethod.CLABE,
                    Reference = charge.PaymentMethod.Reference,
                    Name = charge.PaymentMethod.Name,
                    Agreement = Agreement
                };

            }
            catch (OpenpayException ex)
            {
                payment.ChargeSuccess = false;
                payment.ResultCode = ex.ErrorCode;
                payment.ResultData = ex.Description;
                payment.ResultCategory = ex.Category;
            }

            return payment;

        }

        public PaymentModel CreateTDCPayment(PaymentModel payment)
        {
            OpenpayAPI openpayAPI = new OpenpayAPI(PrivateKey, MerchantId);
            openpayAPI.Production = IsProductionEnvironment;

            try
            {

                Customer customer = openpayAPI.CustomerService.Get(payment.ClientId);

                ChargeRequest request = new ChargeRequest
                {
                    Method = PaymentMethod.CARD,
                    SourceId = payment.TokenId,
                    Amount = payment.Amount,
                    OrderId = payment.OrderId,
                    Description = payment.Description,
                    DeviceSessionId = payment.DeviceSessionId,
                    Customer = customer,
                    Use3DSecure = payment.Use3DSecure,
                    RedirectUrl = payment.RedirectUrl
                };
                
                Charge charge = openpayAPI.ChargeService.Create(request);

                payment.Id = charge.Id;
                payment.DueDate = request.DueDate;
                payment.Status = charge.Status;
                payment.TransactionType = charge.TransactionType;
                payment.PaymentCardURL = DashboardURL + "/spei-pdf/" + MerchantId + "/" + charge.Id;
                payment.ResultData = charge.ToJson();
                payment.ChargeSuccess = true;

                if (charge.PaymentMethod!=null /*&& charge.PaymentMethod.Type == "redirect"*/)
                {
                    payment.PaymentMethod = new PaymentMethodModel
                    {
                        Type = charge.PaymentMethod.Type,
                        BankName = charge.PaymentMethod.BankName,
                        Clabe = charge.PaymentMethod.CLABE,
                        Reference = charge.PaymentMethod.Reference,
                        Name = charge.PaymentMethod.Name,
                        Agreement = Agreement,
                        RedirectUrl = charge.PaymentMethod.Url
                    };
                    
                }

            }
            catch (OpenpayException ex)
            {
                payment.ChargeSuccess = false;
                payment.ResultCode = ex.ErrorCode;
                payment.ResultData = ex.Description;
                payment.ResultCategory = ex.Category;
            }

            return payment;
        }
    }
}
