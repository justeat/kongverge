using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public class KongAdminDryRun : KongAdminReader, IKongAdminWriter
    {
        public KongAdminDryRun(KongAdminHttpClient httpClient) : base(httpClient) { }

        public Task AddPlugin(KongPlugin plugin) => Task.CompletedTask;

        public Task UpdatePlugin(KongPlugin plugin) => Task.CompletedTask;

        public Task AddRoute(string serviceId, KongRoute route) => Task.CompletedTask;

        public Task AddService(KongService service) => Task.CompletedTask;

        public Task DeletePlugin(string pluginId) => Task.CompletedTask;

        public Task DeleteRoute(string routeId) => Task.CompletedTask;

        public Task DeleteService(string serviceId) => Task.CompletedTask;

        public Task UpdateService(KongService service) => Task.CompletedTask;
    }
}
