using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kongverge.DTOs
{
    public class KongPluginSchema
    {
        [JsonProperty("fields")]
        public Dictionary<string, FieldDefinition> Fields { get; set; } = new Dictionary<string, FieldDefinition>();

        [JsonProperty("flexible")]
        public bool Flexible { get; set; }
    }

    public class FieldDefinition
    {
        public static string NoDefault = Guid.NewGuid().ToString();

        [JsonProperty("enum")]
        public string[] Enum { get; set; }

        [JsonProperty("default")]
        public JToken Default { get; set; } = NoDefault;

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("schema")]
        public KongPluginSchema Schema { get; set; }
    }
}
