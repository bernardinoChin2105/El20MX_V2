using System;

namespace MVC_Project.Integrations.Recurly.Models
{
    public class RecurlyErrorException : Exception
    {
        public Error Error {get; set;}

        public RecurlyErrorException(Error error)
        {
            this.Error = error;
        }
    }
}
