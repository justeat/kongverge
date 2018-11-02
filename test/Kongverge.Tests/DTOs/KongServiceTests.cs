using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Moq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(KongService) + nameof(Equals))]
    public class KongServiceEqualityScenarios : EqualityScenarios<KongService>
    {
        protected override void OnlyThePersistenceValuesAreDifferent()
        {
            OtherInstance.Id = this.Create<string>();
            OtherInstance.CreatedAt = this.Create<long>();
        }
    }

    [Story(Title = nameof(KongService) + nameof(IValidatableObject.Validate))]
    public class KongServiceValidationScenarios : ValidatableObjectSteps<KongService>
    {
        protected Mock<KongRoute> RouteMock = new Mock<KongRoute>();
        protected Mock<KongPlugin> PluginMock = new Mock<KongPlugin>();

        protected bool PluginsValid;
        protected bool RoutesValid;
        protected RoutesExample Routes;

        [BddfyFact(DisplayName = nameof(ARandomInstanceWithMockedPluginsAndRoutes))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstanceWithMockedPluginsAndRoutes())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIsCorrect())
                .WithExamples(new ExampleTable(nameof(PluginsValid), nameof(RoutesValid), nameof(ErrorMessagesCount))
                {
                    { true, false, 1 },
                    { false, true, 1 },
                    { false, false, 2 },
                    { true, true, 0 }
                })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstanceWithExampleRoutes))]
        public void Scenario2() =>
            this.Given(x => x.ARandomInstanceWithExampleRoutes())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIsCorrect())
                .WithExamples(new ExampleTable(nameof(Routes), nameof(ErrorMessagesCount))
                {
                    { RoutesExample.Null, 1 },
                    { RoutesExample.Empty, 1 },
                    { RoutesExample.Valid, 0 }
                })
                .BDDfy();

        protected void ARandomInstanceWithMockedPluginsAndRoutes()
        {
            SetupMock(PluginMock, PluginsValid);
            SetupMock(RouteMock, RoutesValid);

            Instance = Build<KongService>()
                .With(x => x.Plugins, new[] { PluginMock.Object })
                .With(x => x.Routes, new[] { RouteMock.Object })
                .Create();
        }

        protected void ARandomInstanceWithExampleRoutes()
        {
            IReadOnlyList<KongRoute> routes = null;
            if (Routes == RoutesExample.Empty)
            {
                routes = Array.Empty<KongRoute>();
            }
            else if (Routes ==  RoutesExample.Valid)
            {
                SetupMock(RouteMock, true);
                routes = new[] { RouteMock.Object };
            }

            Instance = Build<KongService>()
                .With(x => x.Plugins, new KongPlugin[0])
                .With(x => x.Routes, routes)
                .Create();
        }

        public enum RoutesExample
        {
            Null,
            Empty,
            Valid
        }
    }

    [Story(Title = nameof(KongService) + nameof(KongObject.ToJsonStringContent))]
    public class KongServiceSerializationScenarios : KongObjectSerializationScenarios<KongService>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.RoutesIsNotSerialized())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.PluginsIsNotNull())
                .And(x => x.RoutesIsNotNull())
                .BDDfy();

        protected void PluginsIsNotSerialized() => Serialized.Contains("\"plugins\":").Should().BeFalse();

        protected void RoutesIsNotSerialized() => Serialized.Contains("\"routes\":").Should().BeFalse();

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();

        protected void RoutesIsNotNull() => Instance.Routes.Should().NotBeNull();
    }

    [Story(Title = nameof(KongService) + nameof(IKongvergeConfigObject.ToConfigJson))]
    public class KongServiceConfigSerializationScenarios : KongvergeConfigObjectSerializationScenarios<KongService> { }
}
