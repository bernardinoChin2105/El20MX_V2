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
        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            language: { url: '//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json' },
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
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });

                $.getJSON(sSource, aoData, function (json) {
                    //console.log(json);

                    if (json.success) {
                        if (json.iTotalRecords > 0) {
                            $(".btn-d0").css("display", "none");
                        }
                        fnCallback(json);
                    }
                    else {
                        toastr['error'](json.message);
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });
    };
};