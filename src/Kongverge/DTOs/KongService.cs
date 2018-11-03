using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public class KongService : KongObject, IKongPluginHost, IKongEquatable<KongService>, IKongvergeConfigObject
    {
        public const string ObjectName = "service";

        private const int DefaultTimeout = 60000;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; } = 80;

        [JsonProperty("protocol")]
        public string Protocol { get; set; } = "http";

        [JsonProperty("retries")]
        public byte Retries { get; set; } = 5;

        [JsonProperty("connect_timeout")]
        public uint ConnectTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("write_timeout")]
        public uint WriteTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("read_timeout")]
        public uint ReadTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();
        
        [JsonProperty("routes", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongRoute> Routes { get; set; } = Array.Empty<KongRoute>();

        public override string ToString()
        {
            return $"{{{ToStringIdSegment()}Name: {Name}}}";
        }

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
        }

        public void AssignParentId(KongPlugin plugin)
        {
            plugin.ConsumerId = null;
            plugin.RouteId = null;
            plugin.ServiceId = Id;
        }

        public async Task Validate(IReadOnlyCollection<string> availablePlugins, ICollection<string> errorMessages)
        {
            if (!new[] { "http", "https" }.Contains(Protocol))
            {
                errorMessages.Add("Protocol is invalid (must be either 'http' or 'https').");
            }

            if (Uri.CheckHostName(Host) == UriHostNameType.Unknown)
            {
                errorMessages.Add("Host is invalid.");
            }

            if (!string.IsNullOrEmpty(Path) && !Uri.IsWellFormedUriString(Path, UriKind.Relative))
            {
                errorMessages.Add("Path is invalid.");
            }

            if (Retries > 25)
            {
                errorMessages.Add("Retries is invalid (must be between 0 and 25).");
            }

            if (ConnectTimeout > 300000)
            {
                errorMessages.Add("ConnectTimeout is invalid (must be between 0 and 300000).");
            }

            if (WriteTimeout > 300000)
            {
                errorMessages.Add("WriteTimeout is invalid (must be between 0 and 300000).");
            }

            if (ReadTimeout > 300000)
            {
                errorMessages.Add("ReadTimeout is invalid (must be between 0 and 300000).");
            }

            await ValidatePlugins(availablePlugins, errorMessages);

            await ValidateRoutes(availablePlugins, errorMessages);
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

        private async Task ValidateRoutes(IReadOnlyCollection<string> availablePlugins, ICollection<string> errorMessages)
        {
            if (Routes == null || !Routes.Any())
            {
                errorMessages.Add("Routes cannot be null or empty.");
                return;
            }
            foreach (var route in Routes)
            {
                await route.Validate(availablePlugins, errorMessages);
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
