var QuotationIndexControlador = function (htmlTableId, baseUrl, modalEditAction, modalDeleteAction, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.dataTable = {};

    this.init = function () {
        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: El20Utils.lenguajeTabla({}),
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'id', title: "Id", visible: false },
                { data: 'uuid', title: "Uuid", visible: false },
                { data: 'account', title: "Cliente" },
                {
                    data: null, orderName: "startedAt", title: "Fecha", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.startedAt !== null && data.startedAt !== "") {
                            return moment(data.startedAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                { data: 'total', title: "Monto" },
                { data: 'partialitiesNumber', title: "Parcialidades" },
                { data: 'status', title: "Estatus" },
                {
                    data: null,
                    title: "Detalle",
                    className: '',
                    render: function (data) {
                        var link = "<ul>";
                        $.each(data.details, function (key, value) {
                            link += "<li><a href='" + value.link + "'>" + value.name + " </a></li>";//class='btn btn-success btn-xs btn-block btn-outline'
                        });
                        link += "</ul>";
                        return link;
                    }
                },
                {
                    data: null,
                    className: 'personal-options',
                    render: function (data) {
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button class="btn btn-light btn-edit"><span class="fas fa-edit"></span></button>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": getFiltros("form#SearchForm") });

                $.getJSON(sSource, aoData, function (json) {
                    if (json.success) {
                        fnCallback(json);
                    }
                    else {
                        toastr['error'](json.message, null, { 'positionClass': 'toast-top-center' }); 
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });;

        $(this.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-active',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var id = row.data().uuid;

                swal({
                    title: "Confirmación",
                    text: "¿Desea activar la cotización?",
                    showCancelButton: true,
                    confirmButtonClass: "btn-danger",
                    confirmButtonText: "Aceptar",
                    cancelButtonText: "Cancelar",
                    closeOnConfirm: false,
                    closeOnCancel: false
                },
                    function (isConfirm) {
                        if (isConfirm) {
                            $.ajax({
                                type: 'POST',
                                async: true,
                                data: { uuid: id },
                                url: '/Quotation/Delete',
                                success: function (data) {
                                    if (!data) {
                                        swal({
                                            type: 'error',
                                            title: data.Mensaje.Titulo,
                                            text: data.Mensaje.Contenido
                                        })
                                    } else {
                                        swal("Cotización activada");
                                        self.dataTable.ajax.reload();
                                    }
                                },
                                error: function (xhr) {
                                    console.log('error');
                                }
                            });
                        } else {
                            swal("Cancelado", "Operación cancelada", "error");
                        }
                    });
            });

        $(this.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-delete',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var id = row.data().uuid;

                swal({
                    title: "Confirmación",
                    text: "¿Desea desactivar la cotización?",
                    showCancelButton: true,
                    confirmButtonClass: "btn-danger",
                    confirmButtonText: "Aceptar",
                    cancelButtonText: "Cancelar",
                    closeOnConfirm: false,
                    closeOnCancel: false
                },
                    function (isConfirm) {
                        if (isConfirm) {
                            $.ajax({
                                type: 'POST',
                                async: true,
                                data: { uuid: id },
                                url: '/Quotation/Delete',
                                success: function (data) {
                                    if (!data) {
                                        swal({
                                            type: 'error',
                                            title: data.Mensaje.Titulo,
                                            text: data.Mensaje.Contenido
                                        })
                                    } else {
                                        swal("Cotización desactivada");
                                        self.dataTable.ajax.reload();
                                    }
                                },
                                error: function (xhr) {
                                    console.log('error');
                                }
                            });
                        } else {
                            swal("Cancelado", "Operación cancelada", "error");
                        }
                    });
            });

        $(self.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-edit',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var uuid = row.data().uuid;

                var form = document.createElement('form');
                document.body.appendChild(form);
                form.method = 'GET';
                form.action = "/Quotation/Edit?uuid=" + uuid;

                var input = document.createElement('input');
                input.type = 'hidden';
                input.name = "uuid";
                input.value = uuid;
                form.appendChild(input);
                form.submit();
            });
        function getFiltros(form) {
            var $inputs = $(form + ' [filtro="true"]');
            var nFiltros = $inputs.length;
            //alert(nFiltros);
            var filtros = [];
            for (i = 1; i <= nFiltros; i++) {
                var input = $.grep($inputs, function (item) { return $(item).attr('filtro-order') == i; });
                filtros.push($(input).val());
            }

            return JSON.stringify(filtros);
        }        
    }
}