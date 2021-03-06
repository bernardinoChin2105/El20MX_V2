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
    public interface ITypeVoucherService : IService<TypeVoucher>
    {
    }
    public class TypeVoucherService : ServiceBase<TypeVoucher>, ITypeVoucherService
    {
        private IRepository<TypeVoucher> _repository;
        public TypeVoucherService(IRepository<TypeVoucher> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }       
    }
}
