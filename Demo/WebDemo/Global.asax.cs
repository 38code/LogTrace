using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebDemo
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }
        public override void Init()
        {
            BeginRequest += Global_BeginRequest;
            PreSendRequestContent += (s, e) => Trace.Flush();
            base.Init();
        }

        private void Global_BeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication) sender;
            Trace.WriteLine(app.Context.Request.Url.ToString(),"*url*");
            Trace.WriteLine(Dns.GetHostName(),"*HostName*");
            Trace.WriteLine(string.Join(",", Dns.GetHostAddresses(Dns.GetHostName()).Select(it => it.ToString()).Where(it => it.Contains("."))), "HostAddresses");
            Trace.WriteLine("127.0.0.1", "RealIP");
           

        }
    }
}