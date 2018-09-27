using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public class ExtendibleKongObject : KongObject
    {
        [JsonProperty("plugins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        public override void StripPersistedValues()
        {
            base.StripPersistedValues();
            foreach (var plugin in Plugins)
            {
                plugin.StripPersistedValues();
            }
        }

        public string ToConfigJson()
        {
            StripPersistedValues();
            return this.ToNormalizedJson() + Environment.NewLine;
        }

        public override object GetMatchValue() => null;

        public virtual void AssignParentId(KongPlugin plugin)
        {
            plugin.ConsumerId = null;
            plugin.ServiceId = null;
            plugin.RouteId = null;
        }

        public virtual Task Validate(ICollection<string> errorMessages)
        {
            return Task.CompletedTask;
        }
    }
}
