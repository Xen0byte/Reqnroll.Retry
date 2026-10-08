namespace Reqnroll.Retry.xUnit.Tests.Features;

/// <summary>
///     Makes the feature class generated from "RetryOutcomes.feature" abstract, which stops xUnit from running its scenarios.
/// </summary>
/// <remarks>
///     These scenarios are skipped or fail by design, so they would never pass when run by xUnit. <see cref="RetryOutcomeTests" /> runs them instead, and passes when their outcome is the expected one.
/// </remarks>
public abstract partial class RetryOutcomesFeature;
