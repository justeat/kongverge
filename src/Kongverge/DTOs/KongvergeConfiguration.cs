using System;
using System.Collections.Generic;

namespace Kongverge.DTOs
{
    public class KongvergeConfiguration
    {
        public IReadOnlyList<KongService> Services { get; set; } = Array.Empty<KongService>();
        public ExtendibleKongObject GlobalConfig { get; set; } = new ExtendibleKongObject();
    }
}
