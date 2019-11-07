using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public class KongAdminDryRun : KongAdminReader, IKongAdminWriter
    {
        public KongAdminDryRun(KongAdminHttpClient httpClient) : base(httpClient) { }

        public Task PutConsumer(KongConsumer consumer) => Task.CompletedTask;
        public Task PutService(KongService service) => Task.CompletedTask;
        public Task PutRoute(KongRoute route) => Task.CompletedTask;
        public Task PutPlugin(KongPlugin plugin) => Task.CompletedTask;
        public Task DeleteConsumer(string consumerId) => Task.CompletedTask;
        public Task DeleteService(string serviceId) => Task.CompletedTask;
        public Task DeleteRoute(string routeId) => Task.CompletedTask;
        public Task DeletePlugin(string pluginId) => Task.CompletedTask;
    }
}
