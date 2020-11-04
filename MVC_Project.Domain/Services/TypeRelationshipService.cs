using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface ITypeRelationshipService : IService<TypeRelationship>
    {
    }

    public class TypeRelationshipService : ServiceBase<TypeRelationship>, ITypeRelationshipService
    {
        private IRepository<TypeRelationship> _repository;
        public TypeRelationshipService(IRepository<TypeRelationship> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
