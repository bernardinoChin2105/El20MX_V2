using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class AllianceFilterViewModel
    {
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Aliado")]
        public string AllyName { get; set; }
    }

    public class AlliancesListVM
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string allyName { get; set; }
        public decimal allyCommisionPercent { get; set; }
        public decimal customerDiscountPercent { get; set; }
        public string createdAt { get; set; }
        public string status { get; set; }
    }

    public class AllianceViewModel
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }

        [Display(Name = "Nombre")]
        public string name { get; set; }

        [Display(Name = "Aliado")]
        public Int64 allyId { get; set; }
        public SelectList allyList { get; set; }

        [Display(Name = "% comisión aliado")]
        public decimal allyCommisionPercent { get; set; }

        [Display(Name = "% descuento cliente")]
        public decimal customerDiscountPercent { get; set; }

        [Display(Name = "Código promoción")]
        public string promotionCode { get; set; }

        [Display(Name = "Periodo a aplicar")]
        public bool applyPeriod { get; set; }

        [Display(Name = "Periodo inicial")]
        public int initialPeriod { get; set; }

        [Display(Name = "Periodo final")]
        public int finalPeriod { get; set; }

        [Display(Name = "% comisión aliado al finalizar")]
        public decimal finalAllyCommisionPercent { get; set; }

        [Display(Name = "Vigencia de alianza")]
        public bool allianceValidity { get; set; }

        [Display(Name = "Fecha Fin")]
        public DateTime finalDate { get; set; }

        public AllianceViewModel() {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Seleccionar", Value = "-1" });

            allyList = new SelectList(list);
        }
    }

    public class AllyFilterViewModel
    {
        public Int64 Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }
    }

    public class AlliesListVM
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }
        public string name { get; set; }
        public string createdAt { get; set; }
        public string modifiedAt { get; set; }
        public string status { get; set; }
    }

    #region modelos para promociones
    public class PromotionFilterViewModel
    {
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Tipo")]
        public Int64 Type { get; set; }
        public SelectList typeList { get; set; }

        public PromotionFilterViewModel()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Todos", Value = "-1" });

            typeList = new SelectList(list);

        }
    }

    public class PromotionViewModel
    {
        public Int64 id { get; set; }
        public Guid uuid { get; set; }

        [Display(Name = "Nombre")]
        public string name { get; set; }

        [Display(Name = "Descuento en %")]
        public decimal discount { get; set; }

        [Display(Name = "Descuento en $")]
        public decimal discountRate { get; set; }

        [Display(Name = "Tipo")]
        public string TypeId { get; set; }
        public SelectList TypeList { get; set; }

        [Display(Name = "Clientes")]
        public Int64[] AccountId { get; set; }
        //public SelectList AccountList { get; set; }
        public MultiSelectList AccountMultipleList { get; set; }

        [Display(Name = "Periodo a aplicar")]
        public bool hasPeriod { get; set; }

        [Display(Name = "Periodo inicial")]
        public int initialPeriod { get; set; }

        [Display(Name = "Periodo final")]
        public int finalPeriod { get; set; }

        [Display(Name = "Vigencia de código")]
        public bool hasValidity { get; set; }

        [Display(Name = "Fecha Fin")]
        public DateTime finalDate { get; set; }

        //public DateTime validityInitialAt { get; set; }
        //public DateTime validityFinalAt { get; set; }

        public PromotionViewModel()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Seleccionar", Value = "-1" });
            TypeList = new SelectList(list);

        }
    }

    #endregion
}
 