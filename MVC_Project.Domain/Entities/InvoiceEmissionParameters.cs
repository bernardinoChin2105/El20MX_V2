using System;

namespace MVC_Project.Domain.Entities
{
    public class InvoiceEmissionParameters : IEntity
    {
        public virtual Int64 id { get; set; }
        public virtual string serie { get; set; }
        public virtual int folio { get; set; }
        public virtual string moneda { get; set; }
        public virtual string formaPago { get; set; }
        public virtual string metodoPago { get; set; }
        public virtual string lugarExpedicion { get; set; }
        public virtual string rfcEmisor { get; set; }
        public virtual string nombreEmisor { get; set; }
        public virtual string regimenEmisor { get; set; }
        public virtual string usoCFDIReceptor { get; set; }
        public virtual string claveProdServ { get; set; }
        public virtual string claveUnidad { get; set; }
        public virtual string status { get; set; }
        public virtual string errosNotificationEmail { get; set; }
    }
}
