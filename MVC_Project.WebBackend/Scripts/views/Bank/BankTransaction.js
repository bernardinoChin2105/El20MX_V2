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
                //{ data: 'bank', title: "" },
                {
                    data: null,
                    title: "Estatus de la Cuenta",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        console.log(data)
                        //<div class="onoffswitch">
                        //    <input type="checkbox" checked="" disabled="" class="onoffswitch-checkbox" id="example3">
                        //        <label class="onoffswitch-label" for="example3">
                        //            <span class="onoffswitch-inner"></span>
                        //            <span class="onoffswitch-switch"></span>
                        //        </label>
                        //</div>
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a class="link" href="' + self.bankAccountsUrl + '?uuid=' + data.uuid + '">Ver más</a>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
                { data: 'bank', title: "Nombre de la Cuenta" },
                { data: 'bank', title: "Número de la Cuenta" },
                { data: 'bank', title: "Clabe" },
                { data: 'bank', title: "Moneda" },
                //{data: 'phone', title: "Fecha Saldo Inicial" },
                { data: 'email', title: "Saldo Inicial" },
                { data: 'status', title: "Estatus de Conexión" },
                {
                    data: null,
                    title: "Acciones",
                    className: 'work-options',
                    render: function (data) {
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button class="btn btn-light btn-desvincular" data-uuid="' + data.uuid + '"><span class="fa fa-trash"></span></button>' +
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



        var params = {
            // Set up the token you created in the Quickstart:
            token: '58aa3a0a0a98be385f473dee61dca92e04392a6a29c2843359e08369f983f6b0',
            config: {
                // Set up the language to use:
                locale: 'es',
                entrypoint: {
                    // Set up the country to start:
                    country: 'MX',
                    // Set up the site organization type to start:
                    //siteOrganizationType: '56cf4f5b784806cf028b4568'
                },
                navigation: {
                    displayStatusInToast: true,
                    // Hide the site 'Renapo'
                    //"hideSites": ["Renapo"],
                    // Hide the 'Blockchain' and 'Digital Wallets' organization types.
                    "hideSiteOrganizationTypes": ["Blockchain", "Digital Wallet", "Government", "Utility"]
                }
            }
        };

        self.syncWidget = new SyncWidget(params);
        //syncWidget.open();

        self.syncWidget.$on("success", function (credential) {
            console.log(credential, "credencial");
            // do something on success
            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                async: true,
                data: { idCredential: credential },
                url: self.createBankCredentialUrl,
                success: function (result) {
                    console.log("result", result);

                    if (!result.success) {
                        toastr["error"](result.Mensaje);
                    } else {
                        toastr["success"](result.Mensaje);
                        self.dataTable.DataTable().draw();
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
        });


        $(".btn-token").click(function () {
            El20Utils.mostrarCargador();
            try {
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    //async: true,
                    //data: { token: self.token },
                    url: self.getTokenUrl,
                    success: function (result) {
                        console.log("result", result);

                        if (!result.success) {
                            toastr["error"](result.Mensaje.message);
                        } else {
                            //toastr["success"](result.Mensaje);
                            self.syncWidget.setToken(result.data);
                            self.syncWidget.open();
                        }
                        El20Utils.ocultarCargador();
                        //$("#ModalImporterClients").modal("show");                       
                    },
                    error: function (xhr) {
                        //console.log("error: " + xhr);
                        El20Utils.ocultarCargador();
                        //loading.hideloading();
                    }
                }).always(function () {
                });

            } catch (e) {
                throw 'BankIndexControlador -> GetToken: ' + e;
            }
        });

        $(".btn-desvincular").click(function () {
            var uuid = $(this).data("uuid");
            El20Utils.mostrarCargador();
            try {
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    //async: true,
                    data: { uuid: uuid },
                    url: self.unlinkBankUrl,
                    success: function (result) {
                        console.log("result", result);

                        if (!result.success) {
                            toastr["error"](result.Mensaje.message);
                        } else {
                            toastr["success"](result.Mensaje);
                            self.dataTable.DataTable().draw();
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

            } catch (e) {
                throw 'BankIndexControlador -> GetToken: ' + e;
            }
        });
    };
};