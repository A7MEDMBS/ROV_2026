using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ROV_GUI_Control.VisionTask
{
    public static class TaskExtensions
    {
        public static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            using (var timeoutCts = new CancellationTokenSource())
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken))
            {
                var delayTask = Task.Delay(timeout, timeoutCts.Token);
                var completedTask = await Task.WhenAny(task, delayTask);

                if (completedTask == task)
                {
                    timeoutCts.Cancel(); // Cancel the delay task
                    await task; // Re-throw any exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            using (var timeoutCts = new CancellationTokenSource())
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken))
            {
                var delayTask = Task.Delay(timeout, timeoutCts.Token);
                var completedTask = await Task.WhenAny(task, delayTask);

                if (completedTask == task)
                {
                    timeoutCts.Cancel(); // Cancel the delay task
                    return await task; // Re-throw any exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}
