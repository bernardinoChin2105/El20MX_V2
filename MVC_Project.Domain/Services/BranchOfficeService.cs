using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBranchOfficeService : IService<BranchOffice>
    {
        Tuple<IEnumerable<BranchOffice>, int> GetBranchOffice(string filtros, int? skip, int? take);
    }
    public class BranchOfficeService : ServiceBase<BranchOffice>, IBranchOfficeService
    {
        private IRepository<BranchOffice> _repository;
        public BranchOfficeService(IRepository<BranchOffice> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public Tuple<IEnumerable<BranchOffice>, int> GetBranchOffice(string filtros, int? skip, int? take)
        {
            var branchOffice = _repository.Session.QueryOver<BranchOffice>();

            var count = branchOffice.RowCount();

            if (skip.HasValue)
                branchOffice.Skip(skip.Value);

            if (take.HasValue)
                branchOffice.Take(take.Value);

            var list = branchOffice.OrderBy(u => u.createdAt).Desc.List();
            return new Tuple<IEnumerable<BranchOffice>, int>(list, count);
        }
    }
}
