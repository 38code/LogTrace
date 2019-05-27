using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogTrace;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: System.Web.PreApplicationStartMethod(typeof(Startup), "Init")]
namespace LogTrace
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Startup
    {
        private static bool _init;
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            if (_init) return;
            _init = true;
            DynamicModuleUtility.RegisterModule(typeof(LogTraceHttpModule));
        }
    }
}
