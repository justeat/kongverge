using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestServices
    {
        public static IReadOnlyList<KongService> CreateServices(this Fixture fixture, int count) =>
            Enumerable.Range(0, count).Select(x => fixture.CreateService()).ToArray();

        private static KongService CreateService(this Fixture fixture) =>
            fixture
                .Build<KongService>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Without(x => x.Routes)
                .Without(x => x.Plugins)
                .Create();

        public static KongService AsTarget(this KongService kongService, bool modified = false)
        {
            var target = kongService.Clone();
            if (modified)
            {
                target.Host = Guid.NewGuid().ToString();
            }
            return target;
        }
    }
}
