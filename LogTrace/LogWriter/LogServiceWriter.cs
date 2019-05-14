using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogTrace.Core;

namespace LogTrace.LogWriter
{
    /// <summary>
    /// 用于以SimpleLogService的格式写入日志到文件
    /// </summary>
    internal sealed class LogServiceWriter : FileLogWriter, IFlushAsync
    {
        #region Public Constructors

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="dir"> 文件输出路径 </param>
        /// <param name="writeLevel"> 写入日志的等级 </param>
        public LogServiceWriter(string dir, SourceLevels writeLevel)
        {
            _writeLevel = writeLevel;
            _cache = new MemoryCache(Guid.NewGuid().ToString());
            DirectoryPath = dir;
            _queue = new ConcurrentQueue<List<LogItem>>();
            FileMaxSize = DefaultFileMaxSize;
            BatchMaxWait = TimeSpan.FromSeconds(5);
        }

        #endregion Public Constructors
        
        static class Utf8Bytes
        {
            public static byte[] Assembly { get; } = Encoding.UTF8.GetBytes("Assembly : ");
            public static byte[] Comma { get; } = Encoding.UTF8.GetBytes("%2C");
            public static byte[] Star { get; } = Encoding.UTF8.GetBytes("*");
            public static byte[] Data { get; } = Encoding.UTF8.GetBytes("Data : ");
            public static byte[] Detail { get; } = Encoding.UTF8.GetBytes("Detail : ");
            public static byte[] Method { get; } = Encoding.UTF8.GetBytes("Method : ");
            public static byte[] Newline { get; } = Encoding.UTF8.GetBytes("%0D%0A");
            public static byte[] Newline2 { get; } = Encoding.UTF8.GetBytes("%250D%250A");
            public static byte[] Null { get; } = Encoding.UTF8.GetBytes("<null>");
            public static byte[] Plus { get; } = Encoding.UTF8.GetBytes(" + ");
            public static byte Space { get; } = Encoding.UTF8.GetBytes(" ")[0];
            private static readonly byte[] Number = Encoding.UTF8.GetBytes("0123456789");
            public static byte NumberToByte(int i) => Number[i];
            private static readonly HashSet<byte> InvalidChar = new HashSet<byte>(Encoding.UTF8.GetBytes("\r\n,\"\'"));
            public static bool IsInvalidChars(byte b) => InvalidChar.Contains(b);
        }

        #region Private Fields

        //单个文件容量阈值
        private const long DefaultFileMaxSize = 5*1024*1024; //兆

        /// <summary>
        /// 需要转义的字符
        /// </summary>
        private static readonly char[] ReplaceChars = { '\n', '\r', '%', '"', ',', '\0' };

        private readonly SourceLevels _writeLevel;

        /// <summary>
        /// 队列
        /// </summary>
        private readonly ConcurrentQueue<List<LogItem>> _queue;

        /// <summary>
        /// 缓存
        /// </summary>
        private MemoryCache _cache;

        /// <summary>
        /// 异步刷新内容到文件的任务
        /// </summary>
        private Task _flushTask;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// 写入器名称
        /// </summary>
        public override string Name => nameof(LogServiceWriter);

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 追加日志
        /// </summary>
        /// <param name="item"> </param>
        public override void Append(LogItem item)
        {
            var key = item.LogGroupID.ToString("n"); //根据日志id从缓存中获取其他日志信息
            var list = _cache[key] as List<LogItem>;

            if (list == null)
            {
                if (item.IsLast)
                {
                    return;
                }
                list = new List<LogItem>();
                _cache.Add(key, list, new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(90), //90秒超时后日志将被输出
                    RemovedCallback = RemovedCallback
                });
            }

