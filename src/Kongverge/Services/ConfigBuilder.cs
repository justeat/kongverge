using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Serilog;

namespace Kongverge.Services
{
    public class ConfigBuilder
    {
        public virtual async Task<KongvergeConfiguration> FromKong(IKongAdminReader kongReader)
        {
            Log.Information("Reading configuration from Kong");

            Log.Verbose("Getting consumers from Kong");
            var consumers = await kongReader.GetConsumers();

            Log.Verbose("Getting plugins from Kong");
            var plugins = await kongReader.GetPlugins();

            Log.Verbose("Getting services from Kong");
            var services = await kongReader.GetServices();

            Log.Verbose("Getting routes from Kong");
            var routes = await kongReader.GetRoutes();

            foreach (var consumer in consumers)
            {
                consumer.Plugins = plugins.Where(x => x.Consumer?.Id == consumer.Id).ToArray();
            }
            foreach (var service in services)
            {
                PopulateServiceTree(service, routes, plugins);
            }

            var configuration = new KongvergeConfiguration
            {
                Services = services.ToArray(),
                GlobalConfig = new GlobalConfig
                {
                    Plugins = plugins.Where(x => x.IsGlobal()).ToArray(),
                    Consumers = consumers.ToArray()
                }
            };

            Log.Information($"Configuration from Kong: {configuration}");

            return configuration;
        }

        private static void PopulateServiceTree(KongService service, IReadOnlyCollection<KongRoute> routes, IReadOnlyCollection<KongPlugin> plugins)
        {
            service.Plugins = plugins.Where(x => x.Service?.Id == service.Id).ToArray();
            service.Routes = routes.Where(x => x.Service.Id == service.Id).ToArray();
            foreach (var serviceRoute in service.Routes)
            {
                serviceRoute.Plugins = plugins.Where(x => x.Route?.Id == serviceRoute.Id).ToArray();
            }
        }
    }
}
