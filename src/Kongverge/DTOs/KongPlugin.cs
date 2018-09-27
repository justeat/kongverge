using System.Collections.Generic;
using System.Net.Http;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public sealed class KongPlugin : KongObject, IKongEquatable<KongPlugin>
    {
        [JsonProperty("consumer_id")]
        public string ConsumerId { get; set; }

        [JsonProperty("service_id")]
        public string ServiceId { get; set; }

        [JsonProperty("route_id")]
        public string RouteId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("config")]
        public Dictionary<string, object> Config { get; set; }

        public bool IsGlobal() =>
            ConsumerId == null &&
            ServiceId == null &&
            RouteId == null;

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}";
        }

        public StringContent ToJsonStringContent() => JsonConvert.SerializeObject(this).AsJsonStringContent();

        public override void StripPersistedValues()
        {
            base.StripPersistedValues();
            ConsumerId = null;
            ServiceId = null;
            RouteId = null;
        }

        public override object GetMatchValue() => Name;

        public object GetEqualityValues() =>
             new
             {
                Name,
                Config
             };

        public bool Equals(KongPlugin other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();
    }
}
