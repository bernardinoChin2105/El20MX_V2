$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

$("#btnClearForm").click(function () {
    $("#SearchForm").each(function () {
        this.reset();
    });
    $('#table').DataTable().draw();
});

$(".btn-filter-rol").click(function () {
    $('#table').DataTable().draw();
});

var DateInit = JSON.parse(window.StartDate);
console.log(DateInit, "jasksa")
$('#RegisterAt').daterangepicker(
    {
        format: 'DD/MM/YYYY',
        todayBtn: "linked",
        keyboardNavigation: false,
        forceParse: false,
        calendarWeeks: true,
        autoclose: true,
        language: 'es',
        locale: {
            applyLabel: "Aplicar",
            cancelLabel: "Cancelar",
            fromLabel: "De",
            toLabel: "a"
        },
        //minDate: DateInit.MinDate,
        maxDate: DateInit.MaxDate,
        startDate: DateInit.MinDate,
        //opens: 'left'
    }).on('apply.daterangepicker', function (e, picker) {
        //console.log(picker.startDate.format('DD/MM/YYYY'));
        //console.log(picker.endDate.format('DD/MM/YYYY'));
        $('#FilterInitialDate').val(picker.startDate.format('DD/MM/YYYY'));
        $('#FilterEndDate').val(picker.endDate.format('DD/MM/YYYY'));
    });