            if (item.IsFirst && (list.Count > 0)) //日志标识为第一条,但缓存中已有日志
            {
                if (list[0].IsFirst == false) //如果缓存中的日志第一条不是日志头
                {
                    list.Insert(0, item);
                }
            }
            else if (item.IsLast)
            {
                if (list[0].IsFirst)
                {
                    list[0] = item; //替换日志头
                }
                else
                {
                    list.Insert(0, item);
                }
                _cache.Remove(key);
            }
            else
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        /// <filterpriority> 2 </filterpriority>
        public override void Dispose()
        {
            var cache = Interlocked.Exchange(ref _cache, null);
            cache?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <exception cref="IOException"> 发生了 I/O 错误。- 或 -另一个线程可能已导致操作系统的文件句柄位置发生意外更改。 </exception>
        /// <exception cref="ObjectDisposedException"> 流已关闭。 </exception>
        /// <exception cref="NotSupportedException"> 当前流实例不支持写入。 </exception>
        public override void Flush()
        {
            while (_queue.TryDequeue(out var logs))
            {
                if ((logs == null) || (logs.Count == 0))
                {
                    return;
                }
                var log = logs[0];
                if (((int) _writeLevel & (int) log.Level) == 0)
                {
                    continue;
                }

                ChangeFileIfFull();
                AppendLine();
                Append(log.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                AppendComma();
                Append(log.LogGroupID.ToString("n"));
                AppendComma();
                WriteLevel(log.Level);
                AppendComma();
                Append(log.Listener.Name);
                AppendComma();
                for (int i = 1, length = logs.Count; i < length; i++)
                {
                    log = logs[i];
                    var message = log.Message;
                    base.Append(log.Time.ToString("HH:mm:ss.fff"));
                    base.Append(Utf8Bytes.Comma);
                    WriteLevel(log.Level);
                    base.Append(Utf8Bytes.Comma);
                    if (log.Level > TraceEventType.Warning || string.IsNullOrEmpty(log.Category))
                    {
                        base.Append(DoubleDecode(log.Category ?? log.Source)); //没有分类时,显示来源
                    }
                    else if (log.Category[0] == '*' && log.Category[log.Category.Length - 1] == '*')
                    {
                        base.Append(DoubleDecode(log.Category)); //没有分类时,显示来源
                    }
                    else
                    {
                        base.Append(Utf8Bytes.Star);
                        base.Append(DoubleDecode(log.Category)); //没有分类时,显示来源
                        base.Append(Utf8Bytes.Star);
                    }
                    base.Append(Utf8Bytes.Comma);
                    base.Append(DoubleDecode(message ?? "无"));
                    base.Append(Utf8Bytes.Comma);
                    WriteCallStack(log);
                    base.Append(Utf8Bytes.Comma);
                    WriteContent(log.Content);
                    base.Append(Utf8Bytes.Newline);
                }
                AppendComma();
                //追加Category索引
                for (int i = 1, length = logs.Count; i < length; i++)
                {
                    log = logs[i];
                    var message = log.Message;
                    if (string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(log.Category)
                        ||string.IsNullOrWhiteSpace(log.Category))
                    {
                        continue;
                    }
                    var categoryBytes = Encoding.UTF8.GetBytes(log.Category.Replace("*",""));
                    Append(categoryBytes);
                    Append(Utf8Bytes.Comma);
                    var bytes = Encoding.UTF8.GetBytes(message);
                    for (int j = 0, l = bytes.Length; j < l; j++)
                    {
                        if (Utf8Bytes.IsInvalidChars(bytes[j]))
                        {
                            bytes[j] = Utf8Bytes.Space;
                        }
                    }
                    Append(bytes);
                    Append(Utf8Bytes.Newline);
                }
            }
            base.Flush();
        }

        private void WriteCallStack(LogItem log)
        {
            if (log.File != null)
            {
                base.Append(DoubleDecode(log.File));
                base.Append("%252C"); //逗号
                base.Append(DoubleDecode(log.Message));
                base.Append(":");
                base.Append(log.LineNumber.ToString());
            }
            if (log.Callstack != null)
            {
                if (log.File!=null)
                {
                    base.Append(Utf8Bytes.Newline2);
                }
                base.Append(DoubleDecode(log.Callstack));
            }
        }
        

        /// <summary>
        /// 异步刷新
        /// </summary>
        /// <param name="token"> </param>
        /// <returns> </returns>
        public async Task FlushAsync(CancellationToken token)
        {
            if (_flushTask != null)
            {
                await _flushTask;
            }
            _flushTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    Flush();
                }
                catch
                {
                    
                }
            }, token);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 二次转义
        /// </summary>
        /// <param name="text"> </param>
        /// <returns> </returns>
        private static string DoubleDecode(string text)
        {
            if (text == null)
            {
                return "";
            }
            if (text.IndexOfAny(ReplaceChars) >= 0)
            {
                return text
                    .Replace("%", "%2525") //2次转义
                    .Replace(",", "%252C")
                    .Replace("\n", "%250A")
                    .Replace("\r", "%250D")
                    .Replace("\0", "%2500")
                    .Replace("\"", "%2522");
            }
            return text;
        }

