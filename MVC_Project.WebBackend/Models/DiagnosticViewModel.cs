﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class DiagnosticViewModel
    {
        [Display(Name = "Id")]
        public string id { get; set; }

        [Display(Name = "RFC")]
        public string rfc { get; set; }

        [Display(Name = "Nombre/Razón Social")]
        public string businessName { get; set; }

        [Display(Name = "CAD Comercial")]
        public string commercialCAD { get; set; }

        [Display(Name = "Plan")]
        public string plans { get; set; }

        [Display(Name = "Usuario(Email)")]
        public string email { get; set; }

        [Display(Name = "Fecha Registro")]
        public DateTime createdAt { get; set; }

        [Display(Name = "Estatus ante SAT")]
        public string statusSAT { get; set; }

        [Display(Name = "Régimen Fiscal")]
        public string taxRegime { get; set; }

        [Display(Name = "Actividades Económicas")]
        public string economicActivities { get; set; }

        [Display(Name = "Obligaciones Fiscales")]
        public string fiscalObligations { get; set; }

        [Display(Name = "Email Buzón Tributario")]
        public string taxMailboxEmail { get; set; }

        //public List<DiagnosticDetailsViewModel> diagnosticDetails { get; set; }
        public List<InvoicesGroup> diagnosticDetails { get; set; }
    }

    public class DiagnosticDetailsViewModel
    {
        public Int64 id { get; set; }
        public int year { get; set; }
        public string month { get; set; }
        public string typeTaxPayer { get; set; }
        public int numberCFDI { get; set; }
        public decimal totalAmount { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class ExtractionsFilter
    {
        public string taxpayer { get; set; }
        public string extractor { get; set; }
        public string periodFrom { get; set; }
        public string periodTo { get; set; }
        public string status { get; set; }
    }

    public class InvoicesGroup
    {
        public Int32 year { get; set; }
        public string month { get; set; }   
        public IssuerReceiverGroup issuer { get; set; }
        public IssuerReceiverGroup receiver { get; set; }
    }

    public class IssuerReceiverGroup
    {
        public Int32 numberTotal { get; set; }
        public decimal amountTotal { get; set; }
        public string type { get; set; }
    }

    //Información fiscal del contribuyente
    public class TaxpayerInfo
    {
        public string id { get; set; }
        public string personType { get; set; }
        public string registrationDate { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }    
}