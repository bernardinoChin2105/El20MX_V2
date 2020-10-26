$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

//$("#RFC").keyup(function () {
//    this.value = this.value.toUpperCase();
//});

var InvoiceControlador = function (htmlTableId, searchUrl, addressUrl, branchOfficeUrl, locationsUrl, codeSATUrl, UnitSATUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.searchUrl = searchUrl;
    this.addressUrl = addressUrl;
    this.branchOfficeUrl = branchOfficeUrl;
    this.locationsUrl = locationsUrl;
    this.codeSATUrl = codeSATUrl;
    this.UnitSATUrl = UnitSATUrl;
    this.dataTable = {};

    this.quantity = $("#Quantity");
    this.unitPrice = $("#UnitPrice");    
    this.discountRate = $("#DiscountRateProServ");
    this.taxesIEPS = $("#TaxesIEPS");
    this.taxesIVA = $("#TaxesIVA");
    this.subtotal = $("#Subtotal");

    this.init = function () {

        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            //language: El20Utils.lenguajeTabla({}),
            //"bProcessing": true,
            //"bServerSide": true,
            //"sAjaxSource": this.baseUrl,
            //orderMulti: false,
            "bLengthChange": false,
            "bInfo": false,
            "paging": false,
            searching: false,
            ordering: false               
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

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

        $('.money').mask("##,###,##0.00", { reverse: true });

        //$("#DiscountRate").keyup(function () {
        //    console.log($(this).val(), "funciona aquí");
        //});

        $("#Quantity,#UnitPrice,#DiscountRateProServ").keyup(function () {
            var quantity = parseFloat(self.quantity.val() !== "" ? self.quantity.val() : 0);
            var unitPrice = parseFloat(self.unitPrice.val() !== "" ? (self.unitPrice.val().replace(",","")) : 0);
            var discountRate = parseFloat(self.discountRate.val() !== "" ? self.discountRate.val() : 0);    
            //console.log(discountRate, "descuento");

            //dudas con el impuesto
            //var taxesIEPS = parseFloat(self.taxesIEPS);
            //var taxesIVA = parseFloat(self.taxesIVA);
            //var subtotal = parseFloat(self.subtotal);

            //Calcular el subtotal    
            if (quantity > 0 && unitPrice > 0) {
                var sub = quantity * unitPrice;

                var discount = discountRate > 0 || discountRate < 0 ? (discountRate * sub) / 100 : 0;
                //console.log("desceutno", discount);
                var subtotal = sub - discount;
                //console.log(subtotal, "subtotal sin el descuento")
                //console.log(subtotal.toFixed(2), "subtotal ")
                self.subtotal.val(subtotal.toFixed(2));
            }

        });

        //Borrar valores cuando se oculte el modal
        $('#ServProdModal').on('hide.bs.modal', function (e) {
            // do something...
            $("#ConceptForm").each(function () {
                //console.log("holas");
                this.reset();
            });
        });

        //Agregar a la lista de conceptos del producto
        $("#addConcept").click(function () {
            //console.log("estoy aqui")
            var t = self.dataTable;
            var ind = t.data().count();
            console.log("hhajas", index);
            var index = parseInt(ind);
            index++;

            t.row.add([
                index,
                self.quantity.val(),
                $("#SATCode").val(),
                $("#ProductServiceDescription").val(),
                $("#SATUnit").val(),
                self.unitPrice.val(),
                self.discountRate.val(),
                self.taxesIEPS.val(),
                self.taxesIVA.val(),
                self.subtotal.val()
            ]).draw(false);

            $("#ServProdModal").modal("hide");
        });

        //Obtener información de las sucursales
        $("#BranchOffice").change(function () {
            var value = $(this).val();
            //console.log(value, "respuesta");
            var SerieFolio = $("#SerieFolio");
            var Serie = $("#Serie");
            var Folio = $("#Folio");

            if (value !== "") {
                $.ajax({
                    type: 'Get',
                    async: true,
                    data: { sucursalId: value },
                    url: self.branchOfficeUrl,
                    success: function (json) {
                        //console.log(json, "respuesta");
                        SerieFolio.val(json.serie + " " + json.folio);
                        Serie.val(json.serie);
                        Folio.val(json.folio);
                    },
                    error: function (xhr) {
                        //console.log('error');
                        SerieFolio.val("");
                        Serie.val("");
                        Folio.val("");
                    }
                });
            } else {
                SerieFolio.val("");
                Serie.val("");
                Folio.val("");
            }

        });

        //Validar si el régimen es de arrandamiento para habilitar el campo de numero de cuenta
        $("#IssuingTaxRegimeId").change(function () {
            //console.log("valor", $(this).val());
            var val = $(this).val();
            if (val === "606")
                $("#PropertyAccNum").removeClass("hide");
            else {
                $("#PropertyAccNum").addClass("hide");
                $("#PropertyAccountNumber").val("");
            }
        });

        //Validar el checkbox de impuestos
        $('input').on('ifClicked', function (ev) {
            $(ev.target).click();
        });

        $("#TaxesChk").click(function () {
            //console.log("estoy dento", !this.checked);
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
                //console.log(query, process, "esto trae");
                var type = $("#TypeInvoice").val();
                return $.get(self.searchUrl + "?field=Name&value=" + query + "&typeInvoice=" + type, function (result) {
                    //console.log(result, "respuesta");
                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.businessName, type: item.taxRegime };
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
                $('#CustomerId').attr('value', item.id);
                $('#ReceiverType').attr('value', item.type);
                GetReceiver(item.id, item.type);
                return item.name;
            }
        });

        //Buscar información del cliente por RFC
        $('#RFC').typeahead({
            source: function (query, process) {
                //console.log(query, process, "esto trae");
                var type = $("#TypeInvoice").val();
                return $.get(self.searchUrl + "?field=RFC&value=" + query + "&typeInvoice=" + type, function (result) {
                    //console.log(result.data, "respuesta");
                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.rfc, type: item.taxRegime };
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
                $('#CustomerId').attr('value', item.id);
                $('#ReceiverType').attr('value', item.taxRegime);
                GetReceiver(item.id, item.taxRegime);
                return item.name;
            }
        });

        $("#ZipCode").blur(function () {
            //console.log("perdio el focus");
            var value = $(this).val();
            var cmbColony = $("#Colony");
            var cmbMunicipality = $("#Municipality");
            var cmbState = $("#State");
            var cmbCountry = $("#Country");
            //console.log("valor", value);

            $.ajax({
                type: 'Get',
                async: true,
                data: { zipCode: value },
                url: self.locationsUrl,
                success: function (json) {
                    //console.log(json, "respuesta");
                    if (json.Data.success) {
                        var datos = json.Data.data;
                        if (datos.length > 0) {
                            //console.log(datos, "que esta aquí");
                            cmbState.val(datos[0].stateId);

                            //Llenado de Países
                            cmbCountry.html('<option value="-1">Seleccione...</option>');
                            cmbCountry.append('<option value="' + datos[0].countryId + '">' + datos[0].nameCountry + '</option>');
                            cmbCountry.val(datos[0].countryId);

                            //Llenado de municipios
                            //console.log(datos[0], "registro 0");
                            cmbMunicipality.html('<option value="-1">Seleccione...</option>');
                            cmbMunicipality.append('<option value="' + datos[0].municipalityId + '">' + datos[0].nameMunicipality + '</option>');
                            cmbMunicipality.val(datos[0].municipalityId);

                            //Llenado de colonias
                            cmbColony.html('<option value="-1">Seleccione...</option>');
                            cmbColony.append(datos.map(function (data, index) {
                                return $('<option value="' + data.id + '">' + data.nameSettlementType + ' ' + data.nameSettlement + '</option>');
                            }));
                            cmbColony.val(datos[0].id);
                        } else {
                            cmbState.val(-1);
                            cmbMunicipality.html('<option value="-1">Seleccione...</option>').val(-1);
                            cmbColony.html('<option value="-1">Seleccione...</option>').val(-1);
                            toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                        }
                    } else {
                        cmbState.val(-1);
                        cmbCountry.html('<option value="-1">Seleccione...</option>').val(-1);
                        cmbMunicipality.html('<option value="-1">Seleccione...</option>').val(-1);
                        cmbColony.html('<option value="-1">Seleccione...</option>').val(-1);
                        toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                    }

                },
                error: function (xhr) {
                    console.log('error');
                    cmbState.val(-1);
                    cmbCountry.html('<option value="-1">Seleccione...</option>').val(-1);
                    cmbMunicipality.html('<option value="-1">Seleccione...</option>').val(-1);
                    cmbColony.html('<option value="-1">Seleccione...</option>').val(-1);
                }
            });
        });

        //Buscar información de la clave de producto o servicio
        $('#SATCode').typeahead({
            source: function (query, process) {
                //console.log(query, process, "esto trae");
                return $.get(self.codeSATUrl + "?Concept=" + query, function (result) {
                    //console.log(result, "respuesta");
                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.code, type: item.description };
                        return JSON.stringify(aItem);
                    });
                    return process(resultList);
                });
            },
            highlighter: function (obj) {
                var item = JSON.parse(obj);
                var newItem = item.name + " - " + item.type;
                var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
                return newItem.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
                    return '<strong>' + match + '</strong>';
                });
            },
            updater: function (obj) {
                var item = JSON.parse(obj);
                return item.name;
            }
        });

        //Buscar información de la clave de unidad
        $('#SATUnit').typeahead({
            source: function (query, process) {
                //console.log(query, process, "esto trae");
                return $.get(self.UnitSATUrl + "?Clave=" + query, function (result) {
                    //console.log(result, "respuesta");
                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.code, type: item.name };
                        return JSON.stringify(aItem);
                    });
                    return process(resultList);
                });
            },
            highlighter: function (obj) {
                var item = JSON.parse(obj);
                //console.log(item, "high")
                var newItem = item.name + " - " + item.type;
                var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
                return newItem.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
                    return '<strong>' + match + '</strong>';
                });
            },
            updater: function (obj) {
                var item = JSON.parse(obj);
                return item.name;
            }
        });

        function GetReceiver(id, type) {
            //console.log(id, type, "respuesta");
            $.ajax({
                type: 'Get',
                async: true,
                data: { id: id, type: type },
                url: self.addressUrl,
                success: function (json) {
                    console.log(json, "respuesta");
                    if (json.success) {
                        var data = json.data;
                        $("#CustomerName").val(data.BusinessName);
                        $("#RFC").val(data.RFC);
                        $("#Street").val(data.Street);
                        $("#OutdoorNumber").val(data.OutdoorNumber);
                        $("#InteriorNumber").val(data.InteriorNumber);
                        $("#Colony").val(data.Colony);
                        $("#ZipCode").val(data.ZipCode);
                        $("#Municipality").val(data.Municipality);
                        $("#State").val(data.State);
                        $("#Country").val(data.Country);

                        //Falta agregar el email
                        //ListCustomerEmail lista
                        //CustomerEmailId                    
                    }
                    //else {
                    //}

                },
                error: function (xhr) {
                    console.log('error', xhr);
                    //cmbState.val(-1);
                    //cmbCountry.html('<option value="-1">Seleccione...</option>').val(-1);
                    //cmbMunicipality.html('<option value="-1">Seleccione...</option>').val(-1);
                    //cmbColony.html('<option value="-1">Seleccione...</option>').val(-1);
                }
            });
        }
    };

};