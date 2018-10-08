using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
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

    [Story(Title = nameof(KongService) + nameof(KongService.Validate))]
    public class KongServiceValidationScenarios : Fixture
    {
        protected const string And = "_";

        protected KongService Instance;
        protected ICollection<string> ErrorMessages = new List<string>();

        [BddfyFact(DisplayName = nameof(ARandomInstance))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.Validating())
                .Then(x => x.ItIsValid())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(RouteProtocolsAreEmpty))]
        public void Scenario2() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.RouteProtocolsAreEmpty())
                .When(x => x.Validating())
                .Then(x => x.ItIsInvalid())
                .BDDfy();

        protected void ARandomInstance() => Instance = this.Create<KongService>();

        protected void RouteProtocolsAreEmpty() => Instance.Routes[0].Protocols = null;

        protected Task Validating() => Instance.Validate(ErrorMessages);

        protected void ItIsValid() => ErrorMessages.Count.Should().Be(0);

        protected void ItIsInvalid() => ErrorMessages.Count.Should().BeGreaterThan(0);
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
