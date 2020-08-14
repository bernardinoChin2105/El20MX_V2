var OrderIndexControlador = function (htmlTableId, btnClearFormId, searchFormId, baseUrl, modalDetailAction, modalDeleteAction) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.dataTable = {};
    this.btnClearForm = $('#' + btnClearFormId);
    this.searchForm = $('#' + searchFormId);
    this._modal = $('#modalDetails-container');
    this.modalDetail = modalDetailAction;
    this.initBtnClearForm = function () {
        this.btnClearForm.click(function () {
            self.searchForm.each(function () {
                this.reset();
            });
            self.dataTable.ajax.reload();
        });
    }
    this.init = function modalDetailAction() {
        self.initBtnClearForm();
        self.dataTable = this.htmlTable.DataTable({
            language: El20Utils.lenguajeTabla({}),
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'Id', title: "Folio", visible: true },
                { data: 'Cliente', title: "Cliente" },
                { data: 'Estatus', title: "Estatus" },
                { data: 'Tienda', title: "Tienda" },
                { data: 'Vendedor', title: "Vendedor" },
                {
                    data: null, orderName: "CreatedAt", title: "Fecha Creación", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.CreatedAt !== null && data.CreatedAt !== "") {
                            return moment(data.CreatedAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                {
                    data: null, orderName: "ShipperAt", title: "Fecha de entrega", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.UpdatedAt !== null && data.UpdatedAt !== "") {
                            return moment(data.UpdatedAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                {
                    data: null,
                    className: 'order-options',
                    render: function (data) {
                        //console.log(data)
                        var deshabilitar = "";
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            deshabilitar +
                            '<button class="btn btn-light btn-details"><span class="fas fa-info"></span></button>' +
                            //'<button class="btn btn-light btn-delete" style="margin-left:5px;"><span class="fas fa-trash"></span></button>' +
                            '</div>';
                        return buttons;
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": getFiltros("form#SearchForm") });

                $.getJSON(sSource, aoData, function (json) {
                    //if (json.success === true)
                    fnCallback(json);
                    //else
                    //    console.log(json.Mensaje + " Error al obtener los elementos");
                });
            }
        });
        $(this.htmlTable, "tbody").on('click',
            'td.order-options .btn-group .btn-details',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                self._modal.find('.modal-body').load(self.modalDetail + "?orderId=" + row.data().Id,
                    function () {
                        self._modal.modal('show');
                        //$("#btn-actionDescuento-edit").on("click",
                        //    function () {
                        //        if (!$('#edit-descuentoImpuesto-form').valid()) {
                        //            return;
                        //        }
                        //        var formData = $('#edit-descuentoImpuesto-form').serializeFormJSON();
                        //        $.ajax({
                        //            type: "POST",
                        //            url: $('#edit-descuentoImpuesto-form').attr('action'),
                        //            data: JSON.stringify(formData),
                        //            dataType: "json",
                        //            contentType: 'application/json; charset=utf-8',
                        //            processData: false,
                        //            success: function (result) {
                        //                operacionExitosa();
                        //                self._modal.modal('hide');
                        //                self.dataTable.ajax.reload();
                        //            },
                        //            error: function (jqXHR, textStatus, errorThrown) {
                        //                console.log("fail");
                        //            }
                        //        });
                        //    });
                    });
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