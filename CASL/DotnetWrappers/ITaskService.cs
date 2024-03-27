// <copyright file="ITaskService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.DotnetWrappers;

using System;
using System.Threading.Tasks;

/// <summary>
/// Creates a new task for asynchronous operations to be performed.
/// </summary>
internal interface ITaskService : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether or not the task has ran to completion.
    /// </summary>
    bool IsCompletedSuccessfully { get; }

    /// <summary>Gets a value indicating whether cancellation has been requested for this <see cref="ITaskService" />.</summary>
    /// <value>Whether cancellation has been requested for this <see cref="ITaskService" />.</value>
    /// <remarks>
    /// <para>
    /// This property indicates whether cancellation has been requested for this task service, such as
    /// due to a call to its <see cref="Cancel()"/> method.
    /// </para>
    /// <para>
    /// If this property returns true, it only guarantees that cancellation has been requested. It does not
    /// guarantee that every handler registered with the corresponding token has finished executing, nor
    /// that cancellation requests have finished propagating to all registered handlers. Additional
    /// synchronization may be required, particularly in situations where related objects are being
    /// canceled concurrently.
    /// </para>
    /// </remarks>
    bool IsCancellationRequested { get; }

    /// <summary>
    /// Gets a value indicating whether if the task is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Sets the delegate of type <see cref="Action"/> to be executed on another thread
    /// once the <see cref="Start"/>() method has been invoked.
    /// </summary>
    /// <param name="action">The delegate that represents the code to execute in the task.</param>
    void SetAction(Action action);

    /// <summary>
    /// Starts the <see cref="Task"/>, scheduling it for execution to the current <see cref="TaskScheduler"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Occurs if the <see cref="SetAction(Action)"/> method has not been invoked yet.
    /// </exception>
    void Start();

    /// <summary>
    /// Creates a continuation that executes when the target task completes according to the specified <see cref="TaskContinuationOptions"/>.
    /// <para>
    ///     The continuation receives a cancellation token and uses a specified scheduler.
    /// </para>
    /// </summary>
    /// <param name="continuationAction">
    ///     An action to run according to the specified <paramref name="taskContinuationOptions"/>. When run, the
    ///     delegate will be passed the completed task as an argument.
    /// </param>
    /// <param name="taskContinuationOptions">
    ///     Options for when the continuation is scheduled and how it behaves. This includes
    ///     criteria, such as System.Threading.Tasks.TaskContinuationOptions.OnlyOnCanceled,
    ///     as well as execution options, such as System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously.
    /// </param>
    /// <param name="scheduler">
    ///     The System.Threading.Tasks.TaskScheduler to associate with the continuation task
    ///     and to use for its execution.
    /// </param>
    /// <returns>A new continuation System.Threading.Tasks.Task.</returns>
    /// <exception cref="ObjectDisposedException">
    ///     The System.Threading.CancellationTokenSource that created the token has already
    ///     been disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     The continuationAction argument is null. -or- The scheduler argument is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The continuationOptions argument specifies an invalid value for System.Threading.Tasks.TaskContinuationOptions.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Occurs if the <see cref="SetAction(Action)"/> method has not been invoked yet.
    /// </exception>
    Task ContinueWith(Action<Task> continuationAction, TaskContinuationOptions taskContinuationOptions, TaskScheduler scheduler);

    /// <summary>
    /// Cancels the task.
    /// </summary>
    /// <remarks>
    ///     This method will only return once the task has been cancelled.
    /// </remarks>
    void Cancel();
}
