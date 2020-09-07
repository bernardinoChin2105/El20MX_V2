using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IWebhookService : IService<Webhook>
    {
    }

    public class WebhookService : ServiceBase<Webhook>, IWebhookService
    {
        private IRepository<Webhook> _repository;
        public WebhookService(IRepository<Webhook> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }      
    }
}
