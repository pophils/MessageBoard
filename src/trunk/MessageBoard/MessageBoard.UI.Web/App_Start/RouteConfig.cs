using System.Web.Mvc;
using System.Web.Routing;

namespace MessageBoard.UI.Web.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");



            routes.MapRoute(
             "Save New Message",
             "save-message", new { controller = "Message", action = "Save" }
            );

            routes.MapRoute(
           "Load previous Messages",
           "load-previous-messages", new { controller = "Message", action = "PreviousMessages" }
          );

            routes.MapRoute(
         "Load trending Messages",
         "get-trending-messages", new { controller = "Message", action = "GetTrendingMessages" }
        );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}