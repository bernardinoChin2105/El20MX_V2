using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Helpers;
using MVC_Project.Domain.Repositories;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    #region Interfaces

    public interface IOrderService : IService<Order>
    {
        IList<Order> FilterBy(string filtros, int? skip, int? take);
        IList<Store> FilterStore();
        IList<Staff> FilterStaff();
        IList<OrderItems> OrdenDetail(int orderId);
        int TotalFilterBy(string filtros, int? skip, int? take);
    }

    #endregion Interfaces

    public class OrderService : ServiceBase<Order>, IOrderService
    {
        private IRepository<Order> _repository;

        public OrderService(IRepository<Order> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public IList<Order> FilterBy(string filtros, int? skip, int? take)
        {
            filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
            var filters = filtros.Split(',').ToList();

            Customer customerAlias = null;
            Store storeAlias = null;
            Staff staffAlias = null;
            var query = _repository.Session.QueryOver<Order>()
            .JoinAlias(x => x.Store, () => storeAlias)
            .JoinAlias(x => x.Staff, () => staffAlias)
            .JoinAlias(x => x.Customer, () => customerAlias);
            if (!String.IsNullOrWhiteSpace(filters[0]))
            {
                query = query.WhereRestrictionOn(() => customerAlias.FirstName).IsInsensitiveLike("%" + filters[0] + "%");
            }
            if (!String.IsNullOrWhiteSpace(filters[2]))
            {
                int id = Convert.ToInt32(filters[2]);
                query = query.Where(() => storeAlias.Id == id);
            }
            if (!String.IsNullOrWhiteSpace(filters[3]))
            {
                int id = Convert.ToInt32(filters[3]);
                query = query.Where(() => staffAlias.Id == id);
            }
            if (!String.IsNullOrWhiteSpace(filters[4]))
            {
                DateTime Inicio = DateTime.Parse(filters[4]);
                query = query.Where(c => c.CreatedAt.Date >= Inicio);
            }
            if (!String.IsNullOrWhiteSpace(filters[5]))
            {
                DateTime Fin = DateTime.Parse(filters[5]);
                query = query.Where(c => c.CreatedAt.Date <= Fin);
            }
            if (skip.HasValue)
            {
                query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query.Take(take.Value);
            }

            return query.OrderBy(u => u.CreatedAt).Desc.List();
        }
        public int TotalFilterBy(string filtros, int? skip, int? take)
        {
            filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
            var filters = filtros.Split(',').ToList();

            Customer customerAlias = null;
            Store storeAlias = null;
            Staff staffAlias = null;
            var query = _repository.Session.QueryOver<Order>()
            .JoinAlias(x => x.Store, () => storeAlias)
            .JoinAlias(x => x.Staff, () => staffAlias)
            .JoinAlias(x => x.Customer, () => customerAlias);
            if (!String.IsNullOrWhiteSpace(filters[0]))
            {
                query = query.WhereRestrictionOn(() => customerAlias.FirstName).IsInsensitiveLike("%" + filters[0] + "%");
            }
            if (!String.IsNullOrWhiteSpace(filters[2]))
            {
                int id = Convert.ToInt32(filters[2]);
                query = query.Where(() => storeAlias.Id == id);
            }
            if (!String.IsNullOrWhiteSpace(filters[3]))
            {
                int id = Convert.ToInt32(filters[3]);
                query = query.Where(() => staffAlias.Id == id);
            }
            if (!String.IsNullOrWhiteSpace(filters[4]))
            {
                DateTime Inicio = DateTime.Parse(filters[4]);
                query = query.Where(c => c.CreatedAt.Date >= Inicio);
            }
            if (!String.IsNullOrWhiteSpace(filters[5]))
            {
                DateTime Fin = DateTime.Parse(filters[5]);
                query = query.Where(c => c.CreatedAt.Date <= Fin);
            }
            if (skip.HasValue)
            {
                query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query.Take(take.Value);
            }

            return query.RowCount();
        }
        public IList<Store> FilterStore()
        {
            var query = _repository.Session.QueryOver<Store>();
            return query.OrderBy(u => u.Nombre).Asc.List();
        }
        public IList<Staff> FilterStaff()
        {
            var query = _repository.Session.QueryOver<Staff>();
            return query.OrderBy(u => u.FirstName).Asc.List();
        }
        public IList<OrderItems> OrdenDetail(int orderId)
        {
            Order OrderAlias = null;
            Producto ProductoAlias = null;
            var query = _repository.Session.QueryOver<OrderItems>()
                .JoinAlias(x => x.Order, () => OrderAlias)
                .JoinAlias(x => x.Producto, () => ProductoAlias)
                .Where(() => OrderAlias.Id == orderId);
            
            return query.OrderBy(u => u.Id).Asc.List();
        }
    }
}