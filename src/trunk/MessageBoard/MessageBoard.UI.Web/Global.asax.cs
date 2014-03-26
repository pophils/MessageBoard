using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MessageBoard.UI.Web.App_Start;

namespace MessageBoard.UI.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
           
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            MvcHandler.DisableMvcResponseHeader = true;  // spoofing IIS response header
        }
    }


    // Class spoofs IIS server in response header
    public class ServerHeaderSpoofModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += context_PreSendRequestHeaders;
        }

        public void Dispose()
        {
          
        }

        private void context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.Response.Headers.Set("Server", "nginx");
        }
    }
}