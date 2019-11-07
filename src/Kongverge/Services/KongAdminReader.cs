using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;

namespace Kongverge.Services
{
    public interface IKongAdminReader
    {
        Task<KongConfiguration> GetConfiguration();
        Task<IReadOnlyCollection<KongConsumer>> GetConsumers();
        Task<IReadOnlyCollection<KongService>> GetServices();
        Task<IReadOnlyCollection<KongRoute>> GetRoutes();
        Task<IReadOnlyCollection<KongPlugin>> GetPlugins();
        Task<KongSchema> GetSchema(string schemaPath);
    }

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

        public async Task<IReadOnlyCollection<KongConsumer>> GetConsumers() =>
            await GetPagedResponse<KongConsumer>("/consumers");

        public async Task<IReadOnlyCollection<KongService>> GetServices() =>
            await GetPagedResponse<KongService>("/services");

        public async Task<IReadOnlyCollection<KongRoute>> GetRoutes() =>
            await GetPagedResponse<KongRoute>("/routes");

        public async Task<IReadOnlyCollection<KongPlugin>> GetPlugins() =>
            await GetPagedResponse<KongPlugin>("/plugins");

        public async Task<KongSchema> GetSchema(string schemaPath)
        {
            var response = await HttpClient.GetAsync($"/schemas/{schemaPath}");
            var value = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KongSchema>(value);
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
