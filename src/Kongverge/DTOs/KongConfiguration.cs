using System;
using System.Collections.Generic;
using System.Linq;
using Kongverge.Services;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace Kongverge.DTOs
{
    public class KongConfiguration
    {
        [JsonProperty("plugins")]
        public Plugins Plugins { get; set; }

        [JsonProperty("tagline")]
        public string Tagline { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("license")]
        public License License { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        public IDictionary<string, AsyncLazy<KongSchema>> GetSchemas(IKongAdminReader kongReader)
        {
            return Plugins.Available
                .Where(x => x.Value)
                .Select(x => $"plugins/{x.Key}")
                .Append("plugins")
                .Append("consumers")
                .Append("routes")
                .Append("services")
                .Append("certificates")
                .ToDictionary(x => x, x => new AsyncLazy<KongSchema>(() => kongReader.GetSchema(x)));
        }
    }

    public class Plugins
    {
        [JsonProperty("enabled_in_cluster")]
        public string[] Enabled { get; set; }

        [JsonProperty("available_on_server")]
        public Dictionary<string, bool> Available { get; set; }
    }

    public class License
    {
        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("license_creation_date")]
        public DateTime Created { get; set; }

        [JsonProperty("product_subscription")]
        public string Product { get; set; }

        [JsonProperty("admin_seats")]
        public int AdminSeats { get; set; }

        [JsonProperty("support_plan")]
        public string SupportPlan { get; set; }

        [JsonProperty("license_expiration_date")]
        public DateTime Expires { get; set; }
    }
}
