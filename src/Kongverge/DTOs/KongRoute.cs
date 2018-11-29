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

        private static readonly string[] AllowedProtocols = { "http", "https" };

        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        [JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceReference Service { get; set; }
        
        [JsonProperty("hosts")]
        public IEnumerable<string> Hosts { get; set; }

        [JsonProperty("protocols")]
        public IEnumerable<string> Protocols { get; set; } = new[] { "http", "https" };

        [JsonProperty("methods")]
        public IEnumerable<string> Methods { get; set; }

        [JsonProperty("paths")]
        public IEnumerable<string> Paths { get; set; }

        [JsonProperty("regex_priority")]
        public ushort RegexPriority { get; set; }

        [JsonProperty("strip_path")]
        public bool StripPath { get; set; } = true;

        [JsonProperty("preserve_host")]
        public bool PreserveHost { get; set; }

        public override string ToString()
        {
            return $@"{{{ToStringIdSegment()}Hosts: {EnumerableSegment(Hosts)}, Paths: {EnumerableSegment(Paths)}, Methods: {EnumerableSegment(Methods)}, Protocols: {EnumerableSegment(Protocols)}}}";
        }

        private static string EnumerableSegment(IEnumerable<string> values)
        {
            if (values == null)
            {
                return "null";
            }
            return $"[{string.Join(", ", values)}]";
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
            if (IsNullOrEmpty(Protocols) || Protocols.Any(x => !AllowedProtocols.Contains(x)))
            {
                errorMessages.Add("Route Protocols is invalid (must contain one or both of 'http' or 'https').");
            }

            if (IsNullOrEmpty(Hosts) && IsNullOrEmpty(Methods) && IsNullOrEmpty(Paths))
            {
                errorMessages.Add("At least one of Route 'Hosts', 'Methods', or 'Paths' must be set.");
            }

            if (Hosts?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errorMessages.Add("Route Hosts is invalid (cannot contain null or empty values).");
            }
            if (Hosts?.Any(x => !string.IsNullOrWhiteSpace(x) && x.StartsWith("*.") && x.EndsWith(".*")) == true)
            {
                errorMessages.Add("Route Hosts is invalid (values cannot begin and end with a wildcard, only one wildcard at the start or end is allowed).");
            }
            if (Hosts?.Any(x => !string.IsNullOrWhiteSpace(x) && Uri.CheckHostName(RemoveWildcards(x)) == UriHostNameType.Unknown) == true)
            {
                errorMessages.Add("Route Hosts is invalid (values must be valid hostnames or IP addresses, with a single optional wildcard at the start or end).");
            }

            if (Methods?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errorMessages.Add("Route Methods is invalid (cannot contain null or empty values).");
            }

            if (Paths?.Any(string.IsNullOrWhiteSpace) == true)
            {
                errorMessages.Add("Route Paths is invalid (cannot contain null or empty values).");
            }

            await ValidatePlugins(availablePlugins, errorMessages);
        }

        private static string RemoveWildcards(string input)
        {
            if (input.StartsWith("*."))
            {
                input = input.Substring(2);
            }
            if (input.EndsWith(".*"))
            {
                input = input.Substring(0, input.Length - 2);
            }

            return input;
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
