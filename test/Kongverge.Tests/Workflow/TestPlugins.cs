using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestPlugins
    {
        public static IReadOnlyList<KongPlugin> CreatePlugins(this Fixture fixture, int count) =>
            Enumerable.Range(0, count).Select(x => fixture.CreatePlugin()).ToArray();

        private static KongPlugin CreatePlugin(this Fixture fixture) =>
            fixture
                .Build<KongPlugin>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();

        public static bool IsTheSameAs(this KongPlugin kongPlugin, KongPlugin expectedKongPlugin) =>
            ReferenceEquals(kongPlugin, expectedKongPlugin);

        public static bool CorrespondsToKongService(this KongPlugin kongPlugin, KongService kongService) =>
            kongPlugin.Service?.Id == kongService.Id &&
            !string.IsNullOrWhiteSpace(kongPlugin.Service?.Id) &&
            kongPlugin.Consumer?.Id == null &&
            kongPlugin.Route?.Id == null;

        public static bool CorrespondsToKongRoute(this KongPlugin kongPlugin, KongRoute kongRoute) =>
            kongPlugin.Route?.Id == kongRoute.Id &&
            !string.IsNullOrWhiteSpace(kongPlugin.Route?.Id) &&
            kongPlugin.Consumer?.Id == null &&
            kongPlugin.Service?.Id == null;

        public static bool CorrespondsToExistingPlugin(this KongPlugin kongPlugin, KongPlugin existingKongPlugin) =>
            kongPlugin.Id == existingKongPlugin.Id &&
            !string.IsNullOrWhiteSpace(kongPlugin.Id) &&
            kongPlugin.CreatedAt == existingKongPlugin.CreatedAt &&
            kongPlugin.CreatedAt.HasValue;

        public static KongPlugin AsTarget(this KongPlugin kongPlugin, bool modified = false)
        {
            var target = kongPlugin.Clone();
            if (modified)
            {
                if (target.Config.HasValues)
                {
                    target.Config[0] = Guid.NewGuid().ToString();
                }
                else
                {
                    target.Config.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                }
            }
            return target;
        }
    }
}
