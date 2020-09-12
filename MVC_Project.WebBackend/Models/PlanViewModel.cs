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
        //public List<PlanLabelsViewModel> LabelFeatures { get; set; }
        public List<PlanAssignmentLabels> LabelAssignment { get; set; }
    }
    public class PlanLabelsViewModel
    {
        public Int64 Id { get; set; }
        public Int64 ChargeId { get; set; }
        public string Label { get; set; }
        public decimal Value { get; set; }        
    }

    public class PlanAssignmentLabels
    {
        public Int64 Id { get; set; }
        public Int64 AssignmentId { get; set; }
        public string Label { get; set; }
        public string fieldType { get; set; }
        public string operation { get; set; }
        public string dataType { get; set; }
        public string providerData { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }
}