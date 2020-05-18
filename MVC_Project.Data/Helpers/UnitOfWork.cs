using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using MVC_Project.Data.Mappings;
using MVC_Project.Domain.Helpers;

namespace MVC_Project.Data.Helpers
{
    public class UnitOfWork : IUnitOfWork
    {
        private static readonly ISessionFactory _sessionFactory;
        private static Configuration configuration;
        private ITransaction _transaction;

        public ISession Session { get; set; }

        static UnitOfWork()
        {
            _sessionFactory = Fluently.Configure()
                .Database(
                MsSqlConfiguration.MsSql2012.ConnectionString(
                    conection => conection.FromConnectionStringWithKey("DBConnectionString")))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<RoleMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PermissionMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<RolePermissionMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<StoreMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<StaffMap>())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<PaymentMap>())
                .ExposeConfiguration(cfg => configuration = cfg)
                .BuildSessionFactory();
        }

        public UnitOfWork()
        {
            Session = _sessionFactory.OpenSession();
        }

        public void BeginTransaction()
        {
             if (Session!=null && Session.IsOpen) Session = _sessionFactory.OpenSession();
            _transaction = Session.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                if (_transaction != null && _transaction.IsActive)
                    _transaction.Commit();
            }
            catch
            {
                if (_transaction != null && _transaction.IsActive)
                    _transaction.Rollback();

                //throw;
            }
            finally
            {
                Session.Dispose();
            }
        }

        public void Rollback()
        {
            try
            {
                if (_transaction != null && _transaction.IsActive)
                    _transaction.Rollback();
            }
            finally
            {
                Session.Dispose();
            }
        }
    }
}