using System.Collections.Generic;

namespace Kongverge.DTOs
{
    public interface IKongPluginHost : IKongParentOf<KongPlugin>
    {
        IReadOnlyList<KongPlugin> Plugins { get; set; }
    }

    public interface IKongParentOf<in T> where T : KongObject
    {
        void AssignParentId(T child);
    }
}
