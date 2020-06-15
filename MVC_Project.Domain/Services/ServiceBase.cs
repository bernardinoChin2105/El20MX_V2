using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public class ServiceBase<M> : IService<M> where M : IEntity
    {
        private IRepository<M> _baseRepository;

        public ServiceBase(IRepository<M> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public void Create(IEnumerable<M> entities)
        {
            _baseRepository.Create(entities);
        }

        public void Create(M entity)
        {
            _baseRepository.Create(entity);
        }

        public void Delete(int id)
        {
            _baseRepository.Delete(id);
        }

        public IEnumerable<M> FindBy(Expression<Func<M, bool>> predicate)
        {
            return _baseRepository.FindBy(predicate);
        }

        public M FirstOrDefault(Expression<Func<M, bool>> predicate)
        {
            return _baseRepository.FirstOrDefault(predicate);
        }

        public IEnumerable<M> GetAll()
        {
            return _baseRepository.GetAll().ToList();
        }

        public M GetById(int id)
        {
            return _baseRepository.GetById(id);
        }

        public void Update(M entity)
        {
            _baseRepository.Update(entity);
        }
    }
}
