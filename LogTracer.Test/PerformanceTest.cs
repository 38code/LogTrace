using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogTracer.Test
{
   public static class PerformanceTest
    {
        public static void Start()
        {
            CodeTimer.Initialize();
            CodeTimer.Time("峰值测试", 1, () =>
            {
                for (int i = 0; i < 500; i++)
                {
                    var thread = new Thread(() =>
                    {
                        for (int j = 0; j < 1000; j++)
                        {
                            for (int k = 0; k < 100; k++)
                            {
                                Trace.TraceInformation("这是测试数据，差不多就可以了，一般就是50个字符大小，不要随便跑这个测试用例，否则死机别怪我");
                            }
                            Trace.Flush();
                        }
                    });
                    thread.Start();
                }

            });
        }
    }
}
