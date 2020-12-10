using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace MVC_Project.Domain.Services
{
    public interface IProcessService : IService<Process>
    {
        Process GetByCode(string code);
        ProcessExecution CreateExecution(ProcessExecution processExecution);
        ProcessExecution UpdateExecution(ProcessExecution processExecution);

        IList<ProcessExecution> GetAllExecutions();
    }

    public class ProcessService : ServiceBase<Process>, IProcessService
    {
        private IRepository<Process> _repository;

        public ProcessService(IRepository<Process> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Process GetByCode(string code)
        {
            var payments = _repository.Session.QueryOver<Process>().Where(x => x.Code == code);
            return payments.List().FirstOrDefault();
        }

        public ProcessExecution CreateExecution(ProcessExecution processExecution)
        {
            _repository.Session.Save(processExecution);
            return processExecution;
        }

        public ProcessExecution UpdateExecution(ProcessExecution processExecution)
        {
            _repository.Session.Update(processExecution);
            return processExecution;
        }

        public IList<ProcessExecution> GetAllExecutions()
        {
            return _repository.Session.Query<ProcessExecution>().OrderByDescending(x => x.StartAt).ToList();
        }
    }
}
