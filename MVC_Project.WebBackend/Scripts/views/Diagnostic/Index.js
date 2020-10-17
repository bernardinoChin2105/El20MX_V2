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
        //minDate: DateInit.MinDate,
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
            language: El20Utils.lenguajeTabla({}),
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
                        if (data.createdAt !== null && data.createdAt !== "") {
                            return moment(data.createdAt).format('DD-MMM-YYYY');
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
                    if (json.success) {
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

    this.finishExtraction = function (uuid) {
        $.get("Diagnostic/FinishExtraction", { uuid : uuid }, function (data) {
            if (data.finish) {
                if (data.success) {
                    El20Utils.ocultarCargador();
                    let form = document.createElement("form");
                    form.setAttribute("method", "GET");
                    form.setAttribute("action", "/Diagnostic/DiagnosticDetail");

                    let hiddenClave = document.createElement("input");
                    hiddenClave.setAttribute("type", "hidden");
                    hiddenClave.setAttribute("name", "id");
                    hiddenClave.setAttribute("value", uuid);
                    form.appendChild(hiddenClave);

                    document.body.appendChild(form);
                    form.submit();
                }
                else {
                    El20Utils.ocultarCargador();  
                    toastr['error'](data.message);
                }
            }
            else {
                setTimeout(self.finishExtraction(uuid), 2000);
            }
        }).fail(function () {
            toastr['error']('No es posible finalizar el diagnostico');
            El20Utils.ocultarCargador();
        });
    }

    $('#btn-generate-diagnostic').on("click", function () {
        El20Utils.mostrarCargador();
        $.get("Diagnostic/GenerateDx0", function (data) {
            if (data.success) {
                self.finishExtraction(data.uuid)
            }
            else {
                toastr['error'](data.message);
                El20Utils.ocultarCargador();
            }
        }).fail(function () {
            toastr['error']('No es posible generar el diagnostico');
            El20Utils.ocultarCargador();
        });
    });
};