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

$("#RFC").keyup(function () {
    this.value = this.value.toUpperCase();
});


var ProviderIndexControlador = function (htmlTableId, baseUrl, editUrl, exportUrl, uploadUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
    this.exportUrl = exportUrl;
    this.uploadUrl = uploadUrl;
    this.dataTable = {};

    this.init = function () {
        var primeravez = true;

        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: { url: 'Scripts/custom/dataTableslang.es_MX.json' },
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'id', title: "Id", visible: false },
                { data: 'rfc', title: "RFC" },
                { data: 'businessName', title: "Nombre Colaborador" },
                { data: 'phone', title: "Teléfono" },
                { data: 'email', title: "Email" },
                {
                    data: null,
                    title: "+ de Mi Colaborador",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<div class="dropdown">' +
                            '<button class="btn btn-light btn-menu" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><span class="fas fa-ellipsis-h"></span></button>' +
                            '<div class="dropdown-menu" aria-labelledby="dropdownMenuButton">' +
                            '<a class="dropdown-item" href="' + self.editUrl + '?uuid=' + data.uuid + '">Perfil Completo del Colaborador</a>' +
                            '<a class="dropdown-item" href="#">Estado de Cuenta</a>' +
                            '<a class="dropdown-item" href="#">Lista de (CFDI\'s)</a>' +
                            '</div>' +
                            '</div>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                //console.log(aoData, "que es?")
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {
                    //console.log(json)
                    primeravez = false;
                    if (json.success === false) {
                        toastr['error'](json.Mensaje.message);
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    } else {
                        fnCallback(json);
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
    };
};


var _validFileExtensions = [".xlsx", ".xls"];
var loadFile = function (event, imgid, input) {
    if (input.type === "file") {
        var sFileName = input.value;
        if (sFileName.length > 0) {
            var blnValid = false;
            for (var j = 0; j < _validFileExtensions.length; j++) {
                var sCurExtension = _validFileExtensions[j];
                if (sFileName.substr(sFileName.length - sCurExtension.length, sCurExtension.length).toLowerCase() === sCurExtension.toLowerCase()) {
                    blnValid = true;
                    //var output = document.getElementById(imgid).innerHTML(sFileName);
                    //output.src = URL.createObjectURL(event.target.files[0]);
                    var salida = sFileName.split("\\");
                    document.getElementById(imgid).innerHTML = salida[salida.length - 1];
                    $(".btn-save-import").attr("disabled", false);
                    break;
                }
            }

            if (!blnValid) {
                toastr["error"]('Favor de seleccionar un formato de Excel permitido.');

                input.value = "";
                $(".btn-save-import").attr("disabled", true);

                return false;
            }
        } else {
            $(".btn-save-import").attr("disabled", true);
            document.getElementById(imgid).innerHTML = "Seleccione el archivo...";//sFileName;
        }
    }
    return true;
};


function Guardar(e) {
    if (!$('#postImportar').valid()) return;
    $("#ModalImporterClients").modal("hide");
    El20Utils.mostrarCargador();

    var data_ = new FormData($("#postImportar")[0]);
    var url = $("#postImportar").attr("action");
    $.ajax({
        type: 'POST',
        //contentType: 'application/json',
        data: data_,
        processData: false,
        contentType: false,
        async: true,
        beforeSend: function () {
        },
        url: url,
        success: function (result) {
            console.log("result", result);
            if (!result.Success) {
                toastr["error"](result.Mensaje);
            } else {
                toastr["success"](result.Mensaje);
                $("input[name='Excel']").val("");
                $(".btn-save-import").attr("disabled", true);
                $('#table').DataTable().draw();
            }
            El20Utils.ocultarCargador();
            $("#ModalImporterClients").modal("show");

            //if ($.fn.DataTable.isDataTable('#clientesExcel')) {
            //    $('#clientesExcel').DataTable().destroy();
            //}
            //$('#clientesExcel tbody').empty();

            //$("div.alert").addClass("hide")
            //if (result != null && result.Success) {
            //    if (result.Tipo == 2 && result.SinGuardar.length > 0) {
            //        $(".alert-success").html(result.Mensaje).removeClass("hide");
            //        cargarTabla(result.SinGuardar);
            //        setTimeout(function () {
            //            $(".alert-success").addClass("hide");
            //        }, 7000);
            //    }
            //} else {
            //    if (result != null && result.Tipo == 0) {
            //        $(".alert-danger").html(result.Mensaje).removeClass("hide");
            //        //IntranetUtils.ocultarCargador();
            //        loading.hideloading();
            //        setTimeout(function () {
            //            $(".alert-danger").addClass("hide");
            //        }, 7000);
            //    } else if (result != null && result.Tipo == 1 && result.SinGuardar.length > 0) {
            //        $(".alert-warning").html(result.Mensaje).removeClass("hide");
            //        cargarTabla(result.SinGuardar);
            //        setTimeout(function () {
            //            $(".alert-warning").addClass("hide");
            //        }, 7000);
            //    }
            //}
        },
        error: function (xhr) {
            //console.log("error: " + xhr);
            //El20Utils.ocultarCargador();
            //loading.hideloading();
        }
    }).always(function () {
    });
}

function cargarTabla(jsonObject) {
    table = $('#clientesExcel').DataTable({
        "aaData": jsonObject,
        //"lengthMenu": [[50, 100, 200, 300, 400, 500], [50, 100, 200, 300, 400, 500]],
        "lengthMenu": IntranetUtils.GRID_TOTAL_ITEMS,
        "columns": [
            { title: "ID_CLIENTE", "data": "ID_CLIENTE" },
            { title: "NOMBRE", "data": "NOMBRE" },
            { title: "CORREO_ELECTRONICO", "data": "CORREO_ELECTRONICO" },
            { title: "PRESUPUESTO_ASIGNADO", "data": "PRESUPUESTO" },
            { title: "OBSERVACIONES", "data": "OBSERVACIONES" }
        ],
        "initComplete": function (settings, json) {
            //IntranetUtils.ocultarCargador();
            loading.hideloading();
        }
    });
}