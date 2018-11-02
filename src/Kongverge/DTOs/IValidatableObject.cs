using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kongverge.DTOs
{
    public interface IValidatableObject
    {
        Task Validate(IReadOnlyCollection<string> availablePlugins, ICollection<string> errorMessages);
    }
}
