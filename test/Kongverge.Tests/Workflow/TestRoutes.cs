using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestRoutes
    {
        public static IReadOnlyList<KongRoute> CreateRoutes(this Fixture fixture, int count) =>
            Enumerable.Range(0, count).Select(x => fixture.CreateRoute()).ToArray();

        private static KongRoute CreateRoute(this Fixture fixture) =>
            fixture
                .Build<KongRoute>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Without(x => x.Plugins)
                .Create();

        public static KongRoute AsTarget(this KongRoute kongRoute, bool modified = false)
        {
            var target = kongRoute.Clone();
            if (modified)
            {
                target.Hosts = new [] { Guid.NewGuid().ToString() };
            }
            return target;
        }

        public static KongRoute WithIdAndCreatedAtAndServiceReference(this KongRoute kongRoute, string serviceId)
        {
            kongRoute.WithIdAndCreatedAt();
            kongRoute.Service = new KongRoute.ServiceReference
            {
                Id = serviceId
            };
            return kongRoute;
        }
    }
}
