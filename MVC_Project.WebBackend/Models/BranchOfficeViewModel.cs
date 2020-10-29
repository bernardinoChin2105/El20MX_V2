using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Models
{
    public class BranchOfficeViewModel
    {
        public string uuid { get; set; }
        public Int64 id { get; set; }
        [DisplayName("Nombre")]
        public string name { get; set; }

        [DisplayName("Serie")]
        public string serie { get; set; }
        [DisplayName("Folio")]
        public Int64 folio { get; set; }

        [DisplayName("Logo")]
        public string logo { get; set; }

        [DisplayName("Calle")]
        public string street { get; set; }
        [DisplayName("Número exterior")]
        public string outdoorNumber { get; set; }
        [DisplayName("Número interior")]
        public string interiorNumber { get; set; }
        [DisplayName("Código postal")]
        public string zipCode { get; set; }

        [DisplayName("Colonia")]
        public Int64 colony { get; set; }
        [DisplayName("Municipio")]
        public Int64 municipality { get; set; }
        [DisplayName("Estado")]
        public Int64 state { get; set; }
        [DisplayName("Pais")]
        public Int64 country { get; set; }

        public string password { get; set; }
        public HttpPostedFileBase cer { get; set; }
        public HttpPostedFileBase key { get; set; }

        public string cerUrl { get; set; }
        public string keyUrl { get; set; }

        public List<SelectListItem> listColony { get; set; }
        public List<SelectListItem> listMunicipality { get; set; }
        public List<SelectListItem> listState { get; set; }
        public List<SelectListItem> listCountry { get; set; }

        public BranchOfficeViewModel()
        {
            listColony = new List<SelectListItem>();
            listMunicipality = new List<SelectListItem>();
            listState = new List<SelectListItem>();
            listCountry = new List<SelectListItem>();
        }
    }
    public class BranchOfficeData
    {
        public string uuid { get; set; }
        public Int64 id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }

    public class LogoBranchOfficeViewModel
    {
        public string uuid { get; set; }
        public string fileName { get; set; }
        public HttpPostedFileBase image { get; set; }
    }
}