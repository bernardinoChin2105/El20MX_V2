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
        public Int64 id { get; set; }
        [DisplayName("Nombre")]
        public string name { get; set; }

        [DisplayName(".cer")]
        public HttpPostedFileBase cer { get; set; }
        [DisplayName(".key")]
        public HttpPostedFileBase key { get; set; }

        public string cerUrl { get; set; }
        public string keyUrl { get; set; }

        [DisplayName("ciec")]
        public string ciec { get; set; }
        [DisplayName("fiel")]
        public string fiel { get; set; }

        public string ciecStatus { get; set; }
        public string fielStatus { get; set; }

        public string street { get; set; }
        public string outdoorNumber { get; set; }
        public string interiorNumber { get; set; }
        public string zipCode { get; set; }

        public Int64 colony { get; set; }
        public Int64 municipality { get; set; }
        public Int64 state { get; set; }
        public Int64 country { get; set; }

        public List<SelectListItem> listColony { get; set; }
        public List<SelectListItem> listMunicipality { get; set; }
        public List<SelectListItem> listState { get; set; }
        public List<SelectListItem> listCountry { get; set; }
    }
    public class BranchOfficeData
    {
        public string uuid { get; set; }
        public Int64 id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }
}