namespace Reqnroll.Retry.xUnit;

/// <summary>
///     A feature generator provider that generates every feature file with the <see cref="RetryFeatureGenerator" />.
/// </summary>
public sealed class RetryFeatureGeneratorProvider(RetryFeatureGenerator retryFeatureGenerator) : IFeatureGeneratorProvider
{
    // Take Precedence Over Reqnroll's Default Provider Only, So More Specific Providers From Other Plugins Still Apply
    public int Priority => PriorityValues.Lowest - 1;

    public bool CanGenerate(ReqnrollDocument document) => true; // Apply To All Feature Files

    public IFeatureGenerator CreateGenerator(ReqnrollDocument document) => retryFeatureGenerator;
}
