namespace Reqnroll.Retry.NUnit;

/// <summary>
///     A test method decorator that adds the NUnit [Retry] attribute to all generated test methods.
///     This enables automatic retry functionality for BDD scenarios.
/// </summary>
/// <remarks>
///     The NUnit RetryAttribute specifies the total number of attempts (not retries after failure), so the attribute is given the retry count plus one for the initial attempt.
///     A retry count of 1 means up to 2 total attempts. A retry count of 2 means up to 3 total attempts.
///     NUnit only retries assertion failures by default, so the attribute also retries on any exception, which requires NUnit 4.5 or later.
/// </remarks>
public sealed class RetryDecorator(int retryCount) : ITestMethodDecorator
{
    private const string RetryAttribute = "NUnit.Framework.RetryAttribute";
    private const string RetryExceptionsProperty = "RetryExceptions";

    public int Priority => PriorityValues.Low;

    public bool CanDecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod) => true; // Apply To All Test Methods

    public void DecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        CodeTypeReference attributeTypeReference = new (RetryAttribute, CodeTypeReferenceOptions.GlobalReference);

        // The Initial Attempt Plus One Attempt Per Retry
        CodeAttributeArgument tryCount = new (new CodePrimitiveExpression(retryCount + 1));

        // Retry On Any Exception, Like Transient Network Or Timeout Errors, Rather Than Only On Assertion Failures
        CodeAttributeArgument retryExceptions = new
        (
            RetryExceptionsProperty, new CodeArrayCreateExpression
            (
                new CodeTypeReference(typeof(Type), CodeTypeReferenceOptions.GlobalReference),
                [new CodeTypeOfExpression(new CodeTypeReference(typeof(Exception), CodeTypeReferenceOptions.GlobalReference))]
            )
        );

        CodeAttributeDeclaration retryAttribute = new (attributeTypeReference, tryCount, retryExceptions);

        testMethod.CustomAttributes.Add(retryAttribute);
    }
}
