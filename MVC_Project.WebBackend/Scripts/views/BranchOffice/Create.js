var BranchOfficeCreateController = function (urlUpdateLogo, urlGetLocations) {
    var _urlUpdateLogo = urlUpdateLogo;
    var _urlGetLocations = urlGetLocations;

    this.init = function () {
        $('.chosen-select').chosen({ width: '100%', no_results_text: "Sin resultados para ", placeholder_text_single: "Seleccione..." });

        $.validator.setDefaults({ ignore: ":hidden:not(.chosen-select)" }) //for all select having            
        $('.chosen-select').on('change', function () {
            $(this).valid();
        });

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
            }, "El campo debe ser alfanumérico."
        );

        $.validator.addMethod("Numeric",
            function (value, element) {
                return value.match(/^[0-9]+$|^$/);
            }, "El campo debe ser numérico."
        );

        $.validator.addMethod("Alphabetic",
            function (value, element) {
                return value.match(/^[a-zA-ZÀ-ÿ\u00f1\u00d1 ]+$|^$/);
            }, "El campo debe ser alfabético."
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
                },
                colony: {
                    required: true,
                },
                municipality: {
                    required: true,
                },
                state: {
                    required: true,
                },
                country: {
                    required: true,
                }
            }
        });
    }

    $(".view-password").mouseup(function () {
        $(".view-password-input").attr('type', 'password').data("view", false);
    }).mousedown(function () {
        if ($(".view-password-input").val() !== "") {
            $(".view-password-input").attr('type', 'text').data("view", true);
        }
    });

    $("#btn-save").on("click", function () {
        if (!$('#CreateForm').valid()) {
            $('html, body').animate({
                scrollTop: ($('.error').offset().top - 300)
            }, 2000);
            return;
        }
        $('#CreateForm').submit();
    });

    $("#zipCode").blur(function () {
        ClearCombos();
        var value = $(this).val();
        if (!value) {
            return;
        }
        var cmbColony = $("#colony");
        var cmbMunicipality = $("#municipality");
        var cmbState = $("#state");
        var cmbCountry = $("#country");

        $.ajax({
            type: 'Get',
            async: true,
            data: { zipCode: value },
            url: _urlGetLocations,
            success: function (json) {
                if (json.Data.success) {
                    var datos = json.Data.data;
                    if (datos.length > 0) {
                        cmbCountry.append($('<option></option>').val(datos[0].countryId).text(datos[0].nameCountry));
                        
                        cmbState.append($('<option></option>').val(datos[0].stateId).text(datos[0].nameState));
                        
                        cmbMunicipality.append($('<option></option>').val(datos[0].municipalityId).text(datos[0].nameMunicipality));
                        
                        datos.forEach(function (item, index) {
                            cmbColony.append($('<option></option>').val(item.id).text(item.nameSettlementType + ' ' + item.nameSettlement));
                        });

                        $(".chosen-select").trigger("chosen:updated").trigger("change");

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
                console.log('error');
            }
        });

        function ClearCombos() {
            var cmbColony = $("#colony");
            var cmbMunicipality = $("#municipality");
            var cmbState = $("#state");
            var cmbCountry = $("#country");

            cmbColony.empty();
            cmbMunicipality.empty();
            cmbState.empty();
            cmbCountry.empty();
            $(".chosen-select").trigger("chosen:updated");
        }
    });
}