using System;
using System.Web;
using System.Reflection;
using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Context;
using NHibernate.Mapping.ByCode;

//using NHibernate.Search.Event;

namespace MessageBoard.Domain.Data
{
   public class NHttpModule: IHttpModule
    {
         private static readonly ISessionFactory _sessionFactory;
         private static NHibernate.Cfg.Configuration config = null;
        
        static NHttpModule() 
        {
            _sessionFactory = CreateSessionFactory();
        }
        
      
        public void Init( HttpApplication context ) 
        {
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
        }

        
        public void Dispose() { }

        
        public static ISession GetCurrentSession() 
        {
            ISession session = null;
            try
            {
               session = _sessionFactory.GetCurrentSession();
            }
            catch
            {
                session = _sessionFactory.OpenSession();
               // CurrentSessionContext.Bind(session);
            }         
            return session;
        }

      
        private static void BeginRequest( object sender, EventArgs e )
        {
            ISession session = _sessionFactory.OpenSession();

            session.BeginTransaction();

            CurrentSessionContext.Bind(session);
        }

        // Unbinds the session, commits the transaction, and closes the session
        private static void EndRequest( object sender, EventArgs e )
        {
            ISession session = CurrentSessionContext.Unbind(_sessionFactory);

            if (session == null) return;

            try
            {
               /* session.Transaction.IsActive*/
                if(session.Transaction.IsActive)
                session.Transaction.Commit();
            }
            catch (Exception ex)
            {
               session.Transaction.Rollback();
            }
            finally
            {
                session.Close();
                session.Dispose();
            }
        }

       
        private static ISessionFactory CreateSessionFactory() 
        {

            var mapper = new ModelMapper();
            mapper.AddMappings(Assembly.Load("MessageBoard.Domain").GetTypes());         
            HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            config = new NHibernate.Cfg.Configuration();
            config.Configure();
               config.AddDeserializedMapping(domainMapping,"domainMapping");
            
             config.Properties[NHibernate.Cfg.Environment.CurrentSessionContextClass]="web";
             config.SetProperty(NHibernate.Cfg.Environment.ShowSql, "true").SetProperty(NHibernate.Cfg.Environment.BatchSize, "100");
            return config.BuildSessionFactory();
        }
    }
}
