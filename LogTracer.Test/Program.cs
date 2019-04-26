using System;
using System.Diagnostics;
using System.Threading;

namespace LogTracer.Test
{
    class Program
    {
        //优化数据槽
        static void Main(string[] args)
        {
            var context = Thread.CurrentThread.ExecutionContext;
            /*for (int i = 0; i < 5; i++)
            {
                Trace.TraceInformation("1");
                Trace.TraceInformation("2");
                Trace.TraceInformation("3");
                Trace.Flush();
            }*/
           
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
