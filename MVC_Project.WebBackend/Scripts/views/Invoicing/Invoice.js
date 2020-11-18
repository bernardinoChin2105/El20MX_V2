$(window).keydown(function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        return false;
    }
});
var InvoiceControlador = function (htmlTableId, searchUrl, addressUrl, branchOfficeUrl, locationsUrl, codeSATUrl, UnitSATUrl, searchCDFIUrl,
    rateTaxesUrl, officeSellos, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.htmlTableImp = $("#tableImpuestos");
    this.htmlTableCFDI = $("#tableComplent");
    this.searchUrl = searchUrl;
    this.addressUrl = addressUrl;
    this.branchOfficeUrl = branchOfficeUrl;
    this.locationsUrl = locationsUrl;
    this.codeSATUrl = codeSATUrl;
    this.UnitSATUrl = UnitSATUrl;
    this.searchCDFIUrl = searchCDFIUrl;
    this.rateTaxesUrl = rateTaxesUrl;
    this.officeSellos = officeSellos;
    this.dataTable = {};
    this.dataTableImp = {};
    this.dataTableCFDI = {};

    this.quantity = $("#Quantity");
    this.unitPrice = $("#UnitPrice");
    this.discountRate = $("#DiscountRateProServ");
    this.taxesIEPS = $("#TaxesIEPS");
    this.taxesIVA = $("#TaxesIVA");
    this.subtotal = $("#SubtotalM");

    this.retencionISR = 0;
    this.retencionIVA = 0;
    this.traslados = 0;

    this.retencionISRTemp = 0;
    this.retencionIVATemp = 0;
    this.trasladosTemp = 0;

    this.retencionISRTempDesc = 0;
    this.retencionIVATempDesc = 0;
    this.trasladosTempDesc = 0;


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
                    "targets": 5, className: 'hide', render: function (data, type, row, meta) {
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
                    "targets": 7, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].DiscountRateProServ" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 8, render: function (data, type, row, meta) {
                        //console.log(data, type, row, meta, "todos")
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].TaxesIVA" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 9, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].TaxesIEPS" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 10, className: 'hide', render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].TaxesGeneral" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 11, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].Subtotal" value="' + data + '" />';
                        return html;
                    }
                },
                {
                    "targets": 12, render: function (data, type, row, meta) {
                        var button = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button type="button" class="btn btn-light btn-edit"  title="Editar Producto o Servicio" style="margin-left:5px;"><span class="fas fa-edit"></span></button>' +
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
                //    .column(8)
                //    .data()
                //    .reduce(function (a, b) {
                //        return intVal(a) + intVal(b);
                //    }, 0);

                //trasladosIEPSIVA = api
                //    .column(9)
                //    .data()
                //    .reduce(function (a, b) {
                //        return intVal(a) + intVal(b);
                //    }, 0);

                // Total over this page
                subtotal = api
                    .column(11, { page: 'current' })
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                //var discount = discountRate > 0 || discountRate < 0 ? (discountRate * sub) / 100 : 0;
                //var discount = subtotal;


                $("#Subtotal").val(subtotal.toFixed(2));
                $("#lblSubtotal").html('$' + subtotal.toFixed(2));
                $(".trSubtotal > th").removeClass("hide");


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

                if (self.traslados > 0) {
                    $(".trTaxT").removeClass("hide");
                    $("#TaxTransferred").val(self.traslados);
                    $("#lblTaxTransferred").html('$' + self.traslados.toFixed(2));
                    subtotal = subtotal + self.traslados;
                } else {
                    $(".trTaxT").addClass("hide");
                }
                //console.log(subtotal, "traslados");

                if (self.retencionIVA > 0) {
                    $(".trTaxWIVA").removeClass("hide");
                    $("#TaxWithheldIVA").val(self.retencionIVA);
                    $("#lblTaxWithheldIVA").html('$' + (self.retencionIVA).toFixed(2));
                    subtotal = subtotal - self.retencionIVA;
                } else {
                    $(".trTaxWIVA").addClass("hide");
                }
                //console.log(subtotal, "iva rete");

                if (self.retencionISR > 0) {
                    $(".trTaxWISR").removeClass("hide");
                    $("#TaxWithheldISR").val(self.retencionISR);
                    $("#lblTaxWithheldISR").html('$' + (self.retencionISR).toFixed(2));
                    subtotal = subtotal - self.retencionISR;
                } else {
                    $(".trTaxWISR").addClass("hide");
                }
                //console.log(subtotal, "isr rete");

                var total = (subtotal - discount).toFixed(2);
                //console.log(total, "total");
                $("#Total").val(total);
                $("#lblTotal").html('$' + total);

            }
        }).on('xhr.dt', function (e, settings, data) {
            El20Utils.ocultarCargador();
        });

        self.dataTableImp = self.htmlTableImp.DataTable({
            "oLanguage": { "sZeroRecords": "", "sEmptyTable": "" },
            "bLengthChange": false,
            "bInfo": false,
            "paging": false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'Tipo' },
                { data: 'Impuesto' },
                { data: 'Porcentaje' },
                { data: 'index' },
            ],
            "columnDefs": [
                {
                    "targets": 0, render: function (data, type, row, meta) {
                        //console.log(data, type, row, meta, "column")
                        return '<input type="text" class="form-control" readonly name="indexPD" value="' + row[0].Tipo + '" />';
                    }
                },
                {
                    "targets": 1, render: function (data, type, row, meta) {
                        //console.log(data, type, row, meta, "todos")
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].Quantity" value="' + row[0].Impuesto + '" />';
                        return html;
                    }
                },
                {
                    "targets": 2, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="ProductServices[' + (meta.row) + '].SATCode" value="' + row[0].Porcentaje + '" />';
                        return html;
                    }
                },
                {
                    "targets": 3, render: function (data, type, row, meta) {
                        var button = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button type="button" class="btn btn-light btn-delete" title="Eliminar impuesto" style="margin-left:5px;"><span class="fas fa-trash"></span></button>' +
                            '</div>';
                        return button;
                    }
                }
            ]
        });

        self.dataTableCFDI = self.htmlTableCFDI.DataTable({
            "oLanguage": { "sZeroRecords": "", "sEmptyTable": "" },
            "bLengthChange": false,
            "bInfo": false,
            "paging": false,
            searching: false,
            ordering: false,
            columns: [
                {
                    "className": 'details-control',
                    "orderable": false,
                    "data": null,
                    "defaultContent": ''
                },
                { data: 'uuid' },
                { data: 'currency' },
                { data: 'exchangeRate' },
                { data: 'previousBalance' },
                { data: 'paid' },
                { data: 'outstanding' },
                { data: 'method' },
                { data: 'numberPartialities' },
                { data: 'folio' },
                { data: 'serie' },
                {
                    data: null,
                    //title: 'opciones',
                    //className: ""
                    render: function (data) {
                        var button = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            '<button type="button" class="btn btn-light btn-delete" title="Eliminar factura relacionada" style="margin-left:5px;"><span class="fas fa-trash"></span></button>' +
                            '</div>';
                        return button;
                    }
                }
            ],
            "columnDefs": [
                {
                    "targets": 1, render: function (data, type, row, meta) {
                        console.log(data, type, row, meta, "todos")
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].uuid" value="' + row.uuid + '" />';
                        return html;
                    }
                },
                {
                    "targets": 2, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].currency" value="' + row.currency + '" />';
                        return html;
                    }
                },
                {
                    "targets": 3, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].exchangeRate" value="' + row.exchangeRate + '" />';
                        return html;
                    }
                },
                {
                    "targets": 4, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].previousBalance" value="' + row.previousBalance + '" />';
                        return html;
                    }
                },
                {
                    "targets": 5, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].paid" value="' + row.paid + '" />';
                        return html;
                    }
                },
                {
                    "targets": 6, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].outstanding" value="' + row.outstanding + '" />';
                        return html;
                    }
                },
                {
                    "targets": 7, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].method" value="' + row.method + '" />';
                        return html;
                    }
                },
                {
                    "targets": 8, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" name="payment[' + (meta.row) + '].numberPartialities" value="' + row.numberPartialities + '" />';
                        return html;
                    }
                },
                {
                    "targets": 9, render: function (data, type, row, meta) {
                        //console.log(data, type, row, meta, "todos")
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].folio" value="' + row.folio + '" />';
                        return html;
                    }
                },
                {
                    "targets": 10, render: function (data, type, row, meta) {
                        var html = '<input type="text" class="form-control" readonly name="payment[' + (meta.row) + '].serie" value="' + row.serie + '" />';
                        return html;
                    }
                },
            ],
        });

        $(this.htmlTable, "tbody").on('click',
            '.btn-group .btn-edit',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var data = row.data();
                self.retencionISRTempDesc = 0;
                self.retencionIVATempDesc = 0;
                self.trasladosTempDesc = 0;
                //console.log(row.data(), "validar que datos estan trayendo ");

                //var id = row.data().uuid;   
                $("#Row").val(data[0]);
                self.quantity.val(data[1]);
                $("#SATCode").val(data[2]);
                $("#ProductServiceDescription").val(data[3]);
                $("#SATUnit").val(data[4]);
                $("#Unit").val(data[5]);
                self.unitPrice.val(data[6]);
                self.discountRate.val(data[7]);
                self.taxesIVA.val(data[8]);
                self.taxesIEPS.val(data[9]);
                self.subtotal.val(data[11]);
                //console.log(data[10], "impuestos");

                var array = data[10].split(";");
                //console.log(array);

                if (array.length > 0) {
                    var t = $("#tableImpuestos").DataTable();
                    var sub = self.quantity.val(data[1]) * self.unitPrice.val(data[6]);

                    array.forEach(function (item, i) {
                        //console.log(item, i, "onjeto");
                        var obj = JSON.stringify(item).replace(/'/g, "\"").replace("\"{", "{").replace("}\"", "}");
                        //console.log(obj,"objeto")
                        var datos = JSON.parse(obj);

                        if (datos.Porcentaje !== "Exento") {
                            var por = parseFloat(datos.Porcentaje);
                            var imp1 = por > 0 ? (por * sub) / 100 : 0;
                            if (datos.Tipo === "Retención") {
                                if (datos.Impuesto === "ISR") {
                                    self.retencionISRTempDesc = self.retencionISRTempDesc + imp1;
                                } else {
                                    self.retencionIVATempDesc = self.retencionIVATempDesc + imp1;
                                }
                            } else {
                                var imp2 = por > 0 ? (por * sub) / 100 : 0;
                                self.trasladosTempDesc = self.trasladosTempDesc + imp2;
                            }
                        }


                        t.row.add([datos]).draw(false);
                    });
                }

                $("#ServProdModal").modal("show");
            });

        $(this.htmlTableImp, "tbody").on('click',
            '.btn-group .btn-delete',
            function () {
                self.dataTableImp
                    .row($(this).parents('tr'))
                    .remove()
                    .draw();
                Prices();
            });

        //Opción de eliminar para los datos relacionados
        $(this.htmlTableCFDI, "tbody").on('click',
            '.btn-group .btn-delete',
            function () {
                self.dataTableCFDI
                    .row($(this).parents('tr'))
                    .remove()
                    .draw();
            });
        //Opción de mostrar detalles para la tabla de relacionados
        $(this.htmlTableCFDI, 'tbody').on('click', 'td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = self.dataTableCFDI.row(tr);

            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
            }
            else {
                // Open this row
                row.child(format(row.data())).show();
                copySelects();
                tr.addClass('shown');
            }
        });

        //funcion para obtener el detalle
        function format(d) {
            console.log(d, "detalles");
            // `d` is the original data object for the row
            return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:0px;">' +
                '<tr>' +
                '<td><label class="col-form-label control-label">Número de operación</label></td>' +
                '<td><input type="text" name="NumOperationCFDI" id="" class="form-control" autocomplete="off" maxlength="50" /></td>' +
                '<td><label class="col-form-label control-label">Monto</label></td>' +
                '<td><input type="text" name="AmountCFDI" id="" class="form-control money" autocomplete="off" maxlength="50" /></td>' +
                '</tr>' +
                '<tr>' +
                '<td><label class="col-form-label control-label">Moneda</label></td>' +
                '<td><select name="CurrencyCFDI" id="" class="form-control"  class="form-control chosen-select"></select></td>' +
                '<td><label class="col-form-label control-label">Tipo de Cambio</label></td>' +
                '<td><input type="text" name="ExchangeRateCFDI" id="" class="form-control money" autocomplete="off" maxlength="50" /></td>' +
                '</tr>' +
                '<tr>' +
                '<td><label class="col-form-label control-label">Forma de Pago</label></td>' +
                '<td><select name="PaymentFormCFDI" id="" class="form-control chosen-select"></select></td>' +
                '<td><label class="col-form-label control-label">Fecha</label></td>' +
                '<td>' +
                //< input type = "text" name = "DateCFDI" id = "" class="form-control" autocomplete = "off" maxlength = "50" />
                '<div id="data_1">' +
                '<div class="input-group date">' +
                '<span class="input-group-addon"><i class="fa fa-calendar"></i></span>' +
                '<input type="text" name="startedAt" id="startedAt" class="form-control required" readonly>' +
                '</div>' +
                '</div>' +
                '</td >' +
                '</tr>' +
                '</table>';
        }
        function copySelects() {
            var $optionsPF = $("#PaymentForm > option").clone();
            $("select[name='PaymentFormCFDI']").append($optionsPF);

            var $optionsC = $("#CurrencyForm > option").clone();
            $("select[name='CurrencyCFDI']").append($optionsC);

            $('.money').mask("##,###,##0.00", { reverse: true });

            $('#data_1 .input-group.date').datepicker({
                //dateFormat: "yy-mm-dd"
                keyboardNavigation: false,
                todayBtn: "linked",
                forceParse: false,
                calendarWeeks: true,
                autoclose: true,
                format: "dd/mm/yyyy",
                language: "es",
                startDate: DateInit.MinDate
            });
        }


        $.validator.addMethod("Alphanumeric",
            function (value, element) {
                return value.match(/^[A-Za-zÀ-ÿ\u00f1\u00d10-9 _.-]+$|^$/);
            }, "El campo debe ser alfanumérico"
        );
        $.validator.addMethod("Numeric",
            function (value, element) {
                return value.match(/^[0-9]+$|^$/);
            }, "El campo debe ser numérico"
        );

        $.validator.addMethod("RFCTrue",
            function (value, element) {
                const re = /^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z\d]{2})([A\d])$/;
                var valid = value.match(re);

                if (valid === null)
                    return false;

                return true;
            }, "Debe ser un RFC válido"
        );

        $('.money').mask("##,###,##0.00", { reverse: true });
        $('.rateMoney').mask("##0.00", { reverse: false });

        //Agregar a la lista de conceptos del producto
        $("#addTaxes").click(function () {
            //console.log("estoy aqui")

            var t = $("#tableImpuestos").DataTable();
            var ind = t.rows().count();
            var index = parseInt(ind);
            index++;
            var radio = $("input[name='taxes']:checked").val();
            var input = "";
            if (radio === "Retención")
                input = $("#Withholdings").val();
            else
                input = $("#Transferred").val();

            t.row.add(
                [{
                    "Tipo": radio,
                    "Impuesto": input,
                    "Porcentaje": $("#Valuation").val(),
                    "index": index
                }]
            ).draw(false);

            Prices();
        });

        $("#InvoicingForm").validate({
            rules: {
                InvoiceComplement: {
                    Alphanumeric: true
                },
                CustomerName: {
                    Alphanumeric: true
                },
                RFC: {
                    required: true,
                    RFCTrue: true,
                    Alphanumeric: true
                },
                Street: {
                    Alphanumeric: true
                },
                OutdoorNumber: {
                    Alphanumeric: true
                },
                InteriorNumber: {
                    Alphanumeric: true
                },
                ZipCode: {
                    required: true,
                    Numeric: true
                },
                MotionNumber: {
                    Alphanumeric: true
                },
                PaymentConditions: {
                    Alphanumeric: true
                },
                Comments: {
                    Alphanumeric: true
                },
            }
        });

        $("#ConceptForm").validate({
            rules: {
                SATCode: {
                    Alphanumeric: true
                },
                ProductServiceDescription: {
                    Alphanumeric: true
                },
            }
        });

        /*Obtener los porcentajes*/
        $(".rateFee").change(function () {
            //console.log($(this).val());
            var cmbTaxes = $("#Valuation");
            cmbTaxes.empty();
            var type = $("input[name='taxes']:checked").val();
            var valueTax = $(this).val();

            //if (type === "Retención") {
            //    valueTax = $("#Withholdings").val();
            //} else {
            //    valueTax = $("#Transferred").val();
            //}
            //console.log(valueTax, type, "valor")

            $.ajax({
                type: 'Get',
                async: true,
                data: { value: valueTax, typeTaxes: type },
                url: self.rateTaxesUrl,
                success: function (json) {
                    //console.log(json, "respuesta");
                    if (json.success) {
                        var data = json.data;
                        data.forEach(function (item, index) {
                            var position = countDecimals(item.maximumValue);
                            //console.log(position, "valor", item.maximumValue);
                            if (position > 10) {
                                var value = item.maximumValue.toString().split(".");
                                cmbTaxes.append($('<option></option>').val(value[0]).text(value[0] + "%"));
                            } else if (item.maximumValue === -100)
                                cmbTaxes.append($('<option></option>').val("Exento").text("Exento"));
                            else
                                cmbTaxes.append($('<option></option>').val(item.maximumValue).text(item.maximumValue + "%"));
                        });

                        $(".chosen-select").trigger("chosen:updated");

                        El20Utils.ocultarCargador();
                    }
                    else {
                        El20Utils.ocultarCargador();
                    }

                },
                error: function (xhr) {
                    console.log('error', xhr);
                    El20Utils.ocultarCargador();
                }
            });
        });

        var countDecimals = function (value) {
            let text = value.toString();
            // verify if number 0.000005 is represented as "5e-6"
            if (text.indexOf('e-') > -1) {
                let [base, trail] = text.split('e-');
                let deg = parseInt(trail, 10);
                return deg;
            }
            // count decimals for number in representation like "0.123456"
            if (Math.floor(value) !== value) {
                return value.toString().split(".")[1].length || 0;
            }
            return 0;
        }
        //countDecimals(val);

        /*Buscador de facturas para complementos*/
        $("#TypeInvoice").change(function () {
            console.log("opción de relación de facturas", $(this).val());
            var typeInvoice = $(this).val();

            if (typeInvoice === "P" || typeInvoice === "E") {
                $("#relacionados").removeClass("hide");
                $("#Condiciones").addClass("hide");
                $("#btnProdServ").addClass("hide");
            } else {
                $("#relacionados").addClass("hide");
                $("#Condiciones").removeClass("hide");
                $("#btnProdServ").addClass("hide");
            }
        });

        $(".search-invoice").click(function () {
            //console.log("entre al evento", $(this).val());
            El20Utils.mostrarCargador();

            var uuid = $("#InvoiceComplement").val();
            console.log("valor del uuid", uuid);
            $.ajax({
                type: 'Get',
                async: true,
                data: { uuid: uuid },
                url: self.searchCDFIUrl,
                success: function (json) {
                    console.log(json, "respuesta");

                    if (json.success) {
                        var data = json.data;
                        if (data === null)
                            toastr['error']("No se encontro la factura con el Folio Fiscal.", null, { 'positionClass': 'toast-top-center' });
                        else {
                            console.log("xml", json.data.xml);
                            xml = json.data.xml;
                            //$("#CDFIS").append('<input type="text" readonly name="cfdi[]" class="form-control" value="' + data.uuid + '">');                            
                            var t = self.dataTableCFDI;
                            var ind = t.rows().count();
                            var index = parseInt(ind);
                            index++;

                            t.row.add(
                                {
                                    "uuid": data.factura.uuid,
                                    "currency": xml.Moneda,
                                    "exchangeRate": xml.TipoCambio,
                                    "previousBalance": xml.Total,
                                    "paid": 0,
                                    "outstanding": 0,
                                    "method": xml.MetodoPago,
                                    "numberPartialities": 0,
                                    "folio": xml.Folio,
                                    "serie": xml.Serie,
                                    "index": index
                                }
                            ).draw(false);

                            $("#InvoiceComplement").val("");
                        }

                        El20Utils.ocultarCargador();
                    }
                    else {
                        toastr['error']("No se encontro la factura con el Folio Fiscal.", null, { 'positionClass': 'toast-top-center' });
                        El20Utils.ocultarCargador();
                    }

                },
                error: function (xhr) {
                    console.log('error', xhr);
                    toastr['error']("Se encontro un error al buscar la factura con el Folio Fiscal.", null, { 'positionClass': 'toast-top-center' });
                    El20Utils.ocultarCargador();
                }
            });

        });

        $("#RFC").keyup(function () {
            this.value = this.value.toUpperCase();
        });

        $("#Quantity,#UnitPrice,#DiscountRateProServ").keyup(function () {
            Prices();
        });

        function Prices() {
            self.retencionISRTemp = 0;
            self.retencionIVATemp = 0;
            self.trasladosTemp = 0;
            var quantity = parseFloat(self.quantity.val() !== "" ? self.quantity.val() : 0);
            var unitPrice = parseFloat(self.unitPrice.val() !== "" ? (self.unitPrice.val().replace(",", "")) : 0);
            var discountRate = parseFloat(self.discountRate.val() !== "" ? self.discountRate.val() : 0);
            var impuestos = $("#tableImpuestos").DataTable();
            //console.log(discountRate, "descuento");

            //dudas con el impuesto
            //var taxesIEPS = parseFloat(self.taxesIEPS);
            //var taxesIVA = parseFloat(self.taxesIVA);
            //var subtotal = parseFloat(self.subtotal);

            //Calcular el subtotal    
            if (quantity > 0 && unitPrice > 0) {
                var sub = quantity * unitPrice;
                var impIVAISR = 0; //parseFloat(self.taxesIVA.val() !== "" ? self.taxesIVA.val() : 0);
                var impIVAIESP = 0; //parseFloat(self.taxesIEPS.val() !== "" ? self.taxesIVA.val() : 0);

                var discount = discountRate > 0 || discountRate < 0 ? (discountRate * sub) / 100 : 0;
                //console.log("desceutno", discount);
                //console.log("holas")
                if (impuestos.rows().count() > 0) {
                    //console.log(impuestos, "tabla")
                    for (var i = 0; i < impuestos.rows().count(); i++) {
                        var datos = impuestos.row(i).data()[0];
                        //console.log(datos, "datos");
                        if (datos.Porcentaje !== "Exento") {
                            var por = parseFloat(datos.Porcentaje);
                            var imp1 = por > 0 ? (por * sub) / 100 : 0;
                            if (datos.Tipo === "Retención") {
                                if (datos.Impuesto === "ISR") {
                                    self.retencionISRTemp = self.retencionISRTemp + imp1;
                                } else {
                                    self.retencionIVATemp = self.retencionIVATemp + imp1;
                                }
                                impIVAISR = impIVAISR + imp1;
                                //console.log(impIVAISR, imp1, "iva isr");
                            } else {
                                var imp2 = por > 0 ? (por * sub) / 100 : 0;
                                self.trasladosTemp = self.trasladosTemp + imp2;
                                impIVAIESP = impIVAIESP + imp2;
                            }
                        }
                    }
                }
                //console.log("holas", self.trasladosTemp, self.retencionIVATemp, self.retencionISRTemp);

                var subtotal = sub - discount; // - impIVAISR + impIVAIESP;
                //console.log(subtotal, "subtotal sin el descuento")
                //console.log(subtotal.toFixed(2), "subtotal ")
                self.taxesIVA.val(impIVAISR.toFixed(2));
                self.taxesIEPS.val(impIVAIESP.toFixed(2));
                self.subtotal.val(subtotal.toFixed(2)).trigger("click");
            }
        }

        //Borrar valores cuando se oculte el modal
        $('#ServProdModal').on('hide.bs.modal', function (e) {
            // do something...
            $("#ConceptForm").each(function () {
                //console.log("holas");
                this.reset();
                $("#Row").val("");
                var table = $("#tableImpuestos").DataTable();

                table.clear().draw();
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
            var impuestos = $("#tableImpuestos").DataTable().rows().data();
            var arrayImpuestos = new Array();

            if (impuestos.count()) {
                impuestos.each(function (item, i) {
                    // console.log(item[0], i, "array");
                    var obj = JSON.stringify(item[0]).replace(/"/g, "'");
                    arrayImpuestos.push(obj);
                });
            }

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
                        self.taxesIVA.val(),
                        self.taxesIEPS.val(),
                        arrayImpuestos.join(";"),
                        self.subtotal.val(),
                        indexR
                    ]
                ).draw(false);

                self.retencionISR = self.retencionISR + self.retencionISRTemp - self.retencionISRTempDesc;
                self.retencionIVA = self.retencionIVA + self.retencionIVATemp - self.retencionIVATempDesc;
                self.traslados = self.traslados + self.trasladosTemp - self.trasladosTempDesc;
            } else {
                var ind = t.rows().count();
                var index = parseInt(ind);
                self.retencionISR = self.retencionISR + self.retencionISRTemp;
                self.retencionIVA = self.retencionIVA + self.retencionIVATemp;
                self.traslados = self.traslados + self.trasladosTemp;
                //console.log("se agregar", self.retencionISR, self.retencionIVA, self.traslados);
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
                        self.taxesIVA.val(),
                        self.taxesIEPS.val(),
                        arrayImpuestos.join(";"),
                        self.subtotal.val(),
                        index
                    ]
                ).draw(false);
            }

            $("input[name='taxes']").attr("checked", false);
            //$("input[name='taxes']").attr("checked", false);
            $("#TaxesChk").trigger("click");

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
            //var value = $(this).val();
            //var column = self.dataTable.column(9);

            //// Toggle the visibility
            //if (value === "IEPS") {
            //    $(".trTaxT").removeClass("hide");
            //    column.visible(true);
            //}
            //else {
            //    $(".trTaxT").addClass("hide");
            //    column.visible(false);
            //}
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
                        if (json.logo !== null)
                            $("#logo").attr("src", json.logo);
                        else
                            $("#logo").attr("src", "#");

                        if (!json.sello) {
                            swal({
                                title: "Sucursal sin CSD",
                                text: "Esta sucursar no cuenta con un Certificado de Sello Digital registrado",
                                type: "warning",
                                showCancelButton: true,
                                confirmButtonClass: "btn-danger",
                                confirmButtonText: "Ir a registrar CSD",
                                cancelButtonText: "Cambiar sucursal",
                                closeOnConfirm: false
                            },
                                function (inputValue) {
                                    if (inputValue === true)
                                        window.location = self.officeSellos;
                                    else {
                                        $("#BranchOffice").val("");
                                        $('#BranchOffice').trigger('chosen:updated');
                                        $("#logo").attr("src", "");
                                    }
                                    // swal("Deleted!", "Your imaginary file has been deleted.", "success");
                                });
                        }
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
            var cmbTaxes = $("#Valuation");
            cmbTaxes.empty();

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
                $("#MotionNumber").val("").removeClass("required");
            }
            else {
                international.removeClass("hide");
                $("#MotionNumber").addClass("required");
            }
        });

        $("input[name='taxes']").click(function () {
            //console.log("valor", $(this).val())
            var cmbTaxes = $("#Valuation");
            cmbTaxes.empty();
            if ($(this).val() === "Retención") {
                $(".retenciones").removeClass("hide");
                $(".traslados").addClass("hide");
                $("#Transferred").val("");
            } else {
                $(".retenciones").addClass("hide");
                $(".traslados").removeClass("hide");
                $("#Withholdings").val("");
            }
        });

        $("#InvoiceComplementChk").click(function () {
            //console.log("estoy dento", !this.checked);
            var complement = $(".complement");
            var typeRelationship = $("#TypeRelationship");

            if (!this.checked) {
                complement.addClass("hide");
                typeRelationship.removeClass("required").val("");
                $("#InvoiceComplement").val("");
            }
            else {
                typeRelationship.addClass("required");
                complement.removeClass("hide");
            }
        });

        //Buscar información del cliente por Razon Social
        $('#CustomerName').typeahead({
            source: function (query, process) {
                //console.log(query, process, "esto trae");                
                var type = $("#TypeInvoice").val();
                return $.get(self.searchUrl + "?value=" + query + "&typeInvoice=" + type, function (result) {
                    // console.log(result, "respuesta");
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
                $('#TypeReceptor').attr('value', item.type);
                GetReceiver(item.id, item.type);
                return item.name;
            }
        });

        $("#ZipCode").blur(function () {
            //console.log("perdio el focus");            
            ClearCombos();
            var value = $(this).val();
            if (!value) {
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
                            cmbCountry.append($('<option></option>').val(datos[0].countryId).text(datos[0].nameCountry));

                            cmbState.append($('<option></option>').val(datos[0].stateId).text(datos[0].nameState));

                            cmbMunicipality.append($('<option></option>').val(datos[0].municipalityId).text(datos[0].nameMunicipality));

                            datos.forEach(function (item, index) {
                                cmbColony.append($('<option></option>').val(item.id).text(item.nameSettlementType + ' ' + item.nameSettlement));
                            });

                            $(".chosen-select").trigger("chosen:updated");

                        } else {
                            ClearCombos();
                            toastr["error"]("El registro de Código Postal no se encontró en la base de datos", null, { 'positionClass': 'toast-top-center' });
                        }
                    } else {
                        ClearCombos();
                        toastr["error"]("El registro de Código Postal no se encontró en la base de datos", null, { 'positionClass': 'toast-top-center' });
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

            cmbColony.empty();
            cmbMunicipality.empty();
            cmbState.empty();
            cmbCountry.empty();
            $(".chosen-select").trigger("chosen:updated");
        }

        //Buscar información de la clave de producto o servicio
        $('#SATCode').typeahead({
            minLength: 3,
            source: function (query, process) {
                El20Utils.mostrarCargador();
                //console.log(query, process, "esto trae");
                return $.get(self.codeSATUrl + "?Concept=" + query, function (result) {
                    //console.log(result, "respuesta");
                    var resultList = result.data.map(function (item) {
                        var aItem = { id: item.id, name: item.code, type: item.description };
                        return JSON.stringify(aItem);
                    });
                    El20Utils.ocultarCargador();
                    $('#SATCode').focus();
                    $("body").addClass("modal-open");
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
                    //console.log(json, "respuesta");
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
                    else {
                        El20Utils.ocultarCargador();
                    }

                },
                error: function (xhr) {
                    console.log('error', xhr);
                    El20Utils.ocultarCargador();
                }
            });
        }

        $("#SavedInvoice").click(function () {
            El20Utils.mostrarCargador();
            if (!$('#InvoicingForm').valid()) {
                //return;

                $('html, body').animate({
                    scrollTop: ($('.error').offset().top - 300)
                }, 2000, function () {
                    El20Utils.ocultarCargador();
                });
                return;
            }

            $("#InvoicingForm").attr('action', 'Invoice');
            $('#InvoicingForm').submit();
        });

        $("#validarDatos").click(function () {
            El20Utils.mostrarCargador();
            if (!$('#InvoicingForm').valid()) {
                //return;
                $('html, body').animate({
                    scrollTop: ($('.error').offset().top - 300)
                }, 2000, function () {
                    El20Utils.ocultarCargador();
                });
                return;
            }

            //mandar información de la tabla
            $("#InvoicingForm").attr('action', 'IssueIncomeInvoice');
            $('#InvoicingForm').submit();
        });

        /*Agregar emails*/
        var indexEmail = 1;
        $(".btn-add-email").click(function () {
            var item = '<br/><input class="form-control" type="email" maxlength="300" autocomplete="off" name="ListCustomerEmail[' + indexEmail + ']" />';
            $("#ListEmails").append(item);
            indexEmail++;
        });

    };

};