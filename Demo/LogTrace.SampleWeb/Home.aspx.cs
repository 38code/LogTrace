using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LogTrace.SampleWeb
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //错误写法
            Trace.Warn("asdfasdf");
            //正确的写法
            System.Diagnostics.Trace.TraceInformation("sdddd");
        }
    }
}