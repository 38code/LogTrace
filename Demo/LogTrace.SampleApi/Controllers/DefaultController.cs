using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LogTrace.SampleApi.Controllers
{
    public class DefaultController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Index() {

            try
            {
                Trace.TraceInformation("information text");
                Trace.TraceWarning("wanring text");
                Trace.WriteLine("messsage","your category");
                Trace.WriteIf(true,"whilte true");
                Trace.WriteLineIf(true,"message true new line");
                return Json("abcd");
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
            
        }
    }
}
