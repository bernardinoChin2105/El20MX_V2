using System.Web;
using System.Web.Optimization;

namespace MVC_Project.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // CSS style (bootstrap/inspinia)
            bundles.Add(new StyleBundle("~/Content/template/css").Include(
                      "~/Content/template/bootstrap.min.css",
                      "~/Content/template/animate.css",
                      "~/Content/template/style.css"));

            // Font Awesome icons
            bundles.Add(new ScriptBundle("~/font-awesome/js").Include(
                      "~/fonts/font-awesome/all.js",
                      "~/fonts/font-awesome/v4-shims.js"
                      ));

            bundles.Add(new StyleBundle("~/font-awesome/css").Include(
                      "~/fonts/font-awesome/all.css"));

            // jQuery
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/template/jquery-3.1.1.min.js"));

            // jQueryUI CSS
            bundles.Add(new ScriptBundle("~/Scripts/template/plugins/jquery-ui/jqueryuiStyles").Include(
                        "~/Scripts/template/plugins/jquery-ui/jquery-ui.min.css"));

            // jQueryUI
            bundles.Add(new StyleBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/template/plugins/jquery-ui/jquery-ui.min.js"));

            // Bootstrap
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/template/bootstrap.min.js"));

            // Inspinia script
            bundles.Add(new ScriptBundle("~/bundles/inspinia").Include(
                      "~/Scripts/template/plugins/metisMenu/metisMenu.min.js",
                      "~/Scripts/template/plugins/pace/pace.min.js",
                      "~/Scripts/template/app/inspinia.js"));

            // SlimScroll
            bundles.Add(new ScriptBundle("~/plugins/slimScroll").Include(
                      "~/Scripts/template/plugins/slimscroll/jquery.slimscroll.min.js"));

            // Peity
            bundles.Add(new ScriptBundle("~/plugins/peity").Include(
                      "~/Scripts/template/plugins/peity/jquery.peity.min.js"));

            // Peity
            bundles.Add(new ScriptBundle("~/plugins/bluebird").Include(
                      "~/Scripts/template/plugins/bluebird/bluebird.min.js"));

            // Video responsible
            bundles.Add(new ScriptBundle("~/plugins/videoResponsible").Include(
                      "~/Scripts/template/plugins/video/responsible-video.js"));

            // Lightbox gallery css styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/blueimp/css/").Include(
                      "~/Content/template/plugins/blueimp/css/blueimp-gallery.min.css"));

            // Lightbox gallery
            bundles.Add(new ScriptBundle("~/plugins/lightboxGallery").Include(
                      "~/Scripts/template/plugins/blueimp/jquery.blueimp-gallery.min.js"));

            // Sparkline
            bundles.Add(new ScriptBundle("~/plugins/sparkline").Include(
                      "~/Scripts/template/plugins/sparkline/jquery.sparkline.min.js"));

            // Morriss chart css styles
            bundles.Add(new StyleBundle("~/plugins/morrisStyles").Include(
                      "~/Content/template/plugins/morris/morris-0.4.3.min.css"));

            // Morriss chart
            bundles.Add(new ScriptBundle("~/plugins/morris").Include(
                      "~/Scripts/template/plugins/morris/raphael-2.1.0.min.js",
                      "~/Scripts/template/plugins/morris/morris.js"));

            // Flot chart
            bundles.Add(new ScriptBundle("~/plugins/flot").Include(
                      "~/Scripts/template/plugins/flot/jquery.flot.js",
                      "~/Scripts/template/plugins/flot/jquery.flot.tooltip.min.js",
                      "~/Scripts/template/plugins/flot/jquery.flot.resize.js",
                      "~/Scripts/template/plugins/flot/jquery.flot.pie.js",
                      "~/Scripts/template/plugins/flot/jquery.flot.time.js",
                      "~/Scripts/template/plugins/flot/jquery.flot.spline.js"));

            // Rickshaw chart
            bundles.Add(new ScriptBundle("~/plugins/rickshaw").Include(
                      "~/Scripts/template/plugins/rickshaw/vendor/d3.v3.js",
                      "~/Scripts/template/plugins/rickshaw/rickshaw.min.js"));

            // ChartJS chart
            bundles.Add(new ScriptBundle("~/plugins/chartJs").Include(
                      "~/Scripts/template/plugins/chartjs/Chart.min.js"));

            // iCheck css styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/iCheck/iCheckStyles").Include(
                      "~/Content/template/plugins/iCheck/custom.css"));

            // iCheck
            bundles.Add(new ScriptBundle("~/plugins/iCheck").Include(
                      "~/Scripts/template/plugins/iCheck/icheck.min.js"));

            // dataTables css styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/dataTables/dataTablesStyles").Include(
                      "~/Content/template/plugins/dataTables/datatables.min.css"));

            // dataTables
            bundles.Add(new ScriptBundle("~/plugins/dataTables").Include(
                      "~/Scripts/template/plugins/dataTables/datatables.min.js"));

            // jeditable
            bundles.Add(new ScriptBundle("~/plugins/jeditable").Include(
                      "~/Scripts/template/plugins/jeditable/jquery.jeditable.js"));

            // jqGrid styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/jqGrid/jqGridStyles").Include(
                      "~/Content/template/plugins/jqGrid/ui.jqgrid.css"));

            // jqGrid
            bundles.Add(new ScriptBundle("~/plugins/jqGrid").Include(
                      "~/Scripts/template/plugins/jqGrid/i18n/grid.locale-en.js",
                      "~/Scripts/template/plugins/jqGrid/jquery.jqGrid.min.js"));

            // codeEditor styles
            bundles.Add(new StyleBundle("~/plugins/codeEditorStyles").Include(
                      "~/Content/template/plugins/codemirror/codemirror.css",
                      "~/Content/template/plugins/codemirror/ambiance.css"));

            // codeEditor
            bundles.Add(new ScriptBundle("~/plugins/codeEditor").Include(
                      "~/Scripts/template/plugins/codemirror/codemirror.js",
                      "~/Scripts/template/plugins/codemirror/mode/javascript/javascript.js"));

            // codeEditor
            bundles.Add(new ScriptBundle("~/plugins/nestable").Include(
                      "~/Scripts/template/plugins/nestable/jquery.nestable.js"));

            // validate
            bundles.Add(new ScriptBundle("~/plugins/validate").Include(
                      "~/Scripts/template/plugins/validate/jquery.validate.min.js"));

            bundles.Add(new ScriptBundle("~/plugins/validateUnobtrusive").Include(
                      "~/Scripts/template/plugins/validate/jquery.validate.unobtrusive.min.js",
                      "~/Scripts/custom-validations/password-secured-validation.js",
                      "~/Scripts/custom-validations/string-comparer-validation.js"));

            // fullCalendar styles
            bundles.Add(new StyleBundle("~/plugins/fullCalendarStyles").Include(
                      "~/Content/template/plugins/fullcalendar/fullcalendar.css"));

            // fullCalendar
            bundles.Add(new ScriptBundle("~/plugins/fullCalendar").Include(
                      "~/Scripts/template/plugins/fullcalendar/moment.min.js",
                      "~/Scripts/template/plugins/fullcalendar/fullcalendar.min.js",
                      "~/Scripts/template/plugins/fullcalendar/lang/es.js"));

            // vectorMap
            bundles.Add(new ScriptBundle("~/plugins/vectorMap").Include(
                      "~/Scripts/template/plugins/jvectormap/jquery-jvectormap-1.2.2.min.js",
                      "~/Scripts/template/plugins/jvectormap/jquery-jvectormap-world-mill-en.js"));

            // ionRange styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/ionRangeSlider/ionRangeStyles").Include(
                      "~/Content/template/plugins/ionRangeSlider/ion.rangeSlider.css",
                      "~/Content/template/plugins/ionRangeSlider/ion.rangeSlider.skinFlat.css"));

            // ionRange
            bundles.Add(new ScriptBundle("~/plugins/ionRange").Include(
                      "~/Scripts/template/plugins/ionRangeSlider/ion.rangeSlider.min.js"));

            // dataPicker styles
            bundles.Add(new StyleBundle("~/plugins/dataPickerStyles").Include(
                      "~/Content/template/plugins/datapicker/datepicker3.css"));

            // dataPicker
            bundles.Add(new ScriptBundle("~/plugins/dataPicker").Include(
                      "~/Scripts/template/plugins/datapicker/bootstrap-datepicker.js"));

            // nouiSlider styles
            bundles.Add(new StyleBundle("~/plugins/nouiSliderStyles").Include(
                      "~/Content/template/plugins/nouslider/jquery.nouislider.css"));

            // nouiSlider
            bundles.Add(new ScriptBundle("~/plugins/nouiSlider").Include(
                      "~/Scripts/template/plugins/nouslider/jquery.nouislider.min.js"));

            // jasnyBootstrap styles
            bundles.Add(new StyleBundle("~/plugins/jasnyBootstrapStyles").Include(
                      "~/Content/template/plugins/jasny/jasny-bootstrap.min.css"));

            // jasnyBootstrap
            bundles.Add(new ScriptBundle("~/plugins/jasnyBootstrap").Include(
                      "~/Scripts/template/plugins/jasny/jasny-bootstrap.min.js"));

            // switchery styles
            bundles.Add(new StyleBundle("~/plugins/switcheryStyles").Include(
                      "~/Content/template/plugins/switchery/switchery.css"));

            // switchery
            bundles.Add(new ScriptBundle("~/plugins/switchery").Include(
                      "~/Scripts/template/plugins/switchery/switchery.js"));

            // chosen styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/chosen/chosenStyles").Include(
                      "~/Content/template/plugins/chosen/bootstrap-chosen.css"));

            // chosen
            bundles.Add(new ScriptBundle("~/plugins/chosen").Include(
                      "~/Scripts/template/plugins/chosen/chosen.jquery.js"));

            // knob
            bundles.Add(new ScriptBundle("~/plugins/knob").Include(
                      "~/Scripts/template/plugins/jsKnob/jquery.knob.js"));

            // wizardSteps styles
            bundles.Add(new StyleBundle("~/plugins/wizardStepsStyles").Include(
                      "~/Content/template/plugins/steps/jquery.steps.css"));

            // wizardSteps
            bundles.Add(new ScriptBundle("~/plugins/wizardSteps").Include(
                      "~/Scripts/template/plugins/steps/jquery.steps.min.js"));

            // dropZone styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/dropzone/dropZoneStyles").Include(
                      "~/Content/template/plugins/dropzone/basic.css",
                      "~/Content/template/plugins/dropzone/dropzone.css"));

            // dropZone
            bundles.Add(new ScriptBundle("~/plugins/dropZone").Include(
                      "~/Scripts/template/plugins/dropzone/dropzone.js"));

            // summernote styles
            bundles.Add(new StyleBundle("~/plugins/summernoteStyles").Include(
                      "~/Content/template/plugins/summernote/summernote.css",
                      "~/Content/template/plugins/summernote/summernote-bs3.css"));

            // summernote
            bundles.Add(new ScriptBundle("~/plugins/summernote").Include(
                      "~/Scripts/template/plugins/summernote/summernote.min.js"));

            // toastr notification
            bundles.Add(new ScriptBundle("~/plugins/toastr").Include(
                      "~/Scripts/template/plugins/toastr/toastr.min.js"));

            // toastr notification styles
            bundles.Add(new StyleBundle("~/plugins/toastrStyles").Include(
                      "~/Content/template/plugins/toastr/toastr.min.css"));

            // color picker
            bundles.Add(new ScriptBundle("~/plugins/colorpicker").Include(
                      "~/Scripts/template/plugins/colorpicker/bootstrap-colorpicker.min.js"));

            // color picker styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/colorpicker/colorpickerStyles").Include(
                      "~/Content/template/plugins/colorpicker/bootstrap-colorpicker.min.css"));

            // image cropper
            bundles.Add(new ScriptBundle("~/plugins/imagecropper").Include(
                      "~/Scripts/template/plugins/cropper/cropper.min.js"));

            // image cropper styles
            bundles.Add(new StyleBundle("~/plugins/imagecropperStyles").Include(
                      "~/Content/template/plugins/cropper/cropper.min.css"));

            // jsTree
            bundles.Add(new ScriptBundle("~/plugins/jsTree").Include(
                      "~/Scripts/template/plugins/jsTree/jstree.min.js"));

            // jsTree styles
            bundles.Add(new StyleBundle("~/Content/template/plugins/jsTree").Include(
                      "~/Content/template/plugins/jsTree/style.css"));

            // Diff
            bundles.Add(new ScriptBundle("~/plugins/diff").Include(
                      "~/Scripts/template/plugins/diff_match_patch/javascript/diff_match_patch.js",
                      "~/Scripts/template/plugins/preetyTextDiff/jquery.pretty-text-diff.min.js"));

            // Idle timer
            bundles.Add(new ScriptBundle("~/plugins/idletimer").Include(
                      "~/Scripts/template/plugins/idle-timer/idle-timer.min.js"));

            // Tinycon
            bundles.Add(new ScriptBundle("~/plugins/tinycon").Include(
                      "~/Scripts/template/plugins/tinycon/tinycon.min.js"));

            // Chartist
            bundles.Add(new StyleBundle("~/plugins/chartistStyles").Include(
                      "~/Content/template/plugins/chartist/chartist.min.css"));

            // jsTree styles
            bundles.Add(new ScriptBundle("~/plugins/chartist").Include(
                      "~/Scripts/template/plugins/chartist/chartist.min.js"));

            // Awesome bootstrap checkbox
            bundles.Add(new StyleBundle("~/plugins/awesomeCheckboxStyles").Include(
                      "~/Content/template/plugins/awesome-bootstrap-checkbox/awesome-bootstrap-checkbox.css"));

            // Clockpicker styles
            bundles.Add(new StyleBundle("~/plugins/clockpickerStyles").Include(
                      "~/Content/template/plugins/clockpicker/clockpicker.css"));

            // Clockpicker
            bundles.Add(new ScriptBundle("~/plugins/clockpicker").Include(
                      "~/Scripts/template/plugins/clockpicker/clockpicker.js"));

            // Date range picker Styless
            bundles.Add(new StyleBundle("~/plugins/dateRangeStyles").Include(
                      "~/Content/template/plugins/daterangepicker/daterangepicker-bs3.css"));

            // Date range picker
            bundles.Add(new ScriptBundle("~/plugins/dateRange").Include(
                      // Date range use moment.js same as full calendar plugin
                      "~/Scripts/template/plugins/fullcalendar/moment.min.js",
                      "~/Scripts/template/plugins/daterangepicker/daterangepicker.js"));

            // Sweet alert Styless
            bundles.Add(new StyleBundle("~/plugins/sweetAlertStyles").Include(
                      "~/Content/template/plugins/sweetalert/sweetalert.css"));

            // Sweet alert
            bundles.Add(new ScriptBundle("~/plugins/sweetAlert").Include(
                      "~/Scripts/template/plugins/sweetalert/sweetalert.min.js"));

            // Footable Styless
            bundles.Add(new StyleBundle("~/plugins/footableStyles").Include(
                      "~/Content/template/plugins/footable/footable.core.css", new CssRewriteUrlTransform()));

            // Footable alert
            bundles.Add(new ScriptBundle("~/plugins/footable").Include(
                      "~/Scripts/template/plugins/footable/footable.all.min.js"));

            // Select2 Styless
            bundles.Add(new StyleBundle("~/plugins/select2Styles").Include(
                      "~/Content/template/plugins/select2/select2.min.css"));

            // Select2
            bundles.Add(new ScriptBundle("~/plugins/select2").Include(
                      "~/Scripts/template/plugins/select2/select2.full.min.js"));

            // Masonry
            bundles.Add(new ScriptBundle("~/plugins/masonry").Include(
                      "~/Scripts/template/plugins/masonary/masonry.pkgd.min.js"));

            // Slick carousel Styless
            bundles.Add(new StyleBundle("~/plugins/slickStyles").Include(
                      "~/Content/template/plugins/slick/slick.css", new CssRewriteUrlTransform()));

            // Slick carousel theme Styless
            bundles.Add(new StyleBundle("~/plugins/slickThemeStyles").Include(
                      "~/Content/template/plugins/slick/slick-theme.css", new CssRewriteUrlTransform()));

            // Slick carousel
            bundles.Add(new ScriptBundle("~/plugins/slick").Include(
                      "~/Scripts/template/plugins/slick/slick.min.js"));

            // Ladda buttons Styless
            bundles.Add(new StyleBundle("~/plugins/laddaStyles").Include(
                      "~/Content/template/plugins/ladda/ladda-themeless.min.css"));

            // Ladda buttons
            bundles.Add(new ScriptBundle("~/plugins/ladda").Include(
                      "~/Scripts/template/plugins/ladda/spin.min.js",
                      "~/Scripts/template/plugins/ladda/ladda.min.js",
                      "~/Scripts/template/plugins/ladda/ladda.jquery.min.js"));

            // Dotdotdot buttons
            bundles.Add(new ScriptBundle("~/plugins/truncate").Include(
                      "~/Scripts/template/plugins/dotdotdot/jquery.dotdotdot.min.js"));

            // Touch Spin Styless
            bundles.Add(new StyleBundle("~/plugins/touchSpinStyles").Include(
                      "~/Content/template/plugins/touchspin/jquery.bootstrap-touchspin.min.css"));

            // Touch Spin
            bundles.Add(new ScriptBundle("~/plugins/touchSpin").Include(
                      "~/Scripts/template/plugins/touchspin/jquery.bootstrap-touchspin.min.js"));

            // Tour Styless
            bundles.Add(new StyleBundle("~/plugins/tourStyles").Include(
                      "~/Content/template/plugins/bootstrapTour/bootstrap-tour.min.css"));

            // Tour Spin
            bundles.Add(new ScriptBundle("~/plugins/tour").Include(
                      "~/Scripts/template/plugins/bootstrapTour/bootstrap-tour.min.js"));

            // i18next Spin
            bundles.Add(new ScriptBundle("~/plugins/i18next").Include(
                      "~/Scripts/template/plugins/i18next/i18next.min.js"));

            // Clipboard Spin
            bundles.Add(new ScriptBundle("~/plugins/clipboard").Include(
                      "~/Scripts/template/plugins/clipboard/clipboard.min.js"));

            // c3 Styless
            bundles.Add(new StyleBundle("~/plugins/c3Styles").Include(
                      "~/Content/template/plugins/c3/c3.min.css"));

            // c3 Charts
            bundles.Add(new ScriptBundle("~/plugins/c3").Include(
                      "~/Scripts/template/plugins/c3/c3.min.js"));

            // D3
            bundles.Add(new ScriptBundle("~/plugins/d3").Include(
                      "~/Scripts/template/plugins/d3/d3.min.js"));

            // Markdown Styless
            bundles.Add(new StyleBundle("~/plugins/markdownStyles").Include(
                      "~/Content/template/plugins/bootstrap-markdown/bootstrap-markdown.min.css"));

            // Markdown
            bundles.Add(new ScriptBundle("~/plugins/markdown").Include(
                      "~/Scripts/template/plugins/bootstrap-markdown/bootstrap-markdown.js",
                      "~/Scripts/template/plugins/bootstrap-markdown/markdown.js"));

            // Datamaps
            bundles.Add(new ScriptBundle("~/plugins/datamaps").Include(
                      "~/Scripts/template/plugins/topojson/topojson.js",
                      "~/Scripts/template/plugins/datamaps/datamaps.all.min.js"));

            // Taginputs Styless
            bundles.Add(new StyleBundle("~/plugins/tagInputsStyles").Include(
                      "~/Content/template/plugins/bootstrap-tagsinput/bootstrap-tagsinput.css"));

            // Taginputs
            bundles.Add(new ScriptBundle("~/plugins/tagInputs").Include(
                      "~/Scripts/template/plugins/bootstrap-tagsinput/bootstrap-tagsinput.js"));

            // Duallist Styless
            bundles.Add(new StyleBundle("~/plugins/duallistStyles").Include(
                      "~/Content/template/plugins/bootstrap-tagsinput/bootstrap-tagsinput.css"));

            // Duallist
            bundles.Add(new ScriptBundle("~/plugins/duallist").Include(
                      "~/Scripts/template/plugins/dualListbox/jquery.bootstrap-duallistbox.js"));

            // SocialButtons styles
            bundles.Add(new StyleBundle("~/plugins/socialButtonsStyles").Include(
                      "~/Content/template/plugins/bootstrapSocial/bootstrap-social.css"));

            // Typehead
            bundles.Add(new ScriptBundle("~/plugins/typehead").Include(
                      "~/Scripts/template/plugins/typehead/bootstrap3-typeahead.min.js"));

            // Pdfjs
            bundles.Add(new ScriptBundle("~/plugins/pdfjs").Include(
                      "~/Scripts/template/plugins/pdfjs/pdf.js"));

            // Touch Punch
            bundles.Add(new StyleBundle("~/plugins/touchPunch").Include(
                        "~/Scripts/template/plugins/touchpunch/jquery.ui.touch-punch.min.js"));

            // WOW
            bundles.Add(new StyleBundle("~/plugins/wow").Include(
                        "~/Scripts/template/plugins/wow/wow.min.js"));

            // Text spinners styles
            bundles.Add(new StyleBundle("~/plugins/textSpinnersStyles").Include(
                      "~/Content/template/plugins/textSpinners/spinners.css"));

            // Password meter
            bundles.Add(new StyleBundle("~/plugins/passwordMeter").Include(
                        "~/Scripts/template/plugins/pwstrength/pwstrength-bootstrap.min.js",
                        "~/Scripts/template/plugins/pwstrength/zxcvbn.js"));

            // Exportar excel
            bundles.Add(new ScriptBundle("~/bundles/fileDownload").Include(
                "~/Scripts/plugins/fileDownload/jquery.fileDownload.js"));
        }
    }
}
