namespace Reqnroll.Retry.NUnit.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestFixture]
public sealed class RetryAttributeGenerationTests
{
    [Test]
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        // Arrange
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;
        Type? featureType = testAssembly.GetTypes().FirstOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.Contains("Feature"));

        // Assert - Feature Class Was Generated
        Assert.That(featureType, Is.Not.Null, "Expected to find a generated feature class containing 'RetryAttributeFeature' in the assembly.");

        // Act - Find Test Methods In The Generated Feature Class
        MethodInfo[] testMethods = featureType!.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<TestAttribute>() is not null)
            .ToArray();

        // Assert - Test Methods Were Generated
        Assert.That(testMethods.Length, Is.GreaterThan(0), "Expected to find at least one test method with [Test] attribute.");

        // Assert - Each Test Method Has The Retry Attribute
        // Note: NUnit's RetryAttribute does not expose the retry count via a public property,
        // so we only verify the attribute exists here. The file content test verifies the correct value.
        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            Assert.That(retryAttribute, Is.Not.Null, $"Expected test method '{testMethod.Name}' to have the [Retry] attribute.");
        }
    }

    [Test]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        // Arrange
        string projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location)!;

        // Navigate Up To Find The Project Root (From bin/Debug/net10.0 To Project Root)
        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.NUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.That(directory, Is.Not.Null, "Could not find project directory.");

        // Find The Generated Feature File
        string featuresDirectory = Path.Combine(directory!.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        // Assert - Generated File Exists
        Assert.That(File.Exists(generatedFilePath), Is.True, $"Expected generated file to exist at: {generatedFilePath}");

        // Act - Read The Generated Code
        string generatedCode = File.ReadAllText(generatedFilePath);

        // Assert - Generated Code Contains Retry Attribute
        Assert.That(generatedCode.Contains("[global::NUnit.Framework.RetryAttribute(2)]") ||
                   generatedCode.Contains("[NUnit.Framework.RetryAttribute(2)]") ||
                   generatedCode.Contains("[Retry(2)]") ||
                   generatedCode.Contains("RetryAttribute(2)"),
                   Is.True,
                   "Expected generated code to contain the Retry attribute with value 2.");
    }
}
