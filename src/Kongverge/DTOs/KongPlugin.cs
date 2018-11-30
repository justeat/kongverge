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
    public class KongPlugin : KongObject, IKongEquatable<KongPlugin>, IValidatableObject
    {
        public const string ObjectName = "plugin";

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
        public JObject Config { get; set; }

        public bool IsGlobal() =>
            ConsumerId == null &&
            ServiceId == null &&
            RouteId == null;

        public override string ToString()
        {
            return $"{{{ToStringIdSegment()}Name: {Name}}}";
        }

        public override StringContent ToJsonStringContent() => JsonConvert.SerializeObject(this).AsJsonStringContent();

        internal override void StripPersistedValues()
        {
            base.StripPersistedValues();
            ConsumerId = null;
            ServiceId = null;
            RouteId = null;
        }

        public virtual async Task Validate(IDictionary<string, AsyncLazy<KongPluginSchema>> availablePlugins, ICollection<string> errorMessages)
        {
            if (!availablePlugins.ContainsKey(Name))
            {
                errorMessages.Add($"Plugin '{Name}' is not available on Kong server.");
                return;
            }
            var schema = await availablePlugins[Name].Task;
            if (Config == null)
            {
                Config = new JObject();
            }
            ValidateField(schema, Config, errorMessages);
        }

        public override object GetMatchValue() => Name;

        public object GetEqualityValues() =>
             new
             {
                Name,
                Config,
                Enabled
             };

        public bool Equals(KongPlugin other) => this.KongEquals(other);

        public override bool Equals(object obj) => this.KongEqualsObject(obj);

        public override int GetHashCode() => this.GetKongHashCode();

        private static void ValidateField(KongPluginSchema schema, JObject config, ICollection<string> errorMessages)
        {
            foreach (var fieldDefinition in schema.Fields)
            {
                if (config.ContainsKey(fieldDefinition.Key))
                {
                    if (fieldDefinition.Value.Schema == null)
                    {
                        continue;
                    }
                    
                    if (config[fieldDefinition.Key] is JObject container)
                    {
                        // This config field is an object, so recurse to validate and fill in any blanks with their corresponding defaults
                        if (fieldDefinition.Value.Schema.Flexible)
                        {
                            // Drill down into arbitrarily-named children that should each match the schema
                            foreach (var child in container.Properties())
                            {
                                ValidateField(fieldDefinition.Value.Schema, (JObject)child.Value, errorMessages);
                            }
                        }
                        else
                        {
                            ValidateField(fieldDefinition.Value.Schema, container, errorMessages);
                        }
                    }
                    else
                    {
                        errorMessages.Add($"Plugin Config is invalid (field '{config[fieldDefinition.Key].Path}') should be an object.");
                    }
                }
                else
                {
                    // Our config has a missing field
                    if (fieldDefinition.Value.Default.ToString() != FieldDefinition.NoDefault)
                    {
                        // Schema field definition has a default value, so add that to our config
                        config.Add(fieldDefinition.Key, fieldDefinition.Value.Default);
                    }
                    else if (fieldDefinition.Value.Schema != null && !fieldDefinition.Value.Schema.Flexible)
                    {
                        // This config field is an object, so add a new default object and recurse to populate any defaulted fields
                        config.Add(fieldDefinition.Key, new JObject());
                        ValidateField(fieldDefinition.Value.Schema, (JObject)config[fieldDefinition.Key], errorMessages);
                    }
                }
            }

            foreach (var field in config.Properties())
            {
                var key = field.Path.Contains('.')
                    ? field.Path.Substring(field.Path.LastIndexOf('.') + 1)
                    : field.Path;
                if (!schema.Fields.Keys.Contains(key))
                {
                    errorMessages.Add($"Plugin Config is invalid (unknown field '{field.Path}').");
                }
            }
        }
    }
}
