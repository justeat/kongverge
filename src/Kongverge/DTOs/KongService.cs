using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public sealed class KongService : KongObject, IKongPluginHost, IKongEquatable<KongService>, IKongvergeConfigObject
    {
        private const int DefaultTimeout = 60000;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; } = 80;

        [JsonProperty("protocol")]
        public string Protocol { get; set; } = "http";

        [JsonProperty("retries")]
        public int Retries { get; set; } = 5;

        [JsonProperty("connect_timeout")]
        public int ConnectTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("write_timeout")]
        public int WriteTimeout { get; set; } = DefaultTimeout;

        [JsonProperty("read_timeout")]
        public int ReadTimeout { get; set; } = DefaultTimeout;

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

        public async Task Validate(ICollection<string> errorMessages)
        {
            await ValidateRoutesAreValid(errorMessages);
        }

        private async Task ValidateRoutesAreValid(ICollection<string> errorMessages)
        {
            if (Routes == null || !Routes.Any())
            {
                errorMessages.Add("Routes cannot be null or empty");
                return;
            }
            foreach (var route in Routes)
            {
                await route.Validate(errorMessages);
            }

            // TODO: Check if routes Clash
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
