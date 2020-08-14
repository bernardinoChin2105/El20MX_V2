$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

//$("#btnClearForm").click(function () {
//    $("#SearchForm").each(function () {
//        this.reset();
//    });
//    $('#table').DataTable().draw();
//});

//$(".btn-filter-rol").click(function () {
//    $('#table').DataTable().draw();
//});

//  exportUrl, uploadUrl,
var BankIndexControlador = function (htmlTableId, baseUrl, editUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
    //this.exportUrl = exportUrl;
    //this.uploadUrl = uploadUrl;
    this.dataTable = {};

    this.init = function () {
        var primeravez = true;

        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: { url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json' },
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'id', title: "Id", visible: false },
                { data: 'bank', title: "Bancos" },
                { data: 'status', title: "Estatus" },
                //{ data: 'phone', title: "Teléfono" },
                //{ data: 'email', title: "Email" },
                {
                    data: null,
                    title: "Configuración de Bancos",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +                                            
                            '<a class="link" href="' + self.editUrl + '?uuid=' + data.uuid + '">Ver más</a>' +                                                                                
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
                {
                    data: null,
                    title: "Desvincular",
                    className: 'work-options',
                    render: function (data) {
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            
                            '<button class="btn btn-light btn-desvincular" id=""><span class="fa fa-trash"></span></button>' +                                                        
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            //"fnServerData": function (sSource, aoData, fnCallback) {
            //    aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
            //    aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
            //    aoData.push({ "name": "first", "value": primeravez });

            //    $.getJSON(sSource, aoData, function (json) {
            //        primeravez = false;
            //        if (json.success === false) {
            //            toastr['error'](json.Mensaje.message);
            //            console.log(json.Mensaje + " Error al obtener los elementos");
            //        } else {
            //            fnCallback(json);
            //        }
            //    });
            //}
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });



        var params = {
            // Set up the token you created in the Quickstart:
            //token: 'd9e2879cc10cfde6924573a8ecd64489f1f5e7dba21d461cbfbf1e58b3b962df',
            config: {
                // Set up the language to use:
                locale: 'es',
                entrypoint: {
                    // Set up the country to start:
                    country: 'MX',
                    // Set up the site organization type to start:
                    siteOrganizationType: '56cf4f5b784806cf028b4568'
                },
                navigation: {
                    displayStatusInToast: true,
                    // Hide the site 'Renapo'
                    //"hideSites": ["Renapo"],
                    // Hide the 'Blockchain' and 'Digital Wallets' organization types.
                    "hideSiteOrganizationTypes": ["Blockchain", "Digital Wallet", "Government", "Utility"]
                }
            }
        };
        var syncWidget = new SyncWidget(params);
        //syncWidget.open();

        //syncWidget.$on("success", function (credential) {
        //    console.log(credential, "credencial")
        //    // do something on success
        //});
        //$(".btn-export").click(function () {
        //    try {

        //        var aoData = [];
        //        aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

        //        El20Utils.mostrarCargador();

        //        $.fileDownload(self.exportUrl, {
        //            httpMethod: "POST",
        //            data: aoData
        //        }).done(function () { El20Utils.ocultarCargador(); })
        //            .fail(function () { El20Utils.ocultarCargador(); });


        //        /////////////////////////////////////////////////////
        //    } catch (e) {
        //        throw 'CustomerIndexControlador -> Exportar: ' + e;
        //    }
        //});
    };
};

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
//                toastr["error"](result.Mensaje);
//            } else {
//                toastr["success"](result.Mensaje);
//                $("input[name='Excel']").val("");
//                $(".btn-save-import").attr("disabled", true);
//                $('#table').DataTable().draw();
//            }
//            El20Utils.ocultarCargador();
//            $("#ModalImporterClients").modal("show");

//            //if ($.fn.DataTable.isDataTable('#clientesExcel')) {
//            //    $('#clientesExcel').DataTable().destroy();
//            //}
//            //$('#clientesExcel tbody').empty();

//            //$("div.alert").addClass("hide")
//            //if (result != null && result.Success) {
//            //    if (result.Tipo == 2 && result.SinGuardar.length > 0) {
//            //        $(".alert-success").html(result.Mensaje).removeClass("hide");
//            //        cargarTabla(result.SinGuardar);
//            //        setTimeout(function () {
//            //            $(".alert-success").addClass("hide");
//            //        }, 7000);
//            //    }
//            //} else {
//            //    if (result != null && result.Tipo == 0) {
//            //        $(".alert-danger").html(result.Mensaje).removeClass("hide");
//            //        //IntranetUtils.ocultarCargador();
//            //        loading.hideloading();
//            //        setTimeout(function () {
//            //            $(".alert-danger").addClass("hide");
//            //        }, 7000);
//            //    } else if (result != null && result.Tipo == 1 && result.SinGuardar.length > 0) {
//            //        $(".alert-warning").html(result.Mensaje).removeClass("hide");
//            //        cargarTabla(result.SinGuardar);
//            //        setTimeout(function () {
//            //            $(".alert-warning").addClass("hide");
//            //        }, 7000);
//            //    }
//            //}
//        },
//        error: function (xhr) {
//            //console.log("error: " + xhr);
//            //El20Utils.ocultarCargador();
//            //loading.hideloading();
//        }
//    }).always(function () {
//    });
//}