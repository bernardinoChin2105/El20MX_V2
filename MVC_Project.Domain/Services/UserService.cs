using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using MVC_Project.Utils;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MVC_Project.Domain.Services
{
    #region Interfaces

    public interface IUserService : IService<User>
    {
        Tuple<IEnumerable<User>, int> FilterBy(NameValueCollection filtersValue, int? skip, int? take);
        Tuple<IEnumerable<User>, int> FilterBy(NameValueCollection filtersValue, int accountId, int? skip, int? take);
    }

    #endregion Interfaces

    public class UserService : ServiceBase<User>, IUserService
    {
        private IRepository<User> _repository;

        public UserService(IRepository<User> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Tuple<IEnumerable<User>, int>  FilterBy(NameValueCollection filtersValue, int? skip, int? take)
        {
            string FilterName = filtersValue.Get("Name").Trim();
            int FilterStatus = Convert.ToInt32(filtersValue.Get("Status").Trim());

            var query = _repository.Session.QueryOver<User>();
            if (!string.IsNullOrWhiteSpace(FilterName))
            {
                query = query.Where(user => user.name.IsInsensitiveLike("%" + FilterName + "%") || user.profile.firstName.IsInsensitiveLike("%" + FilterName + "%") || user.profile.lastName.IsInsensitiveLike("%" + FilterName + "%"));
            }
            if (FilterStatus != Constants.SEARCH_ALL)
            {
                string FilterStatusBool = SystemStatus.ACTIVE.ToString();
                query = query.Where(user => user.status == FilterStatusBool);
            }
            var count = query.RowCount();

            if (skip.HasValue)
            {
                query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query.Take(take.Value);
            }
            var list = query.OrderBy(u => u.createdAt).Desc.List();
            return new Tuple<IEnumerable<User>, int>(list, count);
        }

        public Tuple<IEnumerable<User>, int> FilterBy(NameValueCollection filtersValue, int accountId, int? skip, int? take)
        {
            string FilterName = filtersValue.Get("Name").Trim();
            int FilterStatus = Convert.ToInt32(filtersValue.Get("Status").Trim());

            Membership membershipAlias = null;
            Account accountAlias = null;

            var query = _repository.Session.QueryOver<User>()
                .JoinAlias(x => x.memberships, () => membershipAlias)
                .JoinAlias(() => membershipAlias.account, () => accountAlias)
                .Where(() => accountAlias.id == accountId);

            if (!string.IsNullOrWhiteSpace(FilterName))
            {
                query = query.Where(user => user.name.IsInsensitiveLike("%" + FilterName + "%") || user.profile.firstName.IsInsensitiveLike("%" + FilterName + "%") || user.profile.lastName.IsInsensitiveLike("%" + FilterName + "%"));
            }
            if (FilterStatus != Constants.SEARCH_ALL)
            {
                string FilterStatusBool = SystemStatus.ACTIVE.ToString();
                query = query.Where(user => user.status == FilterStatusBool);
            }
            var count = query.RowCount();

            if (skip.HasValue)
            {
                query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query.Take(take.Value);
            }
            var list = query.OrderBy(u => u.createdAt).Desc.List();
            return new Tuple<IEnumerable<User>, int>(list, count);
        }

    }
}