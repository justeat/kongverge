using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.Options;
using Serilog;

namespace Kongverge.Workflow
{
    public class KongvergeWorkflow : Workflow
    {
        private readonly IKongAdminWriter _kongWriter;
        private readonly ConfigFileReader _configReader;
        private readonly ConfigBuilder _configBuilder;

        public KongvergeWorkflow(
            IKongAdminReader kongReader,
            IOptions<Settings> configuration,
            IKongAdminWriter kongWriter,
            ConfigFileReader configReader,
            ConfigBuilder configBuilder) : base(kongReader, configuration)
        {
            _kongWriter = kongWriter;
            _configReader = configReader;
            _configBuilder = configBuilder;
        }

        public override async Task<int> DoExecute()
        {
            KongvergeConfiguration targetConfiguration;
            try
            {
                targetConfiguration = await _configReader.ReadConfiguration(Configuration.InputFolder);
            }
            catch (DirectoryNotFoundException ex)
            {
                return ExitWithCode.Return(ExitCode.InputFolderUnreachable, ex.Message);
            }
            catch (InvalidConfigurationFileException ex)
            {
                return ExitWithCode.Return(ExitCode.InvalidConfigurationFile, $"Invalid configuration file {ex.Path}{Environment.NewLine}{ex.Message}");
            }

            var existingConfiguration = await _configBuilder.FromKong(KongReader);

            await ConvergeChildrenPlugins(null, existingConfiguration.GlobalConfig, targetConfiguration.GlobalConfig);
            
            await ConvergeObjects(
                null,
                "service",
                "services",
                existingConfiguration.Services,
                targetConfiguration.Services,
                x => _kongWriter.DeleteService(x.Id),
                x => _kongWriter.AddService(x),
                x => _kongWriter.UpdateService(x),
                ConvergeServiceChildren);

            return ExitWithCode.Return(ExitCode.Success);
        }

        private static async Task ConvergeObjects<T>(
            string parent,
            string singularObjectName,
            string pluralObjectName,
            IReadOnlyCollection<T> existingObjects,
            IReadOnlyCollection<T> targetObjects,
            Func<T, Task> deleteObject,
            Func<T, Task> createObject,
            Func<T, Task> updateObject = null,
            Func<T, T, Task> recurse = null) where T : KongObject, IKongEquatable
        {
            existingObjects = existingObjects ?? Array.Empty<T>();
            updateObject = updateObject ?? (x => Task.CompletedTask);
            recurse = recurse ?? ((e, t) => Task.CompletedTask);

            if (existingObjects.Count == 0 && targetObjects.Count == 0)
            {
                Log.Information($"Target {parent ?? "configuration"} and existing both have zero {pluralObjectName}");
                return;
            }

            var targetPhrase = $"{targetObjects.Count} target {GetName(targetObjects.Count, singularObjectName, pluralObjectName)}";
            var existingPhrase = $"{existingObjects.Count} existing {GetName(existingObjects.Count, singularObjectName, pluralObjectName)}";
            var parentPhrase = parent == null ? string.Empty : $" attached to {parent}";
            Log.Information($"Converging {targetPhrase}{parentPhrase} with {existingPhrase}");

            var targetMatchValues = targetObjects.Select(x => x.GetMatchValue()).ToArray();
            var toRemove = existingObjects.Where(x => !targetMatchValues.Contains(x.GetMatchValue())).ToArray();

            foreach (var existing in toRemove)
            {
                Log.Information($"Deleting {singularObjectName} {existing}{parentPhrase} which exists in Kong but not in target configuration");
                await deleteObject(existing);
            }

            foreach (var target in targetObjects)
            {
                var existing = target.MatchWithExisting(existingObjects);
                if (existing == null)
                {
                    Log.Information($"Creating {singularObjectName} {target}{parentPhrase} which exists in target configuration but not in Kong");
                    await createObject(target);
                }
                else if (target.Equals(existing))
                {
                    Log.Information($"Identical {singularObjectName} {existing}{parentPhrase} found in Kong matching target configuration");
                }
                else
                {
                    var patch = target.DifferencesFrom(existing);
                    Log.Information($"Updating {singularObjectName} {existing}{parentPhrase} which exists in both Kong and target configuration, having the following differences:{Environment.NewLine}{patch}");
                    await updateObject(target);
                }
                await recurse(existing, target);
            }
        }

        private Task ConvergeChildrenPlugins(string parent, IKongPluginHost existing, IKongPluginHost target)
        {
            Task UpsertPlugin(KongPlugin plugin, IKongPluginHost host)
            {
                host.AssignParentId(plugin);
                return _kongWriter.UpsertPlugin(plugin);
            }

            return ConvergeObjects(
                parent,
                "plugin",
                "plugins",
                existing?.Plugins,
                target.Plugins,
                x => _kongWriter.DeletePlugin(x.Id),
                x => UpsertPlugin(x, target),
                x => UpsertPlugin(x, target));
        }

        private async Task ConvergeServiceChildren(KongService existing, KongService target)
        {
            var parent = $"service {target}";
            await ConvergeChildrenPlugins(parent, existing, target);
            await ConvergeObjects(
                parent,
                "route",
                "routes",
                existing?.Routes,
                target.Routes,
                x => _kongWriter.DeleteRoute(x.Id),
                x => _kongWriter.AddRoute(target.Id, x),
                null,
                (e, t) => ConvergeChildrenPlugins($"route {t} attached to service {target}", e, t));
        }

        private static string GetName(int count, string singular, string plural)
        {
            return count == 1
                ? singular
                : plural;
        }
    }
}
