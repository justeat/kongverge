using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.Helpers;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace Kongverge.DTOs
{
    public class GlobalConfig : IKongvergeConfigObject, IKongPluginHost
    {
        [JsonProperty("plugins")]
        public IReadOnlyList<KongPlugin> Plugins { get; set; } = Array.Empty<KongPlugin>();

        public async Task Validate(IDictionary<string, AsyncLazy<KongPluginSchema>> availablePlugins, ICollection<string> errorMessages)
        {
            await ValidatePlugins(availablePlugins, errorMessages);
        }

        private async Task ValidatePlugins(IDictionary<string, AsyncLazy<KongPluginSchema>> availablePlugins, ICollection<string> errorMessages)
        {
            if (Plugins == null)
            {
                errorMessages.Add("Global Plugins cannot be null.");
                return;
            }
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(availablePlugins, errorMessages);
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
