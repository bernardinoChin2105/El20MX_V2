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

var DateInit = JSON.parse(window.StartDate);
$('#RegisterAt').daterangepicker({
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
        //minDate: DateInit.MinDate,
        maxDate: DateInit.MaxDate
        //opens: 'left'
    }).on('apply.daterangepicker', function (e, picker) {
        //console.log(picker.startDate.format('DD/MM/YYYY'));
        //console.log(picker.endDate.format('DD/MM/YYYY'));
        $('#FilterInitialDate').val(picker.startDate.format('DD/MM/YYYY'));
        $('#FilterEndDate').val(picker.endDate.format('DD/MM/YYYY'));
    });


var CustomerInvoicesControlador = function (htmlTableId, baseUrl, downloadPdfUrl, downloadXmlUrl, autocompleteURL, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.downloadPdfUrl = downloadPdfUrl;
    this.downloadXmlUrl = downloadXmlUrl;
    this.autocompleteURL = autocompleteURL; 
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
                { data: 'folio', title: "No. CFDI" },
                { data: 'invoicedAt', title: "Fecha Factura" },
                { data: 'rfc', title: "RFC Cliente" },
                { data: 'businessName', title: "Cliente" },
                { data: 'paymentMethod', title: "Método Pago" },
                { data: 'paymentFormDescription', title: "Forma Pago" },
                { data: 'currency', title: "Divisa" },
                { data: 'amount', title: "SubTotal" },
                { data: 'iva', title: "IVA" },
                { data: 'totalAmount', title: "Total" },
                {
                    data: null,
                    title: "Archivos",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a href="' + self.downloadPdfUrl + '?id=' + data.id + '" class="btn btn-light btn-downloadPdf" title="Descargar PDF"><span class="fas fa-file-pdf"></i></span>' +
                            '<a href="' + self.downloadXmlUrl + '?id=' + data.id + '" class="btn btn-light btn-downloadXml" title="Descargar XML"><span class="fas fa-file-alt"></span></a>' +                            
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
                    console.log(json);
                    fnCallback(json);

                    if (json.success === false) {
                        toastr['error'](json.error);
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });
    };

    $.get(self.autocompleteURL, function (data) {
        console.log(data,"asdasda")
        $(".typeahead_2").typeahead({ source: data.Data });
    }, 'json');
};

$("#RFC").keyup(function () {
    this.value = this.value.toUpperCase();
});