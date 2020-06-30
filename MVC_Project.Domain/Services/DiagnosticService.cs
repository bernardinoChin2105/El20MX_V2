using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IDiagnosticService : IService<Diagnostic>
    {
    }

    public class DiagnosticService : ServiceBase<Diagnostic>, IDiagnosticService
    {
        private IRepository<Diagnostic> _repository;
        public DiagnosticService(IRepository<Diagnostic> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

    }
}
