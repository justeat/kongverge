using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public sealed class KongRoute : KongObject, IKongPluginHost, IKongEquatable<KongRoute>, IValidatableObject
    {
        public const string ObjectName = "route";

        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        [JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceReference Service { get; set; }
        
        [JsonProperty("hosts")]
        public IEnumerable<string> Hosts { get; set; } = Array.Empty<string>();

        [JsonProperty("protocols")]
        public IEnumerable<string> Protocols { get; set; } = new[] { "http", "https" };

        [JsonProperty("methods")]
        public IEnumerable<string> Methods { get; set; } = Array.Empty<string>();

        [JsonProperty("paths")]
        public IEnumerable<string> Paths { get; set; } = Array.Empty<string>();

        [JsonProperty("regex_priority")]
        public int RegexPriority { get; set; }

        [JsonProperty("strip_path")]
        public bool StripPath { get; set; } = true;

        [JsonProperty("preserve_host")]
        public bool PreserveHost { get; set; }

        public override string ToString()
        {
            return $@"{{{ToStringIdSegment()}Paths: [{string.Join(", ", Paths)}], Methods: [{string.Join(", ", Methods)}], Protocols: [{string.Join(", ", Protocols)}]}}";
        }

        public override StringContent ToJsonStringContent()
        {
            var serviceReference = Service;
            var plugins = Plugins;

            Service = null;
            Plugins = null;
            var json = JsonConvert.SerializeObject(this);
            Service = serviceReference;
            Plugins = plugins;

            return json.AsJsonStringContent();
        }

        internal override void StripPersistedValues()
        {
            base.StripPersistedValues();
            foreach (var plugin in Plugins)
            {
                plugin.StripPersistedValues();
            }
            Service = null;
        }

        public void AssignParentId(KongPlugin plugin)
        {
            plugin.ConsumerId = null;
            plugin.ServiceId = null;
            plugin.RouteId = Id;
        }

        public Task Validate(ICollection<string> errorMessages)
        {
            if (IsNullOrEmpty(Protocols) || Protocols.Any(string.IsNullOrWhiteSpace))
            {
                errorMessages.Add("Route Protocols cannot be null or contain null or empty values");
            }

            if (IsNullOrEmpty(Hosts) && IsNullOrEmpty(Methods) && IsNullOrEmpty(Paths))
            {
                errorMessages.Add("At least one of 'hosts', 'methods', or 'paths' must be set");
                return Task.CompletedTask;
            }

            if (Hosts?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errorMessages.Add("Route Hosts cannot contain null or empty values");
            }

            if (Methods?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errorMessages.Add("Route Methods cannot contain null or empty values");
            }

            if (Paths?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errorMessages.Add("Route Paths cannot contain null or empty values");
            }

            return Task.CompletedTask;
        }

        private static bool IsNullOrEmpty(IEnumerable<string> values)
        {
            return values == null || !values.Any();
        }

        public override object GetMatchValue() => this;

        public object GetEqualityValues() =>
            new
            {
                Hosts,
                Protocols,
                Methods,
                Paths,
                RegexPriority,
                StripPath
            };

        public bool Equals(KongRoute other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();

        public class ServiceReference
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }
    }
}
