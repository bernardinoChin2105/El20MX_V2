using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVC_Project.Domain.Services {
    #region Interfaces  
    public interface IPermissionService : IService<Permission>
    {
    }
    #endregion

    public class PermissionService : ServiceBase<Permission>, IPermissionService
    {
        public PermissionService(IRepository<Permission> baseRepository) : base(baseRepository)
        {
        }
    }
}