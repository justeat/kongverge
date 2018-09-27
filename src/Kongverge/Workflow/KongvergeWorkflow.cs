using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.Options;

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
            
            await ConvergeObjects(
                existingConfiguration.Services,
                targetConfiguration.Services,
                x => _kongWriter.DeleteService(x.Id),
                x => _kongWriter.AddService(x),
                x => _kongWriter.UpdateService(x),
                ConvergeServiceChildren);

            await ConvergeChildrenPlugins(existingConfiguration.GlobalConfig, targetConfiguration.GlobalConfig);

            return ExitWithCode.Return(ExitCode.Success);
        }

        private static async Task ConvergeObjects<T>(
            IReadOnlyCollection<T> existingObjects,
            IReadOnlyCollection<T> targetObjects,
            Func<T, Task> deleteObject,
            Func<T, Task> createObject,
            Func<T, Task> updateObject = null,
            Func<T, T, Task> recurse = null) where T : KongObject
        {
            existingObjects = existingObjects ?? Array.Empty<T>();
            updateObject = updateObject ?? (x => Task.CompletedTask);
            recurse = recurse ?? ((e, t) => Task.CompletedTask);

            var targetMatchValues = targetObjects.Select(x => x.GetMatchValue()).ToArray();
            var toRemove = existingObjects.Where(x => !targetMatchValues.Contains(x.GetMatchValue()));

            foreach (var existing in toRemove)
            {
                await deleteObject(existing);
            }

            foreach (var target in targetObjects)
            {
                var existing = target.MatchWithExisting(existingObjects);
                if (existing == null)
                {
                    await createObject(target);
                }
                else if (!target.Equals(existing))
                {
                    await updateObject(target);
                }
                await recurse(existing, target);
            }
        }

        private Task ConvergeChildrenPlugins(ExtendibleKongObject existing, ExtendibleKongObject target)
        {
            Task UpsertPlugin(KongPlugin plugin, ExtendibleKongObject parent)
            {
                parent.AssignParentId(plugin);
                return _kongWriter.UpsertPlugin(plugin);
            }

            return ConvergeObjects(
                existing?.Plugins,
                target.Plugins,
                x => _kongWriter.DeletePlugin(x.Id),
                x => UpsertPlugin(x, target),
                x => UpsertPlugin(x, target));
        }

        private async Task ConvergeServiceChildren(KongService existing, KongService target)
        {
            await ConvergeChildrenPlugins(existing, target);
            await ConvergeObjects(
                existing?.Routes,
                target.Routes,
                x => _kongWriter.DeleteRoute(x.Id),
                x => _kongWriter.AddRoute(target.Id, x),
                null,
                ConvergeChildrenPlugins);
        }
    }
}
