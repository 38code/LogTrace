using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace LogTrace.SampleWeb
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }
        
        public override void Init()
        {
             
            BeginRequest += Global_BeginRequest;
            EndRequest += Global_EndRequest;
            base.Init();
        }

        private void Global_EndRequest(object sender, EventArgs e)
        {
            _stopwatch.Stop();
            double timing = _stopwatch.Elapsed.TotalMilliseconds;
            if (timing > 2000)
            {
                Trace.TraceWarning("API用时过长");
            }
            Trace.WriteLine(timing + " ms", "WebApi Timing");
            Trace.WriteLine("Request End", "LogTrace");
            Trace.Flush();
        }

        [ThreadStatic]
        private static Stopwatch _stopwatch;

        private void Global_BeginRequest(object sender, EventArgs e)
        {
            
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            var app = (HttpApplication)sender;
            Trace.WriteLine("Begin Request", "LogTrace");
            Trace.WriteLine(Dns.GetHostName(), "*HostName*");
            var hostAddress = string.Join(",",
                Dns.GetHostAddresses(Dns.GetHostName()).Select(it => it.ToString()).Where(it => it.Contains(".")));
            Trace.WriteLine(hostAddress, "HostAddresses");
            Trace.WriteLine(hostAddress, "RealIP");
            Trace.WriteLine(app.Context.Request.Url.ToString(), "*Url*");
            Trace.WriteLine(app.Context.Request.ContentType, "ContentType");
            Trace.WriteLine(app.Context.Request.UrlReferrer?.AbsoluteUri, "UrlReferrer");
            Trace.WriteLine(app.Context.Request.UserAgent, "UserAgent");
        }
    }
}