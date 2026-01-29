namespace Reqnroll.Retry.TUnit.Tests;

/// <summary>
///     Tests that verify the Retry attribute is correctly added to generated Reqnroll test methods.
/// </summary>
public sealed class RetryAttributeGenerationTests
{
    private static int ExpectedRetryCount => int.Parse
    (
        typeof(RetryAttributeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == "ReqnrollRetryCount")?.Value ?? "1"
    );

    [Test]
    public async Task Generated_Test_Methods_Should_Have_Retry_Attribute()
    {
        Assembly testAssembly = typeof(RetryAttributeGenerationTests).Assembly;

        Type? featureType = testAssembly.GetTypes().SingleOrDefault(type => type.Name.Contains("RetryAttribute") && type.Name.Contains("Feature"));

        await Assert.That(featureType).IsNotNull().Because(@"Expected to find a generated feature class containing ""RetryAttribute"" and ""Feature"" in the assembly.");

        if (featureType is null) return;

        MethodInfo[] allMethods = featureType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        MethodInfo[] testMethods = allMethods
            .Where(method => method.GetCustomAttributes().Any(attribute => attribute.GetType().FullName?.Contains("TestAttribute") == true)).ToArray();

        string methodNames = string.Join(", ", allMethods.Select(method => method.Name));

        await Assert.That(testMethods.Length).IsGreaterThan(0).Because($"Expected to find at least one test method. Found methods: {methodNames}.");

        foreach (MethodInfo testMethod in testMethods)
        {
            RetryAttribute? retryAttribute = testMethod.GetCustomAttribute<RetryAttribute>();

            await Assert.That(retryAttribute).IsNotNull().Because($@"Expected test method ""{testMethod.Name}"" to have the [Retry] attribute.");

            if (retryAttribute is null) continue;

            await Assert.That(retryAttribute.Times).IsEqualTo(ExpectedRetryCount).Because($@"Expected test method ""{testMethod.Name}"" to have ""Times"" of {ExpectedRetryCount} (configured via ReqnrollRetryCount).");
        }
    }

    [Test]
    public async Task Generated_Feature_Code_File_Should_Contain_Retry_Attribute()
    {
        string? projectDirectory = Path.GetDirectoryName(typeof(RetryAttributeGenerationTests).Assembly.Location);

        await Assert.That(projectDirectory).IsNotNull().Because("Could not determine assembly location directory.");

        if (projectDirectory is null) return;

        DirectoryInfo? directory = new DirectoryInfo(projectDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Reqnroll.Retry.TUnit.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        await Assert.That(directory).IsNotNull().Because("Could not find project directory.");

        if (directory is null) return;

        string featuresDirectory = Path.Combine(directory.FullName, "Features");
        string generatedFilePath = Path.Combine(featuresDirectory, "RetryAttribute.feature.cs");

        await Assert.That(File.Exists(generatedFilePath)).IsTrue().Because($"Expected generated file to exist at: {generatedFilePath}.");

        string generatedCode = File.ReadAllText(generatedFilePath);

        string retryCount = ExpectedRetryCount.ToString();

        bool containsRetryAttribute = generatedCode.Contains($"[global::TUnit.Core.RetryAttribute({retryCount})]") ||
                                      generatedCode.Contains($"[TUnit.Core.RetryAttribute({retryCount})]") ||
                                      generatedCode.Contains($"[Retry({retryCount})]") ||
                                      generatedCode.Contains($"RetryAttribute({retryCount})");

        await Assert.That(containsRetryAttribute).IsTrue().Because($"Expected generated code to contain the Retry attribute with value {retryCount}.");
    }
}
