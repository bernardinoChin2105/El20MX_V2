/**
 * This file is automatically created by Recurly's OpenAPI generation process
 * and thus any edits you make by hand will be lost. If you wish to make a
 * change to this file, please create a Github issue explaining the changes you
 * need and we will usher them to the appropriate places.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace MVC_Project.Integrations.Recurly.Models
{
    
    public class Settings 
    {


        [JsonProperty("accepted_currencies")]
        public List<string> AcceptedCurrencies { get; set; }

        /// <value>
        /// - full:      Full Address (Street, City, State, Postal Code and Country)
        /// - streetzip: Street and Postal Code only
        /// - zip:       Postal Code only
        /// - none:      No Address
        /// </value>
        [JsonProperty("billing_address_requirement")]
        public string BillingAddressRequirement { get; set; }

        /// <value>The default 3-letter ISO 4217 currency code.</value>
        [JsonProperty("default_currency")]
        public string DefaultCurrency { get; set; }

    }
}
