using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IMembershipService : IService<Membership>
    {
        Membership Create(Membership membership, Discount discount);
    }

    public class MembershipService : ServiceBase<Membership>, IMembershipService
    {
        private IRepository<Membership> _repository;
        public MembershipService(IRepository<Membership> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
        public Membership Create(Membership membership, Discount discount)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(membership);
                    _repository.Session.Save(discount);
                    transaction.Commit();
                    return membership;
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
