﻿using MVC_Project.Integrations.Recurly.Models;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
    public class RecurlyService
    {
        //public static string GetSite(string siteId, string provider)
        //{
        //    if (provider == SystemProviders.RECURLY.ToString())
        //    {
        //        //llamadas al metodo, se descargo la librería, en dado caso que no funcione se tiene la clase donde esta la llamada a la api
        //        return "ejemplo";
        //    }
        //    else
        //    {
        //        throw new Exception( "Ejemplo");
        //    }
        //}

        public static AccountResponseModel CreateAccount(dynamic request, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {                
                var recurlyModel = RecurlyServices.CreateAccount(request, siteId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static AccountsListResponse GetAccounts(string siteId)
        {
            var recurlyModel = RecurlyServices.GetAccounts(siteId, 100);

            return recurlyModel;
        }

        public static AccountsListResponse GetNextAccountsPage(string nextUrl)
        {
            var recurlyModel = RecurlyServices.GetNextAccountsPage(nextUrl);

            return recurlyModel;
        }

        public static PlanModel GetPlans(string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.GetPlans(siteId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static AccountSubscriptionsModel GetAccountSuscriptions(string siteId, string accountId)
        {
            var recurlyModel = RecurlyServices.GetAccountSuscriptions(siteId, accountId);

            return recurlyModel;
        }

        public static SubscripcionResponseModel CreateSubscription(SubscriptionCreateModel subscriptionModel, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.CreateSubscription(subscriptionModel, siteId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static SubscriptionChange CreateSubscriptionChange(SubscriptionChangeCreate subscriptionModel, string siteId, string subsciptionId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.CreateSubscriptionChange(subscriptionModel, siteId, subsciptionId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static InvoiceCollection CreatePurchase(PurchaseCreate purchaseCreate, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.CreatePurchase(purchaseCreate, siteId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static CouponRedemption CreateCouponRedemption(CouponRedemptionCreate couponRedemption, string siteId, string accountId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.CreateCouponRedemption(couponRedemption, siteId, accountId);

                return recurlyModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso.");
            }
        }
        
        public static Account DeleteAccount(string accountId, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.DeleteAccount(accountId, siteId);

                return recurlyModel; //new AccountResponseModel { id = recurlyModel.id, state = recurlyModel.state };
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static Invoice GetAccount(string accountId, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.GetAccount(accountId, siteId);
                return recurlyModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static Invoice GetInvoice(string invoiceNumber, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                return RecurlyServices.GetInvoice(invoiceNumber, siteId);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static TransactionsListResponse GetVerifiedTransactions(string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                return RecurlyServices.GetTransactions("verify", "true", 100, siteId);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static TransactionsListResponse GetNextTransactions(string url, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                return RecurlyServices.GetNexTransactions(url);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        public static AccountResponseModel UpdateAccount(dynamic request, string accountId, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                var recurlyModel = RecurlyServices.UpdateAccount(request, accountId, siteId);
                return recurlyModel;
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }

        //Obtener los invoices realizados en el mes
        public static InvoiceListResponse GetInvoiceAll(string startDate, string endDate, string siteId, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                return RecurlyServices.GetInvoiceAll(startDate, endDate, siteId);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly.");
            }
        }

        public static InvoiceListResponse GetNextInvoiceAll(string url, string provider)
        {
            if (provider == SystemProviders.RECURLY.ToString())
            {
                return RecurlyServices.GetNextInvoicesAll(url);
            }
            else
            {
                throw new Exception("No se encontró un proveedor de acceso de recurly");
            }
        }
    }
}
