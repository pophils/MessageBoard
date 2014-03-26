using MessageBoard.Domain.Logic;
using MessageBoard.Domain.Service;

namespace MessageBoard.Domain
{
    public static class DomainModule
    {

        public static IMessageService MessageService()
        {
            return new MessageService();
        }
    }
}