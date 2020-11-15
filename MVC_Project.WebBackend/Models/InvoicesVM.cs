using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Project.WebBackend.Models
{
    public class InvoicesVM
    {
        public string Version { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string Fecha { get; set; }
        public string Sello { get; set; }
        public string FormaPago { get; set; }
        public string NoCertificado { get; set; }
        public string Certificado { get; set; }
        public string CondicionesDePago { get; set; }
        public string SubTotal { get; set; }
        public string Descuento { get; set; }
        public string Moneda { get; set; }
        public string Total { get; set; }
        public string TotalTexto { get; set; }
        public string TipoDeComprobante { get; set; }
        public string MetodoPago { get; set; }
        public string LugarExpedicion { get; set; }
        public string ClaveCatastral { get; set; }
        public string TipoCambio { get; set; }        

        public Emisor Emisor { get; set; }
        public Receptor Receptor { get; set; }

        public Impuestos Impuestos { get; set; }
        public Complemento Complemento { get; set; }
        public List<Concepto> Conceptos { get; set; }
        //public TimbreFiscalDigital TimbreFiscalDigital { get; set; } //Esta dentro de complemento        
        public string QR { get; set; }        
        public string Logo { get; set; }

        public InvoicesVM()
        {            
            Emisor = new Emisor();
            Receptor = new Receptor();
            Complemento = new Complemento();
            Conceptos = new List<Concepto>();
            Impuestos = new Impuestos();
        }
    }

    public class Emisor
    {
        public string Rfc { get; set; }
        public string Nombre { get; set; }
        public string RegimenFiscal { get; set; }
        public string RegimenFiscalTexto { get; set; }
    }
    public class Receptor
    {
        public string Rfc { get; set; }
        public string Nombre { get; set; }
        public string UsoCFDI { get; set; }
        public string UsoCFDITexto { get; set; }
    }
    public class Complemento
    {
        public TimbreFiscalDigital TimbreFiscalDigital { get; set; }

        //Complemento()
        //{
        //    TimbreFiscalDigital = new TimbreFiscalDigital();
        //}
    }
    public class TimbreFiscalDigital
    {
        public string Version { get; set; }
        public string UUID { get; set; }
        public string RfcProvCertif { get; set; }
        public string FechaTimbrado { get; set; }
        public string SelloCFD { get; set; }
        public string NoCertificadoSAT { get; set; }
        public string SelloSAT { get; set; }
    }
    public class Impuestos
    {
        public Traslados Traslados { get; set; }
        public string TotalImpuestosTrasladados { get; set; }
    }
    public class Traslados
    {
        public Traslado Traslado { get; set; }
    }
    public class Traslado
    {
        public string Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public string TasaOCuota { get; set; }
        public string Importe { get; set; }
    }

    public class InformacionAduanera
    {
        public string Concepto { get; set; }
    }

    public class Conceptos
    {
        public Concepto Concepto { get; set; }

        //Conceptos()
        //{
        //    Concepto = new Concepto();
        //}
    }

    public class Concepto
    {
        public string ClaveProdServ { get; set; }
        public string NoIdentificacion { get; set; }
        public string Cantidad { get; set; }
        public string ClaveUnidad { get; set; }
        public string Unidad { get; set; }
        public string Descripcion { get; set; }
        public string ValorUnitario { get; set; }
        public string Importe { get; set; }
        public string Descuento { get; set; }
        public Impuestos Impuestos { get; set; }
        public InformacionAduanera InformacionAduanera { get; set; }      
    }
}