        /// <summary>
        /// 缓存被移除事件
        /// </summary>
        /// <param name="arguments"> </param>
        private void RemovedCallback(CacheEntryRemovedArguments arguments)
        {
            var list = arguments?.CacheItem?.Value as List<LogItem>;
            if (list != null)
            {
                _queue.Enqueue(list);
            }
        }

        /// <summary>
        /// 写入日志正文
        /// </summary>
        /// <param name="content"> </param>
        private void WriteContent(object content)
        {
            if (content == null)
            {
                return;
            }
            var ex = content as Exception;
            if (ex != null)
            {
                base.Append(Utf8Bytes.Assembly);
                base.Append(Utf8Bytes.Newline2);
                base.Append(DoubleDecode(ex.GetType().AssemblyQualifiedName));
                if (ex.TargetSite != null)
                {
                    base.Append(Utf8Bytes.Method);
                    base.Append(Utf8Bytes.Newline2);
                    base.Append(DoubleDecode(ex.TargetSite.ReflectedType?.ToString()));
                    base.Append(Utf8Bytes.Plus);
                    base.Append(Utf8Bytes.Newline2);
                    base.Append(DoubleDecode(ex.TargetSite.ToString()));
                }
                base.Append(Utf8Bytes.Detail);
                base.Append(Utf8Bytes.Newline2);
                base.Append(DoubleDecode(ex.ToString()));
                if (ex.Data.Count == 0)
                {
                    return;
                }
                base.Append(Utf8Bytes.Data);
                base.Append(Utf8Bytes.Newline2);
                foreach (DictionaryEntry item in ex.Data)
                {
                    var value = item.Value;
                    Append(DoubleDecode(item.Key?.ToString()));
                    AppendWhiteSpace();
                    AppendColon();
                    AppendWhiteSpace();
                    if (value == null)
                    {
                        Append(Utf8Bytes.Null);
                    }
                    else
                    {
                        Append(DoubleDecode(value.ToString()));
                    }
                    Append(Utf8Bytes.Newline2);
                }
            }
            var ee = (content as IEnumerable)?.GetEnumerator()
                     ?? content as IEnumerator;
            if (ee != null)
            {

                Append(Utf8Bytes.Assembly);
                AppendColon();
                Append(DoubleDecode(ee.GetType().AssemblyQualifiedName));
                var i = 0;
                while (ee.MoveNext())
                {
                    Append(Utf8Bytes.Newline2);
                    Append(i.ToString());
                    AppendColon();
                    AppendWhiteSpace();
                    Append(DoubleDecode(ee.Current.ToString()));
                    i++;
                }
                return;
            }
            Append(DoubleDecode(content.ToString()));
            return;
            
        }

        private void WriteLevel(TraceEventType logLevel)
        {
            switch (logLevel)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    AppendByte(Utf8Bytes.NumberToByte(1));
                    break;

                case TraceEventType.Warning:
                    AppendByte(Utf8Bytes.NumberToByte(2));
                    break;

                case TraceEventType.Information:
                    AppendByte(Utf8Bytes.NumberToByte(3));
                    break;

                case TraceEventType.Verbose:
                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Resume:
                case TraceEventType.Transfer:
                default:
                    AppendByte(Utf8Bytes.NumberToByte(4));
                    break;
            }
        }

        #endregion Private Methods
    }
}