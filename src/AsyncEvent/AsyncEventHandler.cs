using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncEvent
{
    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <param name="sender">The object firing the event.</param>
    /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <returns>A task that completes when this handler is done handling the event.</returns>
    public delegate Task AsyncEventHandler(object sender, EventArgs eventArgs);

    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="sender">The object firing the event.</param>
    /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <returns>A task that completes when this handler is done handling the event.</returns>
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs eventArgs)
        where TEventArgs : EventArgs;

    /// <summary>
    /// Provides extension methods for use with <see cref="AsyncEventHandler"/> and
    /// <see cref="AsyncEventHandler{TEventArgs}"/>.
    /// </summary>
    public static class AsyncEventExtensions
    {
        /// <summary>
        /// Asynchronously invokes an event, dispatching the provided event arguments to all registered handlers.
        /// </summary>
        /// <param name="eventHandler">This event handler.</param>
        /// <param name="sender">The object firing the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// A task that completes only when all registered handlers complete. A completed task is returned if the event handler is null.
        /// </returns>
        public static Task InvokeAsync(this AsyncEventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler == null)
            {
                return Task.CompletedTask;
            }

            var delegates = eventHandler.GetInvocationList().Cast<AsyncEventHandler>();
            var tasks = delegates.Select(it => it.Invoke(sender, eventArgs));

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Asynchronously invokes an event, dispatching the provided event arguments to all registered handlers.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">This event handler.</param>
        /// <param name="sender">The object firing the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns>A task that completes only when all registered handlers complete.</returns>
        public static Task InvokeAsync<TEventArgs>(
            this AsyncEventHandler<TEventArgs> eventHandler,
            object sender,
            TEventArgs eventArgs)
            where TEventArgs : EventArgs
        {
            var delegates = eventHandler.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>();
            var tasks = delegates.Select(it => it.Invoke(sender, eventArgs));

            return Task.WhenAll(tasks);
        }
    }
}