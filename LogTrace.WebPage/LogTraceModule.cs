using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;

namespace LogTrace.WebPage
{
    public class LogTraceModule:IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += Global_BeginRequest;
            context.EndRequest += Global_EndRequest;
            context.Error += (s, e) =>
            {
                Trace.WriteLine(HttpContext.Current.Error, "HttpUnhandledException");
                Trace.WriteLine(HttpContext.Current.Error.InnerException, "InnerException");

            };
            context.Init();
        }
        private void Global_EndRequest(object sender, EventArgs e)
        {
            double timing = 0;
            if (_stopwatch != null && _stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                timing = _stopwatch.Elapsed.TotalMilliseconds;
            }

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
            Trace.WriteLine("Begin Request", "LogTrace");
            Trace.WriteLine(Dns.GetHostName(), "*HostName*");
            var hostAddress = string.Join(",",
                Dns.GetHostAddresses(Dns.GetHostName()).Select(it => it.ToString()).Where(it => it.Contains(".")));
            Trace.WriteLine(hostAddress, "*HostAddresses*");
            Trace.WriteLine(hostAddress, "RealIP");
            var uidCookie = HttpContext.Current.Request.Cookies["log-uid"];
            if (uidCookie == null || string.IsNullOrEmpty(uidCookie.Value))
            {
                uidCookie = new HttpCookie("log-uid", Guid.NewGuid().ToString("N"));
                HttpContext.Current.Response.Cookies.Add(uidCookie);
            }
            Trace.WriteLine(uidCookie.Value, "LogUid");
            Trace.WriteLine(HttpContext.Current.Request.Url.ToString(), "*Url*");
            Trace.WriteLine(HttpContext.Current.Request.ContentType, "ContentType");
            Trace.WriteLine(HttpContext.Current.Request.UrlReferrer == null ? "" : HttpContext.Current.Request.UrlReferrer.AbsoluteUri, "UrlReferrer");
            Trace.WriteLine(HttpContext.Current.Request.UserAgent, "UserAgent");
        }

        public void Dispose()
        {
            
        }
    }
}
