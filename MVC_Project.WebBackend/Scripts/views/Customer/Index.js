$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

//var DateInit = JSON.parse(window.StartDate);
//$('#RegisterAt').daterangepicker(
//    {
//        format: 'DD/MM/YYYY',
//        todayBtn: "linked",
//        keyboardNavigation: false,
//        forceParse: false,
//        calendarWeeks: true,
//        autoclose: true,
//        language: 'es',
//        locale: {
//            applyLabel: "Aplicar",
//            cancelLabel: "Cancelar",
//            fromLabel: "De",
//            toLabel: "a"
//        },
//        minDate: DateInit.MinDate,
//        maxDate: DateInit.MaxDate
//        //opens: 'left'
//    }).on('apply.daterangepicker', function (e, picker) {
//        //console.log(picker.startDate.format('DD/MM/YYYY'));
//        //console.log(picker.endDate.format('DD/MM/YYYY'));
//        $('#FilterInitialDate').val(picker.startDate.format('DD/MM/YYYY'));
//        $('#FilterEndDate').val(picker.endDate.format('DD/MM/YYYY'));
//    });

$("#btnClearForm").click(function () {
    $("#SearchForm").each(function () {
        this.reset();
    });
    $('#table').DataTable().draw();
});

$(".btn-filter-rol").click(function () {
    $('#table').DataTable().draw();
});


var CustumerIndexControlador = function (htmlTableId, baseUrl, editUrl, downloadUrl, uploadUrl, hasFullAccessController) {
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
                { data: 'cellPhone', title: "Celular" },
                { data: 'email', title: "Email" },
                //{
                //    data: null, orderName: "createdAt", title: "Fecha Registro", autoWidth: false, className: "dt-center td-actions thead-dark",
                //    render: function (data, type, row, meta) {
                //        if (data.RegisterAt !== null && data.RegisterAt !== "") {
                //            return moment(data.RegisterAt).format('DD-MMM-YYYY');
                //        }
                //        return '';
                //    }
                //},
                {
                    data: null,
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a href="' + self.detailUrl + '?id=' + data.uuid + '" class="btn btn-light btn-view" title="Ver detalles"><span class="fas fa-file"></span></a>' +
                            '<a href="' + self.downloadUrl + '?id=' + data.uuid + '" class="btn btn-light btn-download" title="Descargar Detalle de Diagnóstico"><span class="fas fa-print"></i></span>' +
                            //'<button class="btn btn-light btn-delete" style="margin-left:5px;"><span class="fas fa-trash"></span></button>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
                {
                    data: null,
                    className: 'work-options',
                    render: function (data) {
                        //menu para el cliente work
                        //console.log(data)
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a href="' + self.detailUrl + '?id=' + data.uuid + '" class="btn btn-light btn-view" title="Ver detalles"><span class="fas fa-file"></span></a>' +
                            '<a href="' + self.downloadUrl + '?id=' + data.uuid + '" class="btn btn-light btn-download" title="Descargar Detalle de Diagnóstico"><span class="fas fa-print"></i></span>' +
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
                    if (json.iTotalRecords > 0) {
                        $(".btn-d0").css("display", "none");
                    }
                    //if (json.success === true)
                    fnCallback(json);
                    //else
                    //    console.log(json.Mensaje + " Error al obtener los elementos");
                });
            }
        });
    };
};