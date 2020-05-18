var UserPermissionIndexControlador = function (htmlTableId, baseUrl, modalEditAction, modalDeleteAction) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.dataTable = {};
    this.init = function () {
        self.dataTable = this.htmlTable.DataTable({
            language: { url: 'Scripts/template/plugins/dataTables/lang/es_MX.json' },
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'Id', title: "Id", visible: false },
                { data: 'Name', title: "Nombre" },
                { data: 'Email', title: "Email" },
                {
                    data: null, orderName: "CreatedAt", title: "Fecha Creación", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.CreatedAt != null && data.CreatedAt !== "") {
                            return moment(data.CreatedAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                {
                    data: null, orderName: "UpdatedAt", title: "Fecha Modificación", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.UpdatedAt != null && data.UpdatedAt !== "") {
                            return moment(data.UpdatedAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                {
                    data: null,
                    className: 'personal-options',
                    render: function (data) {
                        //console.log(data)
                        var deshabilitar = "";
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            deshabilitar +
                            '<button class="btn btn-light btn-edit"><span class="fas fa-edit"></span></button>' +
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
        $(self.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-edit',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var uuid = row.data().Uuid;

                var form = document.createElement('form');
                document.body.appendChild(form);
                form.method = 'GET';
                form.action = "/UserPermissions/Edit?uuid=" + uuid;

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