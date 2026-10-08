namespace Reqnroll.Retry.NUnit.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestFixture]
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
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;

        Type? featureType = testAssembly.GetTypes().SingleOrDefault(type => type.Name == GeneratedFeatureClassName);

        Assert.That(featureType, Is.Not.Null, $"Expected to find the generated feature class {GeneratedFeatureClassName} in the assembly.");

        if (featureType is null) return;

        MethodInfo[] testMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null).ToArray();

        Assert.That(testMethods.Length, Is.GreaterThan(0), $"Expected to find at least one test method with [{nameof(TestAttribute)}] attribute.");

        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            Assert.That(retryAttribute, Is.Not.Null, $@"Expected test method ""{testMethod.Name}"" to have the [Retry] attribute.");

            // NUnit Does Not Expose The Try Count, So Read It From The Attribute's Constructor Argument
            CustomAttributeData retryAttributeData = testMethod.GetCustomAttributesData().Single(attribute => attribute.AttributeType == typeof(RetryAttribute));

            // NUnit Counts The Initial Attempt, So The Retry Attribute Should Allow One Attempt More Than The Number Of Retries
            Assert.That(retryAttributeData.ConstructorArguments.Single().Value, Is.EqualTo(ExpectedRetryCount + 1), $@"Expected test method ""{testMethod.Name}"" to have a try count of {ExpectedRetryCount + 1} (configured via ReqnrollRetryCount, plus the initial attempt).");

            // NUnit Only Retries Assertion Failures Unless Told Which Exceptions To Retry
            Assert.That(retryAttribute?.RetryExceptions, Does.Contain(typeof(Exception)), $@"Expected test method ""{testMethod.Name}"" to be retried on any exception.");
        }
    }
}
