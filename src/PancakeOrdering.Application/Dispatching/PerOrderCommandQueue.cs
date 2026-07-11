using System.Threading.Channels;
using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application.Dispatching
{
    internal sealed class PerOrderCommandQueue
    {
        private readonly Channel<IQueuedOperation> _channel;
        private readonly Task _consumerTask;

        public PerOrderCommandQueue()
        {
            _channel = Channel.CreateUnbounded<IQueuedOperation>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                });

            _consumerTask = ProcessAsync();
        }

        internal Task<Result> EnqueueAsync(Func<Task<Result>> operation)
        {
            var queuedOperation = new QueuedOperation<Result>(
                operation,
                () => Result.Failure(ErrorCode.InternalError));

            if (!_channel.Writer.TryWrite(queuedOperation))
                return Task.FromResult(Result.Failure(ErrorCode.InternalError));

            return queuedOperation.Task;
        }

        internal Task<Result<T>> EnqueueAsync<T>(Func<Task<Result<T>>> operation)
        {
            var queuedOperation = new QueuedOperation<Result<T>>(
                operation,
                () => Result.Failure<T>(ErrorCode.InternalError));

            if (!_channel.Writer.TryWrite(queuedOperation))
                return Task.FromResult(Result.Failure<T>(ErrorCode.InternalError));

            return queuedOperation.Task;
        }

        private async Task ProcessAsync()
        {
            await foreach (var operation in _channel.Reader.ReadAllAsync())
            {
                await operation.ExecuteAsync();
            }
        }

        private interface IQueuedOperation
        {
            Task ExecuteAsync();
        }

        private sealed class QueuedOperation<TResult> : IQueuedOperation
            where TResult : Result
        {
            private readonly Func<Task<TResult>> _operation;
            private readonly Func<TResult> _internalErrorFactory;
            private readonly TaskCompletionSource<TResult> _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public QueuedOperation(
                Func<Task<TResult>> operation,
                Func<TResult> internalErrorFactory)
            {
                _operation = operation;
                _internalErrorFactory = internalErrorFactory;
            }

            public Task<TResult> Task => _completionSource.Task;

            public async Task ExecuteAsync()
            {
                try
                {
                    var result = await _operation();
                    _completionSource.SetResult(result);
                }
                catch
                {
                    _completionSource.SetResult(_internalErrorFactory());
                }
            }
        }
    }
}
