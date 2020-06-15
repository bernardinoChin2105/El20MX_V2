using MVC_Project.Domain.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MVC_Project.Domain.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        ISession Session { get; }

        IEnumerable<T> GetAll();

        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);

        T GetById(int id);

        void Create(T entity);

        void Update(T entity);

        void Delete(int id);

        void Create(IEnumerable<T> entities);

        T FirstOrDefault(Expression<Func<T, bool>> predicate);
    }
}