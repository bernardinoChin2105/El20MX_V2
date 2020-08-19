using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IDiagnosticService : IService<Diagnostic>
    {
        List<DiagnosticsList> DiagnosticList(string uuid, BasePagination pagination);
    }

    public class DiagnosticService : ServiceBase<Diagnostic>, IDiagnosticService
    {
        private IRepository<Diagnostic> _repository;
        public DiagnosticService(IRepository<Diagnostic> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<DiagnosticsList> DiagnosticList(string uuid, BasePagination pagination)
        {
            String dateinit = null;
            String dateend = null;
            if (pagination.CreatedOnStart != null)
            {
                dateinit = Convert.ToDateTime(pagination.CreatedOnStart).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }
            if (pagination.CreatedOnEnd != null)
            {
                dateend = Convert.ToDateTime(pagination.CreatedOnEnd).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_diagnosticList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @uuid =:uuid, @createdOnStart=:createdOnStart, @createdOnEnd=:createdOnEnd")
                    //.AddEntity(typeof(DiagnosticList))
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("uuid", uuid)
                    .SetParameter("createdOnStart", dateinit)
                    .SetParameter("createdOnEnd", dateend)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(DiagnosticsList)))
                    .List<DiagnosticsList>();
                    //.SetResultSetMapping()
                    //.SetResultTransformer(Transformers.AliasToEntityMap).List<DiagnosticsList>(); 
                    //.List<DiagnosticsList>().ToList();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
