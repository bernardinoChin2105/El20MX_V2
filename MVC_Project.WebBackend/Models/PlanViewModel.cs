using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class PlanViewModel
    {
        public Int64 Id { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Display(Name = "Vigente")]
        public bool isCurrent { get; set; }

        public List<PlanLabelsViewModel> LabelConcepts { get; set; }
        public List<PlanLabelsViewModel> LabelFeatures { get; set; }
        public List<PlanLabelsViewModel> LabelAssignment { get; set; }
    }
    public class PlanLabelsViewModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
}