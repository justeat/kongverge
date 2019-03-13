using System;
using System.Collections.Generic;
using System.Linq;

namespace Kongverge.DTOs
{
    public class KongvergeConfiguration
    {
        public IReadOnlyList<KongService> Services { get; set; } = Array.Empty<KongService>();
        public GlobalConfig GlobalConfig { get; set; } = new GlobalConfig();

        public override string ToString()
        {
            var routes = Services.SelectMany(x => x.Routes).ToArray();
            var pluginsCount = GlobalConfig.Plugins.Count;
            pluginsCount += Services.Sum(x => x.Plugins.Count) + routes.Sum(x => x.Plugins.Count);
            return $"{Services.Count} {KongObject.GetName(0, "service")}, {pluginsCount} {KongObject.GetName(0, "plugin")}, {routes.Length} {KongObject.GetName(0, "route")}";
        }
    }

    public interface IKongvergeConfigObject : IValidatableObject
    {
        string ToConfigJson();
    }
}
