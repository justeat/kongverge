using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;

namespace Kongverge.Services
{
    public class KongAdminWriter : KongAdminReader, IKongAdminWriter
    {
        public KongAdminWriter(KongAdminHttpClient httpClient) : base(httpClient) { }

        public async Task AddService(KongService service)
        {
            var content = service.ToJsonStringContent();
            var response = await HttpClient.PostAsync("/services/", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var added = JsonConvert.DeserializeObject<KongService>(responseBody);
            service.Id = added.Id;
        }

        public async Task UpdateService(KongService service)
        {
            var content = service.ToJsonStringContent();
            await HttpClient.PatchAsync($"/services/{service.Id}", content);
        }

        public async Task DeleteService(string serviceId)
        {
            await DeleteRoutes(serviceId);
            await HttpClient.DeleteAsync($"/services/{serviceId}");
        }

        public async Task AddRoute(string serviceId, KongRoute route)
        {
            var content = route.ToJsonStringContent();
            var response = await HttpClient.PostAsync($"/services/{serviceId}/routes", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var added = JsonConvert.DeserializeObject<KongRoute>(responseBody);
            route.Id = added.Id;
            route.Service = added.Service;
        }

        public async Task DeleteRoute(string routeId)
        {
            await HttpClient.DeleteAsync($"/routes/{routeId}");
        }

        public async Task AddPlugin(KongPlugin plugin)
        {
            var content = plugin.ToJsonStringContent();
            var response = await HttpClient.PostAsync("/plugins", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var added = JsonConvert.DeserializeObject<KongPlugin>(responseBody);
            plugin.Id = added.Id;
        }

        public async Task UpdatePlugin(KongPlugin plugin)
        {
            var content = plugin.ToJsonStringContent();
            await HttpClient.PatchAsync($"/plugins/{plugin.Id}", content);
        }

        public async Task DeletePlugin(string pluginId)
        {
            await HttpClient.DeleteAsync($"/plugins/{pluginId}");
        }

        private async Task DeleteRoutes(string serviceId)
        {
            var routes = await GetServiceRoutes(serviceId);
            await Task.WhenAll(routes.Select(x => DeleteRoute(x.Id)));
        }
    }
}
