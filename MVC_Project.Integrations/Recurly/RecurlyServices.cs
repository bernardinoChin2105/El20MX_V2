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
        public static AccountResponseModel CreateAccount(dynamic model, string siteId)
        {
            AccountResponseModel recurlyModel = new AccountResponseModel();

            string url = "sites/" + siteId + "/accounts";

            //Llamar al servicio para crear la credencial en el recurly y obtener respuesta                  
            var responseRecurly = Recurly.CallServiceRecurly(url, model, "Post");

            recurlyModel = JsonConvert.DeserializeObject<AccountResponseModel>(responseRecurly);

            return recurlyModel;

            //try
            //{
            //    var accountReq = new AccountCreate()
            //    {
            //        Code = "64564564564564",
            //        FirstName = "Benjamin",
            //        LastName = "Du Monde",
            //        Address = new Address()
            //        {
            //            City = "New Orleans",
            //            Region = "LA",
            //            Country = "US",
            //            PostalCode = "70115",
            //            Street1 = "900 Camp St."
            //        }
            //    };
            //    Account account = client.CreateAccount(accountReq);
            //    Console.WriteLine($"Created account {account.Code}");
            //}
            //catch (Recurly.Errors.Validation ex)
            //{
            //    // If the request was not valid, you may want to tell your user
            //    // why. You can find the invalid params and reasons in ex.Error.Params
            //    Console.WriteLine($"Failed validation: {ex.Error.Message}");
            //}
            //catch (Recurly.Errors.ApiError ex)
            //{
            //    // Use ApiError to catch a generic error from the API
            //    Console.WriteLine($"Unexpected Recurly Error: {ex.Error.Message}");
            //}            
        }
    }
}
