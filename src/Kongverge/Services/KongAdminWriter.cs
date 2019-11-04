using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public class KongAdminWriter : KongAdminReader, IKongAdminWriter
    {
        public KongAdminWriter(KongAdminHttpClient httpClient) : base(httpClient) { }

        public Task PutService(KongService service) => PutKongObject("/services", service);

        public Task PutRoute(KongRoute route) => PutKongObject("/routes", route);

        public Task PutPlugin(KongPlugin plugin) => PutKongObject("/plugins", plugin);

        public async Task DeleteService(string serviceId)
        {
            await DeleteRoutes(serviceId);
            await HttpClient.DeleteAsync($"/services/{serviceId}");
        }

        public async Task DeleteRoute(string routeId) => await HttpClient.DeleteAsync($"/routes/{routeId}");

        public async Task DeletePlugin(string pluginId) => await HttpClient.DeleteAsync($"/plugins/{pluginId}");

        private async Task DeleteRoutes(string serviceId)
        {
            var routes = await GetServiceRoutes(serviceId);
            await Task.WhenAll(routes.Select(x => DeleteRoute(x.Id)));
        }

        protected async Task PutKongObject<T>(string path, T kongObject) where T : KongObject
        {
            var content = kongObject.ToJsonStringContent();
            await HttpClient.PutAsync($"{path}/{kongObject.Id}", content);
        }
    }
}
