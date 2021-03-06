﻿using System;
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
        public string Comentarios { get; set; }

        public Emisor Emisor { get; set; }
        public Receptor Receptor { get; set; }

        public Impuestos Impuestos { get; set; }
        public Complemento Complemento { get; set; }
        public List<Concepto> Conceptos { get; set; }
        public CfdiRelacionados CfdiRelacionados { get; set; }
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
            //CfdiRelacionados = new CfdiRelacionados();
        }
    }

    public class CfdiRelacionados
    {
        public string TipoRelación { get; set; }
        public List<CfdiRelacionado> CfdiRelacionado { get; set; }
    }
    public class CfdiRelacionado 
    {
        public string UUID { get; set; }
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
        public List<Pago> Pagos { get; set; }

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
        public List<Traslado> Traslados { get; set; }
        public List<Retenido> Retenidos { get; set; }
        public string TotalImpuestosTrasladados { get; set; }
        public string TotalImpuestosRetenidos { get; set; }

        public string ImpuestosRetenidosIVA { get; set; }
        public string ImpuestosRetenidosISR { get; set; }

        public Impuestos()
        {
            Traslados = new List<Traslado>();
            Retenidos = new List<Retenido>();
        }
    }
    public class Traslado
    {
        public string Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public string TasaOCuota { get; set; }
        public decimal Importe { get; set; }
    }

    public class Retenido
    {
        public string Base { get; set; }
        public string Impuesto { get; set; }
        public string TipoFactor { get; set; }
        public string TasaOCuota { get; set; }
        public decimal Importe { get; set; }
    }

    public class InformacionAduanera
    {
        public string Concepto { get; set; }
    }

    public class Conceptos
    {
        public Concepto Concepto { get; set; }
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
    //public class Pagos
    //{
    //    public List<Pago> Pago { get; set; }
    //}
    public class Pago
    {
        public string FechaPago { get; set; }
        public string FormaDePagoP { get; set; }
        public string MonedaP { get; set; }
        public string Monto { get; set; }
        public string NumOperacion { get; set; }
        public string TipoCambioP { get; set; }        
        public List<DoctoRelacionado> DoctoRelacionado { get; set; }
    }
    public class DoctoRelacionado
    {
        public string IdDocumento { get; set; }
        public string MonedaDR { get; set; }
        public string MetodoDePagoDR { get; set; }
        public string NumParcialidad { get; set; }
        public string ImpSaldoAnt { get; set; }
        public string ImpPagado { get; set; }
        public string ImpSaldoInsoluto { get; set; }
        public string Folio { get; set; }
        public string Serie { get; set; }                
    }
}