using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;

namespace Kongverge.Services
{
    public class KongAdminReader : IKongAdminReader
    {
        protected readonly KongAdminHttpClient HttpClient;

        public KongAdminReader(KongAdminHttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<KongConfiguration> GetConfiguration()
        {
            var response = await HttpClient.GetAsync("/");
            var value = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KongConfiguration>(value);
        }

        public async Task<IReadOnlyCollection<KongService>> GetServices() =>
            await GetPagedResponse<KongService>("/services");

        public async Task<IReadOnlyCollection<KongRoute>> GetRoutes() =>
            await GetPagedResponse<KongRoute>("/routes");

        public async Task<IReadOnlyCollection<KongPlugin>> GetPlugins() =>
            await GetPagedResponse<KongPlugin>("/plugins");

        public async Task<KongPluginSchema> GetPluginSchema(string pluginName)
        {
            var response = await HttpClient.GetAsync($"/plugins/schema/{pluginName}");
            var value = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KongPluginSchema>(value);
        }

        protected Task<IReadOnlyList<KongRoute>> GetServiceRoutes(string serviceId) =>
            GetPagedResponse<KongRoute>($"/services/{serviceId}/routes");

        private async Task<IReadOnlyList<T>> GetPagedResponse<T>(string requestUri)
        {
            var data = new List<T>();
            var lastPage = false;

            do
            {
                var response = await HttpClient.GetAsync(requestUri);
                var value = await response.Content.ReadAsStringAsync();
                var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<T>>(value);

                data.AddRange(pagedResponse.Data);

                if (pagedResponse.Next == null)
                {
                    lastPage = true;
                }
                else
                {
                    requestUri = pagedResponse.Next;
                }
            } while (!lastPage);

            return data.AsReadOnly();
        }
    }
}
