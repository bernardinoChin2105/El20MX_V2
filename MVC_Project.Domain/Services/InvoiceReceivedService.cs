using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IInvoiceReceivedService : IService<InvoiceReceived>
    {
        InvoiceReceived SaveInvoice(InvoiceReceived invoice, Provider provider = null, Customer customer = null);
    }

    public class InvoiceReceivedService : ServiceBase<InvoiceReceived>, IInvoiceReceivedService
    {
        private IRepository<InvoiceReceived> _repository;
        public InvoiceReceivedService(IRepository<InvoiceReceived> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public InvoiceReceived SaveInvoice(InvoiceReceived invoice, Provider provider = null, Customer customer = null)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    if (invoice.id > 0)
                        _repository.Session.Save(invoice);
                    else
                        _repository.Session.Update(invoice);

                    if (provider != null)                    
                        _repository.Session.Save(provider);

                    if (customer != null)
                        _repository.Session.Save(customer);                    

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
