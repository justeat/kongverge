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
    public sealed class KongPlugin : KongObject, IKongEquatable<KongPlugin>, IValidatableObject
    {
        public static readonly string ObjectName = "plugin";

        [JsonProperty("consumer", NullValueHandling = NullValueHandling.Ignore)]
        public Reference Consumer { get; set; }

        [JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
        public Reference Service { get; set; }

        [JsonProperty("route", NullValueHandling = NullValueHandling.Ignore)]
        public Reference Route { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("run_on", NullValueHandling = NullValueHandling.Ignore)]
        public string RunOn { get; set; }

        [JsonProperty("protocols", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Protocols { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("config")]
        public JObject Config { get; set; }

        public bool IsGlobal() =>
            Consumer?.Id == null &&
            Service?.Id == null &&
            Route?.Id == null;

        protected override string[] ToStringSegments => new[]
        {
            ToStringSegment("Id", Id),
            ToStringSegment("Name", Name)
        };

        public override StringContent ToJsonStringContent() => JsonConvert.SerializeObject(this).AsJsonStringContent();

        internal override void StripPersistedValues()
        {
            base.StripPersistedValues();
            Consumer = null;
            Service = null;
            Route = null;
        }

        public async Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            var schemaPath = $"plugins/{Name}";
            if (!schemas.ContainsKey(schemaPath))
            {
                errorMessages.Add($"{KongSchema.Violation<KongPlugin>()} (plugin '{Name}' is not available on Kong server).");
                return;
            }

            var schema = await schemas[schemaPath].Task;
            var baseSchema = await schemas["plugins"].Task;
            var fallbackFields = baseSchema.Fields.Where(b => schema.Fields.All(f => f.Name != b.Name)).ToArray();
            if (fallbackFields.Any())
            {
                schema.Fields = schema.Fields.Concat(fallbackFields).ToArray();
            }

            var node = JObject.FromObject(this);
            schema.Validate<KongPlugin>(node, errorMessages, parent);
            RunOn = node["run_on"].Value<string>();
            Protocols = node["protocols"].Values<string>();
        }

        public bool IsMatch(KongPlugin other) => Id == other.Id || Name == other.Name;

        public object GetEqualityValues() =>
             new
             {
                 Tags,
                 Name,
                 RunOn,
                 Protocols,
                 Enabled,
                 Config
             };

        public bool Equals(KongPlugin other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();
    }
}
