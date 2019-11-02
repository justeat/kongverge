using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Kongverge.DTOs
{
    public interface IValidatableObject
    {
        Task Validate(IDictionary<string, AsyncLazy<KongSchema>> schemas, ICollection<string> errorMessages, KongObject parent = null);
    }
}
