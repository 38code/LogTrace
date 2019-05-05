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
            Stopwatch st=new Stopwatch();
            st.Start();
            //var context = Thread.CurrentThread.ExecutionContext;
            for (int i = 0; i < 5; i++)
            {
                Trace.TraceInformation("1");
                Trace.TraceInformation("asdf");
                Trace.TraceInformation("2");
                Trace.TraceInformation("3");
                Trace.WriteLine("","");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Trace.Flush();
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
