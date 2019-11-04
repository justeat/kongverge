using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    public abstract class SerializationScenarios<T> : Fixture
    {
        protected const string And = "_";

        protected T Instance;
        protected string Serialized;

        protected void ARandomInstance() => Instance = this.Create<T>();
    }

    public abstract class KongObjectSerializationScenarios<T> : SerializationScenarios<T> where T : KongObject
    {
        protected async Task SerializingToStringContent() => Serialized = await Instance.ToJsonStringContent().ReadAsStringAsync();
    }

    public abstract class KongvergeConfigObjectSerializationScenarios<T> : SerializationScenarios<T> where T : IKongvergeConfigObject
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToConfigJson))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToConfigJson())
                .Then(x => x.IdIsNotSerialized())
                .And(x => x.CreatedAtIsNotSerialized())
                .And(x => x.UpdatedAtIsNotSerialized())
                .And(x => x.ConsumerReferenceIsNotSerialized())
                .And(x => x.ServiceReferenceIsNotSerialized())
                .And(x => x.RouteReferenceIsNotSerialized())
                .And(x => x.ClientCertificateReferenceIsNotSerialized())
                .BDDfy();

        protected string SerializingToConfigJson() => Serialized = Instance.ToConfigJson();

        protected void IdIsNotSerialized() => Serialized.Should().NotContain("\"id\":");

        protected void CreatedAtIsNotSerialized() => Serialized.Should().NotContain("\"created_at\":");

        protected void UpdatedAtIsNotSerialized() => Serialized.Should().NotContain("\"updated_at\":");

        protected void ConsumerReferenceIsNotSerialized() => Serialized.Should().NotContain("\"consumer\":");

        protected void ServiceReferenceIsNotSerialized() => Serialized.Should().NotContain("\"service\":");

        protected void RouteReferenceIsNotSerialized() => Serialized.Should().NotContain("\"route\":");

        protected void ClientCertificateReferenceIsNotSerialized() => Serialized.Should().NotContain("\"client_certificate\":");
    }
}
