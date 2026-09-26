using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ROV_GUI_Control.VisionTask
{
    public static class ProcessExtensions
    {
        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken)
        {
            if (process.HasExited)
                return;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(true);

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                if (process.HasExited)
                    return;

                await tcs.Task; // will complete on exit or cancellation
            }
        }


        public static Task WaitForExitAsync(this Process process)
        {
            if (process.HasExited)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(true);

            if (process.HasExited)
                return Task.CompletedTask;

            return tcs.Task;
        }
    }
}
