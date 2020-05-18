using MVC_Project.Data.Helpers;
using MVC_Project.Domain.Entities;
using MVC_Project.Domain.Services;
using MVC_Project.Utils;
using MVC_Project.Web.Models;
using MVC_Project.Web.Utils;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.Web.Controllers
{
    [Authorize]
    public class ReportServiceController : BaseController
    {
        private IOrderService _orderService;

        public ReportServiceController(IOrderService orderService, IRoleService roleService)
        {
            _orderService = orderService;
        }

        // GET: ReportService
        public ActionResult Index()
        {
            ReportOrdersViewModel model = new ReportOrdersViewModel();
            List<SelectListItem> listStatus = new List<SelectListItem>();
            List<SelectListItem> listStores = new List<SelectListItem>();
            List<SelectListItem> listStaff = new List<SelectListItem>();
            listStatus.Add(new SelectListItem
            {
                Text = "Todos",
                Value = "2"
            });
            listStatus.Add(new SelectListItem
            {
                Text = "Activos",
                Value = "1",
                Selected = true
            });
            listStatus.Add(new SelectListItem
            {
                Text = "Inactivos",
                Value = "0"
            });
            

            var Stores = _orderService.FilterStore();
            listStores = Stores.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Nombre
            }).ToList();
            var Staff = _orderService.FilterStaff();
            listStaff = Staff.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.FirstName
            }).ToList();
            ViewBag.OpcionesStatus = listStatus;
            ViewBag.OpcionesStore = listStores;
            ViewBag.OpcionesStaff = listStaff;
            return View(model);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllByFilter(JQueryDataTableParams param, string filtros)
        {
            try
            {
                int TotalDatos = _orderService.TotalFilterBy(filtros, param.iDisplayStart, param.iDisplayLength);
                var orders = _orderService.FilterBy(filtros, param.iDisplayStart, param.iDisplayLength);
                IList<OrdersData> OrdesResponse = new List<OrdersData>();
                foreach (var order in orders)
                {
                    OrdersData orderData = new OrdersData();
                    orderData.Id = order.Id;
                    orderData.Estatus = Convert.ToString( order.OrderStatus );
                    orderData.Cliente = order.Customer.FirstName;
                    orderData.CreatedAt = order.CreatedAt;
                    orderData.ShipperAt = order.RequiredAt;
                    orderData.Tienda = order.Store.Nombre;
                    orderData.Vendedor = order.Staff.FirstName;
                    OrdesResponse.Add(orderData);
                }
                return Json(new
                {
                    success = true,
                    sEcho = param.sEcho,
                    iTotalRecords = TotalDatos,
                    iTotalDisplayRecords = TotalDatos,
                    aaData = OrdesResponse
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    Data = new { Mensaje = new { title = "Error", message = ex.Message } },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    MaxJsonLength = Int32.MaxValue
                };
            }
        }

        /// <summary>
        /// Realiza la generación del archivo Excel para el reporte Corte de Caja.
        /// </summary>
        /// <param name="modeloFiltro">Modelo que contiene los filtros que se utilizarán para realizar la consulta de la información que contendrá el archivo excel.</param>
        /// <returns></returns>
        [FileDownload]
        [HttpPost]
        public ActionResult ExportExcel(ReportOrdersViewModel modeloFiltro)
        {
            ReportOrdersViewModel modelo = new ReportOrdersViewModel();
            string filtros = "[" + modeloFiltro.Nombre + "," + modeloFiltro.Status + "," + modelo.Store + "," + modelo.Staff + "," + modelo.Inicio + "," + modelo.Fin+"]";
            List<ExportOrdersViewModel> pagosPorCajero = _orderService.FilterBy(filtros, null, null).Select(x => new ExportOrdersViewModel
            {
                Id = x.Id,
                Cliente = x.Customer.FirstName,
                CreatedAt = x.CreatedAt,
                ShipperAt = x.RequiredAt,
                Tienda = x.Store.Nombre,
                Vendedor = x.Staff.FirstName
            }).ToList();

            //Lista que contiene las columnas que serán excluidas.
            List<string> columnasExcluidas = new List<string> { "FechaPagoFormateada" };

            //Diccionario que contiene los nuevos nombres de las columnas.
            Dictionary<string, string> dicNuevosNombres = new Dictionary<string, string>();
            dicNuevosNombres.Add("Id", "Id Cliente");
            dicNuevosNombres.Add("Cliente", "Cliente");
            dicNuevosNombres.Add("CreatedAt", "Fecha pedido");
            dicNuevosNombres.Add("ShipperAt", "Fecha entrega");
            dicNuevosNombres.Add("Tienda", "Tienda");
            dicNuevosNombres.Add("Vendedor", "Vendedor");

            var stream = ExcelUtil.ExportDataSet<ExportOrdersViewModel>(null, pagosPorCajero, null, true, true, 30, null, columnasExcluidas, "Corte Caja", dicNuevosNombres);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte Ordenes.xlsx");
        }
        [Authorize]
        [HttpGet]
        public ActionResult ModalDetails(int orderId)
        {
            OrdersData model = new OrdersData();
            Order orderBO = _orderService.GetById(orderId);
            model.Id = orderBO.Id;

            IList <ReportOrdersDetail> modelDetail = new List<ReportOrdersDetail>();
            IList<OrderItems> details = _orderService.OrdenDetail(orderId);
            foreach(OrderItems order in details)
            {
                ReportOrdersDetail Dato = new ReportOrdersDetail();
                Dato.Id = order.Id;
                Dato.Nombre = order.Producto.Nombre;
                Dato.Cantidad = order.Cantidad;
                Dato.Descuento = order.Descuento;
                Dato.Precio = order.PrecioLista;
                modelDetail.Add(Dato);
            }
            model.DetailItems = modelDetail;
            return PartialView("ModalDetalles", model);
        }
    }
}