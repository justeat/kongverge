using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.Services;

namespace Kongverge.Tests
{
    public class VirtualFileProvider : IFileProvider
    {
        private readonly IDictionary<string, string> _files = new Dictionary<string, string>();

        public bool DirectoryExists(string path)
        {
            return _files.Keys.Any(x => x.StartsWith(path));
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return _files.Keys.Where(x => x.StartsWith(path)).ToArray();
        }

        public Task<string> LoadTextContent(string path)
        {
            return Task.FromResult(_files[path]);
        }

        public Task SaveTextContent(string path, string content)
        {
            _files[path] = content;
            return Task.CompletedTask;
        }

        public void Delete(string path)
        {
            _files.Remove(path);
        }

        public void CreateDirectory(string path) { }
    }
}
