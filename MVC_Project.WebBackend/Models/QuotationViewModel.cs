using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class QuotationViewModel
    {
        public string account { get; set; }
    }

    public class QuotationData
    {
        public string uuid { get; set; }
        public Int64 id { get; set; }
        public string account { get; set; }
        public decimal total { get; set; }
        public int partialitiesNumber { get; set; }
        public string status { get; set; }
        public string quoteLink { get; set; }
        public string quoteName { get; set; }
        public DateTime startedAt { get; set; }
        public List<QuotationDetails> details { get; set; }
    }

    public class QuotationCreate
    {
        public Int64 id { get; set; }
        [DisplayName("Fecha en que se inicia regularización")]
        public DateTime startedAt { get; set; }
        [DisplayName("Monto total")]
        public decimal total { get; set; }
        [DisplayName("Pagos diferidos")]
        public bool hasDeferredPayment { get; set; }
        [DisplayName("Número de parcialidades")]
        public virtual int partialitiesNumber { get; set; }
        [DisplayName("Anticipo")]
        public virtual decimal advancePayment { get; set; }
        [DisplayName("Mensualidad")]
        public virtual decimal monthlyCharge { get; set; }

        public List<SelectListItem> accounts { get; set; }
        [DisplayName("Cliente")]
        public Int64 accountId { get; set; }
        [DisplayName("Cotización")]
        public HttpPostedFileBase[] files { get; set; }

        public List<SelectListItem> statusQuotation { get; set; }
        [DisplayName("Estatus de la Cotización")]
        public string quoteStatus { get; set; }

        public List<QuotationDetails> detail { get; set; }

        public QuotationCreate()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Seleccionar", Value = "-1" });

            accounts = list;
        }
    }

    public class QuotationDetails
    {
        public string name { get; set; }
        public string link { get; set; }
    }
}