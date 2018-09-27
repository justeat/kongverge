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
        protected bool AreEqual;
        protected bool SameHashCodes;

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .When(x => x.CheckingEquality())
                .And(x => x.CheckingHashCodes())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherRandomInstance))]
        public void Scenario2() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherRandomInstance())
                .When(x => x.CheckingEquality())
                .And(x => x.CheckingHashCodes())
                .Then(x => x.TheyAreNotEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(OnlyThePersistenceValuesAreDifferent))]
        public void Scenario3() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.OnlyThePersistenceValuesAreDifferent())
                .When(x => x.CheckingEquality())
                .And(x => x.CheckingHashCodes())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        protected void ARandomInstance() => Instance = this.Create<T>();

        protected void AnotherRandomInstance() => OtherInstance = this.Create<T>();

        protected void AnotherInstanceClonedFromTheFirst() => OtherInstance = Instance.Clone();

        protected abstract void OnlyThePersistenceValuesAreDifferent();

        protected void CheckingEquality() => AreEqual = Instance.Equals(OtherInstance);

        protected void CheckingHashCodes() => SameHashCodes = Instance.GetHashCode().Equals(OtherInstance.GetHashCode());

        protected void TheyAreEqual()
        {
            AreEqual.Should().BeTrue();
            SameHashCodes.Should().BeTrue();
        }

        protected void TheyAreNotEqual()
        {
            AreEqual.Should().BeFalse();
            SameHashCodes.Should().BeFalse();
        }
    }
}
