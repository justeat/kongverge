using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;

namespace Kongverge.Tests.DTOs
{
    public abstract class ValidatableObjectSteps<T> : Fixture where T : IValidatableObject
    {
        private ICollection<string> _errorMessages;

        protected T Instance;
        protected int ErrorMessagesCount;
        
        protected Task Validating()
        {
            _errorMessages = new List<string>();
            return Instance.Validate(ValidationFixture.AvailablePlugins, _errorMessages);
        }

        protected void TheErrorMessagesCountIs(int count) => _errorMessages.Count.Should().Be(count);
    }
}
