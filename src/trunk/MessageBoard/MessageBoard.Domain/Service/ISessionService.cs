using System.Collections.Generic;

namespace MessageBoard.Domain.Service
{
    public interface ISessionService
    {
        T Get<T>(object id);

        IList<T> Get<T>();

        IList<T> Get<T>(int startRowIndex, int maximumRows);

        int Count<T>();
    }
}
