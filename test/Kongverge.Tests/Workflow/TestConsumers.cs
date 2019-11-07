using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestConsumers
    {
        public static IReadOnlyList<KongConsumer> CreateConsumers(this Fixture fixture, int count) =>
            Enumerable.Range(0, count).Select(x => fixture.CreateConsumer()).ToArray();

        private static KongConsumer CreateConsumer(this Fixture fixture) =>
            fixture
                .Build<KongConsumer>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Without(x => x.UpdatedAt)
                .Without(x => x.Plugins)
                .Create();

        public static KongConsumer AsTarget(this KongConsumer kongConsumer, bool modified = false)
        {
            var target = kongConsumer.Clone();
            if (modified)
            {
                target.CustomId = Guid.NewGuid().ToString();
            }
            return target;
        }
    }
}
