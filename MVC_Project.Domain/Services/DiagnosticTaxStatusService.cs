﻿using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IDiagnosticTaxStatusService : IService<DiagnosticTaxStatus>
    {
    }
    public class DiagnosticTaxStatusService : ServiceBase<DiagnosticTaxStatus>, IDiagnosticTaxStatusService
    {
        private IRepository<DiagnosticTaxStatus> _repository;
        public DiagnosticTaxStatusService(IRepository<DiagnosticTaxStatus> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
