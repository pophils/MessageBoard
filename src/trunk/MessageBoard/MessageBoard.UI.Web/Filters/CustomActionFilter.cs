﻿using System.Collections.Generic;
using System.Web.Mvc;

namespace MessageBoard.UI.Web.Filters
{
    /// <summary>
    /// This class will intercept requests before they are executed and log them. 
    /// Ref: Owolabi http://www.asp.net/mvc/tutorials/hands-on-labs/aspnet-mvc-4-custom-action-filters
    /// </summary>
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