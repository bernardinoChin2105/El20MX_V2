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
    
    public class ExternalTransaction
    {

        /// <value>The total amount of the transcaction. Cannot excceed the invoice total.</value>
        [JsonProperty("amount")]
        public float? Amount { get; set; }

        /// <value>Datetime that the external payment was collected. Defaults to current datetime.</value>
        [JsonProperty("collected_at")]
        public DateTime? CollectedAt { get; set; }

        /// <value>Used as the transaction's description.</value>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <value>Payment method used for the external transaction.</value>
        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

    }
}
