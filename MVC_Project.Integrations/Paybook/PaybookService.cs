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
            try
            {
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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return user;
        }

        //Obtener lista de usuarios en paybook
        public static List<UserPaybook> GetAllUsers()
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
            try
            {
                string url = "/sessions";
                var dataUser = new
                {
                    id_user = idUser
                };

                var responseUsers = Paybook.CallServicePaybook(url, dataUser, "Post", true);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseUsers);
                var option = model.First(x => x.Key == "response").Value;
                token = (string)option;
                //users = JsonConvert.DeserializeObject<List<UserPaybook>>(rItemValueJson.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return token;
        }

        //Obtener lista de credenciales en paybook
        public static List<CredentialsPaybook> GetCredentials(string idCredential, string organization, string token)
        {
            List<CredentialsPaybook> credentials = new List<CredentialsPaybook>();
            try
            {
                var dataCredential = new
                {
                    id_credential = idCredential,
                    id_site_organization = organization
                };

                string url = "/credentials";
                var responseUsers = Paybook.CallServicePaybook(url, dataCredential, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseUsers);
                var option = model.First(x => x.Key == "response").Value;
                JObject rItemValueJson = (JObject)option;
                credentials = JsonConvert.DeserializeObject<List<CredentialsPaybook>>(rItemValueJson.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return credentials;
        }

        //Obtener lista de cuentas en paybook
        public static List<AccountsPaybook> GetAccounts(string idCredential, string token)
        {            
            List<AccountsPaybook> accounts = new List<AccountsPaybook>();
            try
            {
                var dataCredential = new
                {
                    id_credential = idCredential
                };

                string url = "/accounts";
                var responseUsers = Paybook.CallServicePaybook(url, dataCredential, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseUsers);
                var option = model.First(x => x.Key == "response").Value;
                JObject rItemValueJson = (JObject)option;
                accounts = JsonConvert.DeserializeObject<List<AccountsPaybook>>(rItemValueJson.ToString());
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
                var dataAccount = new
                {
                    id_credential = idCredential,
                    id_account = idAccount,
                    skip = 0,
                    limit = 10,
                    //id_transaction = "",
                    //dt_refresh_from = "", //Timestamp (optional) Filters by transaction refresh date, expected UNIX timestamp
                    //dt_refresh_to = "", //Timestamp (optional) Filters by transaction refresh date, expected UNIX timestamp
                    //dt_transaction_from = "", //Timestamp (optional) Filters by transaction date, expected UNIX timestamp
                    //dt_transaction_to = "", //Timestamp (optional) Filters by transaction date, expected UNIX timestamp
                };

                string url = "/transactions";
                var responseUsers = Paybook.CallServicePaybook(url, dataAccount, "Get", false, token);
                var model = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseUsers);
                var option = model.First(x => x.Key == "response").Value;
                JObject rItemValueJson = (JObject)option;
                transactions = JsonConvert.DeserializeObject<List<TransactionsPaybook>>(rItemValueJson.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return transactions;
        }
    }
}
