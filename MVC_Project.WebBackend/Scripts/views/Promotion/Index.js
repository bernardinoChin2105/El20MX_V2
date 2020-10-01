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

var PromotionIndexControlador = function (htmlTableId, baseUrl, editUrl, ActiveInactiveUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
    this.ActiveInactiveUrl = ActiveInactiveUrl;
    this.dataTable = {};

    this.init = function () {
        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: El20Utils.lenguajeTabla({}),
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            deferLoading: 0,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'id', title: "Id", visible: false },
                { data: 'name', title: "Nombre" },
                { data: 'type', title: "Tipo" },
                { data: 'discountRate', title: "%" },
                { data: 'discountRate', title: "$" },
                {
                    data: null,
                    title: "Opciones",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        //var deshabilitar = data.status === "Activo" ? '<button class="btn btn-light btn-delete" title="Desactivar" style="margin-left:5px;"><span class="far fa-check-square "></span></button>' :
                        //    '<button class="btn btn-light btn-active" title="Activar" style="margin-left:5px;"><span class="far fa-square"></span></button>';

                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            //deshabilitar+
                            '<a href="' + self.editUrl + '?uuid=' + data.uuid + '" class="btn btn-light" title="Editar al aliado"><span class="fas fa-edit"></span></a>' +
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
                    console.log(json);
                    if (json.success === false) {
                        toastr['error'](json.Mensaje.message);
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });


        //$(this.htmlTable, "tbody").on('click',
        //    'td.menu-options .btn-group .btn-active',
        //    function () {
        //        var tr = $(this).closest('tr');
        //        var row = self.dataTable.row(tr);
        //        console.log(row.data().uuid, "josad");
        //        var id = row.data().uuid;

        //        swal({
        //            title: "Confirmación",
        //            text: "¿Desea activar la alianza?",
        //            showCancelButton: true,
        //            confirmButtonClass: "btn-danger",
        //            confirmButtonText: "Aceptar",
        //            cancelButtonText: "Cancelar",
        //            closeOnConfirm: false,
        //            closeOnCancel: false
        //        },
        //            function (isConfirm) {
        //                if (isConfirm) {
        //                    $.ajax({
        //                        type: 'POST',
        //                        async: true,
        //                        data: { uuid: id },
        //                        url: self.ActiveInactiveUrl,
        //                        success: function (data) {
        //                            if (!data.success) {
        //                                swal({
        //                                    type: 'error',
        //                                    title: "Error al realizar la operación.",
        //                                    text: data.error
        //                                });
        //                            } else {
        //                                swal("Estatus cambiado!");
        //                                self.dataTable.draw();
        //                            }
        //                        },
        //                        error: function (xhr) {
        //                            console.log('error');
        //                        }
        //                    });
        //                } else {
        //                    swal("Cancelado", "Operación cancelada", "error");
        //                }
        //            });
        //    });

        //$(this.htmlTable, "tbody").on('click',
        //    'td.menu-options .btn-group .btn-delete',
        //    function () {
        //        var tr = $(this).closest('tr');
        //        var row = self.dataTable.row(tr);
        //        console.log(row.data().uuid, "josad");
        //        var id = row.data().uuid;

        //        swal({
        //            title: "Confirmación",
        //            text: "¿Desea inactivar la alianza?",
        //            showCancelButton: true,
        //            confirmButtonClass: "btn-danger",
        //            confirmButtonText: "Aceptar",
        //            cancelButtonText: "Cancelar",
        //            closeOnConfirm: false,
        //            closeOnCancel: false
        //        },
        //            function (isConfirm) {
        //                if (isConfirm) {
        //                    $.ajax({
        //                        type: 'POST',
        //                        async: true,
        //                        data: { uuid: id },
        //                        url: self.ActiveInactiveUrl,
        //                        success: function (data) {
        //                            if (!data.success) {
        //                                swal({
        //                                    type: 'error',
        //                                    title: "Error al realizar la operación.",
        //                                    text: data.error
        //                                });
        //                            } else {
        //                                swal("Estatus cambiado!");
        //                                self.dataTable.draw();
        //                            }
        //                        },
        //                        error: function (xhr) {
        //                            console.log('error');
        //                        }
        //                    });
        //                } else {
        //                    swal("Cancelado", "Operación cancelada", "error");
        //                }
        //            });
        //    });
    };
};
