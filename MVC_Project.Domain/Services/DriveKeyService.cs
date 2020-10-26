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
    public interface IDriveKeyService : IService<DriveKey>
    {
        List<DriveKeyViewModel> GetDriveKey(string concept);
    }
    public class DriveKeyService : ServiceBase<DriveKey>, IDriveKeyService
    {
        private IRepository<DriveKey> _repository;
        public DriveKeyService(IRepository<DriveKey> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
        public List<DriveKeyViewModel> GetDriveKey(string concept)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_UnitSearchList " +
                "@concept =:concept ")
                    .SetParameter("concept", concept)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(DriveKeyViewModel)))
                    .List<DriveKeyViewModel>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
