using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public class ConfigBuilder
    {
        public virtual async Task<KongvergeConfiguration> FromKong(IKongAdminReader kongReader)
        {
            var plugins = await kongReader.GetPlugins();
            var services = await kongReader.GetServices();
            var routes = await kongReader.GetRoutes();

            foreach (var existingService in services)
            {
                PopulateServiceTree(existingService, routes, plugins);
            }

            return new KongvergeConfiguration
            {
                Services = services.ToArray(),
                GlobalConfig = new ExtendibleKongObject
                {
                    Plugins = plugins.Where(x => x.IsGlobal()).ToArray()
                }
            };
        }

        private static void PopulateServiceTree(KongService service, IReadOnlyCollection<KongRoute> routes, IReadOnlyCollection<KongPlugin> plugins)
        {
            service.Plugins = plugins.Where(x => x.ServiceId == service.Id).ToArray();
            service.Routes = routes.Where(x => x.Service.Id == service.Id).ToArray();
            foreach (var serviceRoute in service.Routes)
            {
                serviceRoute.Plugins = plugins.Where(x => x.RouteId == serviceRoute.Id).ToArray();
            }
        }
    }
}
