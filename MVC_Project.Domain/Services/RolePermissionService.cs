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
        void UpdateRolePermissions(IEnumerable<RolePermission> newRolePermissions, IEnumerable<RolePermission> updateRolePermissions, IEnumerable<RolePermission> oldRolePermissions);
    }
    #endregion
    public class RolePermissionService : ServiceBase<RolePermission>, IRolePermissionService
    {
        private IRepository<RolePermission> _repository;
        public RolePermissionService(IRepository<RolePermission> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public void UpdateRolePermissions(IEnumerable<RolePermission> newRolePermissions, IEnumerable<RolePermission> updateRolePermissions, IEnumerable<RolePermission> oldRolePermissions)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    foreach (var permission in newRolePermissions)
                        _repository.Session.Save(permission);

                    foreach (var permission in updateRolePermissions)
                        _repository.Session.Update(permission);

                    foreach (var permission in oldRolePermissions)
                        _repository.Session.Delete(permission);

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
