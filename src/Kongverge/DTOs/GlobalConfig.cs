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

        [JsonProperty("consumers")]
        public IReadOnlyList<KongConsumer> Consumers { get; set; } = Array.Empty<KongConsumer>();

        public async Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null)
        {
            await ValidatePlugins(schemas, errorMessages);
            await ValidateConsumers(schemas, errorMessages);
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

        private async Task ValidateConsumers(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages)
        {
            if (Consumers == null)
            {
                errorMessages.Add("Global Consumers cannot be null.");
                return;
            }
            foreach (var consumer in Consumers)
            {
                await consumer.Validate(schemas, errorMessages);
            }
        }

        public virtual void AssignParentId(KongPlugin child)
        {
            child.Consumer = null;
            child.Service = null;
            child.Route = null;
        }

        public string ToConfigJson()
        {
            foreach (var plugin in Plugins)
            {
                plugin.StripPersistedValues();
            }
            foreach (var consumer in Consumers)
            {
                consumer.StripPersistedValues();
            }
            return this.ToNormalizedJson() + Environment.NewLine;
        }
    }
}
