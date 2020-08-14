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
        public static void CreateUser(string name, string idExternal = null)
        {
            try
            {
                string url = "/users";
                var dataUser = new 
                {
                    id_external = idExternal,
                    name = name
                };
                var responseUsers = Paybook.CallServicePaybook(url, dataUser, "Post", true);
                
            }
            catch (Exception ex)
            {

            }
        }

        //Obtener lista de usuarios en paybook
        public static void GetAllUsers()
        {
            try
            {
                string url = "/users";
                var responseUsers = Paybook.CallServicePaybook(url, null, "Get", true);

                //        //Crea un usuario
                //        var responseUserPost = Paybook.CallServicePaybook("users", dataUser, "Post");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
