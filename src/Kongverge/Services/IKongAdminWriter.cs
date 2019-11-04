using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public interface IKongAdminWriter
    {
        Task PutService(KongService service);
        Task PutRoute(KongRoute route);
        Task PutPlugin(KongPlugin plugin);
        Task DeleteService(string serviceId);
        Task DeleteRoute(string routeId);
        Task DeletePlugin(string pluginId);
    }
}
