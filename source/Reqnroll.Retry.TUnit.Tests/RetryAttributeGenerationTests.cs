using System.Reflection;

namespace ReqnrollRetry.TUnitTests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
public sealed class RetryAttributeGenerationTests
{
    [Test]
    public async Task Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        // Arrange
        System.Reflection.Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;
        Type? featureType = testAssembly.GetTypes().FirstOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.EndsWith("Feature"));

        // Assert - Feature Class Was Generated
        await Assert.That(featureType).IsNotNull().Because("Expected to find a generated feature class containing 'RetryAttributeFeature' in the assembly.");

        // Debug - List All Methods And Their Attributes
        System.Reflection.MethodInfo[] allMethods = featureType!.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

        // Act - Find Test Methods In The Generated Feature Class
        // Note: TUnit Uses TUnit.Core.TestAttribute For Test Methods
        System.Reflection.MethodInfo[] testMethods = allMethods
            .Where(method => method.GetCustomAttributes().Any(attribute => attribute.GetType().FullName?.Contains("TestAttribute") == true))
            .ToArray();

        // Assert - Test Methods Were Generated
        string methodNames = string.Join(", ", allMethods.Select(method => method.Name));
        await Assert.That(testMethods.Length).IsGreaterThan(0).Because($"Expected to find at least one test method. Found methods: {methodNames}");

        // Assert - Each Test Method Has The Retry Attribute
        foreach (System.Reflection.MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            await Assert.That(retryAttribute).IsNotNull().Because($"Expected test method '{testMethod.Name}' to have the [Retry] attribute.");
            await Assert.That(retryAttribute!.Times).IsEqualTo(2).Because($"Expected test method '{testMethod.Name}' to have Times of 2 (configured via ReqnrollRetryCount).");
        }
    }

    [Test]
    public async Task Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        // Arrange
        string projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location)!;

        // Navigate Up To Find The Project Root (From bin/Debug/net10.0 To Project Root)
        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.TUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        await Assert.That(directory).IsNotNull().Because("Could not find project directory.");

        // Find The Generated Feature File
        string featuresDirectory = Path.Combine(directory!.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        // Assert - Generated File Exists
        await Assert.That(File.Exists(generatedFilePath)).IsTrue().Because($"Expected generated file to exist at: {generatedFilePath}");

        // Act - Read The Generated Code
        string generatedCode = File.ReadAllText(generatedFilePath);

        // Assert - Generated Code Contains Retry Attribute
        bool containsRetryAttribute = generatedCode.Contains("[global::TUnit.Core.RetryAttribute(2)]") ||
                                       generatedCode.Contains("[TUnit.Core.RetryAttribute(2)]") ||
                                       generatedCode.Contains("[Retry(2)]") ||
                                       generatedCode.Contains("RetryAttribute(2)");

        await Assert.That(containsRetryAttribute).IsTrue().Because("Expected generated code to contain the Retry attribute with value 2.");
    }
}
