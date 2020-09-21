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
    public interface IAllianceService : IService<Alliance>
    {
        List<AllianceList> GetAlliancesList(BasePagination pagination, string name, string allyName);
    }

    public class AllianceService : ServiceBase<Alliance>, IAllianceService
    {
        private IRepository<Alliance> _repository;
        public AllianceService(IRepository<Alliance> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<AllianceList> GetAlliancesList(BasePagination pagination, string name, string allyName)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_alliancesList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @Name=:Name, @AName=:AName ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("Name", name)
                    .SetParameter("AName", allyName)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(AllianceList)))
                    .List<AllianceList>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
