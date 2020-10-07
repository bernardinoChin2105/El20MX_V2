﻿using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using MVC_Project.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MVC_Project.Domain.Services
{
    public interface IPromotionService : IService<Promotion>
    {
        List<PromotionsList> GetPromotionList(BasePagination pagination, string name, string type);
        Promotion Save(Promotion promotion, List<PromotionAccount> promotionsAccount, List<Discount> discounts);
        Promotion GetValidityPromotion(string type);
        Promotion Update(Promotion promotion, List<PromotionAccount> promotionsAccount = null, List<Discount> discounts = null);
    }

    public class PromotionService : ServiceBase<Promotion>, IPromotionService
    {
        private IRepository<Promotion> _repository;
        public PromotionService(IRepository<Promotion> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<PromotionsList> GetPromotionList(BasePagination pagination, string name, string type)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_promotionsList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @Name=:Name, @Type=:Type ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("Name", name)
                    .SetParameter("Type", type)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(PromotionsList)))
                    .List<PromotionsList>();

            if (list != null) return list.ToList();
            return null;
        }

        public Promotion GetValidityPromotion(string type)
        {
            var promocion = _repository.FirstOrDefault(x => x.type == type &&
               x.status == SystemStatus.ACTIVE.ToString());

            if (promocion == null)
                return null;

            if (promocion.hasValidity)
            {
                if (promocion.validityInitialAt > DateTime.Now.Date || promocion.validityFinalAt < DateTime.Now.Date)
                {
                    return null;
                }
            }
            
            return promocion;
        }

        public Promotion Save(Promotion promotion, List<PromotionAccount> promotionsAccount, List<Discount> discounts)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Save(promotion);

                    foreach(var item in promotionsAccount)
                        _repository.Session.Save(item);

                    foreach (var item in discounts)
                        _repository.Session.Save(item);

                    transaction.Commit();
                    return promotion;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Promotion Update(Promotion promotion, List<PromotionAccount> promotionsAccount = null, List<Discount> discounts = null)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.Update(promotion);

                    if (promotionsAccount != null) {
                        _repository.Session.Update(promotionsAccount);
                    }

                    if (discounts != null)
                    {
                        _repository.Session.Update(discounts);
                    }

                    transaction.Commit();
                    return promotion;
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
