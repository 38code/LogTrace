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
            try
            {
                System.Diagnostics.Trace.TraceInformation("information text");
                System.Diagnostics.Trace.TraceInformation("information text");
                System.Diagnostics.Trace.TraceWarning("wanring text");
                System.Diagnostics.Trace.WriteLine("messsage", "your category");
                System.Diagnostics.Trace.WriteIf(true, "whilte true");
                System.Diagnostics.Trace.WriteLineIf(true, "message true new line");
                //your code
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                throw;
            }
        }
    }
}