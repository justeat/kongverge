using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestGlobalConfigs
    {
        public static IReadOnlyList<ExtendibleKongObject> CreateGlobalConfigs(this Fixture fixture, int count) =>
            Enumerable.Range(0, count).Select(x => fixture.CreateGlobalConfig()).ToArray();

        private static ExtendibleKongObject CreateGlobalConfig(this Fixture fixture) =>
            fixture
                .Build<ExtendibleKongObject>()
                .Without(x => x.Plugins)
                .Create();
    }
}
