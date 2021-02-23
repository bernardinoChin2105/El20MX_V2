using NHibernate;
using MVC_Project.Data.Helpers;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MVC_Project.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : IEntity
    {
        private UnitOfWork _unitOfWork;

        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
        }

        public ISession Session { get { return _unitOfWork.Session; } }

        public void Create(T entity)
        {
            Session.Save(entity);
            Session.Flush();
        }

        public void Delete(Int64 id)
        {
            Session.Delete(Session.Load<T>(id));
            Session.Flush();
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return Session.Query<T>().Where(predicate).ToList<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return Session.Query<T>().ToList<T>();
        }

        public T GetById(Int64 id)
        {
            return Session.Get<T>(id);
        }

        public void Update(T entity)
        {
            Session.Update(entity);
            Session.Flush();
        }

        public void Create(IEnumerable<T> entities)
        {
            for (int i = 0; i < entities.Count(); i++)
            {
                Session.Save(entities.ElementAt(i));
                // 1000, same as the ADO batch size
                if (i % 1000 == 0)
                {
                    // flush a batch of inserts and release memory:
                    Session.Flush();
                    Session.Clear();
                }
            }
        }

        public void Update(IEnumerable<T> entities)
        {
            for (int i = 0; i < entities.Count(); i++)
            {
                Session.Update(entities.ElementAt(i));
                // 1000, same as the ADO batch size
                if (i % 1000 == 0)
                {
                    // flush a batch of inserts and release memory:
                    Session.Flush();
                    Session.Clear();
                }
            }
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return Session.Query<T>().FirstOrDefault(predicate);
        }
    }
}