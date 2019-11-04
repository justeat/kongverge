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
    public sealed class KongService : KongObject, IKongPluginHost, IKongParentOf<KongRoute>, IKongEquatable<KongService>, IKongvergeConfigObject
    {
        public static readonly string ObjectName = "service";

        private const int DefaultTimeout = 60000;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("host", NullValueHandling = NullValueHandling.Ignore)]
        public string Host { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; } = 80;

        [JsonProperty("protocol", NullValueHandling = NullValueHandling.Ignore)]
        public string Protocol { get; set; } = "http";

        [JsonProperty("retries")]
        public byte Retries { get; set; } = 5;

        [JsonProperty("connect_timeout")]
        public uint ConnectTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("write_timeout")]
        public uint WriteTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("read_timeout")]
        public uint ReadTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("client_certificate", NullValueHandling = NullValueHandling.Ignore)]
        public Reference ClientCertificate { get; set; }

        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();
        
        [JsonProperty("routes", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongRoute> Routes { get; set; } = Array.Empty<KongRoute>();

        protected override string[] ToStringSegments => new[]
        {
            ToStringSegment("Id", Id),
            ToStringSegment("Name", Name)
        };

        public override StringContent ToJsonStringContent()
        {
            var routes = Routes;
            var plugins = Plugins;

            Routes = null;
            Plugins = null;
            var json = JsonConvert.SerializeObject(this);
            Routes = routes;
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
            foreach (var route in Routes)
            {
                route.StripPersistedValues();
            }
            ClientCertificate = null;
        }

        public void AssignParentId(KongPlugin child)
        {
            child.Consumer = null;
            child.Route = null;
            child.Service = new Reference { Id = Id };
        }

        public void AssignParentId(KongRoute child)
        {
            child.Service = new Reference { Id = Id };
        }

        public async Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            var schema = await schemas["services"].Task;
            var node = JObject.FromObject(this);
            node.Property("plugins")?.Remove();
            node.Property("routes")?.Remove();
            schema.Validate<KongService>(node, errorMessages, parent);

            await ValidatePlugins(schemas, errorMessages);

            await ValidateRoutes(schemas, errorMessages);
        }

        private async Task ValidatePlugins(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages)
        {
            if (Plugins == null)
            {
                errorMessages.Add($"{KongSchema.Violation<KongService>()} (plugins cannot be null).");
                return;
            }
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(schemas, errorMessages, this);
            }
        }

        private async Task ValidateRoutes(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages)
        {
            if (Routes == null || !Routes.Any())
            {
                errorMessages.Add($"{KongSchema.Violation<KongService>()} (routes cannot be null or empty).");
                return;
            }
            foreach (var route in Routes)
            {
                await route.Validate(schemas, errorMessages, this);
            }
        }

        public string ToConfigJson()
        {
            StripPersistedValues();
            return this.ToNormalizedJson() + Environment.NewLine;
        }

        public override object GetMatchValue() => Name;

        public object GetEqualityValues() =>
            new
            {
                Tags,
                Name,
                Host,
                Port,
                Protocol,
                Retries,
                ConnectTimeout,
                WriteTimeout,
                ReadTimeout,
                Path
            };

        public bool Equals(KongService other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();
    }
}
