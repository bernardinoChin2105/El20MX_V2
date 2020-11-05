$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

//$("#btnClearForm").click(function () {
//    $("#SearchForm").each(function () {
//        this.reset();
//    });
//    $('#table').DataTable().draw();
//});

//$(".btn-filter-rol").click(function () {
//    $('#table').DataTable().draw();
//});

var BankAccountControlador = function (htmlTableId, baseUrl, bankAccountEdit, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.bankAccountEdit = bankAccountEdit;
    this.dataTable = {};

    this.init = function () {
        //var primeravez = true;
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
                //{ data: 'name', title: "Nombre Cuenta" },
                {
                    data: null,
                    title: "Estatus de la Cuenta",
                    className: 'checked',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        var checked = data.isDisable === 0 ? 'checked' : '';
                        var check = '<div class="onoffswitch">' +
                            '<input type="checkbox" ' + checked + ' disabled="" class="onoffswitch-checkbox">' +
                            '<label class="onoffswitch-label">' +
                            '<span class="onoffswitch-inner"></span>' +
                            '<span class="onoffswitch-switch"></span>' +
                            '</label>' +
                            '</div>';
                        return check;//hasFullAccessController ? buttons : "";
                    }
                },
                { data: 'accountProviderType', title: "Nombre de la Cuenta" },
                { data: 'number', title: "Número de la Cuenta" },
                { data: 'clabe', title: "Clabe" },
                { data: 'currency', title: "Moneda" },
                //{data: 'refreshAt', title: "Fecha Saldo Inicial" },
                { data: 'balance', title: "Saldo Actual" },
                {
                    data: null,
                    title: "Estatus de Conexión",
                    className: 'work-conection',
                    render: function (data) {
                        var classCircle = data.status === '1' ? 'red' : 'green';
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<i class="fas fa-circle ' + classCircle + '"></i>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
                {
                    data: null,
                    title: "Acciones",
                    className: 'work-options',
                    render: function (data) {
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button class="btn btn-light btn-edit"><span class="fa fa-pencil-alt"></span></button>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filter", "value": $('form#SearchForm').serialize() });
                //aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {
                    if (json.success === false) {
                        toastr['error'](json.Mensaje.message, null, { 'positionClass': 'toast-top-center' }); 
                        console.log(json.Mensaje + " Error al obtener los elementos");
                    } else {
                        fnCallback(json);
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        $("#table tbody").on("click", "td.work-options .btn-group .btn-edit", function () {
            console.log("dentro");
            El20Utils.mostrarCargador();

            var tr = $(this).closest('tr');
            var row = self.dataTable.row(tr);
            var id = row.data().id;
            var clabe = row.data().clabe;

            $("#IdAccount").val(id);
            $("#clabe").val(clabe);
            El20Utils.ocultarCargador();
            $("#ModalEditClabe").modal("show");
        });      

        $("#ModalEditClabe").on('hidden.bs.modal', function () {
            console.log("cierre de modal");
            $("#IdAccount").val("");
            $("#clabe").val("");
        });

        $("#EditBankAccountForm").validate({
            rules: {
                clabe: {
                    required: true,
                    minlength: 18,
                    maxlength:18
                },
                password: {
                    required: true,
                    minlength: jQuery.validator.format("Ingrese no menos de {0} caracteres."),
                    maxlength: jQuery.validator.format("Ingrese no más de {0} caracteres.")
                }
            }
        });

        $(".btn-save").click(function () {

            if (!$('#EditBankAccountForm').valid()) {
                console.log("error")
                return;
            }

            $("#ModalEditClabe").modal("hide");
            El20Utils.mostrarCargador();

            console.log("siguiendo")

            //var data_ = new FormData($("#EditBankAccountForm")[0]);

            var data = {
                id: $("#IdAccount").val(),
                clabe: $("#clabe").val()
            };
            
            $.ajax({
                url: self.bankAccountEdit,
                type: 'POST',
                data: data,
                dataType: 'json',
                //async: true,
                success: function (result) {
                    console.log("result", result);
                    if (!result.success) {
                        toastr["error"](result.mensaje, null, { 'positionClass': 'toast-top-center' }); 
                    } else {
                        toastr["success"](result.mensaje, null, { 'positionClass': 'toast-top-center' }); 
                        self.dataTable.draw();
                    }
                    El20Utils.ocultarCargador();
                }
            }).always(function () {
            });
        });
    };
};