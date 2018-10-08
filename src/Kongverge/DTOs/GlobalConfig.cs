using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;

namespace Kongverge.DTOs
{
    public class GlobalConfig : IKongvergeConfigObject, IKongPluginHost
    {
        [JsonProperty("plugins")]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        public async Task Validate(ICollection<string> errorMessages)
        {
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(errorMessages);
            }
        }

        public virtual void AssignParentId(KongPlugin plugin)
        {
            plugin.ConsumerId = null;
            plugin.ServiceId = null;
            plugin.RouteId = null;
        }

        public string ToConfigJson()
        {
            foreach (var plugin in Plugins)
            {
                plugin.StripPersistedValues();
            }
            return this.ToNormalizedJson() + Environment.NewLine;
        }
    }
}
