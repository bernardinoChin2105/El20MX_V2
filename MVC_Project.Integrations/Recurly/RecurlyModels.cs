using MVC_Project.Integrations.Recurly.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Recurly
{
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

    public class CurrencyModel
    {
        public string currency { get; set; }
        public decimal setup_fee { get; set; }
        public float unit_amount { get; set; }
    }

    public class HostedPagesModel
    {
        public string success_url { get; set; }
        public string cancel_url { get; set; }
        public bool bypass_confirmation { get; set; }
        public bool display_quantity { get; set; }
    }

    public class PlanDataModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string code { get; set; }
        public string state { get; set; } //Enum
        public string name { get; set; }
        public string description { get; set; }
        public string interval_unit { get; set; } //Enum
        public Int32 interval_length { get; set; }
        public string trial_unit { get; set; } //Enum
        public Int32 trial_length { get; set; }
        public bool trial_requires_billing_info { get; set; }
        public Int32 total_billing_cycles { get; set; }
        public bool auto_renew { get; set; }
        public string accounting_code { get; set; }
        public string revenue_schedule_type { get; set; }  //Enum
        public string setup_fee_revenue_schedule_type { get; set; } //Enum
        public string setup_fee_accounting_code { get; set; }
        public Int32 avalara_transaction_type { get; set; }
        public Int32 avalara_service_type { get; set; }
        public string tax_code { get; set; }
        public bool tax_exempt { get; set; }
        public bool allow_any_item_on_subscriptions { get; set; }
        public string created_at { get; set; } //datetime
        public string updated_at { get; set; } //datetime
        public string deleted_at { get; set; } //datetime
        public List<CurrencyModel> currencies { get; set; }
        public HostedPagesModel hosted_pages { get; set; }
    }

    public class PlanModel
    {
        //public string object { get; set; }
        public string has_more { get; set; }
        public string next { get; set; }
        public List<PlanDataModel> data { get; set; }
    }

    public class AccountCreate
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
        public string transaction_type { get; set; } //moto
        public AcquisitionModel acquisition { get; set; }
        public List<ShippingAddressesModel> shipping_addresses { get; set; }
        public AddressModel address { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }
        public BillingInfoModel billing_info { get; set; }
    }

    public class ShippingModel
    {
        public AddressModel address { get; set; }
        public string address_id { get; set; }
        public string method_id { get; set; }
        public string method_code { get; set; }
        public decimal amount { get; set; }
    }

    public class TiersModel
    {
        public Int32 ending_quantity { get; set; }
        public decimal unit_amount { get; set; }
    }

    public class AddOnsModel
    {
        public string code { get; set; }
        public string add_on_source { get; set; }
        public Int32 quantity { get; set; }
        public decimal? unit_amount { get; set; }
        public List<TiersModel> tiers { get; set; }
        public Int32? usage_percentage { get; set; }
        public string revenue_schedule_type { get; set; } //Enum
    }

    public class SubscriptionCreateModel
    {
        public string plan_code { get; set; }
        public string plan_id { get; set; }
        public string billing_info_id { get; set; }
        public string collection_method { get; set; } //Enum
        public string currency { get; set; }
        public decimal? unit_amount { get; set; }
        public Int32 quantity { get; set; }
        public string coupon_code { get; set; }
        public DateTime? trial_ends_at { get; set; } //datetime
        public DateTime? starts_at { get; set; } //datetime
        public DateTime? next_bill_date { get; set; } //datetime
        public Int32 total_billing_cycles { get; set; }
        public int? renewal_billing_cycles { get; set; }
        public bool auto_renew { get; set; }
        public string revenue_schedule_type { get; set; } //Enum
        public string terms_and_conditions { get; set; }
        public string customer_notes { get; set; }
        public string credit_customer_notes { get; set; }
        public string po_number { get; set; }
        public int net_terms { get; set; }
        public string transaction_type { get; set; } //moto
        public AccountCreate account { get; set; }
        public ShippingModel shipping { get; set; }
        public List<AddOnsModel> add_ons { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }

        public SubscriptionCreateModel()
        {
            quantity = 1;
            total_billing_cycles = 1;
            auto_renew = true;
        }
    }

    public class AccountSubsResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; } 
        public string code { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string parent_account_id { get; set; }
        public string bill_to { get; set; }
    }

    public class PlanSubsResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; } 
        public string code { get; set; }
        public string name { get; set; }
    }

    public class ShippingResponseModel
    {
        //public string object { get; set; } 
        public ShipingAddressResponseModel address { get; set; }
        public MethodResponseModel method { get; set; }
        public decimal amount { get; set; }
    }
    public class ShipingAddressResponseModel
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
        public string created_at { get; set; } //datetime
        public string updated_at { get; set; }//datetime
    }

    public class MethodResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class AddOnsResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; } 
        public string subscription_id { get; set; }
        public string add_on_source { get; set; }
        public Int32 quantity { get; set; }
        public decimal unit_amount { get; set; }
        public string revenue_schedule_type { get; set; } //Enum
        public string tier_type { get; set; } //Enum
        public decimal usage_percentage { get; set; }
        public DateTime created_at { get; set; } //datetime
        public DateTime updated_at { get; set; } //datetime
        public DateTime expired_at { get; set; } //datetime
        public List<TiersModel> tiers { get; set; }
        public AddOnResponseModel add_on { get; set; }
    }

    public class AddOnResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; } 
        public string code { get; set; }
        public string name { get; set; }
        public string add_on_type { get; set; } //Enum
        public string usage_type { get; set; } //Enum
        public decimal usage_percentage { get; set; } //datetime
        public string measured_unit_id { get; set; }
        public string item_id { get; set; }
        public string external_sku { get; set; }
        public string accounting_code { get; set; }
    }

    public class CurrencyResponseModel
    {
        public string currency { get; set; }
        public decimal amount { get; set; }
    }

    public class TrialModel
    {
        public string unit { get; set; } //Enum
        public Int32 length { get; set; }
    }

    public class DiscountModel
    {
        public string type { get; set; } //Enum
        public Int32 percent { get; set; }
        public List<CurrencyResponseModel> currencies { get; set; }
        public TrialModel trial { get; set; }
    }

    public class CouponModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string state { get; set; } //Enum
        public DiscountModel discount { get; set; }
        public string coupon_type { get; set; } //Enum
        public string expired_at { get; set; } //datetime
    }

    public class CouponRedemptionsModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public CouponModel coupon { get; set; }
        public string state { get; set; } //Enum
        public decimal discounted { get; set; }
        public string created_at { get; set; } //datetime
    }

    public class PlanResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class AccountSubscriptionsModel
    {

        public bool has_more { get; set; }
        public string next { get; set; }
        public List<Subscription> data { get; set; }
    }

    public class SubscripcionResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; } 
        public string uuid { get; set; }
        public string state { get; set; } //Enum
        public DateTime current_period_started_at { get; set; } //datetime
        public DateTime current_period_ends_at { get; set; } //datetime
        public DateTime? current_term_started_at { get; set; } //datetime
        public DateTime? current_term_ends_at { get; set; } //datetime
        public DateTime? trial_started_at { get; set; } //datetime
        public DateTime? trial_ends_at { get; set; } //datetime
        public Int32 remaining_billing_cycles { get; set; }
        public Int32 total_billing_cycles { get; set; }
        public Int32 renewal_billing_cycles { get; set; }
        public bool auto_renew { get; set; }
        public DateTime? paused_at { get; set; } //datetime
        public int? remaining_pause_cycles { get; set; }
        public string currency { get; set; }
        public string revenue_schedule_type { get; set; } //Enum
        public decimal unit_amount { get; set; }
        public Int32 quantity { get; set; }
        public decimal add_ons_total { get; set; }
        public decimal subtotal { get; set; }
        public string collection_method { get; set; } //Enum
        public string po_number { get; set; }
        public Int32 net_terms { get; set; }
        public string terms_and_conditions { get; set; }
        public string customer_notes { get; set; }
        public string expiration_reason { get; set; }
        public DateTime created_at { get; set; } //datetime
        public DateTime? updated_at { get; set; } //datetime
        public DateTime? activated_at { get; set; } //datetime
        public DateTime? canceled_at { get; set; } //datetime
        public DateTime? expires_at { get; set; } //datetime
        public DateTime? bank_account_authorized_at { get; set; } //datetime
        public string billing_info_id { get; set; } //Enum
        public AccountSubsResponseModel account { get; set; }
        public PlanSubsResponseModel plan { get; set; }
        public ShippingResponseModel shipping { get; set; }
        public List<CouponRedemptionsModel> coupon_redemptions { get; set; }
        public PendingChangeResponseModel pending_change { get; set; }
        public List<AddOnsResponseModel> add_ons { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }
    }

    public class PendingChangeResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string subscription_id { get; set; }
        public PlanResponseModel plan { get; set; }
        float unit_amount { get; set; }
        int quantity { get; set; }
        DateTime activate_at { get; set; }
        bool activated { get; set; }
        string revenue_schedule_type { get; set; }
        string setup_fee_revenue_schedule_type { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }
        public DateTime created_at { get; set; } //datetime
        public DateTime updated_at { get; set; } //datetime
        public DateTime deleted_at { get; set; } //datetime
    }

    public class AccountsListResponse
    {
        public bool has_more { get; set; }
        public string next { get; set; }
        public List<Account> data { get; set; }
    }
}
