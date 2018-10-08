using System;
using System.Collections.Generic;

namespace Kongverge.DTOs
{
    public class KongvergeConfiguration
    {
        public IReadOnlyList<KongService> Services { get; set; } = Array.Empty<KongService>();
        public GlobalConfig GlobalConfig { get; set; } = new GlobalConfig();
    }

    public interface IKongvergeConfigObject : IValidatableObject
    {
        string ToConfigJson();
    }
}
