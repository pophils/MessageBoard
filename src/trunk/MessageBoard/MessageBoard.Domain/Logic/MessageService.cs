using System;
using System.Collections.Generic;
using System.Linq;
using MessageBoard.Domain.Dal;
using MessageBoard.Domain.Entities;
using MessageBoard.Domain.Service;

namespace MessageBoard.Domain.Logic
{
    public class MessageService : SessionProvider, IMessageService
    {

        public bool SaveMessage(Message message)
        {
            try
            {
                session.SaveOrUpdate(message);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public IList<Message> GetTrendingMessages(int pageNo, int pageSize, out bool canLoadMore)
        {
            canLoadMore = false;
            IEnumerable<Message> messages = null;
            messages = Get<Message>().OrderByDescending(m => m.DateSaved);

            var enumerable = messages.ToList();

            if (enumerable.Count > pageSize)
                canLoadMore = true;
            
            return !enumerable.Any() ? new List<Message>() :
                enumerable.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
        }
        
        public IList<Message> GetTrendingMessages(int refreshStartId, int pageSize)
        {

            IEnumerable<Message> messages = null;
            messages = Get<Message>()
                .Where(m => m.Id > refreshStartId)
                .OrderBy(m => m.DateSaved);


            var enumerable = messages.ToList();

            return !enumerable.Any() ? new List<Message>() :
                enumerable.Skip(0).Take(pageSize).ToList();
        }
       
        public IList<Message> GetPreviousMessages(int firstMessageIdOnFirstLoad, int currentNumOfMessFetched, int pageNo, int pageSize, out bool canLoadMore)
        {
            canLoadMore = false;
            IEnumerable<Message> messages = null;
            messages = Get<Message>()
                .Where(m => m.Id <= firstMessageIdOnFirstLoad)
                .OrderByDescending(m => m.DateSaved);
               

            var enumerable = messages.ToList();

            if (enumerable.Count > (pageSize + currentNumOfMessFetched) )
                canLoadMore = true;
            
            return !enumerable.Any() ? new List<Message>() :
                enumerable.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
        }
   }
}
