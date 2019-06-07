using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kongverge.Services
{
    public interface IFileProvider
    {
        bool DirectoryExists(string path);
        IEnumerable<string> EnumerateFiles(string path);
        Task<string> LoadTextContent(string path);
        Task SaveTextContent(string path, string content);
        void Delete(string path);
        void CreateDirectory(string path);
    }
}
