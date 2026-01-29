namespace Reqnroll.Retry.xUnit.Tests;

/// <summary>
///     Tests that verify the RetryTheory/RetryFact attributes are correctly added to generated Reqnroll test methods.
/// </summary>
public sealed class RetryAttributeGenerationTests
{
    [Fact]
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        // Arrange
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;
        Type? featureType = testAssembly.GetTypes().FirstOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.Contains("Feature"));

        // Assert - Feature Class Was Generated
        Assert.NotNull(featureType);

        // Act - Find Test Methods In The Generated Feature Class (xRetry Uses RetryTheoryAttribute Or RetryFactAttribute)
        MethodInfo[] retryTheoryMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<xRetry.RetryTheoryAttribute>() is not null)
            .ToArray();

        MethodInfo[] retryFactMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<xRetry.RetryFactAttribute>() is not null)
            .ToArray();

        MethodInfo[] allRetryMethods = retryTheoryMethods.Concat(retryFactMethods).ToArray();

        // Assert - Test Methods Were Generated With Retry Attribute
        Assert.True(allRetryMethods.Length > 0, "Expected to find at least one test method with [RetryTheory] or [RetryFact] attribute.");

        // Assert - Each Test Method Has The Correct Retry Count
        foreach (MethodInfo testMethod in retryTheoryMethods)
        {
            xRetry.RetryTheoryAttribute? retryAttribute = testMethod.GetCustomAttribute<xRetry.RetryTheoryAttribute>();

            Assert.NotNull(retryAttribute);
            Assert.Equal(2, retryAttribute!.MaxRetries);
        }

        foreach (MethodInfo testMethod in retryFactMethods)
        {
            xRetry.RetryFactAttribute? retryAttribute = testMethod.GetCustomAttribute<xRetry.RetryFactAttribute>();

            Assert.NotNull(retryAttribute);
            Assert.Equal(2, retryAttribute!.MaxRetries);
        }
    }

    [Fact]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        // Arrange
        string projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location)!;

        // Navigate Up To Find The Project Root (From bin/Debug/net10.0 To Project Root)
        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.xUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        // Find The Generated Feature File
        string featuresDirectory = Path.Combine(directory!.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        // Assert - Generated File Exists
        Assert.True(File.Exists(generatedFilePath), $"Expected generated file to exist at: {generatedFilePath}");

        // Act - Read The Generated Code
        string generatedCode = File.ReadAllText(generatedFilePath);

        // Assert - Generated Code Contains RetryTheory Or RetryFact Attribute (xRetry Replaces SkippableTheory/SkippableFact)
        bool containsRetryAttribute = generatedCode.Contains("RetryTheoryAttribute(2") ||
                                       generatedCode.Contains("RetryFactAttribute(2") ||
                                       generatedCode.Contains("[RetryTheory(2)") ||
                                       generatedCode.Contains("[RetryFact(2)");

        Assert.True(containsRetryAttribute, "Expected generated code to contain the RetryTheory or RetryFact attribute with value 2.");
    }
}
