using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LogTracer.Core
{
    /// <summary>
    /// 内部定时任务
    /// </summary>
    internal sealed class BackgroundTask
    {
        /// <summary>
        /// 健康检查执行时间间隔(秒)
        /// </summary>
        private readonly int _checkInterval;

        /// <summary>
        /// 任务的最后执行时间
        /// </summary>
        private DateTime _lastRunTime;

        /// <summary>
        /// 任务标识
        /// </summary>
        private ActivityTokenSource _taskToken;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger"> 日志跟踪器 </param>
        /// <param name="checkInterval"> 健康检查间隔时间,单位秒(默认30) </param>
        public BackgroundTask(TraceSource logger, int checkInterval = 30)
        {
            Logger = logger;
            Logger?.Entry();
            _checkInterval = checkInterval;
            _lastRunTime = DateTime.MinValue;
            Logger?.Exit();
        }

        /// <summary>
        /// 日志跟踪器
        /// </summary>
        public TraceSource Logger { get; }

        /// <summary>
        /// 判断任务是否正在执行
        /// </summary>
        public bool IsRunning
        {
            get
            {
                Logger?.Entry();
                var b = _taskToken?.CancelIfTimeout(); //如果超时,取消任务
                if (b == null)
                {
                    Logger?.Return("false");
                    return false;
                }
                var interval = (DateTime.Now - _lastRunTime).TotalSeconds;
                if (interval < _checkInterval)
                {
                    Logger?.Return(b.Value.ToString());
                    return b.Value;
                }
                //如果最后执行时间大于强制检查时间,则强制同步多线程字段
                _lastRunTime = DateTime.Now;
                Thread.MemoryBarrier();
                var value = _taskToken?.CancelIfTimeout() ?? false;
                Logger?.Return(value.ToString());
                return value;
            }
        }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 如果任务没有在执行则执行任务
        /// </summary>
        public void RunIfStop()
        {
            Logger?.Entry();
            if (IsRunning)
            {
                Logger?.Exit();
                return;
            }

            var token = new ActivityTokenSource(Timeout); //新建一个任务标识,10秒无响应则取消任务
            //任务标识,如果更新失败,说明其他线程已经更新了,当前线程主动退出
            if (Interlocked.CompareExchange(ref _taskToken, token, null) != null)
            {
                Logger?.Exit();
                return;
            }
            SynchronizationContext.SetSynchronizationContext(null);
            Task.Factory.StartNew(()=>Run(token)).ConfigureAwait(false);
            //Run(token).ConfigureAwait(false);
            Logger?.Exit();
        }

        public Func<ActivityTokenSource,Task> OnRun;

        /// <summary>
        /// 执行任务,并在任务退出后更新任务标识
        /// </summary>
        /// <param name="token"> </param>
        private async Task Run(ActivityTokenSource token)
        {
            Logger?.Entry();
            try
            {
                var task = OnRun?.Invoke(token);
                if (task == null) return;
                _lastRunTime = DateTime.Now;
                await task;
                if (task.Exception != null)
                {
                    Logger?.Error(task.Exception, nameof(BackgroundTask));
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, nameof(BackgroundTask));
            }
            finally
            {
                Interlocked.CompareExchange(ref _taskToken, null, token);
                Logger?.Exit();
            }
        }
    }
}