//exportUrl, uploadUrl, exportTemplateUrl,
var MyAccountIndexControlador = function (htmlTableId, baseUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    //this.exportUrl = exportUrl;
    //this.uploadUrl = uploadUrl;
    //this.exportTemplateUrl = exportTemplateUrl;
    this.dataTable = {};

    this.init = function () {

        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: El20Utils.lenguajeTabla({}),
            "bProcessing": false,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            //deferLoading: 0,
            columns: [
                { data: 'id', title: "Id", visible: false },
                { data: 'month', title: "Mes" },
                { data: 'year', title: "Año" },
                { data: 'status', title: "Estatus" },
                { data: 'amountTotal', title: "Monto Total" },
                {
                    data: null,
                    title: "Pagar Factura",
                    render: function (data) {
                        //menu para el cliente work
                        console.log(data, "datos");
                        var link = "";
                        if (data.payInvoice)
                            link = '<a href="#" class="btn btn-light btn-downloadPdf">Pagar</span>';
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            link
                        '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
                {
                    data: null,
                    title: "Opciones",
                    className: 'work-options',
                    render: function (data) {
                        //menu para el cliente work
                        console.log(data);

                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a href="' + self.downloadPdfUrl + '?id=' + data.id + '" class="btn btn-light btn-downloadPdf" title="Descargar PDF"><span class="fas fa-file-pdf"></i></span>' +
                            '<a href="' + self.downloadXmlUrl + '?id=' + data.id + '" class="btn btn-light btn-downloadXml" title="Descargar XML"><span class="fas fa-file-alt"></span></a>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

                $.getJSON(sSource, aoData, function (json) {
                    fnCallback(json);
                    console.log("json", json)
                    if (json.success === false) {
                        toastr['error'](json.message, null, { 'positionClass': 'toast-top-center' });
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        $(".btn-export").click(function () {
            try {

                var aoData = [];
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

                El20Utils.mostrarCargador();

                $.fileDownload(self.exportUrl, {
                    httpMethod: "POST",
                    data: aoData
                }).done(function () { El20Utils.ocultarCargador(); })
                    .fail(function () { El20Utils.ocultarCargador(); });


                /////////////////////////////////////////////////////
            } catch (e) {
                throw 'ProviderIndexControlador -> Exportar: ' + e;
            }
        });

        $(".btn-export-template").click(function () {
            try {

                //var aoData = [];
                //aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

                //El20Utils.mostrarCargador();

                $.fileDownload(self.exportTemplateUrl, {
                    httpMethod: "Get"
                    //data: aoData
                }).done(function () { //El20Utils.ocultarCargador(); 
                }).fail(function () { //El20Utils.ocultarCargador(); 
                });


                /////////////////////////////////////////////////////
            } catch (e) {
                throw 'CustomerIndexControlador -> Exportar: ' + e;
            }
        });
    };
};

//$("#RFC").keyup(function () {
//    this.value = this.value.toUpperCase();
//});

//var _validFileExtensions = [".xlsx", ".xls"];
//$('.custom-file-input').on('change', function () {
//    let fileName = $(this).val().split('\\').pop();
//    $(this).next('.custom-file-label').addClass("selected").html(fileName);
//    $(".btn-save-import").attr("disabled", false);
//});

//var loadFile = function (event, imgid, input) {
//    if (input.type === "file") {
//        var sFileName = input.value;
//        if (sFileName.length > 0) {
//            var blnValid = false;
//            for (var j = 0; j < _validFileExtensions.length; j++) {
//                var sCurExtension = _validFileExtensions[j];
//                if (sFileName.substr(sFileName.length - sCurExtension.length, sCurExtension.length).toLowerCase() === sCurExtension.toLowerCase()) {
//                    blnValid = true;
//                    //var output = document.getElementById(imgid).innerHTML(sFileName);
//                    //output.src = URL.createObjectURL(event.target.files[0]);
//                    var salida = sFileName.split("\\");
//                    document.getElementById(imgid).innerHTML = salida[salida.length - 1];
//                    $(".btn-save-import").attr("disabled", false);
//                    break;
//                }
//            }

//            if (!blnValid) {
//                toastr["error"]('Favor de seleccionar un formato de Excel permitido (".xlsx", ".xls").', null, { 'positionClass': 'toast-top-center' }); 

//                input.value = "";
//                $(".btn-save-import").attr("disabled", true);

//                return false;
//            }
//        } else {
//            $(".btn-save-import").attr("disabled", true);
//            document.getElementById(imgid).innerHTML = "Seleccione el archivo...";//sFileName;
//        }
//    }
//    return true;
//};


//function Guardar(e) {
//    if (!$('#postImportar').valid()) return;
//    $("#ModalImporterClients").modal("hide");
//    El20Utils.mostrarCargador();

//    var data_ = new FormData($("#postImportar")[0]);
//    var url = $("#postImportar").attr("action");
//    $.ajax({
//        type: 'POST',
//        //contentType: 'application/json',
//        data: data_,
//        processData: false,
//        contentType: false,
//        async: true,
//        beforeSend: function () {
//        },
//        url: url,
//        success: function (result) {
//            console.log("result", result);
//            if (!result.Success) {
//                toastr["error"](result.Mensaje, null, { 'positionClass': 'toast-top-center' }); 
//            } else {
//                toastr["success"](result.Mensaje, null, { 'positionClass': 'toast-top-center' }); 
//                $("input[name='Excel']").val("");
//                $(".btn-save-import").attr("disabled", true);
//            }
//            $('#table').DataTable().draw();
//            El20Utils.ocultarCargador();
//            //$("#ModalImporterClients").modal("show");            
//        },
//        error: function (xhr) {
//            //console.log("error: " + xhr);
//            //El20Utils.ocultarCargador();
//            //loading.hideloading();
//        }
//    }).always(function () {
//    });
//}

//function cargarTabla(jsonObject) {
//    table = $('#clientesExcel').DataTable({
//        "aaData": jsonObject,
//        //"lengthMenu": [[50, 100, 200, 300, 400, 500], [50, 100, 200, 300, 400, 500]],
//        "lengthMenu": IntranetUtils.GRID_TOTAL_ITEMS,
//        "columns": [
//            { title: "ID_CLIENTE", "data": "ID_CLIENTE" },
//            { title: "NOMBRE", "data": "NOMBRE" },
//            { title: "CORREO_ELECTRONICO", "data": "CORREO_ELECTRONICO" },
//            { title: "PRESUPUESTO_ASIGNADO", "data": "PRESUPUESTO" },
//            { title: "OBSERVACIONES", "data": "OBSERVACIONES" }
//        ],
//        "initComplete": function (settings, json) {
//            //IntranetUtils.ocultarCargador();
//            loading.hideloading();
//        }
//    });
//}