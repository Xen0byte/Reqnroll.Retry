namespace Reqnroll.Retry.xUnit.Tests;

/// <summary>
///     Tests that verify only tests failing with a test error are retried, and only until no retries are left.
/// </summary>
/// <remarks>
///     These scenarios are skipped or fail by design, so they would never pass when run by xUnit, which is why <see cref="RetryOutcomesFeature" /> is abstract.
///     These tests run its scenarios instead, and pass when the outcome of the scenario is the expected one.
/// </remarks>
public sealed class RetryOutcomeTests(ITestOutputHelper testOutputHelper)
{
    private const string IgnoredSkipReason = "Ignored";

    [Fact]
    public async Task Test_Failing_On_Every_Attempt_Should_Be_Retried_Until_No_Retries_Are_Left()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.TestFailingOnEveryAttemptIsRetriedUntilNoRetriesAreLeft());

        Assert.IsType<InvalidOperationException>(outcome);
        Assert.Equal(RetryAttributeGenerationTests.ExpectedRetryCount + 1, RetryAttributeSteps.GetAttemptCount(nameof(RetryAttributeSteps.The_Test_Fails_On_Every_Attempt)));
    }

    [Fact]
    public void Ignored_Test_Method_Should_Keep_Skip_Reason()
    {
        MethodInfo? ignoredTestMethod = typeof(RetryOutcomesFeature).GetMethod(nameof(RetryOutcomesFeature.IgnoredTestIsSkipped));

        Assert.NotNull(ignoredTestMethod);

        if (ignoredTestMethod is null) return;

        Assert.Equal(IgnoredSkipReason, ignoredTestMethod.GetCustomAttribute<FactAttribute>()?.Skip);
    }

    [Fact]
    public async Task Ignored_Test_Should_Be_Skipped()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.IgnoredTestIsSkipped());

        Assert.IsType<SkipException>(outcome);
    }

    [Fact]
    public async Task Test_Skipped_At_Runtime_Should_Not_Be_Retried()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.TestSkippedAtRuntimeIsNotRetried());

        Assert.IsType<SkipException>(outcome);
        Assert.Equal(1, RetryAttributeSteps.GetAttemptCount(nameof(RetryAttributeSteps.The_Test_Is_Skipped_At_Runtime)));
    }

    [Fact]
    public async Task Pending_Test_Should_Not_Be_Retried()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.PendingTestIsNotRetried());

        Assert.NotNull(outcome);
        Assert.Equal(1, RetryAttributeSteps.GetAttemptCount(nameof(RetryAttributeSteps.The_Test_Is_Pending)));
    }

    [Fact]
    public async Task Cancelled_Test_Should_Not_Be_Retried()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.CancelledTestIsNotRetried());

        Assert.IsType<OperationCanceledException>(outcome);
        Assert.Equal(1, RetryAttributeSteps.GetAttemptCount(nameof(RetryAttributeSteps.The_Test_Run_Is_Cancelled)));
    }

    // Run The Scenario Through The Same Lifecycle As xUnit, But Return Its Outcome Instead Of Reporting It To xUnit
    // The Outcome Is Captured With A Plain Catch, Because Assert.ThrowsAsync Rethrows Skip Exceptions, Which Would Skip This Test
    private async Task<Exception?> RunScenarioAsync(Func<RetryOutcomesFeature, Task> scenario)
    {
        RetryOutcomesFeature.FixtureData fixtureData = new ();

        await ((IAsyncLifetime) fixtureData).InitializeAsync();

        try
        {
            RetryOutcomesFeature feature = new RetryOutcomes(fixtureData, testOutputHelper);

            await ((IAsyncLifetime) feature).InitializeAsync();

            try
            {
                await scenario(feature);

                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
            finally
            {
                await ((IAsyncDisposable) feature).DisposeAsync();
            }
        }
        finally
        {
            await ((IAsyncDisposable) fixtureData).DisposeAsync();
        }
    }

    // Private, So That xUnit Does Not Discover The Scenarios It Inherits
    private sealed class RetryOutcomes(RetryOutcomesFeature.FixtureData fixtureData, ITestOutputHelper testOutputHelper) : RetryOutcomesFeature(fixtureData, testOutputHelper);
}
