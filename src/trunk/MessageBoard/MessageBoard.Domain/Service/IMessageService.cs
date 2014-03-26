using System.Collections.Generic;
using MessageBoard.Domain.Entities;

namespace MessageBoard.Domain.Service
{
    public partial interface IMessageService
    {
        #region CUD
        bool SaveMessage(Message message);
        IList<Message> GetTrendingMessages(int pageNo, int pageSize, out bool canLoadMore);
        IList<Message> GetTrendingMessages(int refreshStartId, int pageSize);
        IList<Message> GetPreviousMessages(int firstMessageIdOnFirstLoad, int currentNumOfMessFetched, int pageNo, int pageSize, out bool canLoadMore);

        #endregion

    }
}
