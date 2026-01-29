namespace Reqnroll.Retry.NUnit.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestFixture]
public sealed class RetryAttributeGenerationTests
{
    private static int ExpectedRetryCount => int.Parse
    (
        typeof(RetryAttributeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == "ReqnrollRetryCount")?.Value ?? "1"
    );

    [Test]
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;

        Type? featureType = testAssembly.GetTypes().SingleOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.Contains("Feature"));

        Assert.That(featureType, Is.Not.Null, "Expected to find a generated feature class containing 'RetryAttribute' and 'Feature' in the assembly.");

        MethodInfo[] testMethods = featureType!.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null).ToArray();

        Assert.That(testMethods.Length, Is.GreaterThan(0), "Expected to find at least one test method with [Test] attribute.");

        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            Assert.That(retryAttribute, Is.Not.Null, $"Expected test method '{testMethod.Name}' to have the [Retry] attribute.");
        }
    }

    [Test]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        string projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location)!;

        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.NUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.That(directory, Is.Not.Null, "Could not find project directory.");

        string featuresDirectory = Path.Combine(directory!.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        Assert.That(File.Exists(generatedFilePath), Is.True, $"Expected generated file to exist at: {generatedFilePath}.");

        string generatedCode = File.ReadAllText(generatedFilePath);

        string retryCount = ExpectedRetryCount.ToString();

        bool containsRetryAttribute = generatedCode.Contains($"[global::NUnit.Framework.RetryAttribute({retryCount})]") ||
                                      generatedCode.Contains($"[NUnit.Framework.RetryAttribute({retryCount})]") ||
                                      generatedCode.Contains($"[Retry({retryCount})]") ||
                                      generatedCode.Contains($"RetryAttribute({retryCount})");

        Assert.That(containsRetryAttribute, Is.True, $"Expected generated code to contain the Retry attribute with value {retryCount}.");
    }
}
