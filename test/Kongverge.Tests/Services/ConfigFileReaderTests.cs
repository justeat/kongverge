using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Services;
using Nito.AsyncEx;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Services
{
    [Story(Title = nameof(ConfigFileReader))]
    public class ConfigFileReaderTests : ScenarioFor<ConfigFileReader>
    {
        private const string ExceptionThrownWithErrorMessages = "an exception is thrown showing error messages attributed to their respective file paths";
        private const string TestPath = nameof(TestPath);

        public ConfigFileReaderTests() => Use<IFileProvider>(new VirtualFileProvider());

        [BddfyFact(DisplayName = nameof(ConfigFileReader.ReadConfiguration) + "MoreThanOneGlobalFile")]
        public void Scenario1()
        {
            var path1 = Path.Combine(TestPath, Fixture.Create<string>(), Constants.GlobalConfigFileName);
            var path2 = Path.Combine(TestPath, Fixture.Create<string>(), Constants.GlobalConfigFileName);
            Func<Task> awaiting = null;

            this.Given(() =>
                {
                    Get<IFileProvider>().SaveTextContent(path1, new GlobalConfig().ToConfigJson());
                    Get<IFileProvider>().SaveTextContent(path2, new GlobalConfig().ToConfigJson());
                }, "a directory containing more than one global file")
                .When(() => awaiting = Subject.Awaiting(x => x.ReadConfiguration(TestPath, null)), "reading configuration")
                .Then(() => awaiting.Should().Throw<InvalidConfigurationFilesException>().Which.Message.Should()
                        .Contain($"{path1} => Cannot have more than one {Constants.GlobalConfigFileName} file.").And
                        .Contain($"{path2} => Cannot have more than one {Constants.GlobalConfigFileName} file."),
                    ExceptionThrownWithErrorMessages)
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(ConfigFileReader.ReadConfiguration) + "FilesAreInvalid")]
        public void Scenario2()
        {
            var globalPath = Path.Combine(TestPath, Constants.GlobalConfigFileName);
            var servicePath = Path.Combine(TestPath, $"service{Constants.FileExtension}");
            var availablePlugins = new Dictionary<string, AsyncLazy<KongPluginSchema>>();
            Func<Task> awaiting = null;

            this.Given(() =>
                {
                    Get<IFileProvider>().SaveTextContent(globalPath, new GlobalConfig { Plugins = new[] { new KongPlugin { Name = "test" } } }.ToConfigJson());
                    Get<IFileProvider>().SaveTextContent(servicePath, new KongService { Protocol = "ftp" }.ToConfigJson());
                }, "a directory containing an invalid global file and an invalid service file")
                .When(() => awaiting = Subject.Awaiting(x => x.ReadConfiguration(TestPath, availablePlugins)), "reading configuration")
                .Then(() => awaiting.Should().Throw<InvalidConfigurationFilesException>().Which.Message.Should()
                        .Contain($"{globalPath} => Plugin 'test' is not available on Kong server.").And
                        .Contain($"{servicePath} => Service Protocol is invalid (must be either 'http' or 'https')."),
                    ExceptionThrownWithErrorMessages)
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(ConfigFileReader.ReadConfiguration) + "FileIsNotJson")]
        public void Scenario3()
        {
            var path = Path.Combine(TestPath, Constants.GlobalConfigFileName);
            Func<Task> awaiting = null;

            this.Given(() => Get<IFileProvider>().SaveTextContent(path, "not json"),
                    "a directory containing an invalid global file and an invalid service file")
                .When(() => awaiting = Subject.Awaiting(x => x.ReadConfiguration(TestPath, null)), "reading configuration")
                .Then(() => awaiting.Should().Throw<InvalidConfigurationFilesException>().Which.Message.Should()
                        .Contain($"{path} => Unexpected character encountered while parsing value"),
                    ExceptionThrownWithErrorMessages)
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(ConfigFileReader.ReadConfiguration) + "NoFiles")]
        public void Scenario4()
        {
            KongvergeConfiguration configuration = null;

            this.Given(() => { },"a directory containing no files")
                .When(async () => configuration = await Subject.ReadConfiguration(TestPath, null), "reading configuration")
                .Then(() => configuration.Should().NotBeNull(), "the configuration is returned")
                .And(() => configuration.Services.Should().BeEmpty(), "the services are empty")
                .And(() => configuration.GlobalConfig.Plugins.Should().BeEmpty(), "the global plugins are empty")
                .BDDfy();
        }
    }
}
