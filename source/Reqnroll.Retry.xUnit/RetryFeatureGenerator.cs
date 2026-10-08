namespace Reqnroll.Retry.xUnit;

/// <summary>
///     A feature generator that wraps the body of every generated xUnit test method in a retry loop.
///     This enables automatic retry functionality for BDD scenarios.
/// </summary>
/// <remarks>
///     Reqnroll generates the body of a test method after the test method decorators have run, so the retry loop is added once the whole test class has been generated.
///     After a failed attempt, the test runner is torn down and initialised again, the same way xUnit does it between tests, before the scenario runs again.
///     Skipped tests are never retried, and once all retries have been used up, the test fails with the exception of the final attempt.
///     This derives from <see cref="UnitTestFeatureGenerator" /> because Reqnroll only passes the feature file details, which are needed for Cucumber messages, to generators of that type.
/// </remarks>
public sealed class RetryFeatureGenerator(IUnitTestGeneratorProvider testGeneratorProvider, CodeDomHelper codeDomHelper, ReqnrollConfiguration reqnrollConfiguration, IDecoratorRegistry decoratorRegistry, int retryCount)
    : UnitTestFeatureGenerator(testGeneratorProvider, codeDomHelper, reqnrollConfiguration, decoratorRegistry), IFeatureGenerator
{
    private const string SkippableFactAttribute = "Xunit.SkippableFactAttribute";
    private const string SkippableTheoryAttribute = "Xunit.SkippableTheoryAttribute";
    private const string SkipException = "Xunit.SkipException";
    private const string TestOutputHelperField = "_testOutputHelper";
    private const string AttemptVariable = "__attempt";
    private const string ExceptionVariable = "__exception";
    private const string SkipExceptionVariable = "__skipException";
    private const string RetryMessageFormat = "Attempt {0} Of {1} Failed, Retrying: {2}";
    private const int DefaultRetryCount = 1;

    private CodeDomHelper CodeDomHelper { get; } = codeDomHelper;

    private int RetryCount { get; } = retryCount > 0 ? retryCount : DefaultRetryCount;

    // Re-Implements The Interface Method, Because The Base Class Method Is Not Virtual
    public new UnitTestFeatureGenerationResult GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace)
    {
        UnitTestFeatureGenerationResult generationResult = base.GenerateUnitTestFixture(document, testClassName, targetNamespace);

        IEnumerable<CodeMemberMethod> testMethods = generationResult.CodeNamespace.Types.Cast<CodeTypeDeclaration>()
            .SelectMany(testClass => testClass.Members.OfType<CodeMemberMethod>())
            .Where(IsTestMethod);

        foreach (CodeMemberMethod testMethod in testMethods)
        {
            AddRetryLoop(testMethod);
        }

        return generationResult;
    }

    private static bool IsTestMethod(CodeMemberMethod method) => method.CustomAttributes.Cast<CodeAttributeDeclaration>()
        .Any(attribute => attribute.Name is SkippableFactAttribute or SkippableTheoryAttribute);

    private void AddRetryLoop(CodeMemberMethod testMethod)
    {
        // The Initial Attempt Plus One Attempt Per Retry
        CodePrimitiveExpression maximumAttempts = new (RetryCount + 1);

        CodeVariableReferenceExpression attempt = new (AttemptVariable);

        // Rethrow Skipped Tests Straight Away, So They Are Reported As Skipped Rather Than Retried
        CodeCatchClause skipClause = new (SkipExceptionVariable, new CodeTypeReference(SkipException, CodeTypeReferenceOptions.GlobalReference), new CodeThrowExceptionStatement());

        // Rethrow The Failure Of The Final Attempt, Otherwise Log The Failure And Reset The Test Runner Before The Next Attempt
        CodeCatchClause retryClause = new
        (
            ExceptionVariable, new CodeTypeReference(typeof(Exception), CodeTypeReferenceOptions.GlobalReference),
            new CodeConditionStatement(new CodeBinaryOperatorExpression(attempt, CodeBinaryOperatorType.ValueEquality, maximumAttempts), new CodeThrowExceptionStatement()),
            new CodeExpressionStatement
            (
                new CodeMethodInvokeExpression
                (
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), TestOutputHelperField), "WriteLine",
                    new CodePrimitiveExpression(RetryMessageFormat), attempt, maximumAttempts, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(ExceptionVariable), nameof(Exception.Message))
                )
            ),
            CreateAwaitStatement(GeneratorConstants.TEST_CLEANUP_NAME),
            CreateAwaitStatement(GeneratorConstants.TEST_INITIALIZE_NAME)
        );

        // Return As Soon As An Attempt Passes
        CodeStatement[] attemptStatements = testMethod.Statements.Cast<CodeStatement>().Append(new CodeMethodReturnStatement()).ToArray();

        CodeTryCatchFinallyStatement attemptBlock = new (attemptStatements, [skipClause, retryClause]);

        CodeIterationStatement retryLoop = new
        (
            new CodeVariableDeclarationStatement(typeof(int), AttemptVariable, new CodePrimitiveExpression(1)),
            new CodeBinaryOperatorExpression(attempt, CodeBinaryOperatorType.LessThanOrEqual, maximumAttempts),
            new CodeAssignStatement(attempt, new CodeBinaryOperatorExpression(attempt, CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
            attemptBlock
        );

        testMethod.Statements.Clear();
        testMethod.Statements.Add(retryLoop);
    }

    private CodeExpressionStatement CreateAwaitStatement(string methodName) => new
    (
        CodeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodName))
    );
}
