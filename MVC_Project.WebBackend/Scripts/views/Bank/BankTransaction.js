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

//bankAccountsUrl, getTokenUrl, unlinkBankUrl, createBankCredentialUrl,
var BankTransactionControlador = function (htmlTableId, baseUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    //this.bankAccountsUrl = bankAccountsUrl;
    //this.getTokenUrl = getTokenUrl;
    //this.unlinkBankUrl = unlinkBankUrl;
    //this.createBankCredentialUrl = createBankCredentialUrl;
    this.dataTable = {};

    this.init = function () {
        var primeravez = true;
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
                { data: 'transactionAt', title: "Fecha" },               
                { data: 'bankAccountName', title: "Cuenta/Banco" },
                { data: 'description', title: "Descripción Banco" },
                { data: 'amount', title: "Retiro" },
                { data: 'amount', title: "Depósitos" },
                { data: 'balance', title: "Saldo " },                
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {

                    console.log("respuesta", json)

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