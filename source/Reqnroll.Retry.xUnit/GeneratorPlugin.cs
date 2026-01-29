[assembly: GeneratorPlugin(typeof(Reqnroll.Retry.xUnit.GeneratorPlugin))]

namespace Reqnroll.Retry.xUnit;

/// <summary>
///     A Reqnroll generator plugin that adds xRetry retry functionality to all generated BDD test methods.
///     The retry count can be configured via the "RetryCount" plugin parameter in the project file.
/// </summary>
/// <remarks>
///     xUnit does not have a native retry attribute, so this plugin uses the xRetry library.
///     It replaces SkippableTheory/SkippableFact attributes with RetryTheory/RetryFact attributes.
///     The consuming project must reference the xRetry NuGet package.
/// </remarks>
public sealed class GeneratorPlugin : IGeneratorPlugin
{
    private const string RetryCountParameter = "RetryCount";
    private const int DefaultRetryCount = 1;

    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        int retryCount = GetRetryCount(generatorPluginParameters);

        generatorPluginEvents.RegisterDependencies += (sender, eventArguments) =>
        {
            eventArguments.ObjectContainer.RegisterInstanceAs<ITestMethodDecorator>
            (
                new RetryDecorator(retryCount), "retry"
            );
        };
    }

    private static int GetRetryCount(GeneratorPluginParameters generatorPluginParameters)
    {
        IDictionary<string, string> parameters = generatorPluginParameters.GetParametersAsDictionary();

        return parameters.TryGetValue(RetryCountParameter, out string? retryCountString) && int.TryParse(retryCountString, out int retryCount) && retryCount > 0
            ? retryCount
            : DefaultRetryCount;
    }
}
