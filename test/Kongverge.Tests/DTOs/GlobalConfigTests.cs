using Kongverge.DTOs;
using TestStack.BDDfy;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(GlobalConfig) + nameof(IValidatableObject.Validate))]
    public class GlobalConfigValidationScenarios : KongPluginHostValidationScenarios<GlobalConfig> { }

    [Story(Title = nameof(GlobalConfig) + nameof(IKongvergeConfigObject.ToConfigJson))]
    public class GlobalConfigSerializationScenarios : KongvergeConfigObjectSerializationScenarios<GlobalConfig> { }
}
