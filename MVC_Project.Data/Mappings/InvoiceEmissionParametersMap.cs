using FluentNHibernate.Mapping;
using MVC_Project.Domain.Entities;

namespace MVC_Project.Data.Mappings
{
    public class InvoiceEmissionParametersMap : ClassMap<InvoiceEmissionParameters>
    {
        public InvoiceEmissionParametersMap()
        {
            Table("invoiceEmissionParameters");
            Id(x => x.id).GeneratedBy.Identity().Column("id");

            Map(x => x.serie).Column("serie").Not.Nullable();
            Map(x => x.folio).Column("folio").Not.Nullable();
            Map(x => x.moneda).Column("moneda").Not.Nullable();
            Map(x => x.formaPago).Column("formaPago").Not.Nullable();
            Map(x => x.metodoPago).Column("metodoPago").Not.Nullable();
            Map(x => x.lugarExpedicion).Column("lugarExpedicion").Not.Nullable();
            Map(x => x.rfcEmisor).Column("rfcEmisor").Not.Nullable();
            Map(x => x.nombreEmisor).Column("nombreEmisor").Not.Nullable();
            Map(x => x.regimenEmisor).Column("regimenEmisor").Not.Nullable();
            Map(x => x.usoCFDIReceptor).Column("usoCFDIReceptor").Not.Nullable();
            Map(x => x.claveProdServ).Column("claveProdServ").Not.Nullable();
            Map(x => x.claveUnidad).Column("claveUnidad").Not.Nullable();
            Map(x => x.status).Column("status").Not.Nullable();
            Map(x => x.errosNotificationEmail).Column("errosNotificationEmail").Not.Nullable();
        }
    }
}
