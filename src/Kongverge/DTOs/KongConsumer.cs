using Kongverge.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kongverge.DTOs
{
    public sealed class KongConsumer : KongObject, IKongPluginHost, IKongEquatable<KongConsumer>, IValidatableObject
    {
        public static readonly string ObjectName = "consumer";

        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CustomId { get; set; }

        protected override string[] ToStringSegments => new[]
        {
            ToStringSegment("Id", Id),
            ToStringSegment("CustomId", CustomId),
            ToStringSegment("Username", Username)
        };

        public bool IsMatch(KongConsumer other) => Id == other.Id || Username == other.Username || CustomId == other.CustomId;

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
        }

        public object GetEqualityValues() =>
            new
            {
                Tags,
                Username,
                CustomId
            };

        public bool Equals(KongConsumer other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();

        public async Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            var schema = await schemas["consumers"].Task;
            var node = JObject.FromObject(this);
            node.Property("plugins")?.Remove();
            if (Username == null && CustomId == null)
            {
                errorMessages.Add($"{KongSchema.Violation<KongConsumer>()} (at least one of 'username, custom_id' must be set).");
            }
            schema.Validate<KongConsumer>(node, errorMessages, parent);

            await ValidatePlugins(schemas, errorMessages);
        }

        private async Task ValidatePlugins(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages)
        {
            if (Plugins == null)
            {
                errorMessages.Add($"{KongSchema.Violation<KongConsumer>()} (plugins cannot be null).");
                return;
            }
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(schemas, errorMessages, this);
            }
        }

        public void AssignParentId(KongPlugin child)
        {
            child.Consumer = new Reference { Id = Id };
            child.Service = null;
            child.Route = null;
        }
    }
}
