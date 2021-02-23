using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Data.Mappings
{
    public class BankTransactionMap : ClassMap<BankTransaction>
    {
        public BankTransactionMap()
        {
            Table("bankTransactions");
            Id(x => x.id).GeneratedBy.Identity().Column("id");
            Map(x => x.uuid).Column("uuid").Not.Nullable();

            Map(x => x.transactionId).Column("transactionId").Not.Nullable();
            Map(x => x.description).Column("description").Length(8000).Nullable();
            Map(x => x.amount).Column("amount").Not.Nullable();
            Map(x => x.currency).Column("currency").Not.Nullable();
            Map(x => x.reference).Column("reference").Nullable();
            Map(x => x.transactionAt).Column("transactionAt").Nullable();

            Map(x => x.createdAt).Column("createdAt").Not.Nullable();
            Map(x => x.modifiedAt).Column("modifiedAt").Not.Nullable();
            Map(x => x.status).Column("status").Nullable();

            Map(x => x.statusSend).Column("statusSend").Nullable();
            Map(x => x.linkError).Column("linkError").Nullable();

            References(x => x.bankAccount).Column("bankAccountId").Nullable();
        }
    }
}
