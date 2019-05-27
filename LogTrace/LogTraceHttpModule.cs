using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace LogTrace
{
    /// <summary>
    /// 
    /// </summary>
    public class LogTraceHttpModule : IHttpModule
    {
        /// <summary>
        /// 
        /// </summary>
        /// <see cref="http://somenewkid.blogspot.com/2006/03/trouble-with-modules.html"/>
        /// <param name="app"></param>
        public void Init(HttpApplication app)
        {
            app.BeginRequest += Application_BeginRequest;
            app.PreSendRequestContent += Application_PreSendRequestContent;
            app.Error += App_Error;
        }

        private void App_Error(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender)?.Context;
            if (ctx == null) { Trace.Flush(); return; }
            Trace.WriteLine(ctx.Error, "HttpUnhandledException");
            Trace.WriteLine(ctx.Error.InnerException, "InnerException");
        }

        private void Application_PreSendRequestContent(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender)?.Context;
            var stopwatch = ctx?.Items["TRACE_STOPWATCH"] as Stopwatch;
            if (stopwatch == null) return;
            stopwatch.Stop();
            ctx.Items["TRACE_STOPWATCH"] = null;
            Trace.WriteLine(stopwatch.Elapsed.TotalMilliseconds + " ms", "WebApi Timing");
            Trace.WriteLine("Request End", "LogEnd");
            Trace.Flush();
        }




        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var ctx = ((HttpApplication)sender)?.Context;
            if (ctx == null) return;
            ctx.Items["TRACE_STOPWATCH"] = Stopwatch.StartNew();
            Trace.WriteLine("BeginRequest", "LogStart");
            Trace.WriteLine(Dns.GetHostName(), "*HostName*");
            var hostAddress = string.Join(",",
                Dns.GetHostAddresses(Dns.GetHostName()).Select(it => it.ToString()).Where(it => it.Contains(".")));
            Trace.WriteLine(hostAddress, "*HostAddresses*");
            Trace.WriteLine(hostAddress, "RealIP");
            var uidCookie = ctx.Request.Cookies["log-uid"];
            if (uidCookie == null || string.IsNullOrEmpty(uidCookie.Value))
            {
                uidCookie = new HttpCookie("log-uid", Guid.NewGuid().ToString("N"));
                ctx.Response.Cookies.Add(uidCookie);
            }
            Trace.WriteLine(uidCookie.Value, "LogUid");
            Trace.WriteLine(ctx.Request.Url.ToString(), "*Url*");
            Trace.WriteLine(ctx.Request.ContentType, "ContentType");
            Trace.WriteLine(ctx.Request.UrlReferrer == null ? "" : ctx.Request.UrlReferrer.AbsoluteUri, "UrlReferrer");
            Trace.WriteLine(ctx.Request.UserAgent, "UserAgent");
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {

        }
    }
}
