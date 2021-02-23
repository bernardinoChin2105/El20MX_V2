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

        T GetById(Int64 id);

        void Create(T entity);

        void Update(T entity);

        void Delete(Int64 id);

        void Create(IEnumerable<T> entities);

        void Update(IEnumerable<T> entities);

        T FirstOrDefault(Expression<Func<T, bool>> predicate);
    }
}