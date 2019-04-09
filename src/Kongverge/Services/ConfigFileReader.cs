using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Serilog;

namespace Kongverge.Services
{
    public class ConfigFileReader : IConfigFileReader
    {
        private readonly IFileProvider _fileProvider;

        public ConfigFileReader(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public async Task<KongvergeConfiguration> ReadConfiguration(string folderPath, IDictionary<string, AsyncLazy<KongPluginSchema>> availablePlugins)
        {
            Log.Information($"Reading files from {folderPath}");

            var filePaths = _fileProvider.EnumerateFiles(folderPath).ToArray();

            var fileErrorMessages = new FileErrorMessages();
            var services = new List<KongService>();
            GlobalConfig globalConfig = null;
            var globalConfigFilePaths = filePaths.Where(x => x.EndsWith(Constants.GlobalConfigFileName)).ToArray();
            if (globalConfigFilePaths.Length > 1)
            {
                foreach (var globalConfigFilePath in globalConfigFilePaths)
                {
                    fileErrorMessages.AddErrors(globalConfigFilePath, $"Cannot have more than one {Constants.GlobalConfigFileName} file.");
                    await ParseFile<GlobalConfig>(globalConfigFilePath, availablePlugins, fileErrorMessages);
                }
            }
            else if (globalConfigFilePaths.Any())
            {
                globalConfig = await ParseFile<GlobalConfig>(globalConfigFilePaths.Single(), availablePlugins, fileErrorMessages);
            }
            foreach (var serviceConfigFilePath in filePaths.Except(globalConfigFilePaths))
            {
                services.Add(await ParseFile<KongService>(serviceConfigFilePath, availablePlugins, fileErrorMessages));
            }

            if (fileErrorMessages.Any())
            {
                throw new InvalidConfigurationFilesException(fileErrorMessages.CreateMessage());
            }

            var configuration = new KongvergeConfiguration
            {
                Services = services.AsReadOnly(),
                GlobalConfig = globalConfig ?? new GlobalConfig()
            };

            Log.Information($"Configuration from files: {configuration}");
            return configuration;
        }

        private async Task<T> ParseFile<T>(
            string path,
            IDictionary<string, AsyncLazy<KongPluginSchema>> availablePlugins,
            FileErrorMessages fileErrorMessages) where T : class, IKongvergeConfigObject
        {
            Log.Verbose($"Reading {path}");
            var text = await _fileProvider.LoadTextContent(path);

            var errorMessages = new List<string>();
            try
            {
                var data = JsonConvert.DeserializeObject<T>(text);
                await data.Validate(availablePlugins, errorMessages);
                if (errorMessages.Any())
                {
                    fileErrorMessages.AddErrors(path, errorMessages.ToArray());
                }
                return data;
            }
            catch (Exception e)
            {
                fileErrorMessages.AddErrors(path, e.Message);
            }
            
            return null;
        }

        private class FileErrorMessages : Dictionary<string, List<string>>
        {
            public void AddErrors(string filePath, params string[] errorMessages)
            {
                if (!ContainsKey(filePath))
                {
                    Add(filePath, new List<string>());
                }
                this[filePath].AddRange(errorMessages);
            }

            public string CreateMessage()
            {
                if (Keys.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                var messageHeader = "Invalid configuration file" + (Keys.Count > 1 ? "s" : string.Empty) + ":";
                var message = new StringBuilder(messageHeader, Keys.Count + 1);
                foreach (var filePath in Keys)
                {
                    foreach (var errorMessage in this[filePath])
                    {
                        message.Append($"{Environment.NewLine}{filePath} => {errorMessage}");
                    }
                }

                return message.ToString();
            }
        }
    }
}
