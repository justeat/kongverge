using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    public abstract class SerializationScenarios<T> : Fixture where T : KongObject
    {
        protected const string And = "_";

        protected T Instance;
        protected string Serialized;

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToConfigJson))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToConfigJson())
                .Then(x => x.IdIsNotSerialized())
                .And(x => x.CreatedAtIsNotSerialized())
                .And(x => x.ConsumerIdIsNotSerialized())
                .And(x => x.ServiceIdIsNotSerialized())
                .And(x => x.RouteIdIsNotSerialized())
                .And(x => x.ServiceIsNotSerialized())
                .BDDfy();

        protected void ARandomInstance() => Instance = this.Create<T>();

        protected async Task SerializingToStringContent() => Serialized = await MakeStringContent().ReadAsStringAsync();

        protected abstract string SerializingToConfigJson();

        protected abstract StringContent MakeStringContent();

        protected void IdIsNotSerialized() => Serialized.Contains("\"id\":").Should().BeFalse();

        protected void CreatedAtIsNotSerialized() => Serialized.Contains("\"created_at\":").Should().BeFalse();

        protected void ConsumerIdIsNotSerialized() => Serialized.Contains("\"consumer_id\":").Should().BeFalse();

        protected void ServiceIdIsNotSerialized() => Serialized.Contains("\"service_id\":").Should().BeFalse();

        protected void RouteIdIsNotSerialized() => Serialized.Contains("\"route_id\":").Should().BeFalse();

        protected void ServiceIsNotSerialized() => Serialized.Contains("\"service\":").Should().BeFalse();
    }
}
