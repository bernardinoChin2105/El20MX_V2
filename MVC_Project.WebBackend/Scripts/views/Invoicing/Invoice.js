﻿$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});

var InvoiceControlador = function (htmlTableId, searchUrl, addressUrl, branchOfficeUrl, locationsUrl, codeSATUrl, UnitSATUrl, searchCDFIUrl, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.searchUrl = searchUrl;
    this.addressUrl = addressUrl;
    this.branchOfficeUrl = branchOfficeUrl;
    this.locationsUrl = locationsUrl;
    this.codeSATUrl = codeSATUrl;
    this.UnitSATUrl = UnitSATUrl;
    this.searchCDFIUrl = searchCDFIUrl;
    this.dataTable = {};

    this.quantity = $("#Quantity");
    this.unitPrice = $("#UnitPrice");
    this.discountRate = $("#DiscountRateProServ");
    this.taxesIEPS = $("#TaxesIEPS");
    this.taxesIVA = $("#TaxesIVA");
    this.subtotal = $("#SubtotalM");

    this.init = function () {
        self.dataTable = this.htmlTable.on('preXhr.dt', function (e, settings, data) {
            El20Utils.mostrarCargador();
        }).DataTable({
            //language: El20Utils.lenguajeTabla({}),
            //"bProcessing": true,
            //"bServerSide": true,
            //"sAjaxSource": this.baseUrl,
            //orderMulti: false,
            "oLanguage": { "sZeroRecords": "", "sEmptyTable": "" },
            "bLengthChange": false,
            "bInfo": false,
            "paging": false,
            searching: false,
            ordering: false,
            "columnDefs": [
                {
                    "targets": 0, render: function (data, type, row, meta) {
                        return '<input type="text" class="form-control" readonly name="indexPD" value="' + data + '" />';
                    }
                },
                {
                    "targets": 1, render: function (data, type, row, meta) {
                        //console.log(data, type, row, meta, "todos")
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].Quantity" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 2, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].SATCode" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 3, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].ProductServiceDescription" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 4, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].SATUnit" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 5, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].Unit" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 6, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].UnitPrice" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 7, visible: false, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].DiscountRateProServ" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 8, render: function (data, type, row, meta) {
                        //console.log(data, type, row, meta, "todos")
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].TaxesIEPS" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 9, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].TaxesIVA" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 10, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].Subtotal" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 11, render: function (data, type, row, meta) {
                        var button = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button class="btn btn-light btn-edit"  title="Editar Producto o Servicio" style="margin-left:5px;"><span class="fas fa-edit"></span></button>' +
                            '</div>';
                        return button;
                    }
                }
            ],
            //"createdRow": function (row, data, index) {
            //    //"rowCallback": function (row, data, index) {
            //    console.log(row, data, "create row");
            //    console.log(row, data, index, "create row");
            //    //   var index = 0;                      
            //},
            "footerCallback": function (row, data, start, end, display) {
                //console.log(row, data, start, end, display, "lo que trae")
                var api = this.api();

                // Remove the formatting to get integer data for summation
                var intVal = function (i) {
                    return typeof i === 'string' ?
                        i.replace(/[\$,]/g, '') * 1 :
                        typeof i === 'number' ?
                            i : 0;
                };

                // Total over all pages
                //total = api
                //    .column(9)
                //    .data()
                //    .reduce(function (a, b) {
                //        return intVal(a) + intVal(b);
                //    }, 0);

                //total = api
                //    .column(9)
                //    .data()
                //    .reduce(function (a, b) {
                //        return intVal(a) + intVal(b);
                //    }, 0);

                trasladosIEPSIVA = api
                    .column(8)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                // Total over this page
                subtotal = api
                    .column(9, { page: 'current' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                //var discount = discountRate > 0 || discountRate < 0 ? (discountRate * sub) / 100 : 0;
                //var discount = subtotal;


                $("#Subtotal").val(subtotal.toFixed(2));
                $("#lblSubtotal").html('$' + subtotal.toFixed(2));


                var discountTXT = $("#DiscountRate").val();
                var discountRate = parseFloat(discountTXT !== "" ? discountTXT : 0);
                var discount = discountRate > 0 || discountRate < 0 ? (discountRate * subtotal) / 100 : 0;

                $("#TotalDiscount").val(discount.toFixed(2));
                $("#lblTotalDiscount").html("$" + discount.toFixed(2));
                if (discount > 0) {
                    $(".trDiscount").removeClass("hide");
                } else {
                    $(".trDiscount").addClass("hide");
                }

                $("#TaxTransferred").val(trasladosIEPSIVA);
                $("#lblTaxTransferred").html('$' + trasladosIEPSIVA);
                if (trasladosIEPSIVA > 0) {
                    $(".trTaxT").removeClass("hide");
                } else {
                    $(".trTaxT").addClass("hide");
                }

                $("#TaxWithheldIVA").val();
                $("#lblTaxWithheldIVA").html('$');
                //if (trasladosIEPSIVA > 0) {
                //    $(".trTaxWIVA").removeClass("hide");
                //} else {
                $(".trTaxWIVA").addClass("hide");
                //}

                $("#TaxWithheldISR").val();
                $("#lblTaxWithheldISR").html('$');
                //if (trasladosIEPSIVA > 0) {
                //    $(".trTaxWISR").removeClass("hide");
                //} else {
                $(".trTaxWISR").addClass("hide");
                //}

                var total = (subtotal + discount + trasladosIEPSIVA).toFixed(2);
                $("#Total").val(total);
                $("#lblTotal").html('$' + total);

            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        $(this.htmlTable, "tbody").on('click',
            //'td.menu-options .btn-group .btn-edit',
            '.btn-group .btn-edit',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var data = row.data();
                //console.log(row.data(), "validar que datos estan trayendo ");

                //var id = row.data().uuid;   
                $("#Row").val(data[0]);
                self.quantity.val(data[1]);
                $("#SATCode").val(data[2]);
                $("#ProductServiceDescription").val(data[3]);
                $("#SATUnit").val(data[4]);
                self.unitPrice.val(data[5]);
                self.discountRate.val(data[6]);
                self.taxesIEPS.val(data[7]);
                self.taxesIVA.val(data[8]);
                self.subtotal.val(data[9]);

                $("#ServProdModal").modal("show");
            });

        $.validator.addMethod("Alphanumeric",
            function (value, element) {
                return value.match(/^[A-Za-zÀ-ÿ\u00f1\u00d10-9 _.-]+$|^$/);
            }, "El campo debe ser alfanumérico"
        );

        $("#InvoicingForm").validate({
            rules: {
                Folio: {
                    Alphanumeric: true
                },
                Serie: {
                    Alphanumeric: true
                },
                RFCP: {
                    Alphanumeric: true
                },
                NombreRazonSocial: {
                    Alphanumeric: true
                },
            }
        });

        $('.money').mask("##,###,##0.00", { reverse: true });

        //$("#DiscountRate").keyup(function () {
        //    console.log($(this).val(), "funciona aquí");
        //});
        $("#RFC").keyup(function () {
            this.value = this.value.toUpperCase();
        });

        $("#Quantity,#UnitPrice,#DiscountRateProServ").keyup(function () {
            var quantity = parseFloat(self.quantity.val() !== "" ? self.quantity.val() : 0);
            var unitPrice = parseFloat(self.unitPrice.val() !== "" ? (self.unitPrice.val().replace(",", "")) : 0);
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
                $("#Row").val("");
            });
        });

        //Agregar a la lista de conceptos del producto
        $("#addConcept").click(function () {
            //console.log("estoy aqui")

            if (!$('#ConceptForm').valid()) {
                return;
            }

            var t = self.dataTable;
            var row = $("#Row").val();

            if (row !== "") {
                var indexR = parseInt(row);
                //indexR--;
                t.row(indexR - 1).data(
                    [
                        indexR,
                        self.quantity.val(),
                        $("#SATCode").val(),
                        $("#ProductServiceDescription").val(),
                        $("#SATUnit").val(),
                        $("#Unit").val(),
                        self.unitPrice.val(),
                        self.discountRate.val(),
                        self.taxesIEPS.val(),
                        self.taxesIVA.val(),
                        self.subtotal.val(),
                        indexR
                    ]
                ).draw(false);
            } else {
                var ind = t.rows().count();
                var index = parseInt(ind);
                index++;

                t.row.add(
                    [
                        index,
                        self.quantity.val(),
                        $("#SATCode").val(),
                        $("#ProductServiceDescription").val(),
                        $("#SATUnit").val(),
                        $("#Unit").val(),
                        self.unitPrice.val(),
                        self.discountRate.val(),
                        self.taxesIEPS.val(),
                        self.taxesIVA.val(),
                        self.subtotal.val(),
                        index
                    ]
                ).draw(false);
            }

            $("#ServProdModal").modal("hide");
        });

        //Validar que sea obligatorio el tipo de cambio cuando sea distinto a MXN
        $("#Currency").change(function () {
            //console.log("pruebas")
            var currency = $(this).val();
            if (currency !== "MXN") {
                $(".exchange").removeClass("hide");
                $("#ExchangeRate").addClass("required");
            }
            else {
                $(".exchange").addClass("hide");
                $("#ExchangeRate").removeClass("required");
            }
        });

        //Validar que sea aparezca el IEPS
        $("#Valuation").change(function () {
            var value = $(this).val();
            var column = self.dataTable.column(8);

            // Toggle the visibility
            if (value === "IEPS") {
                $(".trTaxT").removeClass("hide");
                column.visible(true);
            }
            else {
                $(".trTaxT").addClass("hide");
                column.visible(false);
            }
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
                        console.log(json, "respuesta");
                        SerieFolio.val(json.serie + " " + json.folio);
                        Serie.val(json.serie);
                        Folio.val(json.folio);
                        if (json.logo !== null)
                            $("#logo").attr("src", json.logo);
                        else
                            $("#logo").attr("src", "#");
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
            if (val === "606") {
                $("#PropertyAccNum").removeClass("hide");
                $("#PropertyAccountNumber").addClass("required");
            }
            else {
                $("#PropertyAccNum").addClass("hide");
                $("#PropertyAccountNumber").removeClass("required").val("");
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

        $("#InternationalChk").click(function () {
            //console.log("estoy dento", !this.checked);
            var international = $(".international");

            if (!this.checked) {
                international.addClass("hide");
                $("#MotionNumber").val("");
            }
            else
                international.removeClass("hide");
        });

        $("#InvoiceComplementChk").click(function () {
            //console.log("estoy dento", !this.checked);
            var complement = $(".complement");

            if (!this.checked) {
                complement.addClass("hide");
                $("#InvoiceComplement").val("");
            }
            else
                complement.removeClass("hide");
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
            if (!value) {
                ClearCombos();
                return;
            }
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
                            ClearCombos()
                            toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                        }
                    } else {
                        ClearCombos()
                        toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                    }

                },
                error: function (xhr) {
                    ClearCombos();
                }
            });
        });

        function ClearCombos() {
            var cmbColony = $("#Colony");
            var cmbMunicipality = $("#Municipality");
            var cmbState = $("#State");
            var cmbCountry = $("#Country");

            cmbState.val(-1);
            cmbCountry.html('<option value="-1">Seleccione...</option>').val(-1);
            cmbMunicipality.html('<option value="-1">Seleccione...</option>').val(-1);
            cmbColony.html('<option value="-1">Seleccione...</option>').val(-1);
        }

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
                $('#Unit').attr('value', item.type);
                return item.name;
            }
        });

        function GetReceiver(id, type) {
            //console.log(id, type, "respuesta");
            El20Utils.mostrarCargador();
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
                        $("#CustomerEmail").val();
                        if (data.ZipCode !== null) {
                            $("#ZipCode").trigger("blur");
                        }
                        El20Utils.ocultarCargador();

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

        $("#SavedInvoice").click(function () {
            if (!$('#InvoicingForm').valid()) {
                //return;

                $('html, body').animate({
                    scrollTop: ($('.error').offset().top - 300)
                }, 2000);
                return;
            }

            $("#InvoicingForm").attr('action', 'Invoice');
            $('#InvoicingForm').submit();
        });

        $("#validarDatos").click(function () {
            if (!$('#InvoicingForm').valid()) {
                //return;
                $('html, body').animate({
                    scrollTop: ($('.error').offset().top - 300)
                }, 2000);
                return;
            }

            //mandar información de la tabla
            $("#InvoicingForm").attr('action', 'IssueIncomeInvoice');
            $('#InvoicingForm').submit();
        });
    };

};