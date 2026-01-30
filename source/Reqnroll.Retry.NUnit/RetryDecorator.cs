namespace Reqnroll.Retry.NUnit;

/// <summary>
///     A test method decorator that adds the NUnit [Retry] attribute to all generated test methods.
///     This enables automatic retry functionality for BDD scenarios.
/// </summary>
/// <remarks>
///     The NUnit RetryAttribute specifies the total number of attempts (not retries after failure).
///     A value of 1 means no retry. A value of 2 means up to 2 total attempts. A value of 3 means up to 3 total attempts.
///     NOTE: NUnit only retries on assertion failures, not on unexpected exceptions.
/// </remarks>
public sealed class RetryDecorator(int retryCount) : ITestMethodDecorator
{
    private const string RetryAttribute = "NUnit.Framework.RetryAttribute";
    private const int DefaultRetryCount = 1;

    private int RetryCount { get; } = retryCount > 0 ? retryCount : DefaultRetryCount;

    public int Priority => PriorityValues.Low;

    public bool CanDecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod) => true; // Apply To All Test Methods

    public void DecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        CodeTypeReference attributeTypeReference = new (RetryAttribute, CodeTypeReferenceOptions.GlobalReference);

        CodeAttributeDeclaration retryAttribute = new (attributeTypeReference, new CodeAttributeArgument(new CodePrimitiveExpression(RetryCount)));

        testMethod.CustomAttributes.Add(retryAttribute);
    }
}
