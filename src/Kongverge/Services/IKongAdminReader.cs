using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public interface IKongAdminReader
    {
        Task<bool> KongIsReachable();
        Task<KongConfiguration> GetConfiguration();
        Task<IReadOnlyCollection<KongService>> GetServices();
        Task<IReadOnlyCollection<KongRoute>> GetRoutes();
        Task<IReadOnlyCollection<KongPlugin>> GetPlugins();
    }
}
