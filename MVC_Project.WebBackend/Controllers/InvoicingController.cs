using MVC_Project.FlashMessages;
using MVC_Project.Utils;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class InvoicingController : Controller
    {
        // GET: Invoicing
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Invoice()
        {
            InvoiceViewModel model = new InvoiceViewModel();
            //try
            //{
            //    //stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

            //    //Tipo de Comprobante
            //    var TypeVoucher = Enum.GetValues(typeof(TipoComprobante)).Cast<TipoComprobante>()
            //        .Select(e => new SelectListItem
            //        {
            //            Value = e.ToString(),
            //            Text = EnumUtils.GetDescription(e)
            //        }).Where(x => x.Value != "N" & x.Value != "T");

            //    //Forma de pago
            //    var PaymentForm = Enum.GetValues(typeof(MetodoPago)).Cast<MetodoPago>()
            //       .Select(e => new SelectListItem
            //       {
            //           Value = e.ToString(),
            //           Text = EnumUtils.GetDescription(e)
            //       });

            //    //Metodo de pago
            //    var PaymentMethod = Enum.GetValues(typeof(MetodoPago)).Cast<MetodoPago>()
            //      .Select(e => new SelectListItem
            //      {
            //          Value = e.ToString(),
            //          Text = EnumUtils.GetDescription(e)
            //      });

            //    //Uso de CFDI
            //    var UseCFDI = Enum.GetValues(typeof(UsoCFDI)).Cast<UsoCFDI>()
            //      .Select(e => new SelectListItem
            //      {
            //          Value = e.ToString(),
            //          Text = EnumUtils.GetDescription(e)
            //      });

            //    //Moneda
            //    var Currency = Enum.GetValues(typeof(TypeCurrency)).Cast<TypeCurrency>()
            //      .Select(e => new SelectListItem
            //      {
            //          Value = e.ToString(),
            //          Text = EnumUtils.GetDescription(e)
            //      });

                

            //    model.ListTypeVoucher = new SelectList(TypeVoucher);
            //    model.ListPaymentMethod = new SelectList(PaymentForm);
            //    model.ListUseCFDI = new SelectList(PaymentForm);
            //    model.ListPaymentMethod = new SelectList(PaymentMethod);
            //    model.ListCurrency = new SelectList(Currency);

            //}
            //catch (Exception ex)
            //{
            //    //string error = ex.Message.ToString();
            //    MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            //}
            return View(model);
        }
    }
}