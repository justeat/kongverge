using AutoFixture;
using Kongverge.DTOs;
using TestStack.BDDfy;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(GlobalConfig) + nameof(IValidatableObject.Validate))]
    public class GlobalConfigValidationScenarios : KongPluginHostValidationScenarios<GlobalConfig>
    {
        protected override void AValidInstanceWithExamplePlugins() => Instance = Build<GlobalConfig>()
            .With(x => x.Plugins, this.GetExampleCollection(Plugins, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .Create();
    }

    [Story(Title = nameof(GlobalConfig) + nameof(IKongvergeConfigObject.ToConfigJson))]
    public class GlobalConfigSerializationScenarios : KongvergeConfigObjectSerializationScenarios<GlobalConfig> { }
}
