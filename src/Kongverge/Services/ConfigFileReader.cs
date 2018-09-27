using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;
using Serilog;

namespace Kongverge.Services
{
    public class ConfigFileReader
    {
        public virtual async Task<KongvergeConfiguration> ReadConfiguration(string folderPath)
        {
            Log.Information("Reading files from {folderPath}", folderPath);

            var filePaths = Directory.EnumerateFiles(folderPath, $"*{Settings.FileExtension}", SearchOption.AllDirectories);

            var services = new List<KongService>();
            ExtendibleKongObject globalConfig = null;
            foreach (var configFilePath in filePaths)
            {
                if (configFilePath.EndsWith(Settings.GlobalConfigFileName))
                {
                    if (globalConfig != null)
                    {
                        throw new InvalidConfigurationFileException(configFilePath, $"Cannot have more than one {Settings.GlobalConfigFileName} file.");
                    }
                    globalConfig = await ParseFile<ExtendibleKongObject>(configFilePath);
                }
                else
                {
                    services.Add(await ParseFile<KongService>(configFilePath));
                }
            }
            
            return new KongvergeConfiguration
            {
                Services = services.AsReadOnly(),
                GlobalConfig = globalConfig ?? new ExtendibleKongObject()
            };
        }

        private static async Task<T> ParseFile<T>(string path) where T : ExtendibleKongObject
        {
            Log.Information("Reading {path}", path);
            string text;
            using (var reader = File.OpenText(path))
            {
                text = await reader.ReadToEndAsync();
            }

            T data;
            var errorMessages = new List<string>();
            try
            {
                data = JsonConvert.DeserializeObject<T>(text);
                await data.Validate(errorMessages);
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationFileException(path, ex.Message, ex);
            }

            if (errorMessages.Any())
            {
                throw new InvalidConfigurationFileException(path, string.Join(Environment.NewLine, errorMessages));
            }
            
            return data;
        }
    }
}
