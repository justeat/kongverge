using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kongverge.Helpers
{
    public static class JsonConversionExtensions
    {
        public static string ToNormalizedJson(this object value)
        {
            var token = JToken.FromObject(value);
            return JsonConvert.SerializeObject(token.Normalize(), new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
        }

        private static JToken Normalize(this JToken token)
        {
            switch (token)
            {
                case JObject jObject:
                {
                    var normalized = new JObject();
                    foreach (var property in jObject.Properties().OrderBy(x => x.Name))
                    {
                        if (property.Value.Type == JTokenType.Null || property.Value is JArray jArray && jArray.Count == 0)
                        {
                            continue;
                        }
                        normalized.Add(property.Name, property.Value.Normalize());
                    }
                    return normalized;
                }
                case JArray jArray:
                {
                    if (jArray.Count > 0 && jArray.All(x => x is JValue))
                    {
                        var firstType = jArray[0].Type;
                        if (jArray.Select(x => x.Type).All(x => x == firstType))
                        {
                            return new JArray(jArray.OrderBy(x => x));
                        }
                    }
                    for (var i = 0; i < jArray.Count; i++)
                    {
                        jArray[i] = jArray[i].Normalize();
                    }
                    return jArray;
                }
                default:
                    return token;
            }
        }
    }
}
