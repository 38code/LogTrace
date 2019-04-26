using System.Threading;
using System.Threading.Tasks;
using LogTracer.Core;

namespace LogTracer.Writer
{
    /// <summary>
    /// 异步追加日志接口
    /// </summary>
    public interface IAppendAsync
    {
        /// <summary>
        /// 异步写入日志
        /// </summary>
        /// <param name="item"> </param>
        /// <param name="token"> </param>
        /// <returns> </returns>
        Task AppendAsync(LogItem item, CancellationToken token);
    }
}