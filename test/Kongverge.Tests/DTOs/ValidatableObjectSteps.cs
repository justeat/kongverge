using System;
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

        private Mock<TMock> SetupMock<TMock>(bool isValid) where TMock : class, IValidatableObject
        {
            var mock = new Mock<TMock>();
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
            return mock;
        }

        protected IReadOnlyList<TItem> GetExampleCollection<TItem>(CollectionExample example) where TItem : class, IValidatableObject
        {
            IReadOnlyList<TItem> items = null;
            if (example == CollectionExample.Empty)
            {
                items = Array.Empty<TItem>();
            }
            else if (example != CollectionExample.Null)
            {
                var mock = SetupMock<TItem>(example == CollectionExample.Valid);
                items = new[] { mock.Object };
            }

            return items;
        }

        protected Task Validating()
        {
            _errorMessages = new List<string>();
            return Instance.Validate(AvailablePlugins, _errorMessages);
        }

        protected void TheErrorMessagesCountIsCorrect() => _errorMessages.Count.Should().Be(ErrorMessagesCount);
    }
}
