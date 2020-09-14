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
    public interface IPlanService : IService<Plan>
    {
        List<PlansViewModel> GetPlans(BasePagination pagination, string Name);
        Plan SavePlan(Plan plan, IEnumerable<PlanChargeConfiguration> planCharge, IEnumerable<PlanFeatureConfiguration> planFeature, IEnumerable<PlanAssignmentConfiguration> planAssignments);
        Plan UpdatePlan(Plan plan, IEnumerable<PlanChargeConfiguration> newPlanChargeConfig, IEnumerable<PlanFeatureConfiguration> newPlanFeatureConfig, IEnumerable<PlanAssignmentConfiguration> newPlanAssignmentsConfig,
           IEnumerable<PlanChargeConfiguration> updatePlanChargeConfig, IEnumerable<PlanFeatureConfiguration> updatePlanFeatureConfig, IEnumerable<PlanAssignmentConfiguration> updatePlanAssignmentsConfig);
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
                "@PageNum =:PageNum, @PageSize =:PageSize, @Name =:Name ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("Name", Name)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(PlansViewModel)))
                    .List<PlansViewModel>();

            if (list != null) return list.ToList();
            return null;
        }

        public Plan SavePlan(Plan plan, IEnumerable<PlanChargeConfiguration> planChargeConfig, IEnumerable<PlanFeatureConfiguration> planFeatureConfig, IEnumerable<PlanAssignmentConfiguration> planAssignmentsConfig)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(plan);

                    foreach (var charge in planChargeConfig)
                        _repository.Session.Save(charge);

                    foreach (var feature in planFeatureConfig)
                        _repository.Session.Save(feature);

                    foreach (var assignment in planAssignmentsConfig)
                        _repository.Session.Save(assignment);

                    //faltan las características

                    transaction.Commit();
                    return plan;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Plan UpdatePlan(Plan plan, IEnumerable<PlanChargeConfiguration> newPlanChargeConfig, IEnumerable<PlanFeatureConfiguration> newPlanFeatureConfig, IEnumerable<PlanAssignmentConfiguration> newPlanAssignmentsConfig,
           IEnumerable<PlanChargeConfiguration> updatePlanChargeConfig, IEnumerable<PlanFeatureConfiguration> updatePlanFeatureConfig, IEnumerable<PlanAssignmentConfiguration> updatePlanAssignmentsConfig)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Update(plan);

                    #region planChargeConfig
                    foreach (var charge in newPlanChargeConfig)
                        _repository.Session.Save(charge);

                    foreach (var charge in updatePlanChargeConfig)
                        _repository.Session.Update(charge);

                    //foreach (var charge in oldPlanChargeConfig)
                    //    _repository.Session.Delete(charge);
                    #endregion

                    #region planAssignmentsConfig
                    foreach (var assignment in newPlanAssignmentsConfig)
                        _repository.Session.Save(assignment);

                    foreach (var assignment in updatePlanAssignmentsConfig)
                        _repository.Session.Update(assignment);

                    //foreach (var assignment in oldPlanAssignmentsConfig)
                    //    _repository.Session.Delete(assignment);
                    #endregion

                    #region Nuevo
                    foreach (var feature in newPlanFeatureConfig)
                        _repository.Session.Save(feature);

                    foreach (var feature in updatePlanFeatureConfig)
                        _repository.Session.Save(feature);

                    //foreach (var feature in oldPlanFeatureConfig)
                    //    _repository.Session.Delete(feature);
                    #endregion

                    //faltan las características

                    transaction.Commit();
                    return plan;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }
    }
}
