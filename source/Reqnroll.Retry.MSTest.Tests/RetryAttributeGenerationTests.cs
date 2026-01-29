namespace Reqnroll.Retry.MSTest.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
[TestClass]
public sealed class RetryAttributeGenerationTests
{
    [TestMethod]
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        // Arrange
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;
        Type? featureType = testAssembly.GetTypes().FirstOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.Contains("Feature"));

        // Assert - Feature Class Was Generated
        Assert.IsNotNull(featureType, "Expected to find a generated feature class containing 'RetryAttributeFeature' in the assembly.");

        // Act - Find Test Methods In The Generated Feature Class
        MethodInfo[] testMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<TestMethodAttribute>() is not null)
            .ToArray();

        // Assert - Test Methods Were Generated
        Assert.IsTrue(testMethods.Length > 0, "Expected to find at least one test method with [TestMethod] attribute.");

        // Assert - Each Test Method Has The Retry Attribute
        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            Assert.IsNotNull(retryAttribute, $"Expected test method '{testMethod.Name}' to have the [Retry] attribute.");
            Assert.AreEqual(2, retryAttribute.MaxRetryAttempts, $"Expected test method '{testMethod.Name}' to have MaxRetryAttempts of 2 (configured via ReqnrollRetryCount).");
        }
    }

    [TestMethod]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        // Arrange
        string projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location)!;

        // Navigate Up To Find The Project Root (From bin/Debug/net10.0 To Project Root)
        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.MSTest.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.IsNotNull(directory, "Could not find project directory.");

        // Find The Generated Feature File
        string featuresDirectory = Path.Combine(directory.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        // Assert - Generated File Exists
        Assert.IsTrue(File.Exists(generatedFilePath), $"Expected generated file to exist at: {generatedFilePath}");

        // Act - Read The Generated Code
        string generatedCode = File.ReadAllText(generatedFilePath);

        // Assert - Generated Code Contains Retry Attribute
        Assert.IsTrue(generatedCode.Contains("[global::Microsoft.VisualStudio.TestTools.UnitTesting.RetryAttribute(2)]") ||
                     generatedCode.Contains("[Microsoft.VisualStudio.TestTools.UnitTesting.RetryAttribute(2)]") ||
                     generatedCode.Contains("[Retry(2)]") ||
                     generatedCode.Contains("RetryAttribute(2)"),
                     "Expected generated code to contain the Retry attribute with value 2.");
    }
}
