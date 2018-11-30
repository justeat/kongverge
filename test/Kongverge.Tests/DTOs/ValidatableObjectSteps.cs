using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Moq;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace Kongverge.Tests.DTOs
{
    public abstract class ValidatableObjectSteps<T> : Fixture where T : IValidatableObject
    {
        private ICollection<string> _errorMessages;

        protected T Instance;
        protected int ErrorMessagesCount;
        protected IDictionary<string, AsyncLazy<KongPluginSchema>> AvailablePlugins = new Dictionary<string, AsyncLazy<KongPluginSchema>>();

        protected KongPlugin ExamplePlugin;
        protected KongPlugin ExamplePluginWithMissingDefaultConfigFields;
        protected KongPlugin ExamplePluginWithOneInvalidConfigField;
        protected KongPlugin ExamplePluginWithTwoUnknownConfigFields;

        protected ValidatableObjectSteps()
        {
            var examplePluginSchema = new KongPluginSchema
            {
                Fields = new Dictionary<string, FieldDefinition>
                {
                    {
                        "field1",
                        new FieldDefinition
                        {
                            Type = "number",
                            Default = JToken.FromObject(0)
                        }
                    },
                    {
                        "field2",
                        new FieldDefinition
                        {
                            Type = "string"
                        }
                    },
                    {
                        "field3",
                        new FieldDefinition
                        {
                            Type = "table",
                            Schema = new KongPluginSchema
                            {
                                Fields = new Dictionary<string, FieldDefinition>
                                {
                                    {
                                        "field1",
                                        new FieldDefinition
                                        {
                                            Type = "boolean"
                                        }
                                    },
                                    {
                                        "field2",
                                        new FieldDefinition
                                        {
                                            Type = "string",
                                            Default = JToken.FromObject(this.Create<string>())
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            ExamplePlugin = new KongPlugin
            {
                Name = "plugin1",
                Config = JObject.FromObject(new
                {
                    field1 = 1,
                    field2 = this.Create<string>(),
                    field3 = new
                    {
                        field1 = this.Create<bool>(),
                        field2 = this.Create<string>()
                    }
                })
            };

            ExamplePluginWithMissingDefaultConfigFields = ExamplePlugin.Clone();
            ExamplePluginWithMissingDefaultConfigFields.Config.Remove("field1");
            ((JObject)ExamplePluginWithMissingDefaultConfigFields.Config.SelectToken("field3")).Remove("field2");

            ExamplePluginWithOneInvalidConfigField = ExamplePlugin.Clone();
            ExamplePluginWithOneInvalidConfigField.Config.Remove("field3");
            ExamplePluginWithOneInvalidConfigField.Config.Add("field3", JToken.FromObject(this.Create<string>()));

            ExamplePluginWithTwoUnknownConfigFields = ExamplePlugin.Clone();
            ExamplePluginWithTwoUnknownConfigFields.Config.Add(this.Create<string>(), JToken.FromObject(1));
            ((JObject)ExamplePluginWithTwoUnknownConfigFields.Config.SelectToken("field3")).Add(this.Create<string>(), JToken.FromObject(this.Create<bool>()));

            AvailablePlugins.Add(ExamplePlugin.Name, new AsyncLazy<KongPluginSchema>(() => Task.FromResult(examplePluginSchema)));
        }

        private Mock<TMock> SetupMock<TMock>(bool isValid) where TMock : class, IValidatableObject
        {
            var mock = new Mock<TMock>();
            mock
                .Setup(x => x.Validate(AvailablePlugins, It.IsAny<ICollection<string>>()))
                .Callback<IDictionary<string, AsyncLazy<KongPluginSchema>>, ICollection<string>>((ep, em) =>
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

        protected void TheErrorMessagesCountIs(int count) => _errorMessages.Count.Should().Be(count);
    }
}
