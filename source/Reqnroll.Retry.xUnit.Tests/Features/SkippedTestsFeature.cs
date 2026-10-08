namespace Reqnroll.Retry.xUnit.Tests.Features;

/// <summary>
///     Makes the feature class generated from "SkippedTests.feature" abstract, which stops xUnit from running its scenarios.
/// </summary>
/// <remarks>
///     These scenarios are skipped by design, so they would never pass when run by xUnit. <see cref="SkippedScenarioTests" /> runs them instead, and passes when they are skipped as expected.
/// </remarks>
public abstract partial class SkippedTestsFeature;
