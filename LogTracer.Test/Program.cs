using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace LogTracer.Test
{
    class Program
    {
        //优化数据槽
        static void Main(string[] args)
        {

            for (int i = 0; i < 1000; i++)
            {
                Thread t=new Thread(Request);
                t.Start();
            }
            Console.WriteLine("End");
            Console.ReadLine();
        }

        static void Request()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:8001/home.aspx");
            request.Method = WebRequestMethods.Http.Get;
            request.GetResponse();
        }
    }
}
