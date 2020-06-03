using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    #region Interfaces  
    public interface IMembershipPermissionService : IService<MembershipPermission>
    {
    }
    #endregion
    public class MembershipPermissionService : ServiceBase<MembershipPermission>, IMembershipPermissionService
    {
        private IRepository<MembershipPermission> _repository;
        public MembershipPermissionService(IRepository<MembershipPermission> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
