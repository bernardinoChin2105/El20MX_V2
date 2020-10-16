using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IQuotationService : IService<Quotation>
    {
        Tuple<IEnumerable<Quotation>, int> GetQuotation(string filtros, int? skip, int? take);
    }
    public class QuotationService : ServiceBase<Quotation>, IQuotationService
    {
        private IRepository<Quotation> _repository;
        public QuotationService(IRepository<Quotation> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Tuple<IEnumerable<Quotation>, int> GetQuotation(string filtros, int? skip, int? take)
        {
            filtros = filtros.Replace("[", "").Replace("]", "").Replace("\\", "").Replace("\"", "");
            var filters = filtros.Split(',').ToList();

            Account AccountAlias = null;
            var quotation = _repository.Session.QueryOver<Quotation>();

            if (!string.IsNullOrWhiteSpace(filters[0]))
            {
                string nombre = filters[0];
                quotation = quotation
                    .JoinAlias(x => x.account, () => AccountAlias)
                    .Where(x => AccountAlias.name.IsInsensitiveLike("%" + nombre + "%") || AccountAlias.rfc.IsInsensitiveLike("%" + nombre + "%"));
            }

            var count = quotation.RowCount();

            if (skip.HasValue)
                quotation.Skip(skip.Value);

            if (take.HasValue)
                quotation.Take(take.Value);

            var list = quotation.OrderBy(u => u.createdAt).Desc.List();
            return new Tuple<IEnumerable<Quotation>, int>(list, count);
        }
    }
}
