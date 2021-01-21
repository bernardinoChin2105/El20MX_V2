using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ICADAccountService : IService<CADAccount>
    {
        void AssignCustomers(Int64 cadId, IEnumerable<Int64> inserts, IEnumerable<CADAccount> deletes);
    }
    public class CADAccountService : ServiceBase<CADAccount>, ICADAccountService
    {
        private IRepository<CADAccount> _repository;
        public CADAccountService(IRepository<CADAccount> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public void AssignCustomers(Int64 cadId, IEnumerable<Int64> inserts, IEnumerable<CADAccount> deletes)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    foreach (var insert in inserts)
                        _repository.Session.Save(new CADAccount { cad = new User { id = cadId }, account = new Account { id = insert } });

                    foreach (var delete in deletes)
                        _repository.Session.Delete(delete);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

    }
}
