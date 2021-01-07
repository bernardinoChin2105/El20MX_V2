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

$('.chosen-select').chosen({ width: '100%', "disable_search": true, no_results_text: "Sin resultados para: " });

var ChargesIndexControlador = function (htmlTableId, baseUrl, editUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
    this.dataTable = {};

    this.init = function () {
        var primeravez = false;

        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: El20Utils.lenguajeTabla({}),
            "bProcessing": false,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'id', title: "Id", visible: false },
                { data: 'businessName', title: "Nombre/Razón Social" },
                { data: 'rfc', title: "RFC" },
                { data: 'billingStart', title: "Inicio Facturación" },
                { data: 'plan', title: "Plan fijo" },
                { data: 'status', title: "Estatus en el Sistema" },
                {
                    data: null,
                    title: "Opciones",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a class="btn btn-light btn-edit" href="' + self.editUrl + '?uuid=' + data.uuid + '"><span class="fas fa-edit"></span></a>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {
                    primeravez = false;
                    fnCallback(json);
                    if (json.success === false) {
                        toastr['error'](json.message, null, { 'positionClass': 'toast-top-center' }); 
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        
    };
};
