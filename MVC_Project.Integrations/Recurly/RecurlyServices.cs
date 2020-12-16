//using Recurly;
//using Recurly.Resources;
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

        /*Crear una nueva suscripción*/
        public static PlanModel GetPlans(string siteId)
        {
            PlanModel recurlyModel = new PlanModel();

            string url = "sites/" + siteId + "/plans";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, null, "Get");

            recurlyModel = JsonConvert.DeserializeObject<PlanModel>(responseRecurly);

            return recurlyModel;
        }

        public static SubscripcionResponseModel CreateSubscription(dynamic subscription,string siteId)
        {
            SubscripcionResponseModel recurlyModel = new SubscripcionResponseModel();

            string url = "sites/" + siteId + "/subscriptions";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, null, "Get");

            recurlyModel = JsonConvert.DeserializeObject<SubscripcionResponseModel>(responseRecurly);

            return recurlyModel;
        }
    }
}
