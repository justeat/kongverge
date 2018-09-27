using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;
using Serilog;

namespace Kongverge.Services
{
    public class KongAdminReader : IKongAdminReader
    {
        private const string ConfigurationRoute = "/";
        private const string ServicesRoute = "/services";
        private const string RoutesRoute = "/routes";
        private const string PluginsRoute = "/plugins";

        protected readonly KongAdminHttpClient HttpClient;

        public KongAdminReader(KongAdminHttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<bool> KongIsReachable()
        {
            try
            {
                await HttpClient.GetAsync("/");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ERROR: Unable to contact Kong: {baseAddress}", HttpClient.BaseAddress);
                return false;
            }

            return true;
        }

        public async Task<KongConfiguration> GetConfiguration()
        {
            var response = await HttpClient.GetAsync(ConfigurationRoute);
            var value = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KongConfiguration>(value);
        }

        public async Task<IReadOnlyCollection<KongService>> GetServices() =>
            await GetPagedResponse<KongService>(ServicesRoute);

        public async Task<IReadOnlyCollection<KongRoute>> GetRoutes() =>
            await GetPagedResponse<KongRoute>(RoutesRoute);

        public async Task<IReadOnlyCollection<KongPlugin>> GetPlugins() =>
            await GetPagedResponse<KongPlugin>(PluginsRoute);

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
