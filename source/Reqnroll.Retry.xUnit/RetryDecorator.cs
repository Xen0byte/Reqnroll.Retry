namespace Reqnroll.Retry.xUnit;

/// <summary>
///     A test method decorator that replaces xUnit test attributes with xRetry retry attributes.
///     This enables automatic retry functionality for BDD scenarios.
/// </summary>
/// <remarks>
///     xUnit does not have a native retry attribute. This decorator uses the xRetry library by replacing SkippableTheory/SkippableFact with RetryTheory/RetryFact attributes.
///     The consuming project must reference the xRetry NuGet package.
/// </remarks>
public sealed class RetryDecorator(int retryCount) : ITestMethodDecorator
{
    private const string SkippableTheoryAttribute = "Xunit.SkippableTheoryAttribute";
    private const string SkippableFactAttribute = "Xunit.SkippableFactAttribute";
    private const string TheoryAttribute = "Xunit.TheoryAttribute";
    private const string FactAttribute = "Xunit.FactAttribute";
    private const string RetryTheoryAttribute = "xRetry.RetryTheoryAttribute";
    private const string RetryFactAttribute = "xRetry.RetryFactAttribute";
    private const int DefaultRetryCount = 1;

    private int RetryCount { get; } = retryCount > 0 ? retryCount : DefaultRetryCount;

    public int Priority => PriorityValues.Low;

    public bool CanDecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod) => true; // Apply To All Test Methods

    public void DecorateFrom(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
    {
        CodeAttributeDeclaration? attributeToReplace = null;

        string? replacementAttributeName = null;

        // Find And Replace Theory/Fact Attributes With RetryTheory/RetryFact
        foreach (CodeAttributeDeclaration attribute in testMethod.CustomAttributes.Cast<CodeAttributeDeclaration>())
        {
            string attributeTypeName = attribute.AttributeType.BaseType;

            if (attributeTypeName == SkippableTheoryAttribute || attributeTypeName == TheoryAttribute)
            {
                attributeToReplace = attribute;
                replacementAttributeName = RetryTheoryAttribute;
                break;
            }

            if (attributeTypeName == SkippableFactAttribute || attributeTypeName == FactAttribute)
            {
                attributeToReplace = attribute;
                replacementAttributeName = RetryFactAttribute;
                break;
            }
        }

        if (attributeToReplace is not null && replacementAttributeName is not null)
        {
            // Remove The Original Attribute
            testMethod.CustomAttributes.Remove(attributeToReplace);

            // Add The Retry Attribute With The Same Display Name And Retry Count
            CodeTypeReference attributeTypeReference = new (replacementAttributeName, CodeTypeReferenceOptions.GlobalReference);

            // Copy Existing Arguments (Like DisplayName) And Add MaxRetries
            List<CodeAttributeArgument> arguments = new ();

            // Add MaxRetries As The First Argument
            arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(RetryCount)));

            // Copy Any Named Arguments From The Original Attribute (Like DisplayName)
            foreach (CodeAttributeArgument existingArgument in attributeToReplace.Arguments.Cast<CodeAttributeArgument>())
            {
                if (string.IsNullOrEmpty(existingArgument.Name) is false)
                {
                    arguments.Add(existingArgument);
                }
            }

            CodeAttributeDeclaration retryAttribute = new (attributeTypeReference, arguments.ToArray());

            testMethod.CustomAttributes.Add(retryAttribute);
        }
    }
}
