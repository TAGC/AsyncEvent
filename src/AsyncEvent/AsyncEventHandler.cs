﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncEvent
{
    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <param name="sender">The object firing the event.</param>
    /// <param name="eventArgs">The object containing the event data.</param>
    /// <returns>A task that completes when this handler is done handling the event.</returns>
    public delegate Task AsyncEventHandler(object sender, EventArgs eventArgs);

    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="sender">The object firing the event.</param>
    /// <param name="eventArgs">The object containing the event data.</param>
    /// <returns>A task that completes when this handler is done handling the event.</returns>
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs eventArgs);

    /// <summary>
    /// Provides extension methods for use with <see cref="AsyncEventHandler" /> and
    /// <see cref="AsyncEventHandler{TEventArgs}" />, as well as functions to convert synchronous event handlers to
    /// asynchronous event handlers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a synchronous event handler to an asynchronous event handler that performs the same actions and returns
        /// <see cref="Task.CompletedTask" />.
        /// </summary>
        /// <param name="eventHandler">The synchronous event handler.</param>
        /// <returns>An asynchronous event handler that performs the same logic and returns a completed task.</returns>
        public static AsyncEventHandler Async(EventHandler eventHandler)
        {
            return (sender, eventArgs) =>
            {
                eventHandler(sender, eventArgs);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Converts a synchronous event handler to an asynchronous event handler that performs the same actions and returns
        /// <see cref="Task.CompletedTask" />.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The synchronous event handler.</param>
        /// <returns>An asynchronous event handler that performs the same logic and returns a completed task.</returns>
        public static AsyncEventHandler<TEventArgs> Async<TEventArgs>(EventHandler<TEventArgs> eventHandler)
        {
            return (sender, eventArgs) =>
            {
                eventHandler(sender, eventArgs);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Asynchronously invokes an event, dispatching the provided event arguments to all registered handlers.
        /// </summary>
        /// <param name="eventHandler">This event handler.</param>
        /// <param name="sender">The object firing the event.</param>
        /// <param name="eventArgs">The object containing the event data.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes only when all registered handlers complete. A completed task is returned
        /// if the event handler is <c>null</c>.
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
        /// <param name="eventArgs">The object containing the event data.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes only when all registered handlers complete. A completed task is returned
        /// if the event handler is <c>null</c>.
        /// </returns>
        public static Task InvokeAsync<TEventArgs>(
            this AsyncEventHandler<TEventArgs> eventHandler,
            object sender,
            TEventArgs eventArgs)
        {
            if (eventHandler == null)
            {
                return Task.CompletedTask;
            }

            var delegates = eventHandler.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>();
            var tasks = delegates.Select(it => it.Invoke(sender, eventArgs));

            return Task.WhenAll(tasks);
        }
    }
}
