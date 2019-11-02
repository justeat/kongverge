using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public abstract class KongObject
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreatedAt { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Tags { get; set; }

        internal virtual void StripPersistedValues()
        {
            Id = null;
            CreatedAt = null;
        }

        public T MatchWithExisting<T>(IEnumerable<T> existingObjects) where T : KongObject
        {
            var existing = existingObjects.SingleOrDefault(x => GetMatchValue().Equals(x.GetMatchValue()));
            if (existing != null)
            {
                Id = existing.Id;
                CreatedAt = existing.CreatedAt;
            }
            return existing;
        }

        public abstract object GetMatchValue();

        public abstract StringContent ToJsonStringContent();

        public override string ToString() => ToString(ToStringSegments);

        protected static string ToString(params string[] segments) => "{" + string.Join(", ", segments.Where(x => !string.IsNullOrEmpty(x))) + "}";

        protected static string ToStringSegment<T>(string name, T value, Func<T, string> serialize = null)
        {
            serialize ??= x => x.ToString();
            return value == null ? string.Empty : $"{name}: {serialize(value)}";
        }

        protected abstract string[] ToStringSegments { get; }

        public static string GetName(int count, string singular)
        {
            return count == 1
                ? singular
                : singular + "s";
        }

        public class Reference
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }
    }
}
