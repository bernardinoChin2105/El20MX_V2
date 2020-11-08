var SATController = function () {
    this.init = function () {
        $(".alert").show();
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

        var self = this;
        this.finishValidation = function (uuid) {
            $.get('SAT/CredentialStatus', { uuid: uuid }, function (data) {
                if (data.finish) {
                    if (data.success) {
                        El20Utils.ocultarCargador();
                        let form = document.createElement("form");
                        form.setAttribute("method", "Get");
                        form.setAttribute("action", 'SAT/Index');
                        document.body.appendChild(form);
                        form.submit();
                    }
                    else {
                        El20Utils.ocultarCargador();
                        toastr['error'](data.message, null, { 'positionClass': 'toast-top-center' }); 
                    }
                }
                else {
                    setTimeout(self.finishValidation(uuid), 2000);
                }

            }).fail(function () {
                //toastr['error']('No es posible finalizar la creación de la cuenta');
                toastr['error']('No es posible finalizar la creación de la cuenta', null, { 'positionClass': 'toast-top-center' }); 
                El20Utils.ocultarCargador();
            });
        }

        $("#btn-ciec").on("click", function () {
            El20Utils.mostrarCargadorForzado();

            var ciecUuid = $("#ciecUuid").val();
            var rfc = $("#rfc").val();
            var ciec = $("#ciec").val();
            $.post('SAT/UpdateCredential', { ciecUuid: ciecUuid, rfc: rfc, ciec: ciec }, function (data) {
                if (data.success) {
                    //$("#uuid").val(data.uuid);
                    self.finishValidation(data.uuid)
                }
                else {
                    toastr['error'](data.message, null, { 'positionClass': 'toast-top-center' }); 
                    El20Utils.ocultarCargador();
                }
            }).fail(function () {
                toastr['error']('No es posible validar el RFC', null, { 'positionClass': 'toast-top-center' }); 
                El20Utils.ocultarCargador();
            });
        });

        $("#btn-efirma").on("click", function () {
            El20Utils.mostrarCargadorForzado();

            var fd = new FormData();
            var cer = $('#cer')[0].files;
            var key = $('#key')[0].files;

            if (cer.length > 0)
                fd.append('cer', cer[0]);

            if (key.length > 0)
                fd.append('key', key[0]);

            var efirmaUuid = $("#efirmaUuid").val();
            fd.append('efirmaUuid', efirmaUuid);

            var efirma = $("#efirma").val();
            fd.append('efirma', efirma);

            var rfc = $("#rfc").val();

            fd.append('rfc', rfc);

            $.ajax({
                type: "POST",
                url: 'SAT/UpdateEfirma',
                data: fd,
                contentType: false,
                processData: false,
                success: function (data) {
                    if (data.success) {
                        self.finishValidation(data.uuid)
                    }
                    else {
                        toastr['error'](data.message, null, { 'positionClass': 'toast-top-center' }); 
                        El20Utils.ocultarCargador();
                    }
                },
                error: function (error) {
                    toastr['error']('No es posible validar el RFC', null, { 'positionClass': 'toast-top-center' }); 
                    El20Utils.ocultarCargador();
                }
            });
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
        $("#EditForm").validate({
            rules: {
            }
        });
        
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
                    width: 120,
                    height: 120,
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
                        url: 'SAT/UpdateImage',
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

        $("#btn-save").on("click", function () {
            if (!$('#EditForm').valid()) {
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
    }
}