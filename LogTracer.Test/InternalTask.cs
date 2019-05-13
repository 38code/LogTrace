using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogTracer.Test
{
    public class InternalTask
    {

        private static SemaphoreSlim slim = new SemaphoreSlim(1);

        public Action<CancellationTokenSource> Task;

        public void Run(CancellationTokenSource source)
        {
            slim.Wait()
            Task.Invoke(source);
        }
    }
}
