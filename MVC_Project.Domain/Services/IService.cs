using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IService<M>
    {
        IEnumerable<M> GetAll();
        IEnumerable<M> FindBy(Expression<Func<M, bool>> predicate);

        M GetById(int id);

        void Create(M entity);

        void Update(M entity);

        void Delete(int id);

        void Create(IEnumerable<M> entities);
    }
}
