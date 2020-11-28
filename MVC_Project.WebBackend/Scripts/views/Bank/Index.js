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

var BankIndexControlador = function (htmlTableId, baseUrl, bankAccountsUrl, getTokenUrl, unlinkBankUrl, createBankCredentialUrl, token, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.bankAccountsUrl = bankAccountsUrl;
    this.getTokenUrl = getTokenUrl;
    this.unlinkBankUrl = unlinkBankUrl;
    this.createBankCredentialUrl = createBankCredentialUrl;
    this.dataTable = {};
    this.token = token;
    this.syncWidget = {};

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
                { data: 'Name', title: "Bancos" },
                { data: 'dateTimeAuthorized', title: "Fecha Autorización" },
                { data: 'dateTimeRefresh', title: "Fecha Última Obt. Información" },
                { data: 'status', title: "Estatus" },
                {
                    data: null,
                    title: "Configuración de Bancos",
                    className: 'menu-options',
                    render: function (data) {
                        //Menu para más opciones de cliente
                        //console.log(data, "que datos trae");
                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<a class="link" href="' + self.bankAccountsUrl + '?idBankCredential=' + data.uuid + '">Ver más</a>' +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                },
                {
                    data: null,
                    title: "Acciones",
                    className: 'work-options',
                    render: function (data) {
                        var btnUpdate = "";                                                
                        if (data.code === 401 || data.code === 411 || data.code === 600) { //el código 600 es cuando la fecha refresh no esta actualizada
                            btnUpdate = '<button class="btn btn-light btn-actualizar" title="Actualizar"><span class="fa fa-sync-alt"></span></button>';
                        }

                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button class="btn btn-light btn-desvincular" title="Desvincular"><span class="fa fa-trash"></span></button>' +
                            btnUpdate +
                            '</div>';
                        return hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                //aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                //aoData.push({ "name": "first", "value": primeravez });

                $.getJSON(sSource, aoData, function (json) {
                    //console.log(json, "hola")
                    //primeravez = false;
                    if (json.success === false) {
                        toastr['error'](json.message, null, { 'positionClass': 'toast-top-center' });
                        //console.log(json.Mensaje + " Error al obtener los elementos");
                        El20Utils.ocultarCargador();
                    } else {
                        fnCallback(json);
                    }
                });
            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        var params = {
            // Set up the token you created in the Quickstart:
            token: self.token,
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
            // do something on success
            //console.log(credential, "respuesta")
            El20Utils.ocultarCargador();
            $.ajax({
                type: 'Get',
                contentType: 'application/json',
                async: true,
                data: { idCredential: credential.id_credential },
                url: self.createBankCredentialUrl,
                success: function (result) {
                    //console.log("result", result);

                    if (!result.success) {
                        toastr["error"](result.Mensaje.message, null, { 'positionClass': 'toast-top-center' });
                    } else {
                        toastr["success"](result.data, null, { 'positionClass': 'toast-top-center' });
                        self.dataTable.draw();
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

        self.syncWidget.$on('error', function (credential) {
            // ... do something when there is some error in the synchronization of credentials  ...
            console.log(credential, "Error en las claves de la conexión.");
            //toastr["error"]("Erorr");
        });

        self.syncWidget.$on('401', () => {
            // ... do something when user session is unauthorized.
            // i.e. refresh user session.
            console.log("holas estoy aquí")
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
                        //console.log("result", result);

                        if (!result.success) {
                            toastr["error"](result.Mensaje.message, null, { 'positionClass': 'toast-top-center' });
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

        //opción para actualizar solicitar nuevamente las credenciales o el token, generar nuevamente la sincronización
        $(self.htmlTable, "tbody").on("click", ".work-options .btn-group .btn-actualizar", function () {
            //El20Utils.mostrarCargador();            

            var tr = $(this).closest('tr');
            var row = self.dataTable.row(tr);
            var code = row.data().code;
            var isTwofa = row.data().isTwofa;
            var credentialId = row.data().credentialProviderId;            

            if (isTwofa === false && code !== 401) {
                self.syncWidget.setEntrypointCredential(credentialId);
            } else {
                var siteId = row.data().siteId;
                //self.syncWidget.setEntrypointSite(siteId);
                params.locale = 'es';
                params.navigation = {
                    displayStatusInToast: true,
                    "hideSiteOrganizationTypes": ["Blockchain", "Digital Wallet", "Government", "Utility"]
                };
                params.entrypoint = {
                    // Set up the country to start:
                    country: 'MX',                    
                    site: siteId
                };
                self.syncWidget.setConfig(params);
                self.syncWidget.open();
            }            
        });

        $("#table tbody").on("click", ".work-options .btn-group .btn-desvincular", function () {
            El20Utils.mostrarCargador();
            //console.log("hoasd")

            var tr = $(this).closest('tr');
            var row = self.dataTable.row(tr);
            var uuid = row.data().uuid;

            try {
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    //async: true,
                    data: { uuid: uuid },
                    url: self.unlinkBankUrl,
                    success: function (result) {
                        //console.log("result", result);

                        if (!result.success) {
                            toastr["error"](result.message, null, { 'positionClass': 'toast-top-center' });
                        } else {
                            toastr["success"](result.message, null, { 'positionClass': 'toast-top-center' });
                            self.dataTable.draw();
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