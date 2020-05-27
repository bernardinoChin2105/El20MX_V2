using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    #region Interfaces

    public interface IProfileService : IService<Profile>
    {
    }

    #endregion Interfaces

    public class ProfileService : ServiceBase<Profile>, IProfileService
    {
        private IRepository<Profile> _repository;

        public ProfileService(IRepository<Profile> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
