using System;
using System.Collections.Generic;
using MessageBoard.Domain.Data;
using MessageBoard.Domain.Service;
using NHibernate;
using NHibernate.Criterion;

namespace MessageBoard.Domain.Dal
{
    public abstract class SessionProvider : ISessionService
    {
        protected ISession session
        {
            get
            {
                ISession _session = NHttpModule.GetCurrentSession(); 
                return _session;
            }
        }
       

        public T Get<T>(object id)
        {
            return session.Get<T>(id);
        }

        public IList<T> Get<T>()
        {
            ICriteria criteria = session.CreateCriteria(typeof(T));
            return criteria.List<T>();
        }

        public IList<T> Get<T>(int startRowIndex, int maximumRows)
        {
            ICriteria criteria = session.CreateCriteria(typeof(T));
            criteria.SetFirstResult(startRowIndex).SetMaxResults(maximumRows);
            return criteria.List<T>();
        }

        public int Count<T>()
        {
            ICriteria criteria = session.CreateCriteria(typeof(T));
            return criteria.SetProjection(Projections.RowCount()).UniqueResult<int>();
        }

        public void Save<T>(T obj)
        {
            try
            {
                session.Transaction.Begin();
                session.Save(obj);
                session.Transaction.Commit();
            }
            catch
            {
                session.Transaction.Rollback();
                session.Close();
            }
        }

        public void Update<T>(T obj)
        {
            try
            {
                session.Transaction.Begin();
                session.Update(obj);
                session.Transaction.Commit();
            }
            catch
            {
                session.Transaction.Rollback();
                session.Close();
            }
        }

        public void SaveOrUpdate<T>(T obj)
        {
            try
            {
                session.Transaction.Begin();
                session.SaveOrUpdate(obj);
                session.Transaction.Commit();
            }
            catch (Exception ex)
            {
                session.Transaction.Rollback();
            }
        }

        public void Merge(Object obj)
        {
            try
            {
                session.Transaction.Begin();
                session.Merge(obj);
                session.Transaction.Commit();
            }
            catch
            {
                session.Transaction.Rollback();
                session.Close();
            }
        }
    }
}
