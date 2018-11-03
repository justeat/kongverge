using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public class KongRoute : KongObject, IKongPluginHost, IKongEquatable<KongRoute>, IValidatableObject
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
        public ushort RegexPriority { get; set; }

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

        public virtual async Task Validate(IReadOnlyCollection<string> availablePlugins, ICollection<string> errorMessages)
        {
            if (IsNullOrEmpty(Protocols) || Protocols.Any(x => !new[] { "http", "https" }.Contains(x)))
            {
                errorMessages.Add("Route Protocols is invalid (must contain one or both of 'http' or 'https').");
            }

            if (IsNullOrEmpty(Hosts) && IsNullOrEmpty(Methods) && IsNullOrEmpty(Paths))
            {
                errorMessages.Add("At least one of Route 'Hosts', 'Methods', or 'Paths' must be set.");
            }

            if (Hosts == null || Hosts.Any(x => string.IsNullOrWhiteSpace(x) || Uri.CheckHostName(x) == UriHostNameType.Unknown))
            {
                errorMessages.Add("Route Hosts is invalid (cannot be null, or contain null, empty or invalid values).");
            }

            if (Methods == null || Methods.Any(string.IsNullOrWhiteSpace))
            {
                errorMessages.Add("Route Methods is invalid (cannot be null, or contain null or empty values).");
            }

            if (Paths == null || Paths.Any(x => string.IsNullOrWhiteSpace(x) || !Uri.IsWellFormedUriString(x, UriKind.Relative)))
            {
                errorMessages.Add("Route Paths is invalid (cannot be null, or contain null, empty or invalid values).");
            }

            await ValidatePlugins(availablePlugins, errorMessages);
        }

        private async Task ValidatePlugins(IReadOnlyCollection<string> availablePlugins, ICollection<string> errorMessages)
        {
            if (Plugins == null)
            {
                errorMessages.Add("Plugins cannot be null.");
                return;
            }
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(availablePlugins, errorMessages);
            }
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
