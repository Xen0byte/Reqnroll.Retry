namespace Reqnroll.Retry.MSTest;

/// <summary>
///     A test method decorator that adds the MSTest [Retry] attribute to all generated test methods.
///     This enables automatic retry functionality for BDD scenarios.
/// </summary>
/// <remarks>
///     The MSTest RetryAttribute was introduced in MSTest 3.8 and requires MSTest.TestFramework 3.8.0 or later.
/// </remarks>
public sealed class RetryDecorator(int retryCount) : ITestMethodDecorator
{
    private const string RetryAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.RetryAttribute";
    private const int DefaultRetryCount = 1;

    private int RetryCount { get; } = retryCount > 0 ? retryCount : DefaultRetryCount;

    public int Priority => PriorityValues.Low;

    public bool CanDecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod) => true; // Apply To All Test Methods

    public void DecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        CodeTypeReference attributeTypeReference = new(RetryAttribute, CodeTypeReferenceOptions.GlobalReference);

        CodeAttributeDeclaration retryAttribute = new(attributeTypeReference, new CodeAttributeArgument(new CodePrimitiveExpression(RetryCount)));

        testMethod.CustomAttributes.Add(retryAttribute);
    }
}
