using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LogTracer.Core
{
    /// <summary>
    /// 用于记录异常的静态方法
    /// </summary>
    public static class TraceSourceExtensions
    {
        private const string InternalSourceName = "logging_tracer_internal_tracesource";
        private const string InternalLoggerListenerName = "tracing_internal_logs";
        /// <summary>
        /// 用于本地日志输出的日志跟踪器单例
        /// </summary>
        public static TraceSource InternalTraceSource { get; } = InitSource();

        /// <summary>
        /// 初始化日志跟踪器
        /// </summary>
        /// <returns></returns>
        private static TraceSource InitSource()
        {
            var source = new TraceSource(InternalSourceName,SourceLevels.Error);
            source.Switch.ShouldTrace(TraceEventType.Verbose);//主动调用一次,以免出现死锁
            if (source.Listeners.Count != 1 || !(source.Listeners[0] is DefaultTraceListener)) return source;
            source.Listeners.Clear();
            source.Listeners.Add(new FileTraceListener() { Name = InternalLoggerListenerName });
            return source;
        }

        /// <summary>
        /// 输出异常信息
        /// </summary>
        public static void Error(this TraceSource source, Exception ex, string title = null,
            [CallerMemberName] string member = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Error, title, ex.Message, ex, member, line, file);

        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="title"> </param>
        /// <param name="message"> </param>
        /// <param name="member"> </param>
        /// <param name="line"> </param>
        /// <param name="file"> </param>
        public static void Log(this TraceSource source, TraceEventType type, string message, string title = null, object data = null,
            [CallerMemberName] string member = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null)
        {
            if ((source == null) || (source.Switch.ShouldTrace(type) == false))
            {
                return;
            }
            try
            {
                source.TraceData(type, 1, new LogItem(type)
                {
                    Category = title,
                    Message = message,
                    Source = source.Name,
                    Time = DateTime.Now,
                    File = file,
                    Method = member,
                    LineNumber = line,
                    TraceEventID = 1,
                    Content = data,
                });
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="title"> </param>
        /// <param name="message"> </param>
        /// <param name="member"> </param>
        /// <param name="line"> </param>
        /// <param name="file"> </param>
        public static void Log(this TraceSource source, string message, string title = null, object data = null,
                [CallerMemberName] string member = null, [CallerLineNumber] int line = 0,
                [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Verbose, message, title, data, member, line, file);

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="title"> </param>
        /// <param name="message"> </param>
        /// <param name="member"> </param>
        /// <param name="line"> </param>
        /// <param name="file"> </param>
        public static void Warn(this TraceSource source, string message, string title = null, object data = null,
                [CallerMemberName] string member = null, [CallerLineNumber] int line = 0,
                [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Warning, message, title, data, member, line, file);

        /// <summary>
        /// 输出提示信息
        /// </summary>
        /// <param name="title"> </param>
        /// <param name="message"> </param>
        /// <param name="member"> </param>
        /// <param name="line"> </param>
        /// <param name="file"> </param>
        public static void Info(this TraceSource source, string message, string title = null, object data = null,
                [CallerMemberName] string member = null, [CallerLineNumber] int line = 0,
                [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Information, message, title, data, member, line, file);


        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="title"> </param>
        /// <param name="message"> </param>
        /// <param name="member"> </param>
        /// <param name="line"> </param>
        /// <param name="file"> </param>
        [Conditional("DEBUG")]
        public static void Debug(this TraceSource source, TraceEventType type, string message, string title = null, object data = null,
            [CallerMemberName] string member = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null)
            => Log(source, type, message, title, data, member, line, file);


       
        /// <summary>
        /// 进入方法
        /// </summary>
        public static void Entry(this TraceSource source, [CallerMemberName] string member = null, object data = null,
            [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Start, null, $"进入方法 {member}", data, member, line, file);

        /// <summary>
        /// 离开方法并有一个返回值
        /// </summary>
        public static void Return(this TraceSource source, string @return, [CallerMemberName] string member = null, object data = null,
            [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Stop, $"return {@return}", $"离开方法 {member}", data, member, line, file);

        /// <summary>
        /// 离开方法
        /// </summary>
        public static void Exit(this TraceSource source, [CallerMemberName] string member = null, object data = null,
            [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
            => Log(source, TraceEventType.Stop, null, $"离开方法 {member}", data, member, line, file);

        /// <summary>
        /// 刷新日志
        /// </summary>
        /// <param name="source"></param>
        public static void FlushAll(this TraceSource source)
        {
            if ((source == null) || (source.Switch.Level == SourceLevels.Off))
            {
                return;
            }
            try
            {
                source.Flush();
            }
            catch// (Exception ex)
            {
                // ignored
            }
        }
    }
}