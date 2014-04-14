using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MessageBoard.Domain.Entities;

namespace MessageBoard.UI.Web.Models
{
    public class MessageVM
    {
        public string Content { get; set; }
    }
 
    public class MessageListWrapper
    {
        public IList<Message> Messages { get; set; }
        public bool CanLoadMore { get; set; }
    }
}