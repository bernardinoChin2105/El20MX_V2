using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IStateService : IService<State>
    {
        List<LocationsViewModel> GetLocationList(string zipCode);
    }
    public class StateService : ServiceBase<State>, IStateService
    {
        private IRepository<State> _repository;
        public StateService(IRepository<State> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<LocationsViewModel> GetLocationList(string zipCode)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_getLocationByZipCode " +
                "@zipCode=:zipCode ")
                    //.AddEntity(typeof(DiagnosticList))
                    .SetParameter("zipCode", zipCode)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(LocationsViewModel)))
                    .List<LocationsViewModel>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
