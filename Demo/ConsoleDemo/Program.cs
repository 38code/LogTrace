using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Assembly sss=   Assembly.Load("Logging.Tracer");
            Trace.TraceInformation("asdf");
            Trace.TraceWarning("dddddd");
            Test();
            Trace.Flush();
            Console.WriteLine("End");
            Console.ReadLine();
        }

        static void Test()
        {
            try
            {
                int a = 0;
                float b = 1.29F;
                string e = "a";
                int d = Convert.ToInt32(e);
                int c = (int)b;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }
        }
    }
}
