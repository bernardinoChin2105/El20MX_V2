using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
    //class RecurlyModels
    //{
    //}

    public class CostModel
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }
    
    public class AcquisitionModel
    {
        public CostModel cost { get; set; }
        public string channel { get; set; }
        public string subchannel { get; set; }
        public string campaign { get; set; }
    }

    public class ShippingAddressesModel
    {        
        public string nickname { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public string vat_number { get; set; }
        public string phone { get; set; }
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
    }

    public class AddressModel
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
    }

    public class CustomFieldsModel
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class BillingInfoModel
    {
        public string token_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string number { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public string cvv { get; set; }
        public string vat_number { get; set; }
        public string ip_address { get; set; }
        public string gateway_token { get; set; }
        public string gateway_code { get; set; }
        public string amazon_billing_agreement_id { get; set; }
        public string paypal_billing_agreement_id { get; set; }
        public string fraud_session_id { get; set; }
        public string transaction_type { get; set; } //moto
        public string three_d_secure_action_result_token_id { get; set; }
        public string iban { get; set; }
        public string name_on_account { get; set; }
        public string account_number { get; set; }
        public string routing_number { get; set; }
        public string sort_code { get; set; }
        public string type { get; set; } //bacs
        public string account_type { get; set; } //Enum
        public string tax_identifier { get; set; }
        public string tax_identifier_type { get; set; } //cpf
        public bool primary_payment_method { get; set; }
        public AddressModel address { get; set; }
    }

    public class CreateAccountModel
    {
        public string code { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string preferred_locale { get; set; } //Enum
        public string cc_emails { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string vat_number { get; set; }
        public bool tax_exempt { get; set; }
        public string exemption_certificate { get; set; }
        public string parent_account_code { get; set; }
        public string parent_account_id { get; set; }
        public string bill_to { get; set; } //Enum
        public string transaction_type { get; set; } //Value "moto"

        public AcquisitionModel acquisition { get; set; }
        public List<ShippingAddressesModel> shipping_addresses { get; set; }
        public AddressModel address { get; set; }
        public BillingInfoModel billing_info { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }
    }

    //Modelos de Respuesta
    public class ShippingAddressesResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string account_id { get; set; }
        public string nickname { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public string vat_number { get; set; }
        public string phone { get; set; }
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }   
        public string created_at { get; set; }//Puede ser un datetime
        public string updated_at { get; set; }//puede ser un datetime
    }

    public class PaymentMethodModel
    {
        //public string object { get; set; } //Enum
        public string card_type { get; set; } //Enum
        public string first_six { get; set; }
        public string last_four { get; set; }
        public string last_two { get; set; } //Enum
        public Int32 exp_month { get; set; }
        public Int32 exp_year { get; set; }
        public string gateway_token { get; set; }
        public string gateway_code { get; set; }
        public string billing_agreement_id { get; set; }
        public string name_on_account { get; set; }
        public string account_type { get; set; } //Enum
        public string routing_number { get; set; }
        public string routing_number_bank { get; set; }
    }

    public class FraudModel
    {
        public Int32 score { get; set; }
        public string decision { get; set; } //Enum
        public object risk_rules_triggered { get; set; }
    }

    public class UpdatedByModel
    {
        public string ip { get; set; }
        public string country { get; set; }
    }

    public class BillingInfoResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string account_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string vat_number { get; set; }
        public bool valid { get; set; }
        public bool primary_payment_method { get; set; }        
        public string created_at { get; set; } //datetime
        public string updated_at { get; set; }//datetime
        public AddressModel address { get; set; }
        public PaymentMethodModel payment_method { get; set; }
        public FraudModel fraud { get; set; }
        public UpdatedByModel updated_by { get; set; }      
    }

    public class AccountResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string state { get; set; }//Enum "active"/"inactive"
        public string hosted_login_token { get; set; } 
        public bool has_live_subscription { get; set; }
        public bool has_active_subscription { get; set; }
        public bool has_future_subscription { get; set; }
        public bool has_canceled_subscription { get; set; }
        public bool has_paused_subscription { get; set; }
        public bool has_past_due_invoice { get; set; }
        public string created_at { get; set; } //Es un datetime
        public string updated_at { get; set; } //Es un datetime
        public string deleted_at { get; set; } //Es un datetime
        public string code { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string preferred_locale { get; set; } //Enum
        public string cc_emails { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string vat_number { get; set; }
        public bool tax_exempt { get; set; }
        public string exemption_certificate { get; set; }
        public string parent_account_id { get; set; }
        public string bill_to { get; set; } //Enum
        public List<ShippingAddressesResponseModel> shipping_addresses { get; set; }
        public AddressModel address { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }     
    }
}
