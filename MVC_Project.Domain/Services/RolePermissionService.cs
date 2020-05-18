using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVC_Project.Domain.Services
{
    #region Interfaces  
    public interface IRolePermissionService : IService<RolePermission>
    {
    }
    #endregion
    public class RolePermissionService : ServiceBase<RolePermission>, IRolePermissionService
    {
        private IRepository<RolePermission> _repository;
        public RolePermissionService(IRepository<RolePermission> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
