var BranchOfficeEdtihController = function (urlUpdateLogo, urlGetLocations) {
    var _urlUpdateLogo = urlUpdateLogo;
    var _urlGetLocations = urlGetLocations;

    this.init = function () {
        $('.chosen-select').chosen({ width: '100%', no_results_text: "Sin resultados para ", placeholder_text_single: "Seleccione..." });

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
        $("#EditForm").validate({
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

        if ($('#hasDeferredPayment').prop("checked")) {
            $("#partialitiesNumber").prop('readonly', false);
            $("#advancePayment").prop('readonly', false);
            $(".btn-opt").prop("disabled", false);
        } else {
            $("#partialitiesNumber").val(1);
            $("#advancePayment").val(0);
            $("#monthlyCharge").val($("#total").val());
            $("#partialitiesNumber").prop('readonly', true);
            $("#advancePayment").prop('readonly', true);
            $(".btn-opt").prop("disabled", true);
        }

        var avatar = $('#avatar');
        var image = document.getElementById('image');
        var input = $('#input');
        var $modal = $('#modal');
        var cropper;
        var fileName = "";
        $('[data-toggle="tooltip"]').tooltip();

        input.on("change", function () {
            var files = this.files;
            var done = function (url) {
                input.value = '';
                image.src = url;
                $modal.modal('show');
            };

            if (files && files.length > 0) {
                var file = files[0];
                fileName = file.name;
                if (URL) {
                    done(URL.createObjectURL(file));
                } else if (FileReader) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        done(reader.result);
                    };
                    reader.readAsDataURL(file);
                }
            }
        });

        $modal.on('shown.bs.modal', function () {
            cropper = new Cropper(image, {
                aspectRatio: 16 / 9
            });
        }).on('hidden.bs.modal', function () {
            cropper.destroy();
            cropper = null;
        });

        $("#crop").on("click", function () {
            var initialAvatarURL;
            var canvas;

            $modal.modal('hide');
            El20Utils.mostrarCargador();
            if (cropper) {
                canvas = cropper.getCroppedCanvas({
                    width: 160,
                    height: 160,
                });
                initialAvatarURL = avatar.attr('src');
                avatar.attr("src", canvas.toDataURL());
                canvas.toBlob(function (blob) {
                    var formData = new FormData();
                    formData.append('uuid', $('#uuid').val());
                    formData.append('fileName', fileName);
                    formData.append('image', blob);

                    $.ajax({
                        type: 'POST',
                        url: _urlUpdateLogo,
                        data: formData,
                        processData: false,
                        contentType: false,
                        success: function (data) {
                            if (data.success)
                                toastr['success']('Actualización exitosa', null, { 'positionClass': 'toast-top-center' });
                            else
                                toastr['error'](data.message, null, { 'positionClass': 'toast-top-center' });
                        },
                        error: function () {
                            avatar.attr("src", initialAvatarURL);
                            toastr['error']('No fue posible realizar la actualización', null, { 'positionClass': 'toast-top-center' });
                        },
                        complete: function () {
                            El20Utils.ocultarCargador();
                        },
                    });
                });
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
        if (!$('#EditForm').valid()) {
            $('html, body').animate({
                scrollTop: ($('.error').offset().top - 300)
            }, 2000);
            return;
        }
        $('#EditForm').submit();
    });

    $("#hasDeferredPayment").on("ifChanged", function () {
        if (this.checked) {
            $("#partialitiesNumber").prop('readonly', false);
            $("#advancePayment").prop('readonly', false);
            $(".btn-opt").prop("disabled", false);
        }
        else {
            $("#partialitiesNumber").val(1);
            $("#advancePayment").val(0);
            $("#monthlyCharge").val($("#total").val());
            $("#partialitiesNumber").prop('readonly', true);
            $("#advancePayment").prop('readonly', true);
            $(".btn-opt").prop("disabled", true);
        }
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
        //console.log("valor", value);

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