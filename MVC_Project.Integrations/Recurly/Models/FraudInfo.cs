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
    
    public class FraudInfo 
    {

        /// <value>Kount decision</value>
        [JsonProperty("decision")]
        public string Decision { get; set; }

        /// <value>Kount rules</value>
        [JsonProperty("risk_rules_triggered")]
        public Dictionary<string, string> RiskRulesTriggered { get; set; }

        /// <value>Kount score</value>
        [JsonProperty("score")]
        public int? Score { get; set; }

    }
}
