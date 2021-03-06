﻿//using Recurly;
//using MVC_Project.Integrations.Recurly.Models;
using MVC_Project.Integrations.Recurly.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
    public class RecurlyServices
    {
        /*con este método se crean los planes*/
        public static AccountResponseModel CreateAccount(dynamic model, string siteId)
        {
            AccountResponseModel recurlyModel = new AccountResponseModel();

            string url = "sites/" + siteId + "/accounts";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, model, "Post");

            recurlyModel = JsonConvert.DeserializeObject<AccountResponseModel>(responseRecurly);

            return recurlyModel;               
        }

        public static AccountsListResponse GetAccounts(string siteId, int pageLimit = 1)
        {
            AccountsListResponse accountsListResponse = new AccountsListResponse();
            string url = "sites/" + siteId + "/accounts";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, new { limit = pageLimit } , "Get");

            accountsListResponse = JsonConvert.DeserializeObject<AccountsListResponse>(responseRecurly);

            return accountsListResponse;
        }

        public static AccountsListResponse GetNextAccountsPage(string nextUrl)
        {
            AccountsListResponse accountsListResponse = new AccountsListResponse();

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(nextUrl, null, "Get");

            accountsListResponse = JsonConvert.DeserializeObject<AccountsListResponse>(responseRecurly);

            return accountsListResponse;
        }

        /*Crear una nueva suscripción*/
        public static PlanModel GetPlans(string siteId, bool activesOnly = true)
        {
            PlanModel recurlyModel = new PlanModel();

            string url = "sites/" + siteId + "/plans";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, activesOnly ? new { state = "active" } : null, "Get");

            recurlyModel = JsonConvert.DeserializeObject<PlanModel>(responseRecurly);

            return recurlyModel;
        }

        public static SubscripcionResponseModel CreateSubscription(SubscriptionCreateModel subscriptionModel,string siteId)
        {
            SubscripcionResponseModel recurlyModel = new SubscripcionResponseModel();

            string url = "sites/" + siteId + "/subscriptions";
            
            var responseRecurly = Recurly.CallServiceRecurly(url, subscriptionModel, "POST");

            recurlyModel = JsonConvert.DeserializeObject<SubscripcionResponseModel>(responseRecurly);

            return recurlyModel;
        }

        public static SubscriptionChange CreateSubscriptionChange(SubscriptionChangeCreate changeCreateModel, string siteId, string subscription_id)
        {
            SubscriptionChange recurlyModel = new SubscriptionChange();

            string url = "sites/" + siteId + "/subscriptions/" + subscription_id + "/change";

            var responseRecurly = Recurly.CallServiceRecurly(url, changeCreateModel, "POST");

            recurlyModel = JsonConvert.DeserializeObject<SubscriptionChange>(responseRecurly);

            return recurlyModel;
        }

        public static InvoiceCollection CreatePurchase(PurchaseCreate purchaseCreate, string siteId)
        {
            InvoiceCollection recurlyModel = new InvoiceCollection();

            string url = "sites/" + siteId + "/purchases";

            var responseRecurly = Recurly.CallServiceRecurly(url, purchaseCreate, "POST");

            recurlyModel = JsonConvert.DeserializeObject<InvoiceCollection>(responseRecurly);

            return recurlyModel;
        }

        public static AccountSubscriptionsModel GetAccountSuscriptions(string siteId, string account_id, bool activesOnly = true)
        {
            AccountSubscriptionsModel recurlyModel = new AccountSubscriptionsModel();

            string url = "sites/" + siteId + "/accounts/" + account_id + "/subscriptions";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, activesOnly ? new { state = "active" } : null, "Get");

            recurlyModel = JsonConvert.DeserializeObject<AccountSubscriptionsModel>(responseRecurly);

            return recurlyModel;
        }

        public static CouponRedemption CreateCouponRedemption(CouponRedemptionCreate body, string siteId, string account_id)
        {
            CouponRedemption recurlyModel = new CouponRedemption();

            string url = "sites/" + siteId + "/accounts/" + account_id + "/coupon_redemptions/active";
              
            var responseRecurly = Recurly.CallServiceRecurly(url, body, "POST");

            recurlyModel = JsonConvert.DeserializeObject<CouponRedemption>(responseRecurly);

            return recurlyModel;
        }
        
        public static Account DeleteAccount(string accountId,string siteId)
        {
            Account accountModel = new Account();

            string url = "sites/" + siteId + "/accounts/" + accountId;            

            //Llamar al servicio para cerrar la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, null, "Delete");
            accountModel = JsonConvert.DeserializeObject<Account>(responseRecurly);

            return accountModel;
        }

        public static Invoice GetAccount(string accountId, string siteId)
        {
            Invoice invoiceModel = new Invoice();

            string url = "sites/" + siteId + "/accounts/" + accountId + "invoices";

            //Llamar al servicio para obtener la cuenta en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, null, "Get");
            invoiceModel = JsonConvert.DeserializeObject<Invoice>(responseRecurly);

            return invoiceModel;
        }

        public static Invoice GetInvoice(string invoiceNumber, string siteId)
        {
            Invoice recurlyModel = new Invoice();
            string url = "sites/" + siteId + "/invoices/number-" + invoiceNumber;

            var responseRecurly = Recurly.CallServiceRecurly(url, null, "GET");

            recurlyModel = JsonConvert.DeserializeObject<Invoice>(responseRecurly);

            return recurlyModel;
        }

        public static TransactionsListResponse GetTransactions(string type, string success, int limit, string siteId)
        {
            TransactionsListResponse recurlyModel = new TransactionsListResponse();
            string url = "sites/" + siteId + "/transactions";

            var responseRecurly = Recurly.CallServiceRecurly(url, new { type, success, limit }, "GET");

            return JsonConvert.DeserializeObject<TransactionsListResponse>(responseRecurly);
        }

        public static TransactionsListResponse GetNexTransactions(string url)
        {
            TransactionsListResponse recurlyModel = new TransactionsListResponse();

            var responseRecurly = Recurly.CallServiceRecurly(url, null, "GET");

            return JsonConvert.DeserializeObject<TransactionsListResponse>(responseRecurly);
        }

        public static AccountResponseModel UpdateAccount(dynamic model, string accountId, string siteId)
        {
            AccountResponseModel recurlyModel = new AccountResponseModel();

            string url = "sites/" + siteId + "/accounts/" + accountId;

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, model, "Put");

            recurlyModel = JsonConvert.DeserializeObject<AccountResponseModel>(responseRecurly);

            return recurlyModel;
        }

        //Obtener todas las listas de facturas del mes
        public static InvoiceListResponse GetInvoiceAll(string startDate, string endDate, string siteId) 
        {
            InvoiceListResponse recurlyModel = new InvoiceListResponse();
            string url = "sites/" + siteId + "/invoices?order=desc&sort=created_at&begin_time=" + startDate + "&end_time=" + endDate;

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, null, "Get");

            recurlyModel = JsonConvert.DeserializeObject<InvoiceListResponse>(responseRecurly);

            return recurlyModel;
        }

        public static InvoiceListResponse GetNextInvoicesAll(string url)
        {
            InvoiceListResponse recurlyModel = new InvoiceListResponse();

            var responseRecurly = Recurly.CallServiceRecurly(url, null, "GET");

            return JsonConvert.DeserializeObject<InvoiceListResponse>(responseRecurly);
        }
    }
}
