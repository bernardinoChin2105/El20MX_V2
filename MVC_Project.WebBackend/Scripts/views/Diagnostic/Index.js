$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

var DateInit = JSON.parse(window.StartDate);
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
        minDate: DateInit.MinDate,
        maxDate: DateInit.MaxDate
        //opens: 'left'
    }).on('apply.daterangepicker', function (e, picker) {
    //console.log(picker.startDate.format('DD/MM/YYYY'));
    //console.log(picker.endDate.format('DD/MM/YYYY'));
    $('#FilterInitialDate').val(picker.startDate.format('DD/MM/YYYY'));
    $('#FilterEndDate').val(picker.endDate.format('DD/MM/YYYY'));
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


var DianosticIndexControlador = function (htmlTableId, baseUrl, detailUrl, downloadUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.detailUrl = detailUrl;
    this.downloadUrl = downloadUrl;
    this.dataTable = {};

    this.init = function () {
        self.dataTable = this.htmlTable.DataTable({
            language: { url: 'Scripts/custom/dataTableslang.es_MX.json' },
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'id', title: "Id", visible: false },
                {
                    data: null, orderName: "createdAt", title: "Fecha Registro", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.RegisterAt !== null && data.RegisterAt !== "") {
                            return moment(data.RegisterAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                { data: 'businessName', title: "Nombre/Razón Social" },
                { data: 'rfc', title: "RFC" },
                { data: 'plans', title: "Plan" },
                {
                    data: null,
                    className: 'personal-options',
                    render: function (data) {
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a href="' + self.detailUrl + '?id=' + data.uuid + '" class="btn btn-light btn-view" title="Ver detalles"><span class="fas fa-file"></span></a>' +
                            '<a href="' + self.downloadUrl + '?id=' + data.uuid + '" class="btn btn-light btn-download" title="Descargar Detalle de Diagnóstico"><span class="fas fa-print"></i></span>'+
                            //'<button class="btn btn-light btn-delete" style="margin-left:5px;"><span class="fas fa-trash"></span></button>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": getFiltros("form#SearchForm") });

                $.getJSON(sSource, aoData, function (json) {
                    //console.log(json);
                    if (json.iTotalRecords > 0) {
                        $(".btn-d0").css("display", "none");
                    }
                    //if (json.success === true)
                    fnCallback(json);
                    //else
                    //    console.log(json.Mensaje + " Error al obtener los elementos");
                });
            }
        });

        function getFiltros(form) {
            var $inputs = $(form + ' [filtro="true"]');
            var nFiltros = $inputs.length;
            var filtros = [];
            for (i = 1; i <= nFiltros; i++) {
                var input = $.each($inputs, function (item) { return $(item).attr('filtro-order') === i; });
                filtros.push($(input).val());
            }

            return JSON.stringify(filtros);
        }

        //$(this.htmlTable, "tbody").on('click',
        //    'td.personal-options .btn-group .btn-active',
        //    function () {
        //        var tr = $(this).closest('tr');
        //        var row = self.dataTable.row(tr);
        //        var id = row.data().Uuid;

        //        swal({
        //            title: "Confirmación",
        //            text: "¿Desea activar el rol?",
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
        //                        url: '/Role/Delete',
        //                        success: function (data) {
        //                            if (!data) {
        //                                swal({
        //                                    type: 'error',
        //                                    title: data.Mensaje.Titulo,
        //                                    text: data.Mensaje.Contenido
        //                                })
        //                            } else {
        //                                swal("Estatus cambiado!");
        //                                self.dataTable.ajax.reload();
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
        //    'td.personal-options .btn-group .btn-delete',
        //    function () {
        //        var tr = $(this).closest('tr');
        //        var row = self.dataTable.row(tr);
        //        var id = row.data().Uuid;

        //        swal({
        //            title: "Confirmación",
        //            text: "¿Desea inactivar el rol?",
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
        //                        url: '/Role/Delete',
        //                        success: function (data) {
        //                            if (!data) {
        //                                swal({
        //                                    type: 'error',
        //                                    title: data.Mensaje.Titulo,
        //                                    text: data.Mensaje.Contenido
        //                                })
        //                            } else {
        //                                swal("Estatus cambiado!");
        //                                self.dataTable.ajax.reload();
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

        //$(self.htmlTable, "tbody").on('click',
        //    'td.personal-options .btn-group .btn-edit',
        //    function () {
        //        var tr = $(this).closest('tr');
        //        var row = self.dataTable.row(tr);
        //        var uuid = row.data().Uuid;

        //        var form = document.createElement('form');
        //        document.body.appendChild(form);
        //        form.method = 'GET';
        //        form.action = "/Role/Edit?uuid=" + uuid;

        //        var input = document.createElement('input');
        //        input.type = 'hidden';
        //        input.name = "uuid";
        //        input.value = uuid;
        //        form.appendChild(input);
        //        form.submit();
        //    });

    };
};