﻿using System;
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
        public string Index() {
            
            Trace.TraceInformation("information text");
            Trace.TraceWarning("wanring text");
            Trace.TraceError("error text");
            return "index result";
        }
    }
}
