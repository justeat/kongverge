using AutoFixture;
using AutoFixture.Dsl;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(KongConsumer) + nameof(Equals))]
    public class KongConsumerEqualityScenarios : EqualityScenarios<KongConsumer> { }

    [Story(Title = nameof(KongConsumer) + nameof(IValidatableObject.Validate))]
    public class KongConsumerValidationScenarios : ValidatableObjectSteps<KongConsumer>
    {
        protected Children Plugins;
        protected string Username;
        protected string CustomId;

        [BddfyFact(DisplayName = nameof(AValidInstanceWithExamplePlugins))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstanceWithExamplePlugins())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Plugins), nameof(ErrorMessagesCount))
                {
                    { Children.Null,     1 },
                    { Children.Empty,    0 },
                    { Children.OneError, 1 }
                })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithExamplePropertyValues))]
        public void Scenario2() =>
            this.Given(x => x.AnInstanceWithExamplePropertyValues())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Username), nameof(CustomId), nameof(Tags), nameof(ErrorMessagesCount))
                {
                    { null,  null,   Set.Null,         1 },
                    { "",    "",     Set.Empty,        0 },
                    { "foo", "foo",  Set.ValidItems,   0 },
                    { "",     "",    Set.InvalidItems, 3 },
                    { "foo", "bar",  Set.InvalidSet,   1 }
                })
                .BDDfy();

        protected void AValidInstanceWithExamplePlugins() => Instance = BuildConsumerWithoutPlugins(this)
            .With(x => x.Plugins, this.CreateChildren(Plugins, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .Create();

        protected void AnInstanceWithExamplePropertyValues() => Instance = BuildConsumerWithoutPlugins(this, Username, CustomId, Tags).Create();

        public static IPostprocessComposer<KongConsumer> BuildConsumerWithoutPlugins(
            Fixture fixture,
            string username = "username",
            string customId = "customId",
            Set tags = Set.Null) => fixture.Build<KongConsumer>()
            .With(x => x.Id, fixture.Create<string>())
            .With(x => x.UpdatedAt, (long?)null)
            .With(x => x.Username, username)
            .With(x => x.CustomId, customId)
            .With(x => x.Plugins, fixture.CreateChildren(Children.Empty, fixture.GetValidKongPlugin, fixture.GetKongPluginWithOneError))
            .With(x => x.Tags, fixture.CreateTags(tags));
    }

    [Story(Title = nameof(KongConsumer) + nameof(KongObject.ToJsonStringContent))]
    public class KongConsumerSerializationScenarios : KongObjectSerializationScenarios<KongConsumer>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.PluginsIsNotNull())
                .BDDfy();

        protected void PluginsIsNotSerialized() => Serialized.Should().NotContain("\"plugins\":");

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();
    }
}
