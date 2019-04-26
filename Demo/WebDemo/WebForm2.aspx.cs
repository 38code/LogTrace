using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebDemo
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            var t1=new Thread(() =>
            {
                System.Diagnostics.Trace.WriteLine("1 child information","*Thread One*");
                var t2=new Thread(() =>
                {
                    System.Diagnostics.Trace.WriteLine("2 child information", "*Thread Two*");
                });
                t2.Start();
            });
            t1.Start();
            System.Diagnostics.Trace.TraceInformation("button1_click");
            System.Diagnostics.Trace.TraceWarning("nonthing do");
        }
    }
}