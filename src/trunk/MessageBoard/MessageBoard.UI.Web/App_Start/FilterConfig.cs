using System.Web.Mvc;
using MessageBoard.UI.Web.Filters;

namespace MessageBoard.UI.Web.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new XframeOptionsFilter()); // prevents clickjacking
        }
    }
}