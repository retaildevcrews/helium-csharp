using Polly.Retry;

namespace CSE.KeyRotation
{
    public interface IKeyRotation
    {
        AsyncRetryPolicy RetryCosmosPolicy { get; }
    }
}
