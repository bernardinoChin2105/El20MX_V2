$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

$("#RFC").keyup(function () {
    this.value = this.value.toUpperCase();
});


var InvoiceControlador = function (htmlTableId, searchUrl, addressUrl, codeSATUrl, UnitSATUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.searchUrl = searchUrl;
    this.addressUrl = addressUrl;
    this.codeSATUrl = codeSATUrl;
    this.UnitSATUrl = UnitSATUrl;
    this.dataTable = {};

    this.init = function () {

        //self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
        //    El20Utils.mostrarCargador();
        //}).DataTable({
        //    language: El20Utils.lenguajeTabla({}),
        //    "bProcessing": true,
        //    "bServerSide": true,
        //    "sAjaxSource": this.baseUrl,
        //    orderMulti: false,
        //    searching: false,
        //    ordering: false,
        //    columns: [
        //        { data: 'id', title: "Id", visible: false },
        //        { data: 'serie', title: "Serie" },
        //        { data: 'folio', title: "Folio" },
        //        { data: 'invoicedAt', title: "Fecha Factura" },
        //        { data: 'rfc', title: "RFC Proveedor" },
        //        { data: 'businessName', title: "Proveedor" },
        //        { data: 'paymentMethod', title: "Método Pago" },
        //        { data: 'paymentFormDescription', title: "Forma Pago" },
        //        { data: 'currency', title: "Divisa" },
        //        { data: 'amount', title: "SubTotal" },
        //        { data: 'iva', title: "IVA" },
        //        { data: 'totalAmount', title: "Total" },
        //        {
        //            data: null,
        //            title: "Archivos",
        //            className: 'menu-options',
        //            render: function (data) {
        //                //Menu para más opciones de cliente
        //                //console.log(data)
        //                var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
        //                    '<a href="' + self.downloadPdfUrl + '?id=' + data.id + '" class="btn btn-light btn-downloadPdf" title="Descargar PDF"><span class="fas fa-file-pdf"></span>' +
        //                    '<a href="' + self.downloadXmlUrl + '?id=' + data.id + '" class="btn btn-light btn-downloadXml" title="Descargar XML"><span class="fas fa-file-alt"></span></a>' +
        //                    '</div>';
        //                return hasFullAccessController ? buttons : "";
        //            }
        //        }
        //    ],
        //    "fnServerData": function (sSource, aoData, fnCallback) {
        //        aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
        //        aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
        //        aoData.push({ "name": "first", "value": primeravez });

        //        $.getJSON(sSource, aoData, function (json) {
        //            primeravez = false;
        //            fnCallback(json);

        //            if (json.success === false) {
        //                toastr['error'](json.error);
        //                //console.log(json.Mensaje + " Error al obtener los elementos");
        //            }
        //        });
        //    }
        //}).on('xhr.dt', function (e, settings, data) {
        //    El20Utils.ocultarCargador();
        //});

        //$.validator.addMethod("Alphanumeric",
        //    function (value, element) {
        //        return value.match(/^[A-Za-zÀ-ÿ\u00f1\u00d10-9 _.-]+$|^$/);
        //    }, "El campo debe ser alfanumérico"
        //);

        //$("#SearchForm").validate({
        //    rules: {
        //        Folio: {
        //            Alphanumeric: true
        //        },
        //        Serie: {
        //            Alphanumeric: true
        //        },
        //        RFCP: {
        //            Alphanumeric: true
        //        },
        //        NombreRazonSocial: {
        //            Alphanumeric: true
        //        },

        //    }
        //});   

        //Validar si el régimen es de arrandamiento para habilitar el campo de numero de cuenta
        $("#IssuingTaxRegimeId").change(function () {
            console.log("valor", $(this).val());
            var val = $(this).val();
            if (val === "606")
                $("#PropertyAccNum").removeClass("hide");
            else
            {
                $("#PropertyAccNum").addClass("hide");
                $("#PropertyAccountNumber").val("");
            }
        });

        //Validar el checkbox de impuestos
        $('input').on('ifClicked', function (ev) {
            $(ev.target).click();
        });

        $("#TaxesChk").click(function () {
            console.log("estoy dento", !this.checked);
            var taxes = $("#taxes");                

            if (!this.checked) {
                taxes.addClass("hide");
                $("#Withholdings").val("");
                $("#Transferred").val("");
                $("#Valuation").val("");
            }
            else
                taxes.removeClass("hide");
        });       

        //Buscar información del cliente por Razon Social
        $('#CustomerName').typeahead({
            source: function (query, process) {
                console.log(query, process, "esto trae");
                var type = $("#TypeInvoice").val();
                return $.get(self.searchUrl + "?field=Name&value=" + query + "&typeInvoice=" + type, function (result) {
                    console.log(data, "respuesta");

                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.rfc };
                        return JSON.stringify(aItem);
                    });
                    return process(resultList);
                });
            },
            highlighter: function (obj) {
                var item = JSON.parse(obj);
                var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
                return item.name.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
                    return '<strong>' + match + '</strong>';
                });
            },
            updater: function (obj) {
                var item = JSON.parse(obj);
                $('#RFCId').attr('value', item.id);
                return item.name;
            }
        });

        //Buscar información del cliente por RFC
        $('#RFC').typeahead({
            source: function (query, process) {
                console.log(query, process, "esto trae");
                var type = $("#TypeInvoice").val();
                return $.get(self.searchUrl + "?field=RFC&value=" + query + "&typeInvoice=" + type, function (result) {
                    console.log(result.data, "respuesta");
                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.rfc };
                        return JSON.stringify(aItem);
                    });
                    return process(resultList);
                });
            },
            //matcher: function (obj) {
            //    var item = JSON.parse(obj);
            //    return ~item.name.toLowerCase().indexOf(this.query.toLowerCase());
            //},
            //sorter: function (items) {
            //    var beginswith = [], caseSensitive = [], caseInsensitive = [], item;
            //    while (aItem = items.shift()) {
            //        var item2 = JSON.parse(aItem);
            //        if (!item.name.toLowerCase().indexOf(this.query.toLowerCase())) beginswith.push(JSON.stringify(item));
            //        else if (~item.name.indexOf(this.query)) caseSensitive.push(JSON.stringify(item));
            //        else caseInsensitive.push(JSON.stringify(item));
            //    }

            //    return beginswith.concat(caseSensitive, caseInsensitive);
            //},
            highlighter: function (obj) {
                var item = JSON.parse(obj);
                var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
                return item.name.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
                    return '<strong>' + match + '</strong>';
                });
            },
            updater: function (obj) {
                var item = JSON.parse(obj);
                $('#RFCId').attr('value', item.id);
                return item.name;
            }
        });


        //$.get(self.autocompleteURL, function (data) {
        //    $(".typeahead_2").typeahead({ source: data.Data });
        //}, 'json');
    };

};