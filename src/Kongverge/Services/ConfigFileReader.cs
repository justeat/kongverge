using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Newtonsoft.Json;
using Serilog;

namespace Kongverge.Services
{
    public class ConfigFileReader
    {
        public virtual async Task<KongvergeConfiguration> ReadConfiguration(string folderPath, IReadOnlyCollection<string> availablePlugins)
        {
            Log.Information($"Reading files from {folderPath}");

            var filePaths = Directory.EnumerateFiles(folderPath, $"*{Settings.FileExtension}", SearchOption.AllDirectories).ToArray();

            var fileErrorMessages = new FileErrorMessages();
            var services = new List<KongService>();
            GlobalConfig globalConfig = null;
            var globalConfigFilePaths = filePaths.Where(x => x.EndsWith(Settings.GlobalConfigFileName)).ToArray();
            if (globalConfigFilePaths.Length > 1)
            {
                foreach (var globalConfigFilePath in globalConfigFilePaths)
                {
                    fileErrorMessages.AddErrors(globalConfigFilePath, $"Cannot have more than one {Settings.GlobalConfigFileName} file.");
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
            
            return new KongvergeConfiguration
            {
                Services = services.AsReadOnly(),
                GlobalConfig = globalConfig ?? new GlobalConfig()
            };
        }

        private static async Task<T> ParseFile<T>(string path, IReadOnlyCollection<string> availablePlugins, FileErrorMessages fileErrorMessages) where T : class, IKongvergeConfigObject
        {
            Log.Verbose($"Reading {path}");
            string text;
            using (var reader = File.OpenText(path))
            {
                text = await reader.ReadToEndAsync();
            }

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
