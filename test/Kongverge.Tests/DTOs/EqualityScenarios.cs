using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    public abstract class EqualityScenarios<T> : Fixture where T : KongObject
    {
        protected const string And = "_";

        protected T Instance;
        protected T OtherInstance;

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherRandomInstance))]
        public void Scenario2() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherRandomInstance())
                .Then(x => x.TheyAreNotEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(OnlyThePersistenceValuesAreDifferent))]
        public void Scenario3() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.OnlyThePersistenceValuesAreDifferent())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(ListValuesAreShuffled))]
        public void Scenario4() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.ListValuesAreShuffled())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(ListPropertiesAreEmptyAndNull))]
        public void Scenario5() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.ListPropertiesAreEmptyAndNull())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        protected virtual void ListValuesAreShuffled() => OtherInstance.Tags = OtherInstance.Tags.Reverse();

        protected virtual void ListPropertiesAreEmptyAndNull()
        {
            Instance.Tags = Array.Empty<string>();
            OtherInstance.Tags = null;
        }

        protected virtual void ARandomInstance() => Instance = this.Create<T>();

        protected void AnotherRandomInstance() => OtherInstance = this.Create<T>();

        protected void AnotherInstanceClonedFromTheFirst() => OtherInstance = Instance.Clone();

        protected virtual void OnlyThePersistenceValuesAreDifferent()
        {
            OtherInstance.Id = this.Create<string>();
            OtherInstance.CreatedAt = this.Create<long>();
            OtherInstance.UpdatedAt = this.Create<long>();
        }

        protected void TheyAreEqual()
        {
            Instance.Should().Be(OtherInstance);
            Instance.GetHashCode().Should().Be(OtherInstance.GetHashCode());
        }

        protected void TheyAreNotEqual()
        {
            Instance.Should().NotBe(OtherInstance);
            Instance.GetHashCode().Should().NotBe(OtherInstance.GetHashCode());
        }
    }
}
