var SATController = function () {
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
                        toastr['error'](data.message);
                    }
                }
                else {
                    setTimeout(self.finishValidation(uuid), 2000);
                }

            }).fail(function () {
                toastr['error']('No es posible finalizar la creación de la cuenta');
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
                    toastr['error'](data.message);
                    El20Utils.ocultarCargador();
                }
            }).fail(function () {
                toastr['error']('No es posible validar el RFC');
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
                        toastr['error'](data.message);
                        El20Utils.ocultarCargador();
                    }
                },
                error: function (error) {
                    toastr['error']('No es posible validar el RFC');
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
        var $image = $('#image');
        var input = $('#input');
        var $progress = $('.progress');
        var $progressBar = $('.progress-bar');
        var $alert = $('.alert');
        var $modal = $('#modal');
        var cropper;

        $('[data-toggle="tooltip"]').tooltip();

        input.on("change", function () {
            var files = this.files;
            var done = function (url) {
                input.value = '';
                image.src = url;
                $alert.hide();
                $modal.modal('show');
            };
            var reader;
            var file;
            var url;

            if (files && files.length > 0) {
                file = files[0];

                if (URL) {
                    done(URL.createObjectURL(file));
                } else if (FileReader) {
                    reader = new FileReader();
                    reader.onload = function (e) {
                        done(reader.result);
                    };
                    reader.readAsDataURL(file);
                }
            }
        });

        $modal.on('shown.bs.modal', function () {
            $image.cropper({
                aspectRatio: 16 / 9,
                crop: function (event) {
                    console.log(event.detail.x);
                    console.log(event.detail.y);
                    console.log(event.detail.width);
                    console.log(event.detail.height);
                    console.log(event.detail.rotate);
                    console.log(event.detail.scaleX);
                    console.log(event.detail.scaleY);
                }
            });

            cropper = $image.data('cropper');
        }).on('hidden.bs.modal', function () {
            //cropper.destroy();
            //cropper = null;
        });

        $("#crop").on("click", function () {
            var initialAvatarURL;
            var canvas;

            $modal.modal('hide');
            if (cropper) {
                canvas = cropper.getCroppedCanvas({
                    width: 120,
                    height: 120,
                });
                initialAvatarURL = avatar.attr('src');
                avatar.attr("src", canvas.toDataURL());
                $progress.show();
                $alert.removeClass('alert-success alert-warning');
                canvas.toBlob(function (blob) {
                    var formData = new FormData();

                    formData.append('avatar', blob, 'avatar.jpg');
                    $.ajax('https://jsonplaceholder.typicode.com/posts', {
                        method: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        xhr: function () {
                            var xhr = new XMLHttpRequest();
                            xhr.upload.onprogress = function (e) {
                                var percent = '0';
                                var percentage = '0%';

                                if (e.lengthComputable) {
                                    percent = Math.round((e.loaded / e.total) * 100);
                                    percentage = percent + '%';
                                    $progressBar.width(percentage).attr('aria-valuenow', percent).text(percentage);
                                }
                            };
                            return xhr;
                        },
                        success: function () {
                            toastr['success']('Actualización exitosa');

                        },
                        error: function () {
                            avatar.attr("src", initialAvatarURL);
                            toastr['error']('Error al actualizar');
                        },
                        complete: function () {
                            $progress.hide();
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