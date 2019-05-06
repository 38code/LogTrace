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
            Error += Global_Error;
            base.Init();
        }

        #region 日志请求记录
        private void Global_Error(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            Trace.TraceError(app.Context.Error?.InnerException?.Message);
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
            var app = (HttpApplication)sender;
            Trace.WriteLine("Begin Request", "LogTrace");
            Trace.WriteLine(Dns.GetHostName(), "*HostName*");
            var hostAddress = string.Join(",",
                Dns.GetHostAddresses(Dns.GetHostName()).Select(it => it.ToString()).Where(it => it.Contains(".")));
            Trace.WriteLine(hostAddress, "HostAddresses");
            Trace.WriteLine(hostAddress, "RealIP");
            Trace.WriteLine(app.Context.Request.Url.ToString(), "*Url*");
            //var contentType =app.Context.Request.RequestContext.HttpContext..ContentType.Content?.Headers?.ContentType?.MediaType ?? "";
            Trace.WriteLine(app.Context.Request.ContentType, "ContentType");
            Trace.WriteLine(app.Context.Request.UrlReferrer?.AbsoluteUri, "UrlReferrer");
            Trace.WriteLine(app.Context.Request.UserAgent, "UserAgent");





        }

        #endregion
    }
}
