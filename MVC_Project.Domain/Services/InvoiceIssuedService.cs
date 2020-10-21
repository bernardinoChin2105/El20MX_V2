﻿using MVC_Project.Domain.Entities;
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
    }
}
