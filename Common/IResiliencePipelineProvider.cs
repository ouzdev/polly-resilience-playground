using Polly;

namespace Common;

public interface IResiliencePipelineProvider<TResult> where TResult : IDisposable
{
    ResiliencePipeline<TResult> GetResiliencePipeline(ResiliencePipelineKey key);
}