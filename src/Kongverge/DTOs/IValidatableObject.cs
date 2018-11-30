using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Kongverge.DTOs
{
    public interface IValidatableObject
    {
        Task Validate(IDictionary<string, AsyncLazy<KongPluginSchema>> availablePlugins, ICollection<string> errorMessages);
    }
}
