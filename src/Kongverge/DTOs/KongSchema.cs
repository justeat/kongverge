using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kongverge.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kongverge.DTOs
{
    public class KongSchema
    {
        private static readonly string NoDefault = Guid.NewGuid().ToString();

        [JsonProperty("one_of")]
        public JToken[] OneOf { get; set; } = Array.Empty<JToken>();

        [JsonProperty("default")]
        public JToken Default { get; set; } = NoDefault;

        [JsonProperty("type")]
        public string Type { get; set; } = FieldType.Record;

        [JsonProperty("legacy")]
        public bool Legacy { get; set; }

        [JsonProperty("unique")]
        public bool Unique { get; set; }

        [JsonProperty("uuid")]
        public bool Uuid { get; set; }

        [JsonProperty("auto")]
        public bool Auto { get; set; }

        [JsonProperty("timestamp")]
        public bool Timestamp { get; set; }

        [JsonProperty("eq", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Eq { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("on_delete")]
        public string OnDelete { get; set; }

        [JsonProperty("abstract")]
        public bool Abstract { get; set; }

        [JsonProperty("fields")]
        public Field[] Fields { get; set; } = new Field[0];

        [JsonProperty("elements")]
        public KongSchema Elements { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("mutually_exclusive_subsets", NullValueHandling = NullValueHandling.Ignore)]
        public string[][] MutuallyExclusiveSubsets { get; set; }

        [JsonProperty("starts_with")]
        public string StartsWith { get; set; }

        [JsonProperty("match")]
        public string Match { get; set; }

        [JsonProperty("match_any")]
        public FieldCheck MatchAny { get; set; }

        [JsonProperty("match_none")]
        public FieldCheck[] MatchNone { get; set; } = Array.Empty<FieldCheck>();

        [JsonProperty("match_all")]
        public FieldCheck[] MatchAll { get; set; } = Array.Empty<FieldCheck>();

        [JsonProperty("gt")]
        public double? Gt { get; set; }

        [JsonProperty("between")]
        public double[] Between { get; set; }

        [JsonProperty("len_min")]
        public int? LenMin { get; set; }

        [JsonProperty("keys")]
        public KongSchema Keys { get; set; }

        [JsonProperty("values")]
        public KongSchema Values { get; set; }

        [JsonProperty("entity_checks")]
        public EntityCheck[] EntityChecks { get; set; } = Array.Empty<EntityCheck>();
        
        public void Validate<T>(JToken node, ICollection<string> errorMessages, KongObject parent = null) where T : KongObject
        {
            if (node != null && !IsValidType(node))
            {
                errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should be of type '{Type}').");
                return;
            }

            switch (Type)
            {
                case FieldType.String:
                    ValidateString<T>(node, errorMessages);
                    break;
                case FieldType.Number:
                case FieldType.Integer:
                    ValidateNumber<T>(node, errorMessages);
                    break;
                case FieldType.Boolean:
                    break;
                case FieldType.Set:
                case FieldType.Array:
                    ValidateCollection<T>(node, errorMessages, parent);
                    break;
                case FieldType.Foreign:
                    ValidateForeign<T>(node, errorMessages, parent);
                    break;
                case FieldType.Record:
                    ValidateRecord<T>(node, errorMessages, parent);
                    break;
                case FieldType.Map:
                    ValidateMap<T>(node, errorMessages, parent);
                    break;
                default: throw new NotImplementedException();
            }
        }

        private bool IsValidType(JToken value)
        {
            switch (Type)
            {
                case FieldType.String:
                    return value.Type == JTokenType.String;

                case FieldType.Number:
                    return value.Type == JTokenType.Integer ||
                           value.Type == JTokenType.Float;

                case FieldType.Integer:
                    return value.Type == JTokenType.Integer;

                case FieldType.Boolean:
                    return value.Type == JTokenType.Boolean;

                case FieldType.Set:
                case FieldType.Array:
                    return value.Type == JTokenType.Array;

                case FieldType.Foreign:
                case FieldType.Record:
                case FieldType.Map:
                    return value.Type == JTokenType.Object;

                default: throw new NotImplementedException();
            }
        }

        private bool HasDefault => Default.ToString() != NoDefault;

        private void ValidateString<T>(JToken node, ICollection<string> errorMessages) where T : KongObject
        {
            var value = (string)node;
            if (OneOf.Any())
            {
                var options = OneOf.Select(x => x.Value<string>()).ToArray();
                if (!options.Contains(value))
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should be one of '{string.Join(", ", options)}').");
                }
            }
            if (StartsWith != null)
            {
                if (value == null || !value.StartsWith(StartsWith))
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should start with '{StartsWith}').");
                }
            }
            if (Match != null)
            {
                if (!LuaPattern.IsMatch(value, Match))
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should match lua pattern '{Match}').");
                }
            }
            if (MatchAny != null)
            {
                if (MatchAny.Patterns.All(x => !LuaPattern.IsMatch(value, x)))
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' {MatchAny.Error}).");
                }
            }
            if (MatchNone.Any())
            {
                foreach (var fieldCheck in MatchNone)
                {
                    if (LuaPattern.IsMatch(value, fieldCheck.Pattern))
                    {
                        errorMessages.Add($"{Violation<T>()} (field '{node.Path}' {fieldCheck.Error}).");
                    }
                }
            }
            if (MatchAll.Any())
            {
                foreach (var fieldCheck in MatchAll)
                {
                    if (!LuaPattern.IsMatch(value, fieldCheck.Pattern))
                    {
                        errorMessages.Add($"{Violation<T>()} (field '{node.Path}' {fieldCheck.Error}).");
                    }
                }
            }
            if (LenMin.HasValue)
            {
                var len = value?.Length;
                if (len < LenMin)
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should have min length '{LenMin.Value}').");
                }
            }
            if (Uuid)
            {
                if (!Guid.TryParse(value, out _))
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should be a 'uuid').");
                }
            }
        }

        private void ValidateNumber<T>(JToken node, ICollection<string> errorMessages) where T : KongObject
        {
            var number = (double)node;
            if (OneOf.Any())
            {
                var options = OneOf.Select(x => x.Value<double>()).ToArray();
                if (!options.Contains(number))
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should be one of '{string.Join(", ", options)}').");
                }
            }
            if (Between != null)
            {
                if (number < Between[0] || number > Between[1])
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should be between '{Between[0]}' and '{Between[1]}').");
                }
            }
            if (Gt.HasValue)
            {
                if (number <= Gt.Value)
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should be greater than '{Gt.Value}').");
                }
            }
        }

        private void ValidateCollection<T>(JToken node, ICollection<string> errorMessages, KongObject parent) where T : KongObject
        {
            var array = (JArray)node;
            if (LenMin.HasValue)
            {
                var len = array.Children().Count();
                if (len < LenMin)
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should have min length '{LenMin.Value}').");
                }
            }
            if (Elements != null)
            {
                foreach (var child in array.Children())
                {
                    if (!Equals(child, JValue.CreateNull()))
                    {
                        Elements.Validate<T>(child, errorMessages, parent);
                    }
                }

                if (Type == FieldType.Set)
                {
                    if (Elements.Type == FieldType.Record &&
                        array.Children().Select(JsonConvert.SerializeObject).Distinct().Count() < array.Children().Count() ||
                        array.Children().Distinct().Count() < array.Children().Count())
                    {
                        errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should have distinct elements).");
                    }
                }
                if (Elements.Type == FieldType.String && MutuallyExclusiveSubsets != null)
                {
                    string[] firstMatchedSubset = null;
                    foreach (var child in array.Children())
                    {
                        var childString = child.ToString();
                        foreach (var subSet in MutuallyExclusiveSubsets)
                        {
                            if (subSet.Contains(childString))
                            {
                                firstMatchedSubset ??= subSet;
                                if (subSet != firstMatchedSubset)
                                {
                                    errorMessages.Add($"{Violation<T>()} (field '{child.Path}' doesn't match mutually exclusive subsets '{JsonConvert.SerializeObject(MutuallyExclusiveSubsets)}').");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ValidateForeign<T>(JToken node, ICollection<string> errorMessages, KongObject parent) where T : KongObject
        {
            if (Equals(Eq, JValue.CreateNull()))
            {
                switch (Reference)
                {
                    case "consumers" when parent is KongConsumer:
                    case "routes" when parent is KongRoute:
                    case "services" when parent is KongService:
                    case "certificates" when parent is KongCertificate:
                        errorMessages.Add($"{Violation<T>()} ('{Reference}' reference should be null).");
                        break;
                }
            }
        }

        private void ValidateRecord<T>(JToken node, ICollection<string> errorMessages, KongObject parent) where T : KongObject
        {
            var record = (JObject)node;
            foreach (var field in Fields)
            {
                if (!record.ContainsKey(field.Name) && field.Schema.Required && field.Schema.HasDefault)
                {
                    // Our record has a missing field and schema has a default value, so add that to our record
                    record.Add(field.Name, field.Schema.Default);
                }

                if (field.Schema.Required && (record[field.Name] == null || Equals(record[field.Name], JValue.CreateNull())))
                {
                    var fieldPath = string.IsNullOrEmpty(record.Path)
                        ? field.Name
                        : $"{record.Path}.{field.Name}";
                    errorMessages.Add($"{Violation<T>()} (field '{fieldPath}' is required).");
                }
                else if (field.Schema.Type == FieldType.Foreign || record[field.Name] != null && !Equals(record[field.Name], JValue.CreateNull()))
                {
                    field.Schema.Validate<T>(record[field.Name], errorMessages, parent);
                }
            }
            foreach (var field in record.Properties())
            {
                var key = field.Path.Contains('.')
                    ? field.Path.Substring(field.Path.LastIndexOf('.') + 1)
                    : field.Path;
                if (Fields.All(x => x.Name != key))
                {
                    errorMessages.Add($"{Violation<T>()} (unknown field '{field.Path}').");
                }
            }
            foreach (var entityCheck in EntityChecks)
            {
                if (entityCheck.AtLeastOneOf != null)
                {
                    if (entityCheck.AtLeastOneOf.All(x => !record.ContainsKey(x)))
                    {
                        errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should have at least one of '{string.Join(", ", entityCheck.AtLeastOneOf)}').");
                    }
                }
            }
        }

        private void ValidateMap<T>(JToken node, ICollection<string> errorMessages, KongObject parent) where T : KongObject
        {
            var map = (JObject)node;
            if (LenMin.HasValue)
            {
                var len = map.Children().Count();
                if (len < LenMin)
                {
                    errorMessages.Add($"{Violation<T>()} (field '{node.Path}' should have min length '{LenMin.Value}').");
                }
            }
            if (Values != null)
            {
                // Drill down into arbitrarily-named keys that should each match the schema
                foreach (var field in map.Properties())
                {
                    Keys?.Validate<T>(JToken.FromObject(field.Name), errorMessages, parent);

                    if (!Equals(field.Value, JValue.CreateNull()))
                    {
                        Values.Validate<T>(field.Value, errorMessages, parent);
                    }
                }
            }
        }

        public static string Violation<T>() where T : KongObject
        {
            var type = typeof(T);
            var name = (string)type.GetField("ObjectName", BindingFlags.Public|BindingFlags.Static).GetValue(null);
            return $"{name} schema violation";
        }
    }

    public class Field : Dictionary<string, KongSchema>
    {
        public Field() { }
        public Field(string name, KongSchema schema) => Add(name, schema);

        public string Name => this.Single().Key;
        public KongSchema Schema => this.Single().Value;
    }

    public class EntityCheck
    {
        [JsonProperty("at_least_one_of")]
        public string[] AtLeastOneOf { get; set; }
    }

    public class FieldCheck
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("patterns")]
        public string[] Patterns { get; set; } = Array.Empty<string>();

        [JsonProperty("err")]
        public string Error { get; set; }
    }

    public static class FieldType
    {
        public const string Foreign = "foreign";
        public const string String = "string";
        public const string Set = "set";
        public const string Array = "array";
        public const string Number = "number";
        public const string Integer = "integer";
        public const string Boolean = "boolean";
        public const string Record = "record";
        public const string Map = "map";
    }
}
