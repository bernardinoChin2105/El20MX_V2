using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NHibernate.Criterion;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPaymentService : IService<Payment>
    {
        Payment GetByOrderId(string OrderId);
        Payment GetByProviderId(string ProviderId);
        PaymentApplication GetPaymentApplicationByKey(string appKey);
        Tuple<IEnumerable<Payment>, int> FilterBy(NameValueCollection filtersValue, int? skip, int? take);
    }

    public class PaymentService : ServiceBase<Payment>, IPaymentService
    {
        private IRepository<Payment> _repository;

        public PaymentService(IRepository<Payment> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Payment GetByOrderId(string OrderId)
        {
            var payments = _repository.Session.QueryOver<Payment>().Where( x=> x.OrderId == OrderId);
            return payments.List().FirstOrDefault();
        }

        public Payment GetByProviderId(string ProviderId)
        {
            var payments = _repository.Session.QueryOver<Payment>().Where(x=> x.ProviderId == ProviderId);
            return payments.List().FirstOrDefault();
        }

        public PaymentApplication GetPaymentApplicationByKey(string appKey)
        {
            var payments = _repository.Session.QueryOver<PaymentApplication>().Where(x => x.AppKey == appKey);
            return payments.List().Count() > 0 ? payments.List().First() : null;
        }

        public Tuple<IEnumerable<Payment>, int> FilterBy(NameValueCollection filtersValue, int? skip, int? take)
        {
            var query = _repository.Session.QueryOver<Payment>();

            
            string FilterOrder = filtersValue.Get("OrderId").Trim();
            string FilterInitialDate = filtersValue.Get("FilterInitialDate").Trim();
            string FilterEndDate = filtersValue.Get("FilterEndDate").Trim();
            int FilterUser = Convert.ToInt32(filtersValue.Get("UserId").Trim());

            DateTime? initialDate = DateUtil.ToDateTime(FilterInitialDate, Constants.DATE_FORMAT);
            DateTime? endDate = DateUtil.ToDateTime(FilterEndDate, Constants.DATE_FORMAT);

            if (FilterUser > 0)
            {
                query = query.Where(x => x.User.id == FilterUser);
            }
            if (!string.IsNullOrWhiteSpace(FilterOrder))
            {
                query = query.Where(x => x.OrderId.IsInsensitiveLike("%" + FilterOrder + "%"));
            }
            if (initialDate.HasValue)
            {
                query = query.Where(x=>x.CreationDate >= initialDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(x => x.CreationDate <= endDate.Value);
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
            var list = query.OrderBy(u => u.CreationDate).Desc.List();

            return new Tuple<IEnumerable<Payment>, int>(list, count);
        }
    }
}
