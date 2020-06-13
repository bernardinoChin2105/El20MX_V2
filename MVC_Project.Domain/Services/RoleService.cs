using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NHibernate.Criterion;
using System.Text;

namespace MVC_Project.Domain.Services {
    #region Interfaces  
    public interface IRoleService : IService<Role>
    {
        IList<Role> ObtenerRoles(string filtros);
        Tuple<IEnumerable<Role>, int>  FilterBy(string filtros, int accountId, int? skip, int? take);
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
            return roles.ToList();
        }

        public Tuple<IEnumerable<Role>, int> FilterBy(string filtros, int accountId, int? skip, int? take)
        {
            filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
            var filters = filtros.Split(',').ToList();
            
            var roles = _repository.Session.QueryOver<Role>()
                .Where(x => x.account.id == accountId);

            if (!string.IsNullOrWhiteSpace(filters[0]))
            {
                string nombre = filters[0];
                roles = roles.Where(role => role.name.IsInsensitiveLike("%" + nombre +"%"));
            }

            var count = roles.RowCount();

            if (skip.HasValue)
                roles.Skip(skip.Value);

            if (take.HasValue)
                roles.Take(take.Value);

            var list = roles.OrderBy(u => u.createdAt).Desc.List();
            return new Tuple<IEnumerable<Role>, int>(list, count);
        }
    }
}