using System.IO;
using System.Linq;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Services;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Services
{
    [Story(Title = nameof(ConfigFileWriter))]
    public class ConfigFileWriterTests : ScenarioFor<ConfigFileWriter>
    {
        private const string TestPath = nameof(TestPath);

        public ConfigFileWriterTests() => Use<IFileProvider>(new VirtualFileProvider());

        [BddfyFact(DisplayName = nameof(ConfigFileWriter.WriteConfiguration) + "NoServicesOrGlobalPlugins")]
        public void Scenario1()
        {
            KongvergeConfiguration configuration = null;

            this.Given(() => configuration = new KongvergeConfiguration(), "a configuration containing no services or global plugins")
                .When(async () => await Subject.WriteConfiguration(configuration, TestPath), "writing configuration")
                .Then(() => Get<IFileProvider>().EnumerateFiles(TestPath).Should().BeEmpty(), "no files are written")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(ConfigFileWriter.WriteConfiguration) + "NoServicesOrGlobalPlugins" + "ExistingFiles")]
        public void Scenario2()
        {
            var globalPath = Path.Combine(TestPath, Constants.GlobalConfigFileName);
            var servicePath = Path.Combine(TestPath, $"service.{Constants.FileExtension}");
            KongvergeConfiguration configuration = null;

            this.Given(() => configuration = new KongvergeConfiguration(), "a configuration containing no services or global plugins")
                .And(async () =>
                {
                    await Get<IFileProvider>().SaveTextContent(globalPath, new GlobalConfig().ToConfigJson());
                    await Get<IFileProvider>().SaveTextContent(servicePath, new KongService().ToConfigJson());
                }, "a directory containing existing files")
                .When(async () => await Subject.WriteConfiguration(configuration, TestPath), "writing configuration")
                .Then(() => Get<IFileProvider>().EnumerateFiles(TestPath).Should().BeEmpty(), "existing files are deleted")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(ConfigFileWriter.WriteConfiguration) + "TwoServicesAndGlobalPlugins")]
        public void Scenario3()
        {
            var globalPath = Path.Combine(TestPath, Constants.GlobalConfigFileName);
            var service1Path = Path.Combine(TestPath, $"service1{Constants.FileExtension}");
            var service2Path = Path.Combine(TestPath, $"service2{Constants.FileExtension}");
            var global = new GlobalConfig { Plugins = new[] { new KongPlugin { Name = "plugin" } } };
            var service1 = new KongService { Name = "service1" };
            var service2 = new KongService { Name = "service2" };
            KongvergeConfiguration configuration = null;

            this.Given(() => configuration = new KongvergeConfiguration
                {
                    Services = new[] { service1, service2 },
                    GlobalConfig = global
                }, "a configuration containing two services and global plugins")
                .When(async () => await Subject.WriteConfiguration(configuration, TestPath), "writing configuration")
                .Then(() => Get<IFileProvider>().EnumerateFiles(TestPath).Count().Should().Be(3), "3 files are written")
                .And(() => Get<IFileProvider>().LoadTextContent(globalPath).Result.Should().Be(global.ToConfigJson()), "the global file is written correctly")
                .And(() => Get<IFileProvider>().LoadTextContent(service1Path).Result.Should().Be(service1.ToConfigJson()), "the first service file is written correctly")
                .And(() => Get<IFileProvider>().LoadTextContent(service2Path).Result.Should().Be(service2.ToConfigJson()), "the second service file is written correctly")
                .BDDfy();
        }
    }
}
