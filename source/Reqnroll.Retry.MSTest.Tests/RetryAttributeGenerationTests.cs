namespace Reqnroll.Retry.MSTest.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestClass]
public sealed class RetryAttributeGenerationTests
{
    private const string GeneratedFeatureClassName = "RetryAttributeGenerationFeature";
    private const string GeneratedFeatureFileName = "RetryAttribute.feature.cs";
    private const string ReqnrollRetryCountKey = "ReqnrollRetryCount";

    private static int ExpectedRetryCount => int.Parse
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

    [TestMethod]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        string? projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location);

        Assert.IsNotNull(projectDirectory, "Could not determine assembly location directory.");

        DirectoryInfo? directory = new (projectDirectory);

        while (directory is not null && File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.MSTest.Tests.csproj")) is false)
        {
            directory = directory.Parent;
        }

        Assert.IsNotNull(directory, "Could not find project directory.");

        string featuresDirectory = Path.Combine(directory.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, GeneratedFeatureFileName);

        Assert.IsTrue(File.Exists(generatedFilePath), $"Expected generated file to exist at: {generatedFilePath}.");

        string generatedCode = File.ReadAllText(generatedFilePath);

        string retryCount = ExpectedRetryCount.ToString();

        bool containsRetryAttribute = generatedCode.Contains($"[global::Microsoft.VisualStudio.TestTools.UnitTesting.{nameof(RetryAttribute)}({retryCount})]") ||
                                      generatedCode.Contains($"[Microsoft.VisualStudio.TestTools.UnitTesting.{nameof(RetryAttribute)}({retryCount})]") ||
                                      generatedCode.Contains($"[Retry({retryCount})]") ||
                                      generatedCode.Contains($"{nameof(RetryAttribute)}({retryCount})");

        Assert.IsTrue(containsRetryAttribute, $"Expected generated code to contain the {nameof(RetryAttribute)} with value {retryCount}.");
    }
}
