using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVC_Project.Domain.Services {
    #region Interfaces  
    public interface IRoleService : IService<Role>
    {
        IList<Role> ObtenerRoles(string filtros);
    }
    #endregion

    public class RoleService : ServiceBase<Role>, IRoleService
    {
        private IRepository<Role> _repository;
        public RoleService(IRepository<Role> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
        public IList<Role> ObtenerRoles(string filtros)
        {
            filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
            var filters = filtros.Split(',').ToList();

            var roles = _repository.GetAll();
            if (!string.IsNullOrWhiteSpace(filters[0]))
            {
                string nombre = filters[0];
                roles = roles.Where(p => p.name.ToLower().Contains(nombre.ToLower()));
            }
            //if (filters[1] != "2")
            //{
            //    bool status = filters[1] == "1" ? true : false;
            //    users = users.Where(p => p.Status == status);
            //}
            return roles.ToList();
        }

        public IList<Role> ObtenerRoles(string filtros, int accountId)
        {
            filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
            var filters = filtros.Split(',').ToList();

            var roles = _repository.FindBy(x => x.account.id == accountId);
            if (!string.IsNullOrWhiteSpace(filters[0]))
            {
                string nombre = filters[0];
                roles = roles.Where(p => p.name.ToLower().Contains(nombre.ToLower()));
            }
            //if (filters[1] != "2")
            //{
            //    bool status = filters[1] == "1" ? true : false;
            //    users = users.Where(p => p.Status == status);
            //}
            return roles.ToList();
        }
    }
}