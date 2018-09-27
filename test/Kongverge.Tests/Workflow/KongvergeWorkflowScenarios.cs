using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Kongverge.Workflow;
using Moq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Workflow
{
    public class KongvergeWorkflowScenarios : WorkflowSteps<KongvergeWorkflow>
    {
        protected const string And = "_";

        protected KongvergeConfiguration Target = new KongvergeConfiguration();

        protected IReadOnlyList<ExtendibleKongObject> GlobalConfigs;
        protected IReadOnlyList<KongService> Services;
        protected IReadOnlyList<KongRoute> Routes;
        protected IReadOnlyList<KongPlugin> Plugins;

        protected List<KongPlugin> InsertedPlugins = new List<KongPlugin>();

        public KongvergeWorkflowScenarios()
        {
            GlobalConfigs = Fixture.CreateGlobalConfigs(2);
            Services = Fixture.CreateServices(4);
            Routes = Fixture.CreateRoutes(6);
            Plugins = Fixture.CreatePlugins(10);
            
            GetMock<IKongAdminWriter>()
                .Setup(x => x.AddRoute(It.IsAny<string>(), It.IsAny<KongRoute>()))
                .Returns<string, KongRoute>((serviceId, route) =>
                {
                    if (serviceId == null)
                    {
                        throw new InvalidOperationException();
                    }
                    route.WithIdAndCreatedAtAndServiceReference(serviceId);
                    return Task.CompletedTask;
                });

            GetMock<IKongAdminWriter>()
                .Setup(x => x.UpsertPlugin(It.IsAny<KongPlugin>()))
                .Returns<KongPlugin>(x =>
                {
                    if (x.Id == null)
                    {
                        InsertedPlugins.Add(x);
                    }
                    x.WithIdAndCreatedAt();
                    return Task.CompletedTask;
                });

            GetMock<IKongAdminWriter>()
                .Setup(x => x.AddService(It.IsAny<KongService>()))
                .Returns<KongService>(x =>
                {
                    x.WithIdAndCreatedAt();
                    return Task.CompletedTask;
                });
        }

        [BddfyFact(DisplayName = nameof(KongIsNotReachable))]
        public void Scenario1() =>
            this.Given(s => s.KongIsNotReachable())
                .When(s => s.Executing())
                .Then(s => s.TheExitCodeIs(ExitCode.HostUnreachable))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(NonExistentInputFolder))]
        public void Scenario2() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.NonExistentInputFolder())
                .When(s => s.Executing())
                .Then(s => s.TheExitCodeIs(ExitCode.InputFolderUnreachable))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(InvalidTargetConfiguration))]
        public void Scenario3() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.InvalidTargetConfiguration())
                .When(s => s.Executing())
                .Then(s => s.TheExitCodeIs(ExitCode.InvalidConfigurationFile))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(NoExistingServicesOrGlobalConfig) + And + nameof(NoTargetServicesOrGlobalConfig))]
        public void Scenario4() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.NoExistingServicesOrGlobalConfig())
                .And(s => s.NoTargetServicesOrGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.NoServicesAreAdded())
                .And(s => s.NoServicesAreUpdated())
                .And(s => s.NoServicesAreDeleted())
                .And(s => s.NoRoutesAreAdded())
                .And(s => s.NoRoutesAreDeleted())
                .And(s => s.NoPluginsAreUpserted())
                .And(s => s.NoPluginsAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(NoExistingServicesOrGlobalConfig) + And + nameof(AnAssortmentOfTargetServicesAndGlobalConfig))]
        public void Scenario5() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.NoExistingServicesOrGlobalConfig())
                .And(s => s.AnAssortmentOfTargetServicesAndGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.TheTargetServicesAreAdded())
                .And(s => s.NoServicesAreUpdated())
                .And(s => s.NoServicesAreDeleted())
                .And(s => s.TheTargetRoutesAreAdded())
                .And(s => s.NoRoutesAreDeleted())
                .And(s => s.TheTargetPluginsAreUpserted())
                .And(s => s.NoPluginsAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(AnAssortmentOfExistingServicesAndGlobalConfig) + And + nameof(NoTargetServicesOrGlobalConfig))]
        public void Scenario6() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.AnAssortmentOfExistingServicesAndGlobalConfig())
                .And(s => s.NoTargetServicesOrGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.NoServicesAreAdded())
                .And(s => s.NoServicesAreUpdated())
                .And(s => s.TheExistingServicesAreDeleted())
                .And(s => s.NoRoutesAreAdded())
                .And(s => s.NoRoutesAreDeleted())
                .And(s => s.NoPluginsAreUpserted())
                .And(s => s.TheExistingPluginsAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(AnAssortmentOfExistingServicesAndGlobalConfig) + And + nameof(AnAssortmentOfTargetServicesAndGlobalConfig))]
        public void Scenario7() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.AnAssortmentOfExistingServicesAndGlobalConfig())
                .And(s => s.AnAssortmentOfTargetServicesAndGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.TheNewServicesAreAdded())
                .And(s => s.TheChangedServicesAreUpdated())
                .And(s => s.TheUnchangedServicesAreNotUpdated())
                .And(s => s.TheRemovedServicesAreDeleted())
                .And(s => s.TheChangedOrNewRoutesAreAdded())
                .And(s => s.TheUnchangedRoutesAreNotAddedOrDeleted())
                .And(s => s.TheChangedOrRemovedRoutesAreDeleted())
                .And(s => s.TheChangedOrNewPluginsAreUpserted())
                .And(s => s.TheUnchangedPluginsAreNotUpserted())
                .And(s => s.NoneOfThePluginsOfChangedOrDeletedRoutesAreUpserted())
                .And(s => s.TheRemovedPluginsAreDeleted())
                .And(s => s.NoneOfThePluginsOfDeletedServicesAreDeleted())
                .And(s => s.NoneOfThePluginsOfChangedOrDeletedRoutesAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        protected void NoExistingServicesOrGlobalConfig()
        {
            Existing.Services = Array.Empty<KongService>();
            Existing.GlobalConfig = new ExtendibleKongObject();
        }

        protected void NoTargetServicesOrGlobalConfig()
        {
            Target.Services = Array.Empty<KongService>();
            Target.GlobalConfig = new ExtendibleKongObject();
            
            SetupTargetConfiguration();
        }

        protected void NonExistentInputFolder() =>
            GetMock<ConfigFileReader>().Setup(x => x.ReadConfiguration(Settings.InputFolder)).ThrowsAsync(new DirectoryNotFoundException());

        protected void InvalidTargetConfiguration() =>
            GetMock<ConfigFileReader>().Setup(x => x.ReadConfiguration(Settings.InputFolder)).ThrowsAsync(new InvalidConfigurationFileException(string.Empty, string.Empty));

        protected void AnAssortmentOfExistingServicesAndGlobalConfig()
        {
            Existing.Services = new[]
            {
                Services[0].AsExisting(),
                Services[1].AsExisting(),
                Services[2].AsExisting()
            };

            Existing.Services[0].Plugins = new[]
            {
                Plugins[3].AsExisting(),
                Plugins[4].AsExisting(),
                Plugins[5].AsExisting()
            };

            Existing.Services[0].Routes = new[]
            {
                Routes[0].AsExisting(),
                Routes[1].AsExisting(),
                Routes[2].AsExisting()
            };
            Existing.Services[0].Routes[0].Plugins = new[]
            {
                Plugins[0].AsExisting(),
                Plugins[1].AsExisting(),
                Plugins[2].AsExisting()
            };
            Existing.Services[0].Routes[1].Plugins = new[]
            {
                Plugins[3].AsExisting(),
                Plugins[4].AsExisting(),
                Plugins[5].AsExisting()
            };

            Existing.Services[1].Routes = new[]
            {
                Routes[3].AsExisting()
            };

            Existing.Services[2].Plugins = new[]
            {
                Plugins[0].AsExisting()
            };
            Existing.Services[2].Routes = new[]
            {
                Routes[4].AsExisting(),
                Routes[5].AsExisting()
            };

            Existing.GlobalConfig = GlobalConfigs[0];
            Existing.GlobalConfig.Plugins = new[]
            {
                Plugins[6].AsExisting(),
                Plugins[7].AsExisting(),
                Plugins[8].AsExisting()
            };
        }

        protected void AnAssortmentOfTargetServicesAndGlobalConfig()
        {
            Target.Services = new[]
            {
                Services[0].AsTarget(true), // Changed
                Services[1].AsTarget(), // Same
                // Services[2] Removed
                Services[3].AsTarget() // Added
            };

            Target.Services[0].Plugins = new[]
            {
                Plugins[3].AsTarget(true), // Changed
                Plugins[4].AsTarget(), // Same
                // Plugins[5] Removed
                Plugins[6].AsTarget() // Added
            };

            Target.Services[0].Routes = new[]
            {
                Routes[0].AsTarget(true), // Changed
                Routes[1].AsTarget(), // Same
                // Routes[2] Removed
                Routes[3].AsTarget() // Added
            };
            Target.Services[0].Routes[0].Plugins = new[]
            {
                Plugins[0].AsTarget(true), // Changed
                Plugins[1].AsTarget(), // Same
                // Plugins[2] Removed
                Plugins[3].AsTarget() // Added
            };
            Target.Services[0].Routes[1].Plugins = new[]
            {
                Plugins[3].AsTarget(true), // Changed
                Plugins[4].AsTarget(), // Same
                // Plugins[5] Removed
                Plugins[6].AsTarget() // Added
            };

            Target.Services[1].Routes = new[]
            {
                // Routes[3] Removed
                Routes[4].AsTarget() // Added
            };

            Target.Services[2].Routes = new[]
            {
                Routes[5].AsTarget() // Added
            };
            Target.Services[2].Routes[0].Plugins = new[]
            {
                Plugins[0].AsTarget(), // Added
                Plugins[1].AsTarget() // Added
            };

            Target.GlobalConfig = GlobalConfigs[1];
            Target.GlobalConfig.Plugins = new[]
            {
                Plugins[6].AsTarget(true), // Changed
                Plugins[7].AsTarget(), // Same
                // Plugins[8] Removed
                Plugins[9].AsTarget() // Added
            };

            SetupTargetConfiguration();
        }

        protected void SetupTargetConfiguration() => GetMock<ConfigFileReader>().Setup(x => x.ReadConfiguration(Settings.InputFolder)).ReturnsAsync(Target);

        protected void NoServicesAreAdded() =>
            GetMock<IKongAdminWriter>().Verify(x => x.AddService(It.IsAny<KongService>()), Times.Never);

        protected void NoServicesAreUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.UpdateService(It.IsAny<KongService>()), Times.Never);

        protected void NoServicesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteService(It.IsAny<string>()), Times.Never);

        protected void NoRoutesAreAdded() =>
            GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(It.IsAny<string>(), It.IsAny<KongRoute>()), Times.Never);

        protected void NoRoutesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(It.IsAny<string>()), Times.Never);

        protected void NoPluginsAreUpserted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.IsAny<KongPlugin>()), Times.Never);

        protected void NoPluginsAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(It.IsAny<string>()), Times.Never);

        protected void TheTargetServicesAreAdded()
        {
            foreach (var targetService in Target.Services)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.AddService(targetService), Times.Once);
            }
        }

        protected void TheExistingServicesAreDeleted()
        {
            foreach (var existingService in Existing.Services)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.DeleteService(existingService.Id), Times.Once);
            }
        }

        protected void TheTargetRoutesAreAdded()
        {
            foreach (var targetService in Target.Services)
            {
                foreach (var targetRoute in targetService.Routes)
                {
                    GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(targetService.Id, targetRoute), Times.Once);
                }
            }
        }

        protected void TheTargetPluginsAreUpserted()
        {
            foreach (var targetService in Target.Services)
            {
                foreach (var targetPlugin in targetService.Plugins)
                {
                    GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                        p.IsTheSameAs(targetPlugin) &&
                        p.CorrespondsToKongService(targetService))), Times.Once);
                }
                foreach (var targetRoute in targetService.Routes)
                {
                    foreach (var targetPlugin in targetRoute.Plugins)
                    {
                        GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                            p.IsTheSameAs(targetPlugin) &&
                            p.CorrespondsToKongRoute(targetRoute))), Times.Once);
                    }
                }
            }
            foreach (var targetPlugin in Target.GlobalConfig.Plugins)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                    p.IsTheSameAs(targetPlugin) &&
                    p.IsGlobal())), Times.Once);
            }
        }

        protected void TheExistingPluginsAreDeleted()
        {
            foreach (var existingPlugin in Existing.GlobalConfig.Plugins)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(existingPlugin.Id), Times.Once);
            }
        }

        protected void TheNewServicesAreAdded() =>
            GetMock<IKongAdminWriter>().Verify(x => x.AddService(Target.Services[2]), Times.Once);

        protected void TheChangedServicesAreUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.UpdateService(Target.Services[0]), Times.Once);

        protected void TheUnchangedServicesAreNotUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.UpdateService(Target.Services[1]), Times.Never);

        protected void TheRemovedServicesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteService(Existing.Services[2].Id), Times.Once);

        protected void TheChangedOrNewRoutesAreAdded()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(Target.Services[0].Id, Target.Services[0].Routes[0]), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(Target.Services[0].Id, Target.Services[0].Routes[2]), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(Existing.Services[1].Id, Target.Services[1].Routes[0]), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(Target.Services[2].Id, Target.Services[2].Routes[0]), Times.Once);
        }

        protected void TheUnchangedRoutesAreNotAddedOrDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.AddRoute(Target.Services[0].Id, Target.Services[0].Routes[1]), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(Existing.Services[0].Routes[1].Id), Times.Never);
        }

        protected void TheChangedOrRemovedRoutesAreDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(Existing.Services[0].Routes[0].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(Existing.Services[1].Routes[0].Id), Times.Once);
        }

        protected void TheChangedOrNewPluginsAreUpserted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Plugins[0]) &&
                p.CorrespondsToKongService(Target.Services[0]) &&
                p.CorrespondsToExistingPlugin(Existing.Services[0].Plugins[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Plugins[2]) &&
                p.CorrespondsToKongService(Target.Services[0]) &&
                InsertedPlugins.Contains(p))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Routes[1].Plugins[0]) &&
                p.CorrespondsToKongRoute(Target.Services[0].Routes[1]) &&
                p.CorrespondsToExistingPlugin(Existing.Services[0].Routes[1].Plugins[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Routes[1].Plugins[2]) &&
                p.CorrespondsToKongRoute(Target.Services[0].Routes[1]) &&
                InsertedPlugins.Contains(p))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[2].Routes[0].Plugins[0]) &&
                p.CorrespondsToKongRoute(Target.Services[2].Routes[0]) &&
                InsertedPlugins.Contains(p))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[2].Routes[0].Plugins[1]) &&
                p.CorrespondsToKongRoute(Target.Services[2].Routes[0]) &&
                InsertedPlugins.Contains(p))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Plugins[0]) &&
                p.IsGlobal() &&
                p.CorrespondsToExistingPlugin(Existing.GlobalConfig.Plugins[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Plugins[2]) &&
                p.IsGlobal() &&
                InsertedPlugins.Contains(p))), Times.Once);
        }

        protected void TheUnchangedPluginsAreNotUpserted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Routes[1].Plugins[1]) &&
                p.CorrespondsToKongRoute(Target.Services[0].Routes[1]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Plugins[1]) &&
                p.CorrespondsToKongService(Target.Services[0]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Plugins[1]) &&
                p.IsGlobal())), Times.Never);
        }

        protected void NoneOfThePluginsOfChangedOrDeletedRoutesAreUpserted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Existing.Services[0].Routes[0].Plugins[0]) &&
                p.CorrespondsToKongRoute(Existing.Services[0].Routes[0]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Existing.Services[0].Routes[0].Plugins[1]) &&
                p.CorrespondsToKongRoute(Existing.Services[0].Routes[0]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.UpsertPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Existing.Services[0].Routes[0].Plugins[2]) &&
                p.CorrespondsToKongRoute(Existing.Services[0].Routes[0]))), Times.Never);
        }

        protected void TheRemovedPluginsAreDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Plugins[2].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Routes[1].Plugins[2].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.GlobalConfig.Plugins[2].Id), Times.Once);
        }

        protected void NoneOfThePluginsOfDeletedServicesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[2].Plugins[0].Id), Times.Never);

        protected void NoneOfThePluginsOfChangedOrDeletedRoutesAreDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Routes[0].Plugins[0].Id), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Routes[0].Plugins[1].Id), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Routes[0].Plugins[2].Id), Times.Never);
        }
    }
}
