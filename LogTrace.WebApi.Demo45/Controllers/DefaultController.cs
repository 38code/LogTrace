using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LogTrace.WebApi.Demo45.Controllers
{
    public class DefaultController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Log()
        {
            Trace.TraceInformation("button1_click");
            return Json("log api result");
        }
        [HttpGet]
        public IHttpActionResult InnerError(int i=1)
        {
            int a = Convert.ToInt32("a");
            return Json("log api result");
        }
    }
}
