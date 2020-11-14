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
    if (!$('#SearchForm').valid()) {
        return;
    }
    $('#table').DataTable().draw();
});

$.validator.addMethod("Alphanumeric",
    function (value, element) {
        return value.match(/^[A-Za-zÀ-ÿ\u00f1\u00d10-9 _.-]+$|^$/);
    }, "El campo debe ser alfanumérico"
);
$("#SearchForm").validate({
    rules: {
        Name: {
            Alphanumeric: true
        }
    }
});

var AllyIndexControlador = function (htmlTableId, baseUrl, editUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.editUrl = editUrl;
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
                { data: 'createdAt', title: "Fecha creación" },
                {
                    data: null,
                    title: "Opciones",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)

                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
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
                    if (json.success === false) {
                        toastr['error'](json.Mensaje.message, null, { 'positionClass': 'toast-top-center' }); 
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });
    };
};
