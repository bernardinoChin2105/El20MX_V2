using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IInvoiceIssuedService : IService<InvoiceIssued>
    {
        List<ListInvoiceIssued> GetAllInvoicesIssued();
        InvoiceIssued SaveInvoice(InvoiceIssued invoice, Provider provider = null, Customer customer = null);
    }

    public class InvoiceIssuedService : ServiceBase<InvoiceIssued>, IInvoiceIssuedService
    {
        private IRepository<InvoiceIssued> _repository;
        public InvoiceIssuedService(IRepository<InvoiceIssued> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<ListInvoiceIssued> GetAllInvoicesIssued()
        {
            return null;
        }

        public InvoiceIssued SaveInvoice(InvoiceIssued invoice, Provider provider = null, Customer customer = null)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {

                    if (provider != null && provider.id == 0)
                        _repository.Session.Save(provider);

                    if (customer != null && customer.id == 0)
                        _repository.Session.Save(customer);

                    if (invoice.id > 0)
                        _repository.Session.Update(invoice);
                    else
                        _repository.Session.Save(invoice);

                    transaction.Commit();
                    return invoice;
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
