using AutoFixture;
using Moq.AutoMock;

namespace Kongverge.Tests
{
    public abstract class ScenarioFor<T> : AutoMocker where T : class
    {
        private T _subject;
        public T Subject => _subject ?? (_subject = CreateInstance<T>());

        protected Fixture Fixture = new Fixture();
    }
}
