using System;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Serilog;

namespace Kongverge.Services
{
    public class KongAdminDryRun : KongAdminReader, IKongAdminWriter
    {
        public KongAdminDryRun(KongAdminHttpClient httpClient) : base(httpClient)
        {
        }

        public Task UpsertPlugin(KongPlugin plugin)
        {
            Log.Information(string.IsNullOrWhiteSpace(plugin.Id) ? $"Adding plugin {plugin.Name}" : $"Updating plugin {plugin.Name}");
            plugin.Id = plugin.Id ?? Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }

        public Task AddRoute(string serviceId, KongRoute route)
        {
            Log.Information(@"Adding route {route}", route);
            route.Id = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }

        public Task AddService(KongService service)
        {
            Log.Information("Adding service {name}", service.Name);
            service.Id = Guid.NewGuid().ToString();
            return Task.CompletedTask;
        }

        public Task DeletePlugin(string pluginId)
        {
            Log.Information("Deleting plugin {id}", pluginId);
            return Task.CompletedTask;
        }

        public Task DeleteRoute(string routeId)
        {
            Log.Information("Deleting route {id}", routeId);
            return Task.CompletedTask;
        }

        public Task DeleteService(string serviceId)
        {
            Log.Information("Deleting service {id}", serviceId);
            return Task.CompletedTask;
        }

        public Task UpdateService(KongService service)
        {
            Log.Information($"Updating service {service.Name}");
            return Task.CompletedTask;
        }
    }
}
