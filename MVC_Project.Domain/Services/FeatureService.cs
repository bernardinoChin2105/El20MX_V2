using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IFeatureService : IService<Feature>
    {
    }
    public class FeatureService: ServiceBase<Feature>, IFeatureService
    {
        private IRepository<Feature> _repository;
        public FeatureService(IRepository<Feature> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
