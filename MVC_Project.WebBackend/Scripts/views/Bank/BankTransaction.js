$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

$("#btnClearForm").click(function () {
    $("#SearchForm").each(function () {
        this.reset();
        $("#NumberBankAccount").html('<option value="-1">Todos</option>');
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
        //minDate: DateInit.MinDate,
        maxDate: DateInit.MaxDate,
        startDate: DateInit.MinDate,
        //opens: 'left'
    }).on('apply.daterangepicker', function (e, picker) {
        //console.log(picker.startDate.format('DD/MM/YYYY'));
        //console.log(picker.endDate.format('DD/MM/YYYY'));
        $('#FilterInitialDate').val(picker.startDate.format('DD/MM/YYYY'));
        $('#FilterEndDate').val(picker.endDate.format('DD/MM/YYYY'));             
    });

//bankAccountsUrl, getTokenUrl, unlinkBankUrl, createBankCredentialUrl,
var BankTransactionControlador = function (htmlTableId, baseUrl, GetBankAccountUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.GetBankAccountUrl = GetBankAccountUrl;
    //this.getTokenUrl = getTokenUrl;
    //this.unlinkBankUrl = unlinkBankUrl;
    //this.createBankCredentialUrl = createBankCredentialUrl;
    this.dataTable = {};

    this.init = function () {
        var primeravez = true;
        if (primeravez) {
            $('#FilterInitialDate').val(DateInit.MinDate.format('DD/MM/YYYY'));
            $('#FilterEndDate').val(DateInit.MaxDate.format('DD/MM/YYYY'));     
            $('#RegisterAt').val(DateInit.MinDate.format('DD/MM/YYYY') + ' - ' + DateInit.MaxDate.format('DD/MM/YYYY'));
        }

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
                { data: 'amountR', title: "Retiro" },
                { data: 'amountD', title: "Depósitos" },
                { data: 'balance', title: "Saldo " },
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {

                    console.log("respuesta", json);

                    primeravez = false;
                    fnCallback(json);                    
                    if (json.success === false) {
                        toastr['error'](json.error);
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    }

                    json.totales.currency !== null ? $("#Currency").val(json.totales.currency) : $("#Currency").val("");
                    json.totales.TotalAmount !== null ? $("#CurrentBalance").val(json.totales.TotalAmount) : $("#CurrentBalance").val("");
                    json.totales.TotalRetirement !== null ? $("#TotalAmount").val(json.totales.TotalRetirement) : $("#TotalAmount").val("");
                    json.totales.TotalDeposits !== null ? $("#TotalDeposits").val(json.totales.TotalDeposits) : $("#TotalDeposits").val("");
                    json.totales.TotalFinal !== null ? $("#FinalBalance").val(json.totales.TotalFinal) : $("#FinalBalance").val("");
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        $("#BankName").change(function () {
            El20Utils.mostrarCargador();
            var cbmAccount = $("#NumberBankAccount");
            var id = $(this).val();

            if (id !== "-1") {
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    //async: true,
                    data: { credentialId: id },
                    url: self.GetBankAccountUrl,
                    success: function (result) {
                        console.log("result", result);

                        if (!result.success) {
                            toastr["error"](result.message);
                        } else {
                            cbmAccount.html("");
                            cbmAccount.append(result.data.map(function (data, index) {
                                return $('<option value="' + data.Value + '">' + data.Text + '</option>');
                            }));
                            cbmAccount.val("-1");
                        }
                        El20Utils.ocultarCargador();
                    },
                    error: function (xhr) {
                        //console.log("error: " + xhr);
                        El20Utils.ocultarCargador();
                        //loading.hideloading();
                    }
                }).always(function () {
                });
            }
            else {
                cbmAccount.html("");
                cbmAccount.html('<option value="-1">Todos</option>');
            }
        });
    };
};