using System.Threading.Tasks;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public interface IConfigFileWriter
    {
        Task WriteConfiguration(KongvergeConfiguration configuration, string folderPath);
    }
}