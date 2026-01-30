namespace Reqnroll.Retry.NUnit.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestFixture]
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
        }
    }

    [Test]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        string? projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location);

        Assert.That(projectDirectory, Is.Not.Null, "Could not determine assembly location directory.");

        if (projectDirectory is null) return;

        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.NUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.That(directory, Is.Not.Null, "Could not find project directory.");

        if (directory is null) return;

        string featuresDirectory = Path.Combine(directory.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, GeneratedFeatureFileName);

        Assert.That(File.Exists(generatedFilePath), Is.True, $"Expected generated file to exist at: {generatedFilePath}.");

        string generatedCode = File.ReadAllText(generatedFilePath);

        string retryCount = ExpectedRetryCount.ToString();

        bool containsRetryAttribute = generatedCode.Contains($"[global::NUnit.Framework.{nameof(RetryAttribute)}({retryCount})]") ||
                                      generatedCode.Contains($"[NUnit.Framework.{nameof(RetryAttribute)}({retryCount})]") ||
                                      generatedCode.Contains($"[Retry({retryCount})]") ||
                                      generatedCode.Contains($"{nameof(RetryAttribute)}({retryCount})");

        Assert.That(containsRetryAttribute, Is.True, $"Expected generated code to contain the {nameof(RetryAttribute)} with value {retryCount}.");
    }
}
