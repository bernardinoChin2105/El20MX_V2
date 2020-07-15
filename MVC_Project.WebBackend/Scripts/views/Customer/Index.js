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


var CustomerIndexControlador = function (htmlTableId, baseUrl, editUrl, downloadUrl, uploadUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
    this.downloadUrl = downloadUrl;
    this.uploadUrl = uploadUrl;
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
                { data: 'rfc', title: "RFC" },
                { data: 'businessName', title: "Nombre/Razón Social" },
                { data: 'phone', title: "Celular" },
                { data: 'email', title: "Email" },
                {
                    data: null,
                    title: "+ de Mis Clientes",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +                            
                            '<button class="btn btn-light btn-delete" style="margin-left:5px;"><span class="fas fa-ellipsis-h"></span></button>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
                {
                    data: null,
                    title: "Trabajar con Mis Clientes",
                    className: 'work-options',
                    render: function (data) {
                        //menu para el cliente work
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button class="btn btn-light btn-delete" style="margin-left:5px;"><span class="fas fa-ellipsis-h"></span></button>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

                $.getJSON(sSource, aoData, function (json) {
                    console.log(json)

                    if (json.success === true)
                        fnCallback(json);
                    else {
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }
                });
            }
        });
    };
};