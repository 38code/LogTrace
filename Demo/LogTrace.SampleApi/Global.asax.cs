using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace LogTrace.SampleApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
        public override void Init()
        {

            BeginRequest += Global_BeginRequest;
            EndRequest += Global_EndRequest;
            Error += (s, e) => {
                Trace.WriteLine(HttpContext.Current?.Error, "HttpUnhandledException");
                Trace.WriteLine(HttpContext.Current?.Error?.InnerException, "InnerException");
            };
            AcquireRequestState += (s, e) => { Trace.WriteLine(HttpContext.Current?.Session?.SessionID, "SessionID"); };
            base.Init();
        }

        private void Global_EndRequest(object sender, EventArgs e)
        {
            _stopwatch?.Stop();
            double timing = _stopwatch?.Elapsed.TotalMilliseconds ?? 0;
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
            var logUid = HttpContext.Current.Request.Cookies["log-uid"]?.Value;
            if (string.IsNullOrEmpty(logUid))
            {
                logUid = Guid.NewGuid().ToString("N");
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("log-uid", logUid));
            }
            Trace.WriteLine(logUid, "LogUid");
            Trace.WriteLine(HttpContext.Current?.Request.Url.ToString(), "*Url*");
            Trace.WriteLine(HttpContext.Current?.Request.ContentType, "ContentType");
            Trace.WriteLine(HttpContext.Current?.Request.UrlReferrer?.AbsoluteUri, "UrlReferrer");
            Trace.WriteLine(HttpContext.Current?.Request.UserAgent, "UserAgent");
        }
    }
}
