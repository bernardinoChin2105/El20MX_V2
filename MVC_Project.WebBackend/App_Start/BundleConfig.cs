using System.Web;
using System.Web.Optimization;

namespace MVC_Project.WebBackend
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // CSS style (bootstrap/inspinia)
            bundles.Add(new StyleBundle("~/Content/template/css").Include(
                          "~/Content/template/bootstrap.min.css",
                          "~/Content/template/plugins/iCheck/square/blue.css",
                          "~/Content/template/timeout-dialog.css",
                          "~/Content/template/animate.css",
                          "~/Content/template/plugins/awesome-bootstrap-checkbox/awesome-bootstrap-checkbox.css",
                          "~/Content/template/style.css",
                          "~/Content/template/stylesEl20mx.css"));

            bundles.Add(new StyleBundle("~/Content/templatepdf/css").Include(
                         "~/Content/template/bootstrap3/bootstrap.min.css"));
                         //"~/Content/template/style.css",
                         //"~/Content/template/stylesEl20mx.css"


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modrnizr para desarrollar y obtener información. De este modo, estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/template").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // Font Awesome icons
            bundles.Add(new ScriptBundle("~/font-awesome/js").Include(
                      "~/fonts/font-awesome/all.js",
                      "~/fonts/font-awesome/v4-shims.js"
                      ));
            //Estilos
            //DataTables
            bundles.Add(new StyleBundle("~/plugins/dataTablesStyles").Include(
                      "~/Content/template/plugins/dataTables/datatables.min.css"));          
            
            bundles.Add(new StyleBundle("~/font-awesome/css").Include(
                      "~/fonts/font-awesome/all.css"));
            
            //Scripts
            //DataTables
            bundles.Add(new ScriptBundle("~/plugins/dataTables").Include(
                      "~/Scripts/plugins/dataTables/datatables.min.js",
                      "~/Scripts/plugins/dataTables/dataTables.bootstrap4.min.js"));


            // fullCalendar styles
            bundles.Add(new StyleBundle("~/plugins/fullCalendarStyles").Include(
                      "~/Content/template/plugins/fullcalendar/fullcalendar.css"));
            //FullCalendar
            bundles.Add(new ScriptBundle("~/plugins/fullCalendar").Include(
                      "~/Scripts/plugins/fullcalendar/moment.min.js",
                      "~/Scripts/plugins/fullcalendar/fullcalendar.min.js", //));
                      "~/Scripts/plugins/fullcalendar/lang/es.js"));

            // dataPicker styles
            bundles.Add(new StyleBundle("~/plugins/dataPickerStyles").Include(
                      "~/Content/template/plugins/datapicker/datepicker3.css"));

            // dataPicker
            bundles.Add(new ScriptBundle("~/plugins/dataPicker").Include(
                      "~/Scripts/plugins/datapicker/bootstrap-datepicker.js"));

            // Sweet alert Styless
            bundles.Add(new StyleBundle("~/plugins/sweetAlertStyles").Include(
                      "~/Content/template/plugins/sweetalert/sweetalert.css"));

            //Validate
            bundles.Add(new ScriptBundle("~/plugins/validate").Include(
                      "~/Scripts/plugins/validate/jquery.validate.min.js"));


            //Validate Unobtrusive
            bundles.Add(new ScriptBundle("~/plugins/validateUnobtrusive").Include(
                      "~/Scripts/custom/jquery.validate.unobtrusive.min.js",
                      "~/Scripts/custom/password-secured-validation.js",
                      "~/Scripts/custom/string-comparer-validation.js"));

            // Sweet alert
            bundles.Add(new ScriptBundle("~/plugins/sweetAlert").Include(
                      "~/Scripts/plugins/sweetalert/sweetalert.min.js"));

            // chosen css styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/chosen/chosenStyles").Include(
                      "~/Content/template/plugins/chosen/bootstrap-chosen.css"));
            // chosen css styles
            bundles.Add(new ScriptBundle("~/plugins/chosen").Include(
                      "~/Scripts/plugins/chosen/chosen.jquery.js"));

            //Custom
            bundles.Add(new ScriptBundle("~/custom/utils").Include(
                      "~/Scripts/custom/Utils.js"));

            //Views
            bundles.Add(new ScriptBundle("~/views/user").Include(
                      "~/Scripts/views/user/Index.js"));
            bundles.Add(new ScriptBundle("~/views/rol").Include(
                      "~/Scripts/views/rol/Index.js"));
            bundles.Add(new ScriptBundle("~/views/diagnosticIndex").Include(
                      "~/Scripts/views/Diagnostic/Index.js"));
            bundles.Add(new ScriptBundle("~/views/customerIndex").Include(
                      "~/Scripts/views/Customer/Index.js"));


            // toastr notification
            bundles.Add(new ScriptBundle("~/plugins/toastr").Include(
                      "~/Scripts/plugins/toastr/toastr.min.js"));

            // toastr notification styles
            bundles.Add(new StyleBundle("~/plugins/toastrStyles").Include(
                      "~/Content/template/plugins/toastr/toastr.min.css"));

            // daterangepicker
            bundles.Add(new ScriptBundle("~/plugins/daterangepicker").Include(
                      "~/Scripts/plugins/daterangepicker/daterangepicker.js"));

            // daterangepicker styles
            bundles.Add(new StyleBundle("~/plugins/daterangepickerStyles").Include(
                      "~/Content/template/plugins/daterangepicker/daterangepicker-bs3.css"));

            // toastr notification
            bundles.Add(new ScriptBundle("~/plugins/jqueryFileDownload.js").Include(
                      "~/Scripts/jquery.fileDownload.js"));

            // jQueryUI CSS
            bundles.Add(new StyleBundle("~/Scripts/plugins/jquery-ui/jqueryuiStyles").Include(
                        "~/Scripts/plugins/jquery-ui/jquery-ui.min.css"));

            // jQueryUI 
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/plugins/jquery-ui/jquery-ui.min.js"));

        }
    }
}
