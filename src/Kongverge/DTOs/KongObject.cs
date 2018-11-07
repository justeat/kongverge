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

        protected string ToStringIdSegment()
        {
            return Id == null ? string.Empty : $"Id: {Id}, ";
        }

        public static string GetName(int count, string singular)
        {
            return count == 1
                ? singular
                : singular + "s";
        }
    }
}
