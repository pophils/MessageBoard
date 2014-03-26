using System;
using System.Linq;
using System.Web.Mvc;
using MessageBoard.Domain;
using MessageBoard.Domain.Entities;
using MessageBoard.UI.Web.Models;
using MessageBoard.UI.Web.Utitlity;
using Omu.ValueInjecter;

namespace MessageBoard.UI.Web.Controllers
{
    public class MessageController : Controller
    {
         
        public ActionResult Save(MessageVM model)
        {
            var message = new Message();
            message.InjectFrom(model);
            message.DateSaved = PageHelper.LocalTime;
            return Json( DomainModule.MessageService().SaveMessage(message) ?
                new NewMessageJsonMessage(){ Success = true, DateSaved =  message.DateSaved.ToShortDateString(), CurrentRefreshStartId = message.Id} : 
                new NewMessageJsonMessage() { Success = false }, JsonRequestBehavior.DenyGet);
        }


        public ActionResult PreviousMessages(int pageNo, int firstMessageIdOnFirstLoad, int currentNumOfMessFetched)
        {
            bool canLoadMore;
            var messages = DomainModule.MessageService().GetPreviousMessages(firstMessageIdOnFirstLoad, currentNumOfMessFetched, pageNo, 10, out canLoadMore);
            return Json( new MessageListWrapperJsonMessage() { Messages = messages, CanLoadMore = canLoadMore }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTrendingMessages(int refreshStartId)
        {
            try
            {
                var messages = DomainModule.MessageService().GetTrendingMessages(refreshStartId, 10);
                return Json(new MessageListWrapperJsonMessage() { Messages = messages, CurrentRefreshStartId = messages.Last().Id }, JsonRequestBehavior.AllowGet);
     
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
         }

    }
}
