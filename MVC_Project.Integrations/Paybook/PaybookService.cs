using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Paybook
{
    public class PaybookService
    {
        /*
         * Metodos para conectar a paybook
         */

        //Crear la cuenta del usuario en paybook
        public static UserPaybook CreateUser(string name, string idExternal = null)
        {
            UserPaybook user = new UserPaybook();

            string url = "/users";
            var dataUser = new
            {
                id_external = idExternal,
                name = name
            };
            var responseUsers = Paybook.CallServicePaybook(url, dataUser, "Post", true);
            var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseUsers);
            var option = model.First(x => x.Key == "response").Value;
            JObject rItemValueJson = (JObject)option;
            user = JsonConvert.DeserializeObject<UserPaybook>(rItemValueJson.ToString());

            return user;
        }

        //Obtener lista de usuarios en paybook
        public static List<UserPaybook> GetAllUsers(string token)
        {
            List<UserPaybook> users = new List<UserPaybook>();
            try
            {
                string url = "/users";
                var responseUsers = Paybook.CallServicePaybook(url, null, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseUsers);
                var option = model.First(x => x.Key == "response").Value;
                JObject rItemValueJson = (JObject)option;
                users = JsonConvert.DeserializeObject<List<UserPaybook>>(rItemValueJson.ToString());
            }           
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return users;
        }

        //Crear token del usuario en paybook
        public static string CreateToken(string idUser)
        {
            string token = string.Empty;

            string url = "/sessions";
            var dataUser = new
            {
                id_user = idUser
            };

            var response = Paybook.CallServicePaybook(url, dataUser, "Post", true);
            var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            var option = model.First(x => x.Key == "response").Value;
            PaybookViewModel tokenModel = JsonConvert.DeserializeObject<PaybookViewModel>(option.ToString());
            return tokenModel.token;
        }

        //Obtener token del usuario en paybook si este ya fue creado anteriormente
        public static bool GetVarifyToken(string token)
        {
            string url = "/sessions/" + token + "/verify";

            var response = Paybook.CallServicePaybook(url, null, "Get", true);
            var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            var option = model.First(x => x.Key == "response").Value;
            return option != null ? Convert.ToBoolean(option) : false;
        }

        //Obtener lista de credenciales en paybook
        // string organization,
        public static List<CredentialsPaybook> GetCredentials(string idCredential, string token)
        {
            List<CredentialsPaybook> credentials = new List<CredentialsPaybook>();
            try
            {

                //var dataCredential = new
                //{
                //    id_credential = idCredential
                //    //,id_site_organization = organization
                //};
                //dataCredential
                string url = "/credentials?id_credential=" + idCredential;
                var response = Paybook.CallServicePaybook(url, null, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                var option = model.First(x => x.Key == "response").Value;
                //JObject rItemValueJson = (JObject)option;
                credentials = JsonConvert.DeserializeObject<List<CredentialsPaybook>>(option.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return credentials;
        }

        //Eliminar credenciales de las cuentas en paybook
        public static bool DeleteCredential(string idCredential, string method, string token)
        {
            bool responseDelete = false;
            try
            {
                string url = "/credentials/"+idCredential;
                var response = Paybook.CallServicePaybook(url, null, method, false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                var option = model.First(x => x.Key == "response").Value;
                //JObject rItemValueJson = (JObject)option;
                responseDelete = (bool)option;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return responseDelete;
        }

        //Obtener lista de cuentas en paybook
        public static List<AccountsPaybook> GetAccounts(string idCredential, string token)
        {            
            List<AccountsPaybook> accounts = new List<AccountsPaybook>();
            try
            {
                string url = "/accounts?id_credential="+ idCredential;
                var response = Paybook.CallServicePaybook(url, null, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                var option = model.First(x => x.Key == "response").Value;
                //JObject rItemValueJson = (JObject)option;
                accounts = JsonConvert.DeserializeObject<List<AccountsPaybook>>(option.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return accounts;
        }

        //Obtener lista de transacciones de las cuentas en paybook
        public static List<TransactionsPaybook> GetTransactions(string idCredential, string idAccount, string token)
        {            
            List<TransactionsPaybook> transactions = new List<TransactionsPaybook>();
            try
            {
                //transactions?id_credential=5f435fe9f9ad2a2e5e50e80a&id_account=5703f88323428348328b45eb&skip=Number&limit=Number
                string url = "/transactions?id_credential="+idCredential+"&id_account="+idAccount+ "&skip=Number&limit=Number";
                var response = Paybook.CallServicePaybook(url, null, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                var option = model.First(x => x.Key == "response").Value;
                //JObject rItemValueJson = (JObject)option;
                transactions = JsonConvert.DeserializeObject<List<TransactionsPaybook>>(option.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return transactions;
        }       
    }
}
