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
        public decimal unit_amount { get; set; }
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
        public PlanDataModel data { get; set; }
    }

    public class AccountModel
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
        public decimal unit_amount { get; set; }
        public List<TiersModel> tiers { get; set; }
        public Int32 usage_percentage { get; set; }
        public string revenue_schedule_type { get; set; } //Enum
    }

    public class SubscripcionModel
    {
        public string plan_code { get; set; }
        public string plan_id { get; set; }
        public string billing_info_id { get; set; }
        public string collection_method { get; set; } //Enum
        public string currency { get; set; }
        public decimal unit_amount { get; set; }
        public string coupon_code { get; set; }
        public Int32 quantity { get; set; }
        public string trial_ends_at { get; set; } //datetime
        public string starts_at { get; set; } //datetime
        public string next_bill_date { get; set; } //datetime
        public Int32 total_billing_cycles { get; set; }
        public Int32 renewal_billing_cycles { get; set; }
        public bool auto_renew { get; set; }
        public string revenue_schedule_type { get; set; } //Enum
        public string terms_and_conditions { get; set; }
        public string customer_notes { get; set; }
        public string credit_customer_notes { get; set; }
        public string po_number { get; set; }
        public Int32 net_terms { get; set; }
        public string transaction_type { get; set; } //moto
        public AccountModel account { get; set; }
        public ShippingModel shipping { get; set; }
        public List<AddOnsModel> add_ons { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }
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
        public string created_at { get; set; } //datetime
        public string updated_at { get; set; } //datetime
        public string expired_at { get; set; } //datetime
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

    public class SubscripcionResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; } 
        public string uuid { get; set; }
        public string state { get; set; } //Enum
        public string current_period_started_at { get; set; } //datetime
        public string current_period_ends_at { get; set; } //datetime
        public string current_term_started_at { get; set; } //datetime
        public string current_term_ends_at { get; set; } //datetime
        public string trial_started_at { get; set; } //datetime
        public string trial_ends_at { get; set; } //datetime
        public Int32 remaining_billing_cycles { get; set; }
        public Int32 total_billing_cycles { get; set; }
        public Int32 renewal_billing_cycles { get; set; }
        public bool auto_renew { get; set; }
        public string paused_at { get; set; } //datetime
        public Int32 remaining_pause_cycles { get; set; }
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
        public string created_at { get; set; } //datetime
        public string updated_at { get; set; } //datetime
        public string activated_at { get; set; } //datetime
        public string canceled_at { get; set; } //datetime
        public string expires_at { get; set; } //datetime
        public string bank_account_authorized_at { get; set; } //datetime
        public string billing_info_id { get; set; } //Enum
        public AccountSubsResponseModel account { get; set; }
        public PlanSubsResponseModel plan { get; set; }
        public ShippingResponseModel shipping { get; set; }    
        public List<CouponRedemptionsModel> coupon_redemptions { get; set; }
        public List<AddOnsResponseModel> add_ons { get; set; }
        public List<CustomFieldsModel> custom_fields { get; set; }
    }

    public class PendingChangeResponseModel
    {
        public string id { get; set; }
        //public string object { get; set; }
        public string subscription_id { get; set; }
        public PlanResponseModel plan { get; set; }

        //public string id { get; set; }
        //public string id { get; set; }
    }

   

    /*
     {
  "pending_change": {    
    "add_ons": [
      {
        "id": "string",
        "object": "string",
        "subscription_id": "string",
        "add_on": {
          "id": "string",
          "object": "string",
          "code": "string",
          "name": "string",
          "add_on_type": "fixed",
          "usage_type": "price",
          "usage_percentage": 0,
          "measured_unit_id": "string",
          "item_id": "string",
          "external_sku": "string",
          "accounting_code": "string"
        },
        "add_on_source": "plan_add_on",
        "quantity": 0,
        "unit_amount": 0,
        "revenue_schedule_type": "never",
        "tier_type": "flat",
        "tiers": [
          {
            "ending_quantity": 999999999,
            "unit_amount": 0
          }
        ],
        "usage_percentage": 0,
        "created_at": "2020-12-13T00:13:11Z",
        "updated_at": "2020-12-13T00:13:11Z",
        "expired_at": "2020-12-13T00:13:11Z"
      }
    ],
    "unit_amount": 0,
    "quantity": 0,
    "shipping": {
      "object": "string",
      "address": {
        "id": "string",
        "object": "string",
        "account_id": "string",
        "nickname": "string",
        "first_name": "string",
        "last_name": "string",
        "company": "string",
        "email": "string",
        "vat_number": "string",
        "phone": "string",
        "street1": "string",
        "street2": "string",
        "city": "string",
        "region": "string",
        "postal_code": "string",
        "country": "string",
        "created_at": "2020-12-13T00:13:11Z",
        "updated_at": "2020-12-13T00:13:11Z"
      },
      "method": {
        "id": "string",
        "object": "string",
        "code": "string",
        "name": "string"
      },
      "amount": 0
    },
    "activate_at": "2020-12-13T00:13:11Z",
    "activated": true,
    "revenue_schedule_type": "never",
    "setup_fee_revenue_schedule_type": "never",
    "invoice_collection": {
      "object": "string",
      "charge_invoice": {
        "id": "string",
        "object": "string",
        "type": "charge",
        "origin": "purchase",
        "state": "open",
        "account": {
          "id": "string",
          "object": "string",
          "code": "string",
          "email": "user@example.com",
          "first_name": "string",
          "last_name": "string",
          "company": "string",
          "parent_account_id": "string",
          "bill_to": "string"
        },
        "billing_info_id": "string",
        "subscription_ids": [
          "string"
        ],
        "previous_invoice_id": "string",
        "number": "string",
        "collection_method": "automatic",
        "po_number": "string",
        "net_terms": 0,
        "address": {
          "name_on_account": "string",
          "company": "string",
          "first_name": "string",
          "last_name": "string",
          "phone": "string",
          "street1": "string",
          "street2": "string",
          "city": "string",
          "region": "string",
          "postal_code": "string",
          "country": "string"
        },
        "shipping_address": {
          "id": "string",
          "object": "string",
          "account_id": "string",
          "nickname": "string",
          "first_name": "string",
          "last_name": "string",
          "company": "string",
          "email": "string",
          "vat_number": "string",
          "phone": "string",
          "street1": "string",
          "street2": "string",
          "city": "string",
          "region": "string",
          "postal_code": "string",
          "country": "string",
          "created_at": "2020-12-13T00:13:11Z",
          "updated_at": "2020-12-13T00:13:11Z"
        },
        "currency": "str",
        "discount": 0,
        "subtotal": 0,
        "tax": 0,
        "total": 0,
        "refundable_amount": 0,
        "paid": 0,
        "balance": 0,
        "tax_info": {
          "type": "string",
          "region": "string",
          "rate": 0
        },
        "vat_number": "string",
        "vat_reverse_charge_notes": "string",
        "terms_and_conditions": "string",
        "customer_notes": "string",
        "line_items": {
          "object": "string",
          "has_more": true,
          "next": "string",
          "data": [
            {
              "id": "string",
              "object": "string",
              "uuid": "string",
              "type": "charge",
              "item_code": "string",
              "item_id": "string",
              "external_sku": "string",
              "revenue_schedule_type": "never",
              "state": "pending",
              "legacy_category": "charge",
              "account": {
                "id": "string",
                "object": "string",
                "code": "string",
                "email": "user@example.com",
                "first_name": "string",
                "last_name": "string",
                "company": "string",
                "parent_account_id": "string",
                "bill_to": "string"
              },
              "subscription_id": "string",
              "plan_id": "string",
              "plan_code": "string",
              "add_on_id": "string",
              "add_on_code": "string",
              "invoice_id": "string",
              "invoice_number": "string",
              "previous_line_item_id": "string",
              "original_line_item_invoice_id": "string",
              "origin": "plan",
              "accounting_code": "string",
              "product_code": "string",
              "credit_reason_code": "general",
              "currency": "str",
              "amount": 0,
              "description": "string",
              "quantity": 1,
              "unit_amount": 0,
              "subtotal": 0,
              "discount": 0,
              "tax": 0,
              "taxable": true,
              "tax_exempt": true,
              "avalara_transaction_type": 0,
              "avalara_service_type": 0,
              "tax_code": "string",
              "tax_info": {
                "type": "string",
                "region": "string",
                "rate": 0
              },
              "proration_rate": 0,
              "refund": true,
              "refunded_quantity": 0,
              "credit_applied": 0,
              "shipping_address": {
                "id": "string",
                "object": "string",
                "account_id": "string",
                "nickname": "string",
                "first_name": "string",
                "last_name": "string",
                "company": "string",
                "email": "string",
                "vat_number": "string",
                "phone": "string",
                "street1": "string",
                "street2": "string",
                "city": "string",
                "region": "string",
                "postal_code": "string",
                "country": "string",
                "created_at": "2020-12-13T00:13:11Z",
                "updated_at": "2020-12-13T00:13:11Z"
              },
              "start_date": "2020-12-13T00:13:11Z",
              "end_date": "2020-12-13T00:13:11Z",
              "created_at": "2020-12-13T00:13:11Z",
              "updated_at": "2020-12-13T00:13:11Z"
            }
          ]
        },
        "transactions": [
          {
            "id": "string",
            "object": "string",
            "uuid": "string",
            "original_transaction_id": "string",
            "account": {
              "id": "string",
              "object": "string",
              "code": "string",
              "email": "user@example.com",
              "first_name": "string",
              "last_name": "string",
              "company": "string",
              "parent_account_id": "string",
              "bill_to": "string"
            },
            "invoice": {
              "id": "string",
              "object": "string",
              "number": "string",
              "type": "charge",
              "state": "open"
            },
            "voided_by_invoice": {
              "id": "string",
              "object": "string",
              "number": "string",
              "type": "charge",
              "state": "open"
            },
            "subscription_ids": [
              "string"
            ],
            "type": "authorization",
            "origin": "api",
            "currency": "str",
            "amount": 0,
            "status": "pending",
            "success": true,
            "refunded": true,
            "billing_address": {
              "first_name": "string",
              "last_name": "string",
              "phone": "string",
              "street1": "string",
              "street2": "string",
              "city": "string",
              "region": "string",
              "postal_code": "string",
              "country": "string"
            },
            "collection_method": "automatic",
            "payment_method": {
              "object": "credit_card",
              "card_type": "American Express",
              "first_six": "string",
              "last_four": "stri",
              "last_two": "st",
              "exp_month": 0,
              "exp_year": 0,
              "gateway_token": "string",
              "gateway_code": "string",
              "billing_agreement_id": "string",
              "name_on_account": "string",
              "account_type": "checking",
              "routing_number": "string",
              "routing_number_bank": "string"
            },
            "ip_address_v4": "string",
            "ip_address_country": "string",
            "status_code": "string",
            "status_message": "string",
            "customer_message": "string",
            "customer_message_locale": "string",
            "payment_gateway": {
              "id": "string",
              "object": "string",
              "type": "string",
              "name": "string"
            },
            "gateway_message": "string",
            "gateway_reference": "string",
            "gateway_approval_code": "string",
            "gateway_response_code": "string",
            "gateway_response_time": 0,
            "gateway_response_values": {},
            "cvv_check": "D",
            "avs_check": "A",
            "created_at": "2020-12-13T00:13:11Z",
            "updated_at": "2020-12-13T00:13:11Z",
            "voided_at": "2020-12-13T00:13:11Z",
            "collected_at": "2020-12-13T00:13:11Z"
          }
        ],
        "credit_payments": [
          {
            "id": "string",
            "object": "string",
            "uuid": "string",
            "action": "payment",
            "account": {
              "id": "string",
              "object": "string",
              "code": "string",
              "email": "user@example.com",
              "first_name": "string",
              "last_name": "string",
              "company": "string",
              "parent_account_id": "string",
              "bill_to": "string"
            },
            "applied_to_invoice": {
              "id": "string",
              "object": "string",
              "number": "string",
              "type": "charge",
              "state": "open"
            },
            "original_invoice": {
              "id": "string",
              "object": "string",
              "number": "string",
              "type": "charge",
              "state": "open"
            },
            "currency": "str",
            "amount": 0,
            "original_credit_payment_id": "string",
            "refund_transaction": {
              "id": "string",
              "object": "string",
              "uuid": "string",
              "original_transaction_id": "string",
              "account": {
                "id": "string",
                "object": "string",
                "code": "string",
                "email": "user@example.com",
                "first_name": "string",
                "last_name": "string",
                "company": "string",
                "parent_account_id": "string",
                "bill_to": "string"
              },
              "invoice": {
                "id": "string",
                "object": "string",
                "number": "string",
                "type": "charge",
                "state": "open"
              },
              "voided_by_invoice": {
                "id": "string",
                "object": "string",
                "number": "string",
                "type": "charge",
                "state": "open"
              },
              "subscription_ids": [
                "string"
              ],
              "type": "authorization",
              "origin": "api",
              "currency": "str",
              "amount": 0,
              "status": "pending",
              "success": true,
              "refunded": true,
              "billing_address": {
                "first_name": "string",
                "last_name": "string",
                "phone": "string",
                "street1": "string",
                "street2": "string",
                "city": "string",
                "region": "string",
                "postal_code": "string",
                "country": "string"
              },
              "collection_method": "automatic",
              "payment_method": {
                "object": "credit_card",
                "card_type": "American Express",
                "first_six": "string",
                "last_four": "stri",
                "last_two": "st",
                "exp_month": 0,
                "exp_year": 0,
                "gateway_token": "string",
                "gateway_code": "string",
                "billing_agreement_id": "string",
                "name_on_account": "string",
                "account_type": "checking",
                "routing_number": "string",
                "routing_number_bank": "string"
              },
              "ip_address_v4": "string",
              "ip_address_country": "string",
              "status_code": "string",
              "status_message": "string",
              "customer_message": "string",
              "customer_message_locale": "string",
              "payment_gateway": {
                "id": "string",
                "object": "string",
                "type": "string",
                "name": "string"
              },
              "gateway_message": "string",
              "gateway_reference": "string",
              "gateway_approval_code": "string",
              "gateway_response_code": "string",
              "gateway_response_time": 0,
              "gateway_response_values": {},
              "cvv_check": "D",
              "avs_check": "A",
              "created_at": "2020-12-13T00:13:11Z",
              "updated_at": "2020-12-13T00:13:11Z",
              "voided_at": "2020-12-13T00:13:11Z",
              "collected_at": "2020-12-13T00:13:11Z"
            },
            "created_at": "2020-12-13T00:13:11Z",
            "updated_at": "2020-12-13T00:13:11Z",
            "voided_at": "2020-12-13T00:13:11Z"
          }
        ],
        "created_at": "2020-12-13T00:13:11Z",
        "updated_at": "2020-12-13T00:13:11Z",
        "due_at": "2020-12-13T00:13:11Z",
        "closed_at": "2020-12-13T00:13:11Z"
      },
      "credit_invoices": [
        {
          "id": "string",
          "object": "string",
          "type": "charge",
          "origin": "purchase",
          "state": "open",
          "account": {
            "id": "string",
            "object": "string",
            "code": "string",
            "email": "user@example.com",
            "first_name": "string",
            "last_name": "string",
            "company": "string",
            "parent_account_id": "string",
            "bill_to": "string"
          },
          "billing_info_id": "string",
          "subscription_ids": [
            "string"
          ],
          "previous_invoice_id": "string",
          "number": "string",
          "collection_method": "automatic",
          "po_number": "string",
          "net_terms": 0,
          "address": {
            "name_on_account": "string",
            "company": "string",
            "first_name": "string",
            "last_name": "string",
            "phone": "string",
            "street1": "string",
            "street2": "string",
            "city": "string",
            "region": "string",
            "postal_code": "string",
            "country": "string"
          },
          "shipping_address": {
            "id": "string",
            "object": "string",
            "account_id": "string",
            "nickname": "string",
            "first_name": "string",
            "last_name": "string",
            "company": "string",
            "email": "string",
            "vat_number": "string",
            "phone": "string",
            "street1": "string",
            "street2": "string",
            "city": "string",
            "region": "string",
            "postal_code": "string",
            "country": "string",
            "created_at": "2020-12-13T00:13:11Z",
            "updated_at": "2020-12-13T00:13:11Z"
          },
          "currency": "str",
          "discount": 0,
          "subtotal": 0,
          "tax": 0,
          "total": 0,
          "refundable_amount": 0,
          "paid": 0,
          "balance": 0,
          "tax_info": {
            "type": "string",
            "region": "string",
            "rate": 0
          },
          "vat_number": "string",
          "vat_reverse_charge_notes": "string",
          "terms_and_conditions": "string",
          "customer_notes": "string",
          "line_items": {
            "object": "string",
            "has_more": true,
            "next": "string",
            "data": [
              {
                "id": "string",
                "object": "string",
                "uuid": "string",
                "type": "charge",
                "item_code": "string",
                "item_id": "string",
                "external_sku": "string",
                "revenue_schedule_type": "never",
                "state": "pending",
                "legacy_category": "charge",
                "account": {
                  "id": "string",
                  "object": "string",
                  "code": "string",
                  "email": "user@example.com",
                  "first_name": "string",
                  "last_name": "string",
                  "company": "string",
                  "parent_account_id": "string",
                  "bill_to": "string"
                },
                "subscription_id": "string",
                "plan_id": "string",
                "plan_code": "string",
                "add_on_id": "string",
                "add_on_code": "string",
                "invoice_id": "string",
                "invoice_number": "string",
                "previous_line_item_id": "string",
                "original_line_item_invoice_id": "string",
                "origin": "plan",
                "accounting_code": "string",
                "product_code": "string",
                "credit_reason_code": "general",
                "currency": "str",
                "amount": 0,
                "description": "string",
                "quantity": 1,
                "unit_amount": 0,
                "subtotal": 0,
                "discount": 0,
                "tax": 0,
                "taxable": true,
                "tax_exempt": true,
                "avalara_transaction_type": 0,
                "avalara_service_type": 0,
                "tax_code": "string",
                "tax_info": {
                  "type": "string",
                  "region": "string",
                  "rate": 0
                },
                "proration_rate": 0,
                "refund": true,
                "refunded_quantity": 0,
                "credit_applied": 0,
                "shipping_address": {
                  "id": "string",
                  "object": "string",
                  "account_id": "string",
                  "nickname": "string",
                  "first_name": "string",
                  "last_name": "string",
                  "company": "string",
                  "email": "string",
                  "vat_number": "string",
                  "phone": "string",
                  "street1": "string",
                  "street2": "string",
                  "city": "string",
                  "region": "string",
                  "postal_code": "string",
                  "country": "string",
                  "created_at": "2020-12-13T00:13:11Z",
                  "updated_at": "2020-12-13T00:13:11Z"
                },
                "start_date": "2020-12-13T00:13:11Z",
                "end_date": "2020-12-13T00:13:11Z",
                "created_at": "2020-12-13T00:13:11Z",
                "updated_at": "2020-12-13T00:13:11Z"
              }
            ]
          },
          "transactions": [
            {
              "id": "string",
              "object": "string",
              "uuid": "string",
              "original_transaction_id": "string",
              "account": {
                "id": "string",
                "object": "string",
                "code": "string",
                "email": "user@example.com",
                "first_name": "string",
                "last_name": "string",
                "company": "string",
                "parent_account_id": "string",
                "bill_to": "string"
              },
              "invoice": {
                "id": "string",
                "object": "string",
                "number": "string",
                "type": "charge",
                "state": "open"
              },
              "voided_by_invoice": {
                "id": "string",
                "object": "string",
                "number": "string",
                "type": "charge",
                "state": "open"
              },
              "subscription_ids": [
                "string"
              ],
              "type": "authorization",
              "origin": "api",
              "currency": "str",
              "amount": 0,
              "status": "pending",
              "success": true,
              "refunded": true,
              "billing_address": {
                "first_name": "string",
                "last_name": "string",
                "phone": "string",
                "street1": "string",
                "street2": "string",
                "city": "string",
                "region": "string",
                "postal_code": "string",
                "country": "string"
              },
              "collection_method": "automatic",
              "payment_method": {
                "object": "credit_card",
                "card_type": "American Express",
                "first_six": "string",
                "last_four": "stri",
                "last_two": "st",
                "exp_month": 0,
                "exp_year": 0,
                "gateway_token": "string",
                "gateway_code": "string",
                "billing_agreement_id": "string",
                "name_on_account": "string",
                "account_type": "checking",
                "routing_number": "string",
                "routing_number_bank": "string"
              },
              "ip_address_v4": "string",
              "ip_address_country": "string",
              "status_code": "string",
              "status_message": "string",
              "customer_message": "string",
              "customer_message_locale": "string",
              "payment_gateway": {
                "id": "string",
                "object": "string",
                "type": "string",
                "name": "string"
              },
              "gateway_message": "string",
              "gateway_reference": "string",
              "gateway_approval_code": "string",
              "gateway_response_code": "string",
              "gateway_response_time": 0,
              "gateway_response_values": {},
              "cvv_check": "D",
              "avs_check": "A",
              "created_at": "2020-12-13T00:13:11Z",
              "updated_at": "2020-12-13T00:13:11Z",
              "voided_at": "2020-12-13T00:13:11Z",
              "collected_at": "2020-12-13T00:13:11Z"
            }
          ],
          "credit_payments": [
            {
              "id": "string",
              "object": "string",
              "uuid": "string",
              "action": "payment",
              "account": {
                "id": "string",
                "object": "string",
                "code": "string",
                "email": "user@example.com",
                "first_name": "string",
                "last_name": "string",
                "company": "string",
                "parent_account_id": "string",
                "bill_to": "string"
              },
              "applied_to_invoice": {
                "id": "string",
                "object": "string",
                "number": "string",
                "type": "charge",
                "state": "open"
              },
              "original_invoice": {
                "id": "string",
                "object": "string",
                "number": "string",
                "type": "charge",
                "state": "open"
              },
              "currency": "str",
              "amount": 0,
              "original_credit_payment_id": "string",
              "refund_transaction": {
                "id": "string",
                "object": "string",
                "uuid": "string",
                "original_transaction_id": "string",
                "account": {
                  "id": "string",
                  "object": "string",
                  "code": "string",
                  "email": "user@example.com",
                  "first_name": "string",
                  "last_name": "string",
                  "company": "string",
                  "parent_account_id": "string",
                  "bill_to": "string"
                },
                "invoice": {
                  "id": "string",
                  "object": "string",
                  "number": "string",
                  "type": "charge",
                  "state": "open"
                },
                "voided_by_invoice": {
                  "id": "string",
                  "object": "string",
                  "number": "string",
                  "type": "charge",
                  "state": "open"
                },
                "subscription_ids": [
                  "string"
                ],
                "type": "authorization",
                "origin": "api",
                "currency": "str",
                "amount": 0,
                "status": "pending",
                "success": true,
                "refunded": true,
                "billing_address": {
                  "first_name": "string",
                  "last_name": "string",
                  "phone": "string",
                  "street1": "string",
                  "street2": "string",
                  "city": "string",
                  "region": "string",
                  "postal_code": "string",
                  "country": "string"
                },
                "collection_method": "automatic",
                "payment_method": {
                  "object": "credit_card",
                  "card_type": "American Express",
                  "first_six": "string",
                  "last_four": "stri",
                  "last_two": "st",
                  "exp_month": 0,
                  "exp_year": 0,
                  "gateway_token": "string",
                  "gateway_code": "string",
                  "billing_agreement_id": "string",
                  "name_on_account": "string",
                  "account_type": "checking",
                  "routing_number": "string",
                  "routing_number_bank": "string"
                },
                "ip_address_v4": "string",
                "ip_address_country": "string",
                "status_code": "string",
                "status_message": "string",
                "customer_message": "string",
                "customer_message_locale": "string",
                "payment_gateway": {
                  "id": "string",
                  "object": "string",
                  "type": "string",
                  "name": "string"
                },
                "gateway_message": "string",
                "gateway_reference": "string",
                "gateway_approval_code": "string",
                "gateway_response_code": "string",
                "gateway_response_time": 0,
                "gateway_response_values": {},
                "cvv_check": "D",
                "avs_check": "A",
                "created_at": "2020-12-13T00:13:11Z",
                "updated_at": "2020-12-13T00:13:11Z",
                "voided_at": "2020-12-13T00:13:11Z",
                "collected_at": "2020-12-13T00:13:11Z"
              },
              "created_at": "2020-12-13T00:13:11Z",
              "updated_at": "2020-12-13T00:13:11Z",
              "voided_at": "2020-12-13T00:13:11Z"
            }
          ],
          "created_at": "2020-12-13T00:13:11Z",
          "updated_at": "2020-12-13T00:13:11Z",
          "due_at": "2020-12-13T00:13:11Z",
          "closed_at": "2020-12-13T00:13:11Z"
        }
      ]
    },
    "custom_fields": [
      {
        "name": "string",
        "value": "string"
      }
    ],
    "created_at": "2020-12-13T00:13:11Z",
    "updated_at": "2020-12-13T00:13:11Z",
    "deleted_at": "2020-12-13T00:13:11Z"
  },

}
     */
}
