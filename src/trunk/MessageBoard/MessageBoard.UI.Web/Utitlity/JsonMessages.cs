using System.Collections.Generic;
using MessageBoard.Domain.Entities;

namespace MessageBoard.UI.Web.Utitlity
{
    public class JsonMessage
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class NewMessageJsonMessage : JsonMessage
    {
        public string DateSaved { get; set; }
        public long CurrentRefreshStartId { get; set; }
    }

    public class MessageListWrapperJsonMessage : JsonMessage
    {
        public IList<Message> Messages { get; set; }
        public bool CanLoadMore { get; set; }
        public long CurrentRefreshStartId { get; set; }
    }

}