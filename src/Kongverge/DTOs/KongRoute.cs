using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace Kongverge.DTOs
{
    public sealed class KongRoute : KongObject, IKongPluginHost, IKongEquatable<KongRoute>, IValidatableObject
    {
        public static readonly string ObjectName = "route";

        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        [JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
        public Reference Service { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("hosts", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Hosts { get; set; }

        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string[]> Headers { get; set; }

        [JsonProperty("protocols")]
        public IEnumerable<string> Protocols { get; set; } = new[] { "http", "https" };

        [JsonProperty("methods", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Methods { get; set; }

        [JsonProperty("paths", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Paths { get; set; }

        [JsonProperty("snis", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Snis { get; set; }

        [JsonProperty("sources", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Endpoint> Sources { get; set; }

        [JsonProperty("destinations", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Endpoint> Destinations { get; set; }

        [JsonProperty("https_redirect_status_code")]
        public ushort HttpsRedirectStatusCode { get; set; } = 426;

        [JsonProperty("regex_priority")]
        public ushort RegexPriority { get; set; }

        [JsonProperty("strip_path")]
        public bool StripPath { get; set; } = true;

        [JsonProperty("preserve_host")]
        public bool PreserveHost { get; set; }

        protected override string[] ToStringSegments => new[]
        {
            ToStringSegment("Id", Id),
            ToStringSegment("Name", Name),
            ToStringSegment("Hosts", Hosts, EnumerableSegment),
            ToStringSegment("Headers", Headers, DictionarySegment),
            ToStringSegment("Paths", Paths, EnumerableSegment),
            ToStringSegment("Snis", Snis, EnumerableSegment),
            ToStringSegment("Sources", Sources, EnumerableSegment),
            ToStringSegment("Destinations", Destinations, EnumerableSegment),
            ToStringSegment("Methods", Methods, EnumerableSegment),
            ToStringSegment("Protocols", Protocols, EnumerableSegment)
        };

        private static string EnumerableSegment(IEnumerable<string> values) => $"[{string.Join(", ", values)}]";

        private static string EnumerableSegment(IEnumerable<Endpoint> values) => $"[{string.Join(", ", values.Select(x => ToString(ToStringSegment(nameof(Endpoint.Ip), x.Ip), ToStringSegment(nameof(Endpoint.Port), x.Port))))}]";

        private static string DictionarySegment(IDictionary<string, string[]> values) => ToString(values.Select(x => ToStringSegment(x.Key, x.Value, EnumerableSegment)).ToArray());

        public override StringContent ToJsonStringContent()
        {
            var plugins = Plugins;

            Plugins = null;
            var json = JsonConvert.SerializeObject(this);
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

        public void AssignParentId(KongPlugin child)
        {
            child.Consumer = null;
            child.Service = null;
            child.Route = new Reference { Id = Id };
        }

        public async Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            var schema = await schemas["routes"].Task;
            var node = JObject.FromObject(this);
            node.Property("plugins")?.Remove();
            if (Protocols != null)
            {
                if (Protocols.Contains("http") && Methods == null && Hosts == null && Headers == null && Paths == null)
                {
                    errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (at least one of 'methods, hosts, headers, paths' must be set).");
                }
                else if (Protocols.Contains("https") && Methods == null && Hosts == null && Headers == null && Paths == null && Snis == null)
                {
                    errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (at least one of 'methods, hosts, headers, paths, snis' must be set).");
                }
                else if (Protocols.Contains("tcp") && Sources == null && Destinations == null)
                {
                    errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (at least one of 'sources, destinations' must be set).");
                }
                else if (Protocols.Contains("tls") && Sources == null && Destinations == null && Snis == null)
                {
                    errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (at least one of 'sources, destinations, snis' must be set).");
                }
                else if (Protocols.Contains("grpc") && Hosts == null && Headers == null && Paths == null)
                {
                    errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (at least one of 'hosts, headers, paths' must be set).");
                }
                else if (Protocols.Contains("grpcs") && Hosts == null && Headers == null && Paths == null && Snis == null)
                {
                    errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (at least one of 'hosts, headers, paths, snis' must be set).");
                }
            }
            schema.Validate<KongRoute>(node, errorMessages, parent);

            await ValidatePlugins(schemas, errorMessages);
        }

        private async Task ValidatePlugins(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages)
        {
            if (Plugins == null)
            {
                errorMessages.Add($"{KongSchema.Violation<KongRoute>()} (plugins cannot be null).");
                return;
            }
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(schemas, errorMessages, this);
            }
        }

        public override object GetMatchValue() => this;

        public object GetEqualityValues() =>
            new
            {
                Tags,
                Name,
                Hosts,
                Headers,
                Protocols,
                Methods,
                Paths,
                Snis,
                Sources,
                Destinations,
                HttpsRedirectStatusCode,
                RegexPriority,
                StripPath
            };

        public bool Equals(KongRoute other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();

        public class Endpoint
        {
            [JsonProperty("ip", NullValueHandling = NullValueHandling.Ignore)]
            public string Ip { get; set; }

            [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
            public int? Port { get; set; }
        }
    }
}
