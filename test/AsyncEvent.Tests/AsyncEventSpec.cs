using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using static AsyncEvent.Extensions;

namespace AsyncEvent.Tests
{
    public class AsyncEventSpec
    {
        [Fact]
        internal async Task All_Handlers_Should_Finish_Before_Event_Task_Completes()
        {
            var notifier = new NonGenericNotifier();

            IEnumerable<TaskCompletionSource<object>> CreateAndSubscribeHandlers()
            {
                while (true)
                {
                    var tcs = new TaskCompletionSource<object>();

                    Task RespondToSomethingHappening(object sender, EventArgs eventArgs) => tcs.Task;

                    notifier.SomethingHappened += RespondToSomethingHappening;

                    yield return tcs;
                }
            }

            // Reversing the order that the handlers signal completion ensures that we test that the event task
            // completes only when _all_ handlers finish, and not just the last one subscribed.
            var tcsArray = CreateAndSubscribeHandlers().Take(5).Reverse().ToArray();
            var eventTask = notifier.OnSomethingHappening();

            foreach (var tcs in tcsArray.Reverse())
            {
                await Task.Delay(10);
                eventTask.IsCompleted.ShouldBeFalse();
                tcs.SetResult(null);
            }

            await eventTask;
        }

        [Fact]
        internal async Task Exceptions_That_Occur_During_Event_Handling_Should_Be_Propagated()
        {
            var notifier = new NonGenericNotifier();

            Task FaultyHandler(object sender, EventArgs eventArgs) => throw new InvalidOperationException();

            notifier.SomethingHappened += FaultyHandler;

            await Should.ThrowAsync<InvalidOperationException>(async () => await notifier.OnSomethingHappening());
        }

        [Fact]
        internal async Task Generic_Async_Event_Should_Run_Asynchronously()
        {
            var notifier = new GenericNotifier();
            var tcs = new TaskCompletionSource<object>();
            int? value = null;

            async Task RespondToSomethingHappening(object sender, ExampleEventArgs eventArgs)
            {
                await tcs.Task;
                value = eventArgs.Value;
            }

            notifier.SomethingHappened += RespondToSomethingHappening;

            var eventTask = notifier.OnSomethingHappening(2);

            await Task.Delay(100);
            value.ShouldBeNull();
            tcs.SetResult(null);

            await eventTask;
            value.ShouldBe(2);
        }

        [Fact]
        internal void Non_Generic_Invoke_Async_Should_Return_Completed_Task_If_No_Handlers_Are_Subscribed()
        {
            var notifier = new NonGenericNotifier();

            notifier.OnSomethingHappening().IsCompleted.ShouldBeTrue();
        }

        [Fact]
        internal void Generic_Invoke_Async_Should_Return_Completed_Task_If_No_Handlers_Are_Subscribed()
        {
            var notifier = new GenericNotifier();

            notifier.OnSomethingHappening(3).IsCompleted.ShouldBeTrue();
        }

        [Fact]
        internal async Task Non_Generic_Async_Event_Should_Run_Asynchronously()
        {
            var notifier = new NonGenericNotifier();
            var tcs = new TaskCompletionSource<object>();
            var eventFired = false;

            async Task RespondToSomethingHappening(object sender, EventArgs eventArgs)
            {
                await tcs.Task;
                eventFired = true;
            }

            notifier.SomethingHappened += RespondToSomethingHappening;

            var eventTask = notifier.OnSomethingHappening();

            await Task.Delay(100);
            eventFired.ShouldBeFalse();
            tcs.SetResult(null);

            await eventTask;
            eventFired.ShouldBeTrue();
        }

        [Fact]
        internal async Task Non_Generic_Synchronous_Event_Handlers_Should_Be_Convertible_To_Equivalent_Asynchronous_Handlers()
        {
            var notifier = new NonGenericNotifier();
            var eventFired = false;

            notifier.SomethingHappened += Async((sender, args) => eventFired = true);

            await notifier.OnSomethingHappening();
            eventFired.ShouldBeTrue();
        }

        [Fact]
        internal async Task Generic_Synchronous_Event_Handlers_Should_Be_Convertible_To_Equivalent_Asynchronous_Handlers()
        {
            var notifier = new GenericNotifier();
            int? value = null;

            notifier.SomethingHappened += Async<ExampleEventArgs>((sender, args) => value = args.Value);

            await notifier.OnSomethingHappening(2);
            value.ShouldBe(2);
        }

        private class ExampleEventArgs
        {
            public int Value { get; set; }
        }

        private class GenericNotifier
        {
            public event AsyncEventHandler<ExampleEventArgs> SomethingHappened;

            public Task OnSomethingHappening(int value) =>
                SomethingHappened.InvokeAsync(this, new ExampleEventArgs { Value = value });
        }

        private class NonGenericNotifier
        {
            public event AsyncEventHandler SomethingHappened;

            public Task OnSomethingHappening() => SomethingHappened.InvokeAsync(this, EventArgs.Empty);
        }
    }
}
