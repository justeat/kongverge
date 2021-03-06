using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Serilog;

namespace Kongverge.Workflow
{
    public class KongvergeWorkflow : Workflow
    {
        private readonly KongvergeWorkflowArguments _arguments;
        private readonly IKongAdminWriter _kongWriter;
        private readonly IConfigFileReader _configReader;
        private readonly ConfigBuilder _configBuilder;

        private OperationStats _createdStats;
        private OperationStats _updatedStats;
        private OperationStats _deletedStats;

        public KongvergeWorkflow(
            IKongAdminReader kongReader,
            KongvergeWorkflowArguments arguments,
            IKongAdminWriter kongWriter,
            IConfigFileReader configReader,
            ConfigBuilder configBuilder) : base(kongReader)
        {
            _arguments = arguments;
            _kongWriter = kongWriter;
            _configReader = configReader;
            _configBuilder = configBuilder;
        }

        public override async Task<int> DoExecute()
        {
            var schemas = KongConfiguration.GetSchemas(KongReader);
            KongvergeConfiguration targetConfiguration;
            try
            {
                targetConfiguration = await _configReader.ReadConfiguration(_arguments.InputFolder, schemas);
            }
            catch (DirectoryNotFoundException ex)
            {
                return ExitWithCode.Return(ExitCode.InputFolderUnreachable, ex.Message);
            }
            catch (InvalidConfigurationFilesException ex)
            {
                return ExitWithCode.Return(ExitCode.InvalidConfigurationFiles, ex.Message);
            }

            var existingConfiguration = await _configBuilder.FromKong(KongReader, _arguments.IgnoreTags);
            
            try
            {
                await ConvergeConfiguration(existingConfiguration, targetConfiguration);
            }
            catch (KongException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                Log.Error(e, $"Error converging target configuration: {e}");
                var currentConfiguration = await _configBuilder.FromKong(KongReader, _arguments.IgnoreTags);
                Log.Information($"Attempting rollback to previous configuration: {existingConfiguration}");
                await ConvergeConfiguration(currentConfiguration, existingConfiguration);
                return ExitWithCode.Return(ExitCode.UnspecifiedError, "An error occurred while attempting to converge target configuration. Rollback was successful.");
            }
            
            return ExitWithCode.Return(ExitCode.Success);
        }

        private async Task ConvergeConfiguration(KongvergeConfiguration existingConfiguration, KongvergeConfiguration targetConfiguration)
        {
            async Task DeleteConsumer(KongConsumer consumer)
            {
                await _kongWriter.DeleteConsumer(consumer.Id);
                _deletedStats.Increment<KongPlugin>(consumer.Plugins.Count);
            }

            async Task DeleteService(KongService service)
            {
                await _kongWriter.DeleteService(service.Id);
                _deletedStats.Increment<KongRoute>(service.Routes.Count);
                _deletedStats.Increment<KongPlugin>(service.Plugins.Count + service.Routes.Sum(x => x.Plugins.Count));
            }

            _createdStats = new OperationStats();
            _updatedStats = new OperationStats();
            _deletedStats = new OperationStats();

            await ConvergeChildrenPlugins(null, existingConfiguration.GlobalConfig, targetConfiguration.GlobalConfig);

            await ConvergeObjects(
                null,
                KongConsumer.ObjectName,
                existingConfiguration.GlobalConfig.Consumers,
                targetConfiguration.GlobalConfig.Consumers,
                DeleteConsumer,
                x => _kongWriter.PutConsumer(x),
                x => _kongWriter.PutConsumer(x),
                (e, t) => ConvergeChildrenPlugins($"{KongConsumer.ObjectName} {t}", e, t));

            await ConvergeObjects(
                null,
                KongService.ObjectName,
                existingConfiguration.Services,
                targetConfiguration.Services,
                DeleteService,
                x => _kongWriter.PutService(x),
                x => _kongWriter.PutService(x),
                ConvergeServiceChildren);

            Log.Information($"Created {_createdStats}");
            Log.Information($"Updated {_updatedStats}");
            Log.Information($"Deleted {_deletedStats}");
            if (_updatedStats.Any())
            {
                Log.Verbose("See https://github.com/benjamine/jsondiffpatch/blob/master/docs/deltas.md for delta format");
            }
        }

