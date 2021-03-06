using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Kongverge.Workflow;
using Moq;
using Nito.AsyncEx;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Workflow
{
    [Story(Title = nameof(KongvergeWorkflow) + nameof(KongvergeWorkflow.DoExecute))]
    public class KongvergeWorkflowScenarios : WorkflowSteps<KongvergeWorkflow>
    {
        protected const string And = "_";

        protected KongvergeConfiguration Target = new KongvergeConfiguration();

        protected KongvergeWorkflowArguments Arguments;
        protected IReadOnlyList<GlobalConfig> GlobalConfigs;
        protected IReadOnlyList<KongConsumer> Consumers;
        protected IReadOnlyList<KongService> Services;
        protected IReadOnlyList<KongRoute> Routes;
        
        public KongvergeWorkflowScenarios()
        {
            Arguments = Fixture.Create<KongvergeWorkflowArguments>();
            Use(Arguments);
            GlobalConfigs = Fixture.CreateGlobalConfigs(2);
            Consumers = Fixture.CreateConsumers(4);
            Services = Fixture.CreateServices(4);
            Routes = Fixture.CreateRoutes(6);
            Plugins = Fixture.CreatePlugins(13);
            
            GetMock<IKongAdminWriter>()
                .Setup(x => x.PutRoute(It.IsAny<KongRoute>()))
                .Returns<KongRoute>(x =>
                {
                    if (x.Service == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return Task.CompletedTask;
                });

            GetMock<IKongAdminWriter>()
                .Setup(x => x.PutPlugin(It.IsAny<KongPlugin>()))
                .Returns(Task.CompletedTask);

            GetMock<IKongAdminWriter>()
                .Setup(x => x.PutService(It.IsAny<KongService>()))
                .Returns(Task.CompletedTask);

            GetMock<IKongAdminWriter>()
                .Setup(x => x.DeletePlugin(It.IsAny<string>()))
                .Returns<string>(x =>
                {
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
                .Then(s => s.TheExitCodeIs(ExitCode.InvalidConfigurationFiles))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(NoExistingServicesOrGlobalConfig) + And + nameof(NoTargetServicesOrGlobalConfig))]
        public void Scenario4() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.NoExistingServicesOrGlobalConfig())
                .And(s => s.NoTargetServicesOrGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.NoConsumersAreAddedOrUpdated())
                .And(s => s.NoConsumersAreDeleted())
                .And(s => s.NoServicesAreAddedOrUpdated())
                .And(s => s.NoServicesAreDeleted())
                .And(s => s.NoRoutesAreAddedOrUpdated())
                .And(s => s.NoRoutesAreDeleted())
                .And(s => s.NoPluginsAreAddedOrUpdated())
                .And(s => s.NoPluginsAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(NoExistingServicesOrGlobalConfig) + And + nameof(AnAssortmentOfTargetServicesAndGlobalConfig))]
        public void Scenario5() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.NoExistingServicesOrGlobalConfig())
                .And(s => s.AnAssortmentOfTargetServicesAndGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.TheTargetConsumersAreAdded())
                .And(s => s.NoConsumersAreDeleted())
                .And(s => s.TheTargetServicesAreAdded())
                .And(s => s.NoServicesAreDeleted())
                .And(s => s.TheTargetRoutesAreAdded())
                .And(s => s.NoRoutesAreDeleted())
                .And(s => s.TheTargetPluginsAreAdded())
                .And(s => s.NoPluginsAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(AnAssortmentOfExistingServicesAndGlobalConfig) + And + nameof(NoTargetServicesOrGlobalConfig))]
        public void Scenario6() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.AnAssortmentOfExistingServicesAndGlobalConfig())
                .And(s => s.NoTargetServicesOrGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.NoConsumersAreAddedOrUpdated())
                .And(s => s.TheExistingConsumersAreDeleted())
                .And(s => s.NoServicesAreAddedOrUpdated())
                .And(s => s.TheExistingServicesAreDeleted())
                .And(s => s.NoRoutesAreAddedOrUpdated())
                .And(s => s.NoRoutesAreDeleted())
                .And(s => s.NoPluginsAreAddedOrUpdated())
                .And(s => s.TheExistingGlobalPluginsAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable) + And + nameof(AnAssortmentOfExistingServicesAndGlobalConfig) + And + nameof(AnAssortmentOfTargetServicesAndGlobalConfig))]
        public void Scenario7() =>
            this.Given(s => s.KongIsReachable())
                .And(s => s.AnAssortmentOfExistingServicesAndGlobalConfig())
                .And(s => s.AnAssortmentOfTargetServicesAndGlobalConfig())
                .When(s => s.Executing())
                .Then(s => s.TheNewConsumersAreAdded())
                .And(s => s.TheChangedConsumersAreUpdated())
                .And(s => s.TheUnchangedConsumersAreNotUpdated())
                .And(s => s.TheRemovedConsumersAreDeleted())
                .And(s => s.TheNewServicesAreAdded())
                .And(s => s.TheChangedServicesAreUpdated())
                .And(s => s.TheUnchangedServicesAreNotUpdated())
                .And(s => s.TheRemovedServicesAreDeleted())
                .And(s => s.TheChangedOrNewRoutesAreAdded())
                .And(s => s.TheUnchangedRoutesAreNotAddedOrDeleted())
                .And(s => s.TheChangedOrRemovedRoutesAreDeleted())
                .And(s => s.TheNewPluginsAreAdded())
                .And(s => s.TheChangedPluginsAreUpdated())
                .And(s => s.TheUnchangedPluginsAreNotUpdated())
                .And(s => s.NoneOfThePluginsOfChangedOrDeletedRoutesAreUpdated())
                .And(s => s.TheRemovedPluginsAreDeleted())
                .And(s => s.NoneOfThePluginsOfDeletedServicesAreDeleted())
                .And(s => s.NoneOfThePluginsOfChangedOrDeletedRoutesAreDeleted())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        protected void NoExistingServicesOrGlobalConfig()
        {
            Existing.Services = Array.Empty<KongService>();
            Existing.GlobalConfig = new GlobalConfig();
        }

        protected void NoTargetServicesOrGlobalConfig()
        {
            Target.Services = Array.Empty<KongService>();
            Target.GlobalConfig = new GlobalConfig();
            
            SetupTargetConfiguration();
        }

        protected void NonExistentInputFolder() =>
            GetMock<IConfigFileReader>().Setup(x => x.ReadConfiguration(Arguments.InputFolder, It.IsAny<IDictionary<string, AsyncLazy<KongSchema>>>())).ThrowsAsync(new DirectoryNotFoundException());

        protected void InvalidTargetConfiguration() =>
            GetMock<IConfigFileReader>().Setup(x => x.ReadConfiguration(Arguments.InputFolder, It.IsAny<IDictionary<string, AsyncLazy<KongSchema>>>())).ThrowsAsync(new InvalidConfigurationFilesException(string.Empty));

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
            Existing.GlobalConfig.Consumers = new[]
            {
                Consumers[0].AsExisting(),
                Consumers[1].AsExisting(),
                Consumers[2].AsExisting()
            };
            Existing.GlobalConfig.Consumers[0].Plugins = new[]
            {
                Plugins[9].AsExisting(),
                Plugins[10].AsExisting(),
                Plugins[11].AsExisting()
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
            if (Existing.Services.Any())
            {
                Target.Services[0].Id = Existing.Services[0].Id;
                Target.Services[0].Name = Guid.NewGuid().ToString();
            }

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
            Target.GlobalConfig.Consumers = new[]
            {
                Consumers[0].AsTarget(true), // Changed
                Consumers[1].AsTarget(), // Same
                // Consumers[2] Removed
                Consumers[3].AsTarget() // Added
            };
            if (Existing.GlobalConfig.Consumers.Any())
            {
                Target.GlobalConfig.Consumers[0].Id = Existing.GlobalConfig.Consumers[0].Id;
                Target.GlobalConfig.Consumers[0].Username = Guid.NewGuid().ToString();
            }
            Target.GlobalConfig.Consumers[0].Plugins = new[]
            {
                Plugins[9].AsTarget(true), // Changed
                Plugins[10].AsTarget(), // Same
                // Plugins[11] Removed
                Plugins[12].AsTarget() // Added
            };

            SetupTargetConfiguration();
        }

        protected void SetupTargetConfiguration() => GetMock<IConfigFileReader>().Setup(x => x.ReadConfiguration(Arguments.InputFolder, It.IsAny<IDictionary<string, AsyncLazy<KongSchema>>>())).ReturnsAsync(Target);

        protected void NoConsumersAreAddedOrUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutConsumer(It.IsAny<KongConsumer>()), Times.Never);

        protected void NoConsumersAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteConsumer(It.IsAny<string>()), Times.Never);

        protected void NoServicesAreAddedOrUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutService(It.IsAny<KongService>()), Times.Never);

        protected void NoServicesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteService(It.IsAny<string>()), Times.Never);

        protected void NoRoutesAreAddedOrUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(It.IsAny<KongRoute>()), Times.Never);

        protected void NoRoutesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(It.IsAny<string>()), Times.Never);

        protected void NoPluginsAreAddedOrUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.IsAny<KongPlugin>()), Times.Never);

        protected void NoPluginsAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(It.IsAny<string>()), Times.Never);

        protected void TheTargetConsumersAreAdded()
        {
            foreach (var targetConsumer in Target.GlobalConfig.Consumers)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.PutConsumer(targetConsumer), Times.Once);
            }
        }

        protected void TheExistingConsumersAreDeleted()
        {
            foreach (var existingConsumer in Target.GlobalConfig.Consumers)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.DeleteConsumer(existingConsumer.Id), Times.Once);
            }
        }


        protected void TheTargetServicesAreAdded()
        {
            foreach (var targetService in Target.Services)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.PutService(targetService), Times.Once);
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
                    GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(targetRoute), Times.Once);
                }
            }
        }

        protected void TheTargetPluginsAreAdded()
        {
            foreach (var targetService in Target.Services)
            {
                foreach (var targetPlugin in targetService.Plugins)
                {
                    GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                        p.IsTheSameAs(targetPlugin) &&
                        p.CorrespondsToKongService(targetService))), Times.Once);
                }
                foreach (var targetRoute in targetService.Routes)
                {
                    foreach (var targetPlugin in targetRoute.Plugins)
                    {
                        GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                            p.IsTheSameAs(targetPlugin) &&
                            p.CorrespondsToKongRoute(targetRoute))), Times.Once);
                    }
                }
            }
            foreach (var targetPlugin in Target.GlobalConfig.Plugins)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                    p.IsTheSameAs(targetPlugin) &&
                    p.IsGlobal())), Times.Once);
            }
            foreach (var targetConsumer in Target.GlobalConfig.Consumers)
            {
                foreach (var targetPlugin in targetConsumer.Plugins)
                {
                    GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                        p.IsTheSameAs(targetPlugin) &&
                        p.CorrespondsToKongConsumer(targetConsumer))), Times.Once);
                }
            }
        }

        protected void TheExistingGlobalPluginsAreDeleted()
        {
            foreach (var existingPlugin in Existing.GlobalConfig.Plugins)
            {
                GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(existingPlugin.Id), Times.Once);
            }
        }

        protected void TheNewConsumersAreAdded() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutConsumer(Target.GlobalConfig.Consumers[2]), Times.Once);

        protected void TheChangedConsumersAreUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutConsumer(Target.GlobalConfig.Consumers[0]), Times.Once);

        protected void TheUnchangedConsumersAreNotUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutConsumer(Target.GlobalConfig.Consumers[1]), Times.Never);

        protected void TheRemovedConsumersAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteConsumer(Existing.GlobalConfig.Consumers[2].Id), Times.Once);

        protected void TheNewServicesAreAdded() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutService(Target.Services[2]), Times.Once);

        protected void TheChangedServicesAreUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutService(Target.Services[0]), Times.Once);

        protected void TheUnchangedServicesAreNotUpdated() =>
            GetMock<IKongAdminWriter>().Verify(x => x.PutService(Target.Services[1]), Times.Never);

        protected void TheRemovedServicesAreDeleted() =>
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteService(Existing.Services[2].Id), Times.Once);

        protected void TheChangedOrNewRoutesAreAdded()
        {
            Target.Services[0].Routes[0].Service?.Id.Should().Be(Target.Services[0].Id);
            Target.Services[0].Routes[2].Service?.Id.Should().Be(Target.Services[0].Id);
            Target.Services[1].Routes[0].Service?.Id.Should().Be(Target.Services[1].Id);
            Target.Services[1].Routes[0].Service?.Id.Should().Be(Target.Services[1].Id);

            GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(Target.Services[0].Routes[0]), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(Target.Services[0].Routes[2]), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(Target.Services[1].Routes[0]), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(Target.Services[2].Routes[0]), Times.Once);
        }

        protected void TheUnchangedRoutesAreNotAddedOrDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.PutRoute(Target.Services[0].Routes[1]), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(Existing.Services[0].Routes[1].Id), Times.Never);
        }

        protected void TheChangedOrRemovedRoutesAreDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(Existing.Services[0].Routes[0].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeleteRoute(Existing.Services[1].Routes[0].Id), Times.Once);
        }

        protected void TheNewPluginsAreAdded()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Plugins[2]) &&
                p.CorrespondsToKongService(Target.Services[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Routes[1].Plugins[2]) &&
                p.CorrespondsToKongRoute(Target.Services[0].Routes[1]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[2].Routes[0].Plugins[0]) &&
                p.CorrespondsToKongRoute(Target.Services[2].Routes[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[2].Routes[0].Plugins[1]) &&
                p.CorrespondsToKongRoute(Target.Services[2].Routes[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Plugins[2]) &&
                p.IsGlobal())), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Consumers[0].Plugins[2]) &&
                p.CorrespondsToKongConsumer(Target.GlobalConfig.Consumers[0]))), Times.Once);
        }

        protected void TheChangedPluginsAreUpdated()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Plugins[0]) &&
                p.CorrespondsToKongService(Target.Services[0]) &&
                p.CorrespondsToExistingPlugin(Existing.Services[0].Plugins[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Routes[1].Plugins[0]) &&
                p.CorrespondsToKongRoute(Target.Services[0].Routes[1]) &&
                p.CorrespondsToExistingPlugin(Existing.Services[0].Routes[1].Plugins[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Plugins[0]) &&
                p.IsGlobal() &&
                p.CorrespondsToExistingPlugin(Existing.GlobalConfig.Plugins[0]))), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Consumers[0].Plugins[0]) &&
                p.CorrespondsToKongConsumer(Target.GlobalConfig.Consumers[0]) &&
                p.CorrespondsToExistingPlugin(Existing.GlobalConfig.Consumers[0].Plugins[0]))), Times.Once);
        }

        protected void TheUnchangedPluginsAreNotUpdated()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Routes[1].Plugins[1]) &&
                p.CorrespondsToKongRoute(Target.Services[0].Routes[1]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.Services[0].Plugins[1]) &&
                p.CorrespondsToKongService(Target.Services[0]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Plugins[1]) &&
                p.IsGlobal())), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Target.GlobalConfig.Consumers[0].Plugins[1]) &&
                p.IsGlobal())), Times.Never);
        }

        protected void NoneOfThePluginsOfChangedOrDeletedRoutesAreUpdated()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Existing.Services[0].Routes[0].Plugins[0]) &&
                p.CorrespondsToKongRoute(Existing.Services[0].Routes[0]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Existing.Services[0].Routes[0].Plugins[1]) &&
                p.CorrespondsToKongRoute(Existing.Services[0].Routes[0]))), Times.Never);
            GetMock<IKongAdminWriter>().Verify(x => x.PutPlugin(It.Is<KongPlugin>(p =>
                p.IsTheSameAs(Existing.Services[0].Routes[0].Plugins[2]) &&
                p.CorrespondsToKongRoute(Existing.Services[0].Routes[0]))), Times.Never);
        }

        protected void TheRemovedPluginsAreDeleted()
        {
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Plugins[2].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.Services[0].Routes[1].Plugins[2].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.GlobalConfig.Plugins[2].Id), Times.Once);
            GetMock<IKongAdminWriter>().Verify(x => x.DeletePlugin(Existing.GlobalConfig.Consumers[0].Plugins[2].Id), Times.Once);
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
