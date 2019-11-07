using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Serilog;

namespace Kongverge.Services
{
    public interface IConfigFileWriter
    {
        Task WriteConfiguration(KongvergeConfiguration configuration, string folderPath);
    }

    public class ConfigFileWriter : IConfigFileWriter
    {
        private readonly IFileProvider _fileProvider;

        public ConfigFileWriter(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public async Task WriteConfiguration(KongvergeConfiguration configuration, string folderPath)
        {
            Log.Information($"Writing files to {folderPath}");
            PrepareOutputFolder(folderPath);

            foreach (var service in configuration.Services)
            {
                await WriteConfigObject(service, folderPath, $"{service.Name}{Constants.FileExtension}");
            }

            if (configuration.GlobalConfig.Plugins.Any() || configuration.GlobalConfig.Consumers.Any())
            {
                await WriteConfigObject(configuration.GlobalConfig, folderPath, Constants.GlobalConfigFileName);
            }
        }

        private async Task WriteConfigObject(IKongvergeConfigObject configObject, string folderPath, string fileName)
        {
            var json = configObject.ToConfigJson();
            var path = Path.Join(folderPath, fileName);
            Log.Verbose($"Writing {path}");
            await _fileProvider.SaveTextContent(path, json);
        }

        private void PrepareOutputFolder(string folderPath)
        {
            if (_fileProvider.DirectoryExists(folderPath))
            {
                foreach (var path in _fileProvider.EnumerateFiles(folderPath))
                {
                    _fileProvider.Delete(path);
                }
            }
            else
            {
                _fileProvider.CreateDirectory(folderPath);
            }
        }
    }
}
