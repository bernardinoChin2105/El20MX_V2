var BranchOfficeCreateController = function (urlUpdateLogo, urlGetLocations) {
    var _urlUpdateLogo = urlUpdateLogo;
    var _urlGetLocations = urlGetLocations;

    this.init = function () {
        $('.chosen-select').chosen({ width: '100%', no_results_text: "Sin resultados para " });

        $(".view-ciec").mouseup(function () {
            $(".view-ciec-input").attr('type', 'password').data("view", false);
        }).mousedown(function () {
            if ($(".view-ciec-input").val() !== "") {
                $(".view-ciec-input").attr('type', 'text').data("view", true);
            }
        });

        $(".view-e-firma").mouseup(function () {
            $(".view-e-firma-input").attr('type', 'password').data("view", false);
        }).mousedown(function () {
            if ($(".view-e-firma-input").val() !== "") {
                $(".view-e-firma-input").attr('type', 'text').data("view", true);
            }
        });

        $('.custom-file-input').on('change', function (e) {
            let fileName = $(this).val().split('\\').pop();
            $(this).next('.custom-file-label').addClass("selected").html(fileName);
        });

        var mem = $('#data_1 .input-group.date').datepicker({
            //dateFormat: "yy-mm-dd"
            keyboardNavigation: false,
            todayBtn: "linked",
            forceParse: false,
            calendarWeeks: true,
            autoclose: true,
            format: "dd/mm/yyyy",
            language: "es"
        });

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

        $.validator.addMethod("Alphabetic",
            function (value, element) {
                return value.match(/^[a-zA-ZÀ-ÿ\u00f1\u00d1 ]+$|^$/);
            }, "El campo debe ser alfabético"
        );
        $("#CreateForm").validate({
            rules: {
                name: {
                    required: true,
                    Alphanumeric: true,
                },
                folio: {
                    required: true,
                    Numeric: true,
                },
                serie: {
                    required: true,
                    Alphanumeric: true,
                },
                zipCode: {
                    required: true,
                    Numeric: true
                },
                street: {
                    Alphanumeric: true,
                },
                outdoorNumber: {
                    Alphanumeric: true,
                },
                interiorNumber: {
                    Alphanumeric: true,
                }
            }
        });
    }

    $("#btn-save").on("click", function () {
        if (!$('#CreateForm').valid()) {
            return;
        }
        $('#CreateForm').submit();
    });

    $("#zipCode").blur(function () {
        //console.log("perdio el focus");
        var value = $(this).val();
        if (!value) {
            ClearCombos();
            return;
        }
        var cmbColony = $("#colony");
        var cmbMunicipality = $("#municipality");
        var cmbState = $("#state");
        var cmbCountry = $("#country");
        //console.log("valor", value);

        $.ajax({
            type: 'Get',
            async: true,
            data: { zipCode: value },
            url: _urlGetLocations,
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
                        ClearCombos();
                        toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                    }

                } else {
                    ClearCombos();
                    toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                }

            },
            error: function (xhr) {
                ClearCombos();
                console.log('error');
            }
        });

        function ClearCombos() {
            var cmbColony = $("#colony");
            var cmbMunicipality = $("#municipality");
            var cmbState = $("#state");
            var cmbCountry = $("#country");

            cmbState.val(-1);
            cmbCountry.html('<option value="-1">Seleccione...</option>').val(-1);
            cmbMunicipality.html('<option value="-1">Seleccione...</option>').val(-1);
            cmbColony.html('<option value="-1">Seleccione...</option>').val(-1);
        }
    });
}