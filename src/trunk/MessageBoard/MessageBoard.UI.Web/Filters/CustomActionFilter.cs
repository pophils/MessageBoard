using System.Collections.Generic;
using System.Web.Mvc;

namespace MessageBoard.UI.Web.Filters
{
    
    public class MbCustomActionFilter : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
           //Todo: Request to action could be logged here in addition to third party service like google analytics
        }
    }


    
    public class XframeOptionsFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.AddHeader("x-frame-options", "DENY");
        }
    }
}