using System.Collections.Generic;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Nito.AsyncEx;

namespace Kongverge.Services
{
    public interface IConfigFileReader
    {
        Task<KongvergeConfiguration> ReadConfiguration(string folderPath, IDictionary<string, AsyncLazy<KongSchema>> schemas);
    }
}
