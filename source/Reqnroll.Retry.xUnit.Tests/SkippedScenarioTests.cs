namespace Reqnroll.Retry.xUnit.Tests;

/// <summary>
///     Tests that verify skipped scenarios are skipped without being retried, and without being reported as failures.
/// </summary>
/// <remarks>
///     A skipped scenario never passes when xUnit runs it, so <see cref="SkippedTestsFeature" /> is abstract, which stops xUnit from running it.
///     These tests run its scenarios instead, and pass when the outcome of the scenario is the expected skip.
/// </remarks>
public sealed class SkippedScenarioTests(ITestOutputHelper testOutputHelper)
{
    private const string IgnoredSkipReason = "Ignored";

    [Fact]
    public void Ignored_Test_Method_Should_Keep_Skip_Reason()
    {
        MethodInfo? ignoredTestMethod = typeof(SkippedTestsFeature).GetMethod(nameof(SkippedTestsFeature.IgnoredTestIsSkipped));

        Assert.NotNull(ignoredTestMethod);

        if (ignoredTestMethod is null) return;

        Assert.Equal(IgnoredSkipReason, ignoredTestMethod.GetCustomAttribute<FactAttribute>()?.Skip);
    }

    [Fact]
    public async Task Ignored_Scenario_Should_Be_Skipped()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.IgnoredTestIsSkipped());

        Assert.IsType<SkipException>(outcome);
    }

    [Fact]
    public async Task Scenario_Skipped_At_Runtime_Should_Be_Skipped_Without_Retrying()
    {
        Exception? outcome = await RunScenarioAsync(feature => feature.TestSkippedAtRuntimeIsSkipped());

        Assert.IsType<SkipException>(outcome);
        Assert.Equal(1, RetryAttributeSteps.RuntimeSkipCount);
    }

    // Run The Scenario Through The Same Lifecycle As xUnit, But Return Its Outcome Instead Of Reporting It To xUnit
    // The Outcome Is Captured With A Plain Catch, Because Assert.ThrowsAsync Rethrows Skip Exceptions, Which Would Skip This Test
    private async Task<Exception?> RunScenarioAsync(Func<SkippedTestsFeature, Task> scenario)
    {
        SkippedTestsFeature.FixtureData fixtureData = new ();

        await ((IAsyncLifetime) fixtureData).InitializeAsync();

        try
        {
            SkippedTestsFeature feature = new SkippedTests(fixtureData, testOutputHelper);

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
    private sealed class SkippedTests(SkippedTestsFeature.FixtureData fixtureData, ITestOutputHelper testOutputHelper) : SkippedTestsFeature(fixtureData, testOutputHelper);
}
