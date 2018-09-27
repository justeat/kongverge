using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public abstract class KongObject
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreatedAt { get; set; }

        public virtual void StripPersistedValues()
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
    }
}
