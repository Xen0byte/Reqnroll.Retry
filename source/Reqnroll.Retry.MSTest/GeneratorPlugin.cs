[assembly: GeneratorPlugin(typeof(Reqnroll.Retry.MSTest.GeneratorPlugin))]

namespace Reqnroll.Retry.MSTest;

/// <summary>
///     A Reqnroll generator plugin that adds the MSTest [Retry] attribute to all generated BDD test methods.
///     The retry count can be configured via the "ReqnrollRetryCount" property in the project file.
/// </summary>
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
