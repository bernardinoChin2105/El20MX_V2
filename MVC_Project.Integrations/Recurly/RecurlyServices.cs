//using Recurly;
//using Recurly.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
    public class RecurlyServices
    {
        public static AccountResponseModel CreateAccount(object model, string RFC, string site_id)
        {
            AccountResponseModel response = new AccountResponseModel();

            try
            {
                return response;
            }catch(Exception ex)
            {
                return response;
            }
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
