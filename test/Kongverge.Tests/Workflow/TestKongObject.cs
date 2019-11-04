using System;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestKongObject
    {
        public static T AsExisting<T>(this T kongObject) where T : KongObject
        {
            var existing = kongObject.Clone();
            kongObject.Id = Guid.NewGuid().ToString();
            kongObject.CreatedAt = new Random(Environment.TickCount).Next(int.MaxValue);
            kongObject.UpdatedAt = new Random(Environment.TickCount).Next(int.MaxValue);
            return existing;
        }
    }
}
