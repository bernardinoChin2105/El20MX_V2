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

//hasFullAccessController
var CustomerInvoicesControlador = function (htmlTableId, baseUrl) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.dataTable = {};

    this.init = function () {
        var primeravez = true;

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
                { data: 'rfc', title: "No. CFDI" },
                { data: 'rfc', title: "Fecha Factura" },
                { data: 'rfc', title: "RFC Cliente" },
                { data: 'businessName', title: "Cliente" },
                { data: 'phone', title: "Método Pago" },
                { data: 'email', title: "Forma Pago" },
                { data: 'rfc', title: "Divisa" },
                { data: 'businessName', title: "SubTotal" },
                { data: 'phone', title: "IVA" },
                { data: 'email', title: "Total" },
                {
                    data: null,
                    title: "Archivos",
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
                            '<a class="dropdown-item" href="#">Lista de Facturas(CFDI\'s)</a>' +
                            '</div>' +
                            '</div>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }               
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {
                    primeravez = false;
                    fnCallback(json);

                    if (json.success === false) {
                        toastr['error'](json.Mensaje.message);
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    } 
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });        
    };
};

$("#RFC").keyup(function () {
    this.value = this.value.toUpperCase();
});