        private async Task ConvergeObjects<T>(
            string parent,
            string objectName,
            IReadOnlyCollection<T> existingObjects,
            IReadOnlyCollection<T> targetObjects,
            Func<T, Task> deleteObject,
            Func<T, Task> createObject,
            Func<T, Task> updateObject = null,
            Func<T, T, Task> recurse = null) where T : KongObject, IKongEquatable<T>
        {
            existingObjects ??= Array.Empty<T>();
            updateObject ??= (x => Task.CompletedTask);
            recurse ??= ((e, t) => Task.CompletedTask);

            if (existingObjects.Count == 0 && targetObjects.Count == 0)
            {
                Log.Verbose($"Target {parent ?? "configuration"} and existing both have zero {KongObject.GetName(0, objectName)}");
                return;
            }

            var targetPhrase = $"{targetObjects.Count} target {KongObject.GetName(targetObjects.Count, objectName)}";
            var existingPhrase = $"{existingObjects.Count} existing {KongObject.GetName(existingObjects.Count, objectName)}";
            var parentPhrase = parent == null ? string.Empty : $" attached to {parent}";
            Log.Verbose($"Converging {targetPhrase}{parentPhrase} with {existingPhrase}");

            var toRemove = existingObjects.Where(x => !targetObjects.Any(x.IsMatch)).ToArray();
            foreach (var existing in toRemove)
            {
                Log.Verbose($"Deleting {objectName} {existing}{parentPhrase} which exists in Kong but not in target configuration");
                await deleteObject(existing);
                _deletedStats.Increment<T>();
            }

            foreach (var target in targetObjects)
            {
                var existing = existingObjects.SingleOrDefault(target.IsMatch);
                if (existing == null)
                {
                    Log.Verbose($"Creating {objectName} {target}{parentPhrase} which exists in target configuration but not in Kong");
                    await createObject(target);
                    _createdStats.Increment<T>();
                }
                else
                {
                    target.MatchWithExisting(existing);
                    if (target.Equals(existing))
                    {
                        Log.Verbose($"Identical {objectName} {existing}{parentPhrase} found in Kong matching target configuration");
                    }
                    else
                    {
                        var patch = target.DifferencesFrom(existing);
                        Log.Verbose($"Updating {objectName} {existing}{parentPhrase} which exists in both Kong and target configuration, having the following differences:{Environment.NewLine}{patch}");
                        await updateObject(target);
                        _updatedStats.Increment<T>();
                    }
                }
                
                await recurse(existing, target);
            }
        }

        private Task ConvergeChildrenPlugins(string parent, IKongPluginHost existing, IKongPluginHost target)
        {
            Task PutPlugin(KongPlugin plugin, IKongPluginHost host)
            {
                host.AssignParentId(plugin);
                return _kongWriter.PutPlugin(plugin);
            }

            return ConvergeObjects(
                parent,
                KongPlugin.ObjectName,
                existing?.Plugins,
                target.Plugins,
                x => _kongWriter.DeletePlugin(x.Id),
                x => PutPlugin(x, target),
                x => PutPlugin(x, target));
        }

        private async Task ConvergeServiceChildren(KongService existing, KongService target)
        {
            async Task PutRoute(KongRoute route, KongService service)
            {
                service.AssignParentId(route);
                await _kongWriter.PutRoute(route);
            }

            async Task DeleteRoute(KongRoute route)
            {
                await _kongWriter.DeleteRoute(route.Id);
                _deletedStats.Increment<KongPlugin>(route.Plugins.Count);
            }

            var parent = $"{KongService.ObjectName} {target}";
            await ConvergeChildrenPlugins(parent, existing, target);
            await ConvergeObjects(
                parent,
                KongRoute.ObjectName,
                existing?.Routes,
                target.Routes,
                DeleteRoute,
                x => PutRoute(x, target),
                x => PutRoute(x, target),
                (e, t) => ConvergeChildrenPlugins($"{KongRoute.ObjectName} {t} attached to {KongService.ObjectName} {target}", e, t));
        }

        private class OperationCount
        {
            public OperationCount(string objectName)
            {
                ObjectName = objectName;
            }

            public string ObjectName { get; }
            public int Count { get; set; }
        }

        private class OperationStats : Dictionary<Type, OperationCount>
        {
            public OperationStats()
            {
                Add(typeof(KongConsumer), new OperationCount(KongConsumer.ObjectName));
                Add(typeof(KongService), new OperationCount(KongService.ObjectName));
                Add(typeof(KongPlugin), new OperationCount(KongPlugin.ObjectName));
                Add(typeof(KongRoute), new OperationCount(KongRoute.ObjectName));
            }

            public void Increment<T>(int count = 1)
            {
                this[typeof(T)].Count += count;
            }

            public override string ToString()
            {
                return string.Join(", ", Keys.Select(x => $"{this[x].Count} {KongObject.GetName(this[x].Count, this[x].ObjectName)}"));
            }

            public bool Any() => Keys.Any(x => this[x].Count > 0);
        }
    }
}
