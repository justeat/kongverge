using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Kongverge.DTOs;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace Kongverge.Tests.DTOs
{
    public static class ValidationFixture
    {
        public static IDictionary<string, AsyncLazy<KongPluginSchema>> AvailablePlugins = new Dictionary<string, AsyncLazy<KongPluginSchema>>
        {
            {
                "example", new AsyncLazy<KongPluginSchema>(() => Task.FromResult(new KongPluginSchema
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
                                                Default = JToken.FromObject(Guid.NewGuid().ToString())
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "field4",
                            new FieldDefinition
                            {
                                Type = "array"
                            }
                        }
                    }
                }))
            }
        };

        public static IReadOnlyList<T> GetExampleCollection<T>(this Fixture fixture, CollectionExample example, Func<T> getValidInstance, Func<T> getInstanceWithOneError)
        {
            IReadOnlyList<T> items = null;
            if (example == CollectionExample.Empty)
            {
                items = Array.Empty<T>();
            }
            else if (example != CollectionExample.Null)
            {
                var item = example == CollectionExample.Valid
                    ? getValidInstance()
                    : getInstanceWithOneError();
                items = new[] { item };
            }

            return items;
        }

        public static string[] GetExampleProtocols(this Fixture fixture, ProtocolsExample example)
        {
            switch (example)
            {
                case ProtocolsExample.Null:
                    return null;
                case ProtocolsExample.Empty:
                    return Array.Empty<string>();
                case ProtocolsExample.Http:
                    return new[] { "http" };
                case ProtocolsExample.Https:
                    return new[] { "https" };
                case ProtocolsExample.Both:
                    return new[] { "http", "https" };
                case ProtocolsExample.Invalid:
                    return new[] { "junk" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static string[] GetExampleStrings(this Fixture fixture, StringsExample example)
        {
            switch (example)
            {
                case StringsExample.Null:
                    return null;
                case StringsExample.Empty:
                    return Array.Empty<string>();
                case StringsExample.ValidHosts:
                    return new[] { "valid1", "*.example.com", "example.*", "10.0.0.1", "2001:0db8:85a3:0000:0000:8a2e:0370:7334" };
                case StringsExample.ValidMethods:
                    return new[] { "OPTIONS", "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE", "CONNECT", "extension-method" };
                case StringsExample.ValidPaths:
                    return new[] { "/valid1", "/(?i)orders/(?i)deliverystate/(?i)driverlocation$", "/(?i)restaurants/\\S+/temporary-offline-status$", "/(?i)invoices/\\d+" };
                case StringsExample.InvalidPaths:
                    return new[] { "notvalid" };
                case StringsExample.InvalidNull:
                    return new string[] { null };
                case StringsExample.InvalidEmpty:
                    return new[] { string.Empty };
                case StringsExample.InvalidWhitespace:
                    return new[] { " " };
                case StringsExample.InvalidColon:
                    return new[] { ":" };
                case StringsExample.InvalidWildcard:
                    return new[] { "*" };
                case StringsExample.LeadingAndTrailingWildcards:
                    return new[] { "*.example.*" };
                case StringsExample.InvalidAll:
                    return new[] { null, string.Empty, " ", ":", "*", "*.example.*" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static KongPlugin GetValidKongPlugin(this Fixture fixture) => new KongPlugin
        {
            Name = "example",
            Config = JObject.FromObject(new
            {
                field1 = 1,
                field2 = fixture.Create<string>(),
                field3 = new
                {
                    field1 = fixture.Create<bool>(),
                    field2 = fixture.Create<string>()
                },
                field4 = fixture.Create<string[]>()
            })
        };

        public static KongPlugin GetKongPluginWithOneError(this Fixture fixture) => new KongPlugin
        {
            Name = "invalid",
            Config = new JObject()
        };

        public static KongRoute GetValidKongRoute(this Fixture fixture) => KongRouteValidationScenarios.BuildValidRouteWithoutPlugins(fixture)
            .With(x => x.Plugins, fixture.GetExampleCollection(CollectionExample.Empty, fixture.GetValidKongPlugin, fixture.GetKongPluginWithOneError))
            .Create();

        public static KongRoute GetKongRouteWithOneError(this Fixture fixture) => KongRouteValidationScenarios.BuildValidRouteWithoutPlugins(fixture)
            .With(x => x.Protocols, fixture.GetExampleProtocols(ProtocolsExample.Null))
            .Create();
    }

    public enum CollectionExample
    {
        Null,
        Empty,
        Valid,
        OneError
    }

    public enum ProtocolsExample
    {
        Null,
        Empty,
        Http,
        Https,
        Both,
        Invalid
    }

    public enum StringsExample
    {
        Null,
        Empty,
        ValidHosts,
        ValidMethods,
        ValidPaths,
        InvalidPaths,
        InvalidNull,
        InvalidEmpty,
        InvalidWhitespace,
        InvalidColon,
        InvalidWildcard,
        LeadingAndTrailingWildcards,
        InvalidAll
    }
}
