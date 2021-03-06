﻿using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Model;
using MVC_Project.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Domain.Services
{
    public interface IBankCredentialService : IService<BankCredential>
    {
        List<BankCredentialsList> GetBankCredentials(Int64 AccountId);
        List<BankAccountsList> GetBanksAccounts(Int64 idCredential);
        List<BankTransactionList> GetBankTransactionList(BasePagination pagination, BankTransactionFilter filter);
        BankCredential CreateWithTransaction(BankCredential bankCredential);
        List<BankTransactionList> GetBankTransactionListNoPagination(BasePagination pagination, BankTransactionFilter filters);
        List<BankTransactionContaLinkList> GetBankTransactionListContaLink(int PageNum, int PageSize, string statusSend);
    }
    public class BankCredentialService : ServiceBase<BankCredential>, IBankCredentialService
    {
        private IRepository<BankCredential> _repository;
        public BankCredentialService(IRepository<BankCredential> baseRepository) : base(baseRepository)
        {
            _repository = baseRepository;
        }

        public List<BankCredentialsList> GetBankCredentials(Int64 accountId)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankCredentialList " +
                "@accountId =:accountId ")
                    .SetParameter("accountId", accountId)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankCredentialsList)))
                    .List<BankCredentialsList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<BankAccountsList> GetBanksAccounts(Int64 idCredential)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankAccountList " +
                "@credentialId =:credentialId ")
                    .SetParameter("credentialId", idCredential)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankAccountsList)))
                    .List<BankAccountsList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<BankTransactionList> GetBankTransactionList(BasePagination pagination, BankTransactionFilter filter)
        {
            String dateinit = null;
            String dateend = null;
            if (pagination.CreatedOnStart != null)
            {
                dateinit = Convert.ToDateTime(pagination.CreatedOnStart).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }
            if (pagination.CreatedOnEnd != null)
            {
                dateend = Convert.ToDateTime(pagination.CreatedOnEnd).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            }
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_bankTransactionList " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @createdOnStart=:createdOnStart, @createdOnEnd=:createdOnEnd, " +
                "@accountId=:accountId, @bankId=:bankId, @bankAccountId=:bankAccountId, @movements=:movements ")
                    .SetParameter("PageNum", pagination.PageNum)
                    .SetParameter("PageSize", pagination.PageSize)
                    .SetParameter("createdOnStart", dateinit)
                    .SetParameter("createdOnEnd", dateend)
                    .SetParameter("accountId", filter.accountId)
                    .SetParameter("bankId", filter.bankId)
                    .SetParameter("bankAccountId", filter.bankAccountId)
                    .SetParameter("movements", filter.movements)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankTransactionList)))
                    .List<BankTransactionList>();

            if (list != null) return list.ToList();
            return null;
        }

        public BankCredential CreateWithTransaction(BankCredential bankCredential)
        {
            using (var transaction = _repository.Session.BeginTransaction())
            {
                try
                {
                    _repository.Session.SaveOrUpdate(bankCredential);
                    transaction.Commit();
                    return bankCredential;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public List<BankTransactionList> GetBankTransactionListNoPagination(BasePagination pagination, BankTransactionFilter filter)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_exportBankTransactionList " +
                "@createdOnStart=:createdOnStart, @createdOnEnd=:createdOnEnd, " +
                "@accountId=:accountId, @bankId=:bankId, @bankAccountId=:bankAccountId, @movements=:movements ")
                    .SetParameter("createdOnStart", pagination.CreatedOnStart != null ? Convert.ToDateTime(pagination.CreatedOnStart).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo) : null)
                    .SetParameter("createdOnEnd", pagination.CreatedOnEnd != null ? Convert.ToDateTime(pagination.CreatedOnEnd).ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo) : null)
                    .SetParameter("accountId", filter.accountId)
                    .SetParameter("bankId", filter.bankId)
                    .SetParameter("bankAccountId", filter.bankAccountId)
                    .SetParameter("movements", filter.movements)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankTransactionList)))
                    .List<BankTransactionList>();

            if (list != null) return list.ToList();
            return null;
        }

        public List<BankTransactionContaLinkList> GetBankTransactionListContaLink(int PageNum, int PageSize, string statusSend)
        {
            var list = _repository.Session.CreateSQLQuery("exec dbo.st_getLastTransactions " +
                "@PageNum =:PageNum, @PageSize =:PageSize, @statusSend=:statusSend ")
                    .SetParameter("PageNum", PageNum)
                    .SetParameter("PageSize", PageSize)
                    .SetParameter("statusSend", statusSend)
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean(typeof(BankTransactionContaLinkList)))
                    .List<BankTransactionContaLinkList>();

            if (list != null) return list.ToList();
            return null;
        }
    }
}
