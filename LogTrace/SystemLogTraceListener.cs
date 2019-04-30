using LogTracer.Core;
using LogTracer.LogWriter;

/// <summary>
/// 将日志写入系统事件的侦听器
/// </summary>
// ReSharper disable once CheckNamespace
public sealed class SystemLogTraceListener : TraceListenerBase
{
    /// <summary>
    /// 以线程为单位记录和输出日志 构造函数
    /// </summary>
    public SystemLogTraceListener() : base(true, null)
    {
    }

    /// <summary>
    /// 创建一个队列
    /// </summary>
    /// <returns> </returns>
    protected override WriteQueue CreateQueue()
    {
        var writer = new SystemLogWriter("Logging.Tracer");
        writer.Initialize(this);
        return new WriteQueue(writer, int.MaxValue);
    }
}
