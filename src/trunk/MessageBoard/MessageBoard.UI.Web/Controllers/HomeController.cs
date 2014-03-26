using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MessageBoard.Domain;
using MessageBoard.UI.Web.Models;

namespace MessageBoard.UI.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            bool canLoadMore;
            var messages = DomainModule.MessageService().GetTrendingMessages(1, 10, out canLoadMore);
            return View(new MessageListWrapper()
                            {
                                Messages = messages,
                                CanLoadMore = canLoadMore
                            });
        }
    }
}
