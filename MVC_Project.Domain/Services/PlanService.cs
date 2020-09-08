﻿using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IPlanService : IService<Plan>
    {
        List<PlansViewModel> GetPlans(BasePagination pagination, string Name);
    }

    public class PlanService : ServiceBase<Plan>, IPlanService
    {
        private IRepository<Plan> _repository;

        public PlanService(IRepository<Plan> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<PlansViewModel> GetPlans(BasePagination pagination, string Name)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_listPlans " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @Name =:accountId ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("Name", Name)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(PlansViewModel)))
                    .List<PlansViewModel>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}