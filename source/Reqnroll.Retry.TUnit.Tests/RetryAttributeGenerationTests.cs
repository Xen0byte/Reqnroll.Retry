using System.Reflection; // Not A Global Using, Because Reqnroll.TUnit's Generated Assembly Hooks Use "Assembly" To Refer To "HookType.Assembly"

namespace Reqnroll.Retry.TUnit.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
public sealed class RetryAttributeGenerationTests
{
    private const string GeneratedFeatureClassName = "RetryAttributeGenerationFeature";
    private const string ReqnrollRetryCountKey = "ReqnrollRetryCount";

    internal static int ExpectedRetryCount => int.Parse
    (
        typeof(RetryAttributeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == ReqnrollRetryCountKey)?.Value ?? "1"
    );

    [Test]
    public async Task Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;

        Type? featureType = testAssembly.GetTypes().SingleOrDefault(type => type.Name == GeneratedFeatureClassName);

        await Assert.That(featureType).IsNotNull().Because($"Expected to find the generated feature class {GeneratedFeatureClassName} in the assembly.");

        if (featureType is null) return;

        MethodInfo[] testMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null).ToArray();

        await Assert.That(testMethods.Length).IsGreaterThan(0).Because($"Expected to find at least one test method with [{nameof(TestAttribute)}] attribute.");

        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            await Assert.That(retryAttribute).IsNotNull().Because($@"Expected test method ""{testMethod.Name}"" to have the [Retry] attribute.");

            if (retryAttribute is null) continue;

            await Assert.That(retryAttribute.Times).IsEqualTo(ExpectedRetryCount).Because($@"Expected test method ""{testMethod.Name}"" to have ""Times"" of {ExpectedRetryCount} (configured via ReqnrollRetryCount).");
        }
    }
}
