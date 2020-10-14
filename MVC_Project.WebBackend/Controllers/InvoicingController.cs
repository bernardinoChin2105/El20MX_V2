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
            try
            {
                //stateList.Insert(0, (new SelectListItem() { Text = "Seleccione...", Value = "-1" }));

                var regimenList = Enum.GetValues(typeof(TipoComprobante)).Cast<TipoComprobante>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = EnumUtils.GetDisplayName(e)
                    });

                //model.ListRegimen = new SelectList(regimenList);
                //model.ListState = new SelectList(stateList);

                /*
                ((TipoComprobante)Enum.Parse(typeof(TipoComprobante), varTipoComprobante, true)).GetDescription();
                ((MetodoPago)Enum.Parse(typeof(MetodoPago), varMetodoPago, true)).GetDescription(),
                ((UsoCFDI)Enum.Parse(typeof(UsoCFDI), varUsoCFDI, true)).GetDescription();
                 
                 */
            }
            catch(Exception ex)
            {
                //string error = ex.Message.ToString();
                MensajeFlashHandler.RegistrarMensaje(ex.Message.ToString(), TiposMensaje.Error);
            }
            return View(model);
        }
    }
}