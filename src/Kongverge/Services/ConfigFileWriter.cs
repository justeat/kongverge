using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Serilog;

namespace Kongverge.Services
{
    public class ConfigFileWriter
    {
        public virtual async Task WriteConfiguration(KongvergeConfiguration configuration, string folderPath)
        {
            Log.Information("Writing files to {folderPath}", folderPath);
            PrepareOutputFolder(folderPath);

            foreach (var service in configuration.Services)
            {
                await WriteConfigObject(service, folderPath, $"{service.Name}{Settings.FileExtension}");
            }

            if (configuration.GlobalConfig.Plugins.Any())
            {
                await WriteConfigObject(configuration.GlobalConfig, folderPath, Settings.GlobalConfigFileName);
            }
        }

        private static async Task WriteConfigObject(ExtendibleKongObject configObject, string folderPath, string fileName)
        {
            var json = configObject.ToConfigJson();
            var path = Path.Join(folderPath, fileName);
            Log.Information("Writing {path}", path);
            using (var stream = File.OpenWrite(path))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(json);
            }
        }

        private static void PrepareOutputFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var path in Directory.EnumerateFiles(folderPath))
                {
                    File.Delete(path);
                }
            }
            else
            {
                Directory.CreateDirectory(folderPath);
            }
        }
    }
}
