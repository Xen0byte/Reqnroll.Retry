namespace Reqnroll.Retry.MSTest.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestClass]
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

    [TestMethod]
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;

        Type? featureType = testAssembly.GetTypes().SingleOrDefault(type => type.Name == GeneratedFeatureClassName);

        Assert.IsNotNull(featureType, $"Expected to find the generated feature class {GeneratedFeatureClassName} in the assembly.");

        MethodInfo[] testMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<TestMethodAttribute>() is not null).ToArray();

        Assert.IsNotEmpty(testMethods, $"Expected to find at least one test method with [{nameof(TestMethodAttribute)}] attribute.");

        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            Assert.IsNotNull(retryAttribute, $@"Expected test method ""{testMethod.Name}"" to have the [Retry] attribute.");
            Assert.AreEqual(ExpectedRetryCount, retryAttribute.MaxRetryAttempts, $@"Expected test method ""{testMethod.Name}"" to have MaxRetryAttempts of {ExpectedRetryCount} (configured via ReqnrollRetryCount).");
        }
    }
}
