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
    }

    public class MembershipService : ServiceBase<Membership>, IMembershipService
    {
        private IRepository<Membership> _repository;
        public MembershipService(IRepository<Membership> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
