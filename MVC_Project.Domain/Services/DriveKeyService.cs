using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IDriveKeyService : IService<DriveKey>
    {
    }
    public class DriveKeyService : ServiceBase<DriveKey>, IDriveKeyService
    {
        private IRepository<DriveKey> _repository;
        public DriveKeyService(IRepository<DriveKey> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
