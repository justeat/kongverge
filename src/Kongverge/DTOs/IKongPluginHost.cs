using System.Collections.Generic;

namespace Kongverge.DTOs
{
    public interface IKongPluginHost
    {
        IReadOnlyList<KongPlugin> Plugins { get; set; }
        void AssignParentId(KongPlugin plugin);
    }
}