using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ISATExtractionProcessService : IService<SATExtractionProcess>
    {
    }
    public class SATExtractionProcessService : ServiceBase<SATExtractionProcess>, ISATExtractionProcessService
    {
        private IRepository<SATExtractionProcess> _repository;
        public SATExtractionProcessService(IRepository<SATExtractionProcess> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
