using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IWebhookProcessService : IService<WebhookProcess>
    {
    }
    public class WebhookProcessService : ServiceBase<WebhookProcess>, IWebhookProcessService
    {
        private IRepository<WebhookProcess> _repository;
        public WebhookProcessService(IRepository<WebhookProcess> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }
    }
}
