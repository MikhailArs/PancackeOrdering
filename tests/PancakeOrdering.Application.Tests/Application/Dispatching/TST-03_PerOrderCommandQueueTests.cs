using System.Collections.Concurrent;
using PancakeOrdering.Application.Dispatching;
using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Application.Tests.Application.Dispatching
{
    [TestFixture]
    [Property("TestSuiteId", "TST-03")]
    public sealed class PerOrderCommandQueueTests
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(3);

        [Test]
        [Property("TestId", "TST-03.01")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-6.1")]
        [Property("Design", "SDD-6.3")]
        [Property("Design", "SDD-7.2.2")]
        public async Task EnqueueAsync_ExecutesOperationsInFifoOrder()
        {
            var queue = new PerOrderCommandQueue();
            var firstStarted = NewCompletionSource<bool>();
            var secondStarted = NewCompletionSource<bool>();
            var thirdStarted = NewCompletionSource<bool>();
            var releaseFirst = NewCompletionSource<bool>();
            var starts = new ConcurrentQueue<int>();
            var completions = new ConcurrentQueue<int>();

            var firstTask = queue.EnqueueAsync(async () =>
            {
                starts.Enqueue(1);
                firstStarted.SetResult(true);
                await releaseFirst.Task;
                completions.Enqueue(1);
                return Result.Success(1);
            });

            var secondTask = queue.EnqueueAsync(() =>
            {
                starts.Enqueue(2);
                secondStarted.SetResult(true);
                completions.Enqueue(2);
                return Task.FromResult(Result.Success(2));
            });

            var thirdTask = queue.EnqueueAsync(() =>
            {
                starts.Enqueue(3);
                thirdStarted.SetResult(true);
                completions.Enqueue(3);
                return Task.FromResult(Result.Success(3));
            });

            await firstStarted.Task.WaitAsync(TestTimeout);
            await Task.Yield();

            Assert.That(secondStarted.Task.IsCompleted, Is.False);
            Assert.That(thirdStarted.Task.IsCompleted, Is.False);

            releaseFirst.SetResult(true);

            var results = await Task.WhenAll(firstTask, secondTask, thirdTask).WaitAsync(TestTimeout);

            Assert.That(results.Select(result => result.IsSuccess), Is.All.True);
            Assert.That(results.Select(result => result.Value), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(starts.ToArray(), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(completions.ToArray(), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        [Property("TestId", "TST-03.02")]
        [Property("Requirement", "NFR-4")]
        [Property("Requirement", "NFR-5")]
        [Property("Design", "SDD-4.6")]
        [Property("Design", "SDD-6.1")]
        [Property("Design", "SDD-6.3")]
        public async Task EnqueueAsync_ExceptionDuringFirstTask_ReturnsInternalErrorAndSecondContinues()
        {
            var queue = new PerOrderCommandQueue();
            var secondStarted = NewCompletionSource<bool>();

            var firstTask = queue.EnqueueAsync(() =>
                Task.FromException<Result>(new InvalidOperationException()));

            var secondTask = queue.EnqueueAsync(() =>
            {
                secondStarted.SetResult(true);
                return Task.FromResult(Result.Success());
            });

            var firstResult = await firstTask.WaitAsync(TestTimeout);
            var secondResult = await secondTask.WaitAsync(TestTimeout);

            Assert.That(firstResult.IsSuccess, Is.False);
            Assert.That(firstResult.Error, Is.EqualTo(ErrorCode.InternalError));
            Assert.That(secondStarted.Task.IsCompleted, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);
        }

        private static TaskCompletionSource<T> NewCompletionSource<T>() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
