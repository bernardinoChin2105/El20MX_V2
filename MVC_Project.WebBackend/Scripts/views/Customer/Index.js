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


var CustomerIndexControlador = function (htmlTableId, baseUrl, editUrl, exportUrl, uploadUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
    this.exportUrl = exportUrl;
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
                { data: 'phone', title: "Teléfono" },
                { data: 'email', title: "Email" },
                {
                    data: null,
                    title: "+ de Mis Clientes",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<div class="dropdown">' +
                            '<button class="btn btn-light btn-menu" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><span class="fas fa-ellipsis-h"></span></button>' +
                            '<div class="dropdown-menu" aria-labelledby="dropdownMenuButton">' +
                            '<a class="dropdown-item" href="' + self.editUrl + '?uuid=' + data.uuid + '">Perfil Completo del Cliente</a>' +
                            '<a class="dropdown-item" href="#">Estado de Cuenta</a>' +
                            '<a class="dropdown-item" href="#">Lista de FActuras(CFDI\'s)</a>' +
                            '</div>' +
                            '</div>' +
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
                        //style="margin-left:5px;"
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<div class="dropdown">' +
                            '<button class="btn btn-light btn-menu" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><span class="fas fa-ellipsis-h"></span></button>' +
                            '<div class="dropdown-menu" aria-labelledby="dropdownMenuButton">' +
                            '<a class="dropdown-item" href="' + self.editUrl + '?uuid=' + data.uuid + '">Hacer CFDI</a>' +
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

                $.getJSON(sSource, aoData, function (json) {
                    //console.log(json)

                    if (json.success === false) {
                        toastr['error'](json.Mensaje.message);
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    } else {
                        fnCallback(json);
                    }
                });
            }
        });


        $(".btn-export").click(function () {            
            try {
                
                var aoData = [];
                    aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

                //IntranetUtils.mostrarCargador();
                //$(document).ready(function (e) {

                $.fileDownload(self.exportUrl, {
                    httpMethod: "POST",
                    data: aoData
                })
                    .done(function () { /*IntranetUtils.ocultarCargador();*/ })
                    .fail(function () { /*IntranetUtils.ocultarCargador(); */});

                //});

                /////////////////////////////////////////////////////
            } catch (e) {
                throw 'CustomerIndexControlador -> Exportar: ' + e;
            }
        });

    };
};
