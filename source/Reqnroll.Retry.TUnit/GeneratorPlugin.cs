[assembly: GeneratorPlugin(typeof(Reqnroll.Retry.TUnit.GeneratorPlugin))]

namespace Reqnroll.Retry.TUnit;

/// <summary>
///     A Reqnroll generator plugin that adds the TUnit [Retry] attribute to all generated BDD test methods.
///     The retry count can be configured via the "ReqnrollRetryCount" property in the project file.
/// </summary>
public sealed class GeneratorPlugin : IGeneratorPlugin
{
    private const string RetryCountParameter = "RetryCount";
    private const int DefaultRetryCount = 1;

    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        int retryCount = GetRetryCount(generatorPluginParameters);

        // A Retry Count Of 0 Disables Retries
        if (retryCount == 0) return;

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

        if (parameters.TryGetValue(RetryCountParameter, out string? retryCountString) is false) return DefaultRetryCount;

        return int.TryParse(retryCountString, out int retryCount) && retryCount >= 0
            ? retryCount
            : throw new ReqnrollException($@"The ""{RetryCountParameter}"" parameter must be a whole number of 0 or more, but is ""{retryCountString}"".");
    }
}
