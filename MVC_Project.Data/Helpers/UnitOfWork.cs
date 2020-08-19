using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using MVC_Project.Data.Mappings;
using MVC_Project.Domain.Helpers;
using NHibernate.Tool.hbm2ddl;

namespace MVC_Project.Data.Helpers
{
    public class UnitOfWork : IUnitOfWork
    {
        private static readonly ISessionFactory _sessionFactory;
        private static Configuration configuration;
        private ITransaction _transaction;

        public ISession Session { get; set; }

        static FluentConfiguration BaseConfiguration()
        {
            var property = new Configuration().SetProperty(Environment.DefaultSchema, "dbo");
            var config = Fluently.Configure(property)
                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(conection => conection.FromConnectionStringWithKey("DBConnectionString")))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<UserMap>());

            return config;
        }

        static UnitOfWork()
        {
            //UpdateSchemaFromMappings();
            _sessionFactory = BaseConfiguration()
                .ExposeConfiguration(cfg => configuration = cfg)
                .BuildSessionFactory();
        }

        public static void UpdateSchemaFromMappings()
        {
            var config = BaseConfiguration();
            new SchemaUpdate(config.BuildConfiguration()).Execute(true, true);
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