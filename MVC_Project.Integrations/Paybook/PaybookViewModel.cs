using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Paybook
{
    public class PaybookViewModel
    {
        public string token { get; set; }
    }

    public class UserPaybook
    {
        public string id_user { get; set; }
        public string id_external { get; set; }
        public string name { get; set; }
        public string dt_create { get; set; }
        public string dt_modify { get; set; }
    }

    public class CredentialsPaybook
    {
        public string id_credential { get; set; }
        public string id_user { get; set; }
        public string id_environment { get; set; }
        public string id_external { get; set; }
        public string id_site { get; set; }
        public string id_site_organization { get; set; }
        public string id_site_organization_type { get; set; }
        public string id_organization { get; set; }
        public int? is_authorized { get; set; }
        public int is_locked { get; set; }
        public int is_twofa { get; set; }
        public int can_sync { get; set; }
        public int ready_in { get; set; }
        public string username { get; set; }
        public int code { get; set; }
        //public List<string> keywords { get; set; }
        public string dt_authorized { get; set; }
        public string dt_execute { get; set; }
        public string dt_ready { get; set; }
        public string dt_refresh { get; set; }
    }

    public class AccountsPaybook
    {
        public string id_account { get; set; }
        public string id_account_type { get; set; }
        public string id_currency { get; set; }
        public string id_user { get; set; }
        public string id_external { get; set; }
        public string id_credential { get; set; }
        public string id_site { get; set; }
        public string id_site_organization { get; set; }
        public string id_site_organization_type { get; set; }
        public int is_disable { get; set; }
        public string name { get; set; }
        public string number { get; set; }
        public double balance { get; set; }
        public string currency { get; set; }
        public string account_type { get; set; }
        public SitePaybook site { get; set; }
        //public ExtraAccountPaybook extra { get; set; }
        //public List<string> keywords { get; set; }
        public long dt_refresh { get; set; }
    }

    public class SitePaybook
    {
        public string id_site { get; set; }
        public string id_site_organization { get; set; }
        public string id_site_organization_type { get; set; }
        public string name { get; set; }
        public string organization { get; set; }
        public string avatar { get; set; }
        public string cover { get; set; }
        public string small_cover { get; set; }
        public string time_zone { get; set; }
    }

    public class ExtraAccountPaybook
    {
        public string owner { get; set; }
        public string owner_address { get; set; }
        public int available { get; set; }
    }

    public class TransactionsPaybook
    {
        public string id_transaction { get; set; }
        public string id_account { get; set; }
        public string id_account_type { get; set; }
        public string id_credential { get; set; }
        public string id_currency { get; set; }
        public string id_disable_type { get; set; }
        public string id_external { get; set; }
        public string id_site { get; set; }
        public string id_site_organization { get; set; }
        public string id_site_organization_type { get; set; }
        public string id_user { get; set; }
        public int is_account_disable { get; set; }
        public int is_deleted { get; set; }
        public int is_disable { get; set; }
        public int is_pending { get; set; }
        public string description { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        //public List<string> attachments { get; set; } temporalmente comentado
        //public ExtraTransactionPaybook extra { get; set; }
        public string reference { get; set; }
        //public List<string> keywords { get; set; }
        public long dt_transaction { get; set; }
        public long dt_refresh { get; set; }
        public long? dt_disable { get; set; }
        public long? dt_deleted { get; set; }
    }

    public class ExtraTransactionPaybook
    {
        public long? dt_disable { get; set; }
        public long? dt_deleted { get; set; }
    }
}
