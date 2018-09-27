using System;
using Kongverge.DTOs;

namespace Kongverge.Tests.Workflow
{
    public static class TestKongObject
    {
        public static T AsExisting<T>(this T kongObject) where T : KongObject =>
            kongObject.Clone().WithIdAndCreatedAt();

        public static T WithIdAndCreatedAt<T>(this T kongObject) where T : KongObject
        {
            if (kongObject.Id == null)
            {
                kongObject.Id = Guid.NewGuid().ToString();
            }
            if (!kongObject.CreatedAt.HasValue)
            {
                kongObject.CreatedAt = new Random(Environment.TickCount).Next(int.MaxValue);
            }
            return kongObject;
        }
    }
}