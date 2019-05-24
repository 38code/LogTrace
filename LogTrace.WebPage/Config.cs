using System;
using System.Diagnostics;
using LogTrace.WebPage;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly:System.Web.PreApplicationStartMethod(typeof(LogTraceConfig),"Init")]
namespace LogTrace.WebPage
{
    public sealed class LogTraceConfig
    {
        [ThreadStatic]
        private static bool _init;
        public static void Init()
        {
            if (_init) return;
            _init = true;
            DynamicModuleUtility.RegisterModule(typeof(LogTraceModule));
        }
    }
}
