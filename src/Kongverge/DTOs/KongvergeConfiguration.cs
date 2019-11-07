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
            var pluginsCount = GlobalConfig.Plugins.Count + GlobalConfig.Consumers.Sum(x => x.Plugins.Count) + Services.Sum(x => x.Plugins.Count) + routes.Sum(x => x.Plugins.Count);

            return $"{GlobalConfig.Consumers.Count} {KongObject.GetName(0, KongConsumer.ObjectName)}," +
                   $" {Services.Count} {KongObject.GetName(0, KongService.ObjectName)}," +
                   $" {pluginsCount} {KongObject.GetName(0, KongPlugin.ObjectName)}," +
                   $" {routes.Length} {KongObject.GetName(0, KongRoute.ObjectName)}";
        }
    }

    public interface IKongvergeConfigObject : IValidatableObject
    {
        string ToConfigJson();
    }
}
