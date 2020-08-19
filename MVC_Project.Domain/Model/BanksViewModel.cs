using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Model
{
    public class BanksViewModel
    {
    }

    public class BankCredentialsList
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string credentialProviderId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }
        public Int64 accountId { get; set; }
        public Int64 banckId { get; set; }
        public string Name { get; set; }
        public Int32 Total { get; set; }
    }

    public class BankAccountsList
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        
        public string accountProviderId { get; set; }
        public string accountProviderType { get; set; }
        public string name { get; set; }
        public double balance { get; set; }
        public string currency { get; set; }
        public string number { get; set; }
        public Int32 isDisable { get; set; }
        public DateTime refreshAt { get; set; }
        public string clabe { get; set; }
        public Int64 bankCredentialId { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
        public string status { get; set; }

        public Int32 Total { get; set; }
    }
}
