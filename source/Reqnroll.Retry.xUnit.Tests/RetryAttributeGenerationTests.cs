namespace Reqnroll.Retry.xUnit.Tests;

/// <summary>
///     Tests that verify the RetryTheory/RetryFact attributes are correctly added to generated Reqnroll test methods.
/// </summary>
public sealed class RetryAttributeGenerationTests
{
    private static int ExpectedRetryCount => int.Parse
    (
        typeof(RetryAttributeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == "ReqnrollRetryCount")?.Value ?? "1"
    );

    [Fact]
    public void Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;

        Type? featureType = testAssembly.GetTypes().SingleOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.Contains("Feature"));

        Assert.NotNull(featureType);

        if (featureType is null) return;

        MethodInfo[] retryTheoryMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<xRetry.RetryTheoryAttribute>() is not null).ToArray();

        MethodInfo[] retryFactMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<xRetry.RetryFactAttribute>() is not null).ToArray();

        MethodInfo[] allRetryMethods = retryTheoryMethods.Concat(retryFactMethods).ToArray();

        Assert.True(allRetryMethods.Length > 0, @"Expected to find at least one test method with [RetryTheory] or [RetryFact] attribute.");

        foreach (MethodInfo testMethod in retryTheoryMethods)
        {
            xRetry.RetryTheoryAttribute? retryAttribute = testMethod.GetCustomAttribute<xRetry.RetryTheoryAttribute>();

            Assert.NotNull(retryAttribute);

            if (retryAttribute is null) continue;

            Assert.Equal(ExpectedRetryCount, retryAttribute.MaxRetries);
        }

        foreach (MethodInfo testMethod in retryFactMethods)
        {
            xRetry.RetryFactAttribute? retryAttribute = testMethod.GetCustomAttribute<xRetry.RetryFactAttribute>();

            Assert.NotNull(retryAttribute);

            if (retryAttribute is null) continue;

            Assert.Equal(ExpectedRetryCount, retryAttribute.MaxRetries);
        }
    }

    [Fact]
    public void Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        string? projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location);

        Assert.NotNull(projectDirectory);

        if (projectDirectory is null) return;

        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.xUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);

        if (directory is null) return;

        string featuresDirectory = Path.Combine(directory.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        Assert.True(File.Exists(generatedFilePath), $"Expected generated file to exist at: {generatedFilePath}.");

        string generatedCode = File.ReadAllText(generatedFilePath);

        string retryCount = ExpectedRetryCount.ToString();

        bool containsRetryAttribute = generatedCode.Contains($"RetryTheoryAttribute({retryCount}") ||
                                      generatedCode.Contains($"RetryFactAttribute({retryCount}") ||
                                      generatedCode.Contains($"[RetryTheory({retryCount})") ||
                                      generatedCode.Contains($"[RetryFact({retryCount})");

        Assert.True(containsRetryAttribute, $"Expected generated code to contain the RetryTheory or RetryFact attribute with value {retryCount}.");
    }
}
