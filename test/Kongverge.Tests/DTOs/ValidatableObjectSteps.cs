using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Moq;

namespace Kongverge.Tests.DTOs
{
    public abstract class ValidatableObjectSteps<T> : Fixture where T : IValidatableObject
    {
        private ICollection<string> _errorMessages;

        protected T Instance;
        protected int ErrorMessagesCount;
        protected IReadOnlyList<string> AvailablePlugins = new [] { "plugin1", "plugin2", "plugin3" };

        protected void SetupMock<TMock>(Mock<TMock> mock, bool isValid) where TMock : class, IValidatableObject
        {
            mock
                .Setup(x => x.Validate(AvailablePlugins, It.IsAny<ICollection<string>>()))
                .Callback<IReadOnlyCollection<string>, ICollection<string>>((ep, em) =>
                {
                    if (!isValid)
                    {
                        em.Add(this.Create<string>());
                    }
                })
                .Returns(Task.CompletedTask);
        }

        protected Task Validating()
        {
            _errorMessages = new List<string>();
            return Instance.Validate(AvailablePlugins, _errorMessages);
        }

        protected void TheErrorMessagesCountIsCorrect() => _errorMessages.Count.Should().Be(ErrorMessagesCount);
    }
}
