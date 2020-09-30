using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IAllyService : IService<Ally>
    {
        List<AlliesList> GetAlliesList(BasePagination pagination, string name);
    }
    public class AllyService : ServiceBase<Ally>, IAllyService
    {
        private IRepository<Ally> _repository;
        public AllyService(IRepository<Ally> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }       

        public List<AlliesList> GetAlliesList(BasePagination pagination, string name)
        {            
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_alliesList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @Name=:Name ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("Name", name)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(AlliesList)))
                    .List<AlliesList>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
