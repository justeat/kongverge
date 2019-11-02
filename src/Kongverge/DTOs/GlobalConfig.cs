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

        public async Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            await ValidatePlugins(schemas, errorMessages);
        }

        private async Task ValidatePlugins(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages)
        {
            if (Plugins == null)
            {
                errorMessages.Add("Global Plugins cannot be null.");
                return;
            }
            foreach (var plugin in Plugins)
            {
                await plugin.Validate(schemas, errorMessages);
            }
        }

        public virtual void AssignParentId(KongPlugin plugin)
        {
            plugin.Consumer = null;
            plugin.Service = null;
            plugin.Route = null;
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
