// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TaskExtensions.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskExtensions {
        public static Task WithCancellation(this Task task, CancellationToken cancellationToken) {
            if (task.IsCompleted || cancellationToken == CancellationToken.None) {
                return task;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return TaskRunner(task, cancellationToken);
        }
        private static async Task TaskRunner(Task task, CancellationToken cancellationToken) {
            static void Callback(object? raw) {
                if (raw is TaskCompletionSource<bool> tcs) {
                    tcs.TrySetResult(true);
                }
            }

            var tcs = new TaskCompletionSource<bool>();
            await using (cancellationToken.Register(callback: Callback, state: tcs)) {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false)) {
                    throw new OperationCanceledException(token: cancellationToken);
                }
            }

            await task.ConfigureAwait(false);
        }
    }
}
