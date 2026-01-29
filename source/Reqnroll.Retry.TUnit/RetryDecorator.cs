namespace Reqnroll.Retry.TUnit;

/// <summary>
///     A test method decorator that adds the TUnit [Retry] attribute to all generated test methods.
///     This enables automatic retry functionality for BDD scenarios.
/// </summary>
/// <remarks>
///     The TUnit RetryAttribute specifies the number of retries on any exception.
///     Custom retry logic can be implemented by inheriting from RetryAttribute and overriding ShouldRetry.
/// </remarks>
public sealed class RetryDecorator(int retryCount) : ITestMethodDecorator
{
    private const string RetryAttribute = "TUnit.Core.RetryAttribute";
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
