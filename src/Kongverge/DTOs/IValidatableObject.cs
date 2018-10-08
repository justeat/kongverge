using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kongverge.DTOs
{
    public interface IValidatableObject
    {
        Task Validate(ICollection<string> errorMessages);
    }
}