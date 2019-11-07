using System.Collections.Generic;
using System.IO;
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

    public class PhysicalFileProvider : IFileProvider
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path, $"*{Constants.FileExtension}", SearchOption.AllDirectories);
        }

        public async Task<string> LoadTextContent(string path)
        {
            using (var reader = File.OpenText(path))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task SaveTextContent(string path, string content)
        {
            using (var stream = File.OpenWrite(path))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
