using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Kongverge.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace Kongverge.Tests.DTOs
{
    public static class ValidationFixture
    {
        public static IDictionary<string, AsyncLazy<KongSchema>> Schemas = new Dictionary<string, AsyncLazy<KongSchema>>
        {
            {
                "consumers", new AsyncLazy<KongSchema>(() => Task.FromResult(JsonConvert.DeserializeObject<KongSchema>(File.ReadAllText("consumers.json"))))
            },
            {
                "routes", new AsyncLazy<KongSchema>(() => Task.FromResult(JsonConvert.DeserializeObject<KongSchema>(File.ReadAllText("routes.json"))))
            },
            {
                "services", new AsyncLazy<KongSchema>(() => Task.FromResult(JsonConvert.DeserializeObject<KongSchema>(File.ReadAllText("services.json"))))
            },
            {
                "certificates", new AsyncLazy<KongSchema>(() => Task.FromResult(JsonConvert.DeserializeObject<KongSchema>(File.ReadAllText("certificates.json"))))
            },
            {
                "plugins", new AsyncLazy<KongSchema>(() => Task.FromResult(JsonConvert.DeserializeObject<KongSchema>(File.ReadAllText("plugins.json"))))
            },
            {
                "plugins/example", new AsyncLazy<KongSchema>(() => Task.FromResult(new KongSchema
                {
                    Fields = new[]
                    {
                        new Field("run_on", new KongSchema
                        {
                            Type = FieldType.String,
                            Required = true,
                            OneOf = new[] { JToken.FromObject("first"), JToken.FromObject("second") },
                            Default = "first"
                        }),
                        new Field("protocols", new KongSchema
                        {
                            Type = FieldType.Set,
                            Required = true,
                            Default = JToken.FromObject(new[] { "grpc", "grpcs", "http", "https" }),
                            Elements = new KongSchema
                            {
                                Type = "string",
                                OneOf = new[] { JToken.FromObject("grpc"), JToken.FromObject("grpcs"), JToken.FromObject("http"), JToken.FromObject("https") }
                            }
                        }),
                        new Field("consumer", new KongSchema
                        {
                            Type = FieldType.Foreign,
                            Eq = JValue.CreateNull(),
                            Reference = "consumers"
                        }),
                        new Field("config", new KongSchema
                        {
                            Type = FieldType.Record,
                            Required = true,
                            Fields = new[]
                            {
                                new Field("field1", new KongSchema
                                {
                                    Type = FieldType.Number,
                                    Default = JToken.FromObject(0),
                                    Gt = 4
                                }),
                                new Field("field2", new KongSchema
                                {
                                    Type = FieldType.String,
                                    OneOf = new[] { JToken.FromObject("foo"), JToken.FromObject("bar"), JToken.FromObject("baz") }
                                }),
                                new Field("field3", new KongSchema
                                {
                                    Type = FieldType.Record,
                                    Fields = new[]
                                    {
                                        new Field("field1", new KongSchema
                                        {
                                            Type = FieldType.Boolean
                                        }),
                                        new Field("field2", new KongSchema
                                        {
                                            Type = FieldType.String,
                                            Uuid = true,
                                            Default = JToken.FromObject(Guid.NewGuid().ToString())
                                        })
                                    }
                                }),
                                new Field("field4", new KongSchema
                                {
                                    Type = FieldType.Array,
                                    Elements = new KongSchema
                                    {
                                        Type = FieldType.String,
                                        OneOf = new[] { JToken.FromObject("foo"), JToken.FromObject("bar"), JToken.FromObject("baz") }
                                    },
                                    LenMin = 2,
                                    Required = true
                                }),
                                new Field("field5", new KongSchema
                                {
                                    Type = FieldType.Integer,
                                    Between = new double[] { 1, 10 },
                                    Required = true
                                }),
                                new Field("field6", new KongSchema
                                {
                                    Type = FieldType.String,
                                    LenMin = 4
                                }),
                                new Field("field7", new KongSchema
                                {
                                    Type = FieldType.String,
                                    Match = "^test%d$"
                                }),
                                new Field("field8", new KongSchema
                                {
                                    Type = FieldType.Map,
                                    LenMin = 1,
                                    Keys = new KongSchema
                                    {
                                        Type = FieldType.String
                                    },
                                    Values = new KongSchema
                                    {
                                        Type = FieldType.Record,
                                        Fields = new[]
                                        {
                                            new Field("field1", new KongSchema
                                            {
                                                Type = FieldType.String,
                                                Uuid = true
                                            }),
                                            new Field("field2", new KongSchema
                                            {
                                                Type = FieldType.String,
                                                LenMin = 10
                                            })
                                        },
                                        EntityChecks = new[]
                                        {
                                            new EntityCheck
                                            {
                                                AtLeastOneOf = new [] { "field1", "field2" }
                                            }
                                        }
                                    },
                                    Required = true
                                }),
                                new Field("field9", new KongSchema
                                {
                                    Type = FieldType.Number,
                                    Gt = 5
                                }),
                                new Field("field10", new KongSchema
                                {
                                    Type = FieldType.Map,
                                    LenMin = 1,
                                    Keys = new KongSchema
                                    {
                                        Type = FieldType.String
                                    },
                                    Values = new KongSchema
                                    {
                                        Type = FieldType.Number
                                    }
                                }),
                                new Field("field11", new KongSchema
                                {
                                    Type = FieldType.Number,
                                    Required = true
                                })
                            }
                        })
                    }
                }))
            }
        };

        public static IReadOnlyList<T> CreateChildren<T>(this Fixture fixture, Children example, Func<T> getValidInstance, Func<T> getInstanceWithOneError)
        {
            IReadOnlyList<T> items = null;
            if (example == Children.Empty)
            {
                items = Array.Empty<T>();
            }
            else if (example != Children.Null)
            {
                var item = example == Children.Valid
                    ? getValidInstance()
                    : getInstanceWithOneError();
                items = new[] { item };
            }

            return items;
        }

        public static string[] CreateProtocols(this Fixture fixture, Protocols example)
        {
            switch (example)
            {
                case Protocols.Null:
                    return null;
                case Protocols.Empty:
                    return Array.Empty<string>();
                case Protocols.Http:
                    return new[] { "http" };
                case Protocols.Https:
                    return new[] { "https" };
                case Protocols.Tcp:
                    return new[] { "tcp" };
                case Protocols.Tls:
                    return new[] { "tls" };
                case Protocols.Grpc:
                    return new[] { "grpc" };
                case Protocols.Grpcs:
                    return new[] { "grpcs" };
                case Protocols.BothHttp:
                    return new[] { "http", "https" };
                case Protocols.BothTcp:
                    return new[] { "tcp", "tls" };
                case Protocols.BothGrpc:
                    return new[] { "grpc", "grpcs" };
                case Protocols.InvalidItems:
                    return new[] { "foo" };
                case Protocols.InvalidSet:
                    return new[] { "http", "tcp" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static string[] CreateMethods(this Fixture fixture, Set example)
        {
            switch (example)
            {
                case Set.Null:
                    return null;
                case Set.Empty:
                    return Array.Empty<string>();
                case Set.ValidItems:
                    return new [] { "GET", "POST", "PUT", "PATCH", "DELETE", "XX" };
                case Set.InvalidItems:
                    return new[] { "get", "p", "123", "*", " ", "", null };
                case Set.InvalidSet:
                    return new[] { "GET", "GET" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static string[] CreatePaths(this Fixture fixture, List example)
        {
            switch (example)
            {
                case List.Null:
                    return null;
                case List.Empty:
                    return Array.Empty<string>();
                case List.ValidItems:
                    return new[] { "/foo", "/(?i)foo/\\S+/bar/\\d+", "/(foo|bar|baz)/pop$" };
                case List.InvalidItems:
                    return new[] { "foo", "/foo//", " ", "", null };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static string[] CreateHosts(this Fixture fixture, List example)
        {
            switch (example)
            {
                case List.Null:
                    return null;
                case List.Empty:
                    return Array.Empty<string>();
                case List.ValidItems:
                    return new[] { "*.foo", "foo.*", "foo", "foo.bar.baz", "pop", "10.0.0.1", "2001:0db8:85a3:0000:0000:8a2e:0370:7334" };
                case List.InvalidItems:
                    return new[] { "*", "*foo*", "*.foo.*", " ", "", null };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static IDictionary<string, string[]> CreateHeaders(this Fixture fixture, List example)
        {
            switch (example)
            {
                case List.Null:
                    return null;
                case List.Empty:
                    return new Dictionary<string, string[]>();
                case List.ValidItems:
                    return new Dictionary<string, string[]>
                    {
                        { "foo", new[] { "1", "2" } },
                        { "bar", new[] { "3", "4" } }
                    };
                case List.InvalidItems:
                    return new Dictionary<string, string[]>
                    {
                        { "foo", new[] { "" } },
                        { "bar", new string[0] },
                        { "host", new[] { "1", "2" } },
                        { "", new[] { "1", "2" } }
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static KongRoute.Endpoint[] CreateEndpoints(this Fixture fixture, Set example)
        {
            switch (example)
            {
                case Set.Null:
                    return null;
                case Set.Empty:
                    return Array.Empty<KongRoute.Endpoint>();
                case Set.ValidItems:
                    return new[]
                    {
                        new KongRoute.Endpoint
                        {
                            Ip = fixture.Create<string>(),
                            Port = fixture.Create<int>() % 65535
                        }
                    };
                case Set.InvalidItems:
                    return new[]
                    {
                        new KongRoute.Endpoint(),
                        new KongRoute.Endpoint
                        {
                            Port = 65536
                        }
                    };
                case Set.InvalidSet:
                    var endpoint = new KongRoute.Endpoint
                    {
                        Ip = fixture.Create<string>(),
                        Port = fixture.Create<int>() % 65535
                    };
                    return new[] { endpoint, endpoint };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static string[] CreateSnis(this Fixture fixture, Set example)
        {
            switch (example)
            {
                case Set.Null:
                    return null;
                case Set.Empty:
                    return Array.Empty<string>();
                case Set.ValidItems:
                    return new[] { "foo.test", "example.com", "", " ", fixture.Create<string>() };
                case Set.InvalidItems:
                    return new string[] { null };
                case Set.InvalidSet:
                    return new[] { "foo", "foo" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static string[] CreateTags(this Fixture fixture, Set example)
        {
            switch (example)
            {
                case Set.Null:
                    return null;
                case Set.Empty:
                    return Array.Empty<string>();
                case Set.ValidItems:
                    return new[] { "foo-bar", "foo-bar-baz", "spock-spaz", "pop", "rock.roll", "rattle~shake", "under_cover" };
                case Set.InvalidItems:
                    return new[] { "foo bar", " ", "", null };
                case Set.InvalidSet:
                    return new[] { "foo", "foo" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public static KongPlugin GetValidKongPlugin(this Fixture fixture) => new KongPlugin
        {
            Name = "example",
            Config = JObject.FromObject(new
            {
                field1 = 5,
                field2 = new[] { "foo", "bar", "baz" }[fixture.Create<int>() % 3],
                field3 = new
                {
                    field1 = fixture.Create<bool>(),
                    field2 = Guid.NewGuid().ToString()
                },
                field4 = new[] { "foo", "bar", "baz" },
                field5 = fixture.Create<int>() % 10 + 1,
                field6 = fixture.Create<string>(),
                field7 = "test" + fixture.Create<int>() % 10,
                field8 = new
                {
                    anything = new
                    {
                        field1 = Guid.NewGuid().ToString(),
                        field2 = fixture.Create<string>()
                    }
                },
                field9 = fixture.Create<int>() + 5,
                field10 = new
                {
                    anything = fixture.Create<int>()
                },
                field11 = fixture.Create<int>()
            })
        };

        public static KongPlugin GetKongPluginWithOneError(this Fixture fixture) => new KongPlugin
        {
            Name = "invalid",
            Config = new JObject()
        };

        public static KongRoute GetValidKongRoute(this Fixture fixture) => KongRouteValidationScenarios.BuildRouteWithoutPlugins(fixture)
            .With(x => x.Plugins, fixture.CreateChildren(Children.Empty, fixture.GetValidKongPlugin, fixture.GetKongPluginWithOneError))
            .Create();

        public static KongRoute GetKongRouteWithOneError(this Fixture fixture) => KongRouteValidationScenarios.BuildRouteWithoutPlugins(fixture)
            .With(x => x.Protocols, fixture.CreateProtocols(Protocols.Null))
            .Create();
    }

    public enum Children
    {
        Null,
        Empty,
        Valid,
        OneError
    }

    public enum Protocols
    {
        Null,
        Empty,
        Http,
        Https,
        Tcp,
        Tls,
        Grpc,
        Grpcs,
        BothHttp,
        BothTcp,
        BothGrpc,
        InvalidItems,
        InvalidSet
    }

    public enum Set
    {
        Null,
        Empty,
        ValidItems,
        InvalidItems,
        InvalidSet
    }

    public enum List
    {
        Null,
        Empty,
        ValidItems,
        InvalidItems
    }
}
