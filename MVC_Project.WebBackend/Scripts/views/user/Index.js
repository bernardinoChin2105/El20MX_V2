var UserIndexControlador = function (htmlTableId, baseUrl, modalEditAction, modalDeleteAction, modelEditPasswordAction, 
    modalEditPasswordId, formEditPasswordId, submitEditPasswordId, hasFullAccessController) {
    var self = this;
    this.htmlTable = $('#' + htmlTableId);
    this.baseUrl = baseUrl;
    this.dataTable = {};
    this.modalEditPassword = $('#' + modalEditPasswordId);
    this.hasFullAccessController = hasFullAccessController;
    const utils = new Utils();
    this.initModal = function () {

    }
    this.init = function () {
        self.dataTable = this.htmlTable.DataTable({
            language: { url: 'Scripts/custom/dataTableslang.es_MX.json' },
            "bProcessing": true,
            "bServerSide": true,
            "sAjaxSource": this.baseUrl,
            orderMulti: false,
            searching: false,
            ordering: false,
            columns: [
                { data: 'Id', title: "Id", visible: false },
                { data: 'Email', title: "Correo" },
                { data: 'RoleName', title: "Rol" },
                { data: 'Name', title: "Nombre" },
                {
                    data: null, orderName: "CreatedAt", title: "Fecha de Creación", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.CreatedAt !== null && data.CreatedAt !== "") {
                            return moment(data.CreatedAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                {
                    data: null, orderName: "UpdatedAt", title: "Último Acceso", autoWidth: false, className: "dt-center td-actions thead-dark",
                    render: function (data, type, row, meta) {
                        if (data.LastLoginAt !== null && data.LastLoginAt !== "") {
                            return moment(data.LastLoginAt).format('DD-MMM-YYYY');
                        }
                        return '';
                    }
                },
                { data: 'Status', title: "Estatus" },
                {
                    data: null,
                    className: 'personal-options',
                    render: function (data) {
                        var deshabilitarBtns = '';
                        if (data.Status == "ACTIVE")
                            deshabilitarBtns += '<button class="btn btn-light btn-change-status" title="Desactivar" style="margin-left:5px;"><span class="far fa-check-square "></span></button>';
                        else if (data.Status == "INACTIVE")
                            deshabilitarBtns += '<button class="btn btn-light btn-change-status" title="Activar" style="margin-left:5px;"><span class="far fa-square"></span></button>';
                        else if (data.Status == "UNCONFIRMED")
                            deshabilitarBtns += '<button class="btn btn-light btn-resend-invite" title="Reenviar invitación" style="margin-left:5px;"><span class="fa fa-paper-plane"></span></button>';
                        else
                            deshabilitarBtns += '';

                        var buttons = '<div class="btn-group" role="group" aria-label="Opciones">' +
                            deshabilitarBtns +
                            '<button class="btn btn-light btn-edit" title="Editar Usuario"><span class="fas fa-user-edit"></span></button>' +
                            //'<button class="btn btn-light btn-edit-password" title="Cambiar Contraseña"><span class="fas fa-key"></span></button>' +
                            '</div>';
                        return self.hasFullAccessController ? buttons : "";
                    }
                }
            ],
            "fnServerData": function (sSource, aoData, fnCallback) {
                aoData.push({ "name": "sSortColumn", "value": this.fnSettings().aoColumns[this.fnSettings().aaSorting[0][0]].orderName });
                aoData.push({ "name": "filtros", "value": $('form#SearchForm').serialize() });
                $.getJSON(sSource, aoData, function (json) {
                    fnCallback(json);
                });
            }
        });

        /*$.fn.dataTable.ext.errMode = function (settings, helpPage, message) {
            console.log(message);
        };*/

        $(this.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-change-status',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var id = row.data().Uuid;

                swal({
                    title: "Confirmación",
                    text: "¿Desea cambiar el estado del usuario?",
                    showCancelButton: true,
                    confirmButtonClass: "btn-danger",
                    confirmButtonText: "Aceptar",
                    cancelButtonText: "Cancelar",
                    closeOnConfirm: false,
                    closeOnCancel: false
                },
                    function (isConfirm) {
                        if (isConfirm) {
                            $.ajax({
                                type: 'POST',
                                async: true,
                                data: { uuid: id },
                                url: '/User/ChangeStatus',
                                success: function (data) {
                                    if (!data) {
                                        swal({
                                            type: 'error',
                                            title: data.Mensaje.Titulo,
                                            text: data.Mensaje.Contenido
                                        })
                                    } else {
                                        swal("Registro actualizado");
                                        self.dataTable.ajax.reload();
                                    }
                                },
                                error: function (xhr) {
                                    console.log('error');
                                }
                            });
                        } else {
                            swal.close();
                        }
                    });
            });

        $(this.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-resend-invite',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var id = row.data().Uuid;

                swal({
                    title: "Confirmación",
                    text: "¿Desea reenviar la invitación al usuario?",
                    showCancelButton: true,
                    confirmButtonClass: "btn-danger",
                    confirmButtonText: "Aceptar",
                    cancelButtonText: "Cancelar",
                    closeOnConfirm: false,
                    closeOnCancel: false
                },
                    function (isConfirm) {
                        if (isConfirm) {
                            $.ajax({
                                type: 'POST',
                                async: true,
                                data: { uuid: id },
                                url: '/User/ResendInvite',
                                success: function (data) {
                                    if (!data) {
                                        swal({
                                            type: 'error',
                                            title: data.Mensaje.Titulo,
                                            text: data.Mensaje.Contenido
                                        })
                                    } else {
                                        swal("Invitación enviada");
                                        self.dataTable.ajax.reload();
                                    }
                                },
                                error: function (xhr) {
                                    console.log('error');
                                }
                            });
                        } else {
                            swal.close();
                        }
                    });
            });

        $(self.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-edit-password',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var uuid = row.data().Uuid;
                var action = modelEditPasswordAction + "?uuid=" + uuid;
                self.modalEditPassword.find('.modal-body').load(action, function (response, status, xhr) {
                    if (status == "error") {
                        return;
                    }
                    self.modalEditPassword.modal("show");
                    let form = $("#" + formEditPasswordId);
                    utils.actualizarValidaciones(form);
                    form.submit(function (e) {
                        e.preventDefault();
                        if (form.valid()) {
                            submitEditPassword(form).then(function (data) {
                                form.find("span.field-validation-error").attr("class", "field-validation-valid");
                                form.find("span.field-validation-error").empty();
                                self.modalEditPassword.modal("hide");
                                toastr["success"](data.Message);
                            }).catch(function (data) {
                                if (data.status == 422) {
                                    dataObj = data.responseJSON;
                                    dataObj.errors.forEach(function (error) {
                                        let span = form.find("span[data-valmsg-for='" + error.propertyName + "']");
                                        span.attr("class", "field-validation-error");
                                        let idMessage = error.propertyName + "-error";
                                        let spanMessage = span.find("#" + idMessage);
                                        if (spanMessage.length <= 0) {
                                            spanMessage = $("<span></span>");
                                            spanMessage.attr('id', idMessage);
                                            span.append(spanMessage);
                                        }
                                        spanMessage.html(error.errorMessage);
                                    });
                                } else {
                                    self.modalEditPassword.modal("hide");
                                }
                            });
                        }
                    });
                    $("#" + submitEditPasswordId).click(function () {
                        form.submit();
                    });

                });
            });

        $(self.htmlTable, "tbody").on('click',
            'td.personal-options .btn-group .btn-edit',
            function () {
                var tr = $(this).closest('tr');
                var row = self.dataTable.row(tr);
                var uuid = row.data().Uuid;

                var form = document.createElement('form');
                document.body.appendChild(form);
                form.method = 'GET';
                form.action = "/User/Edit?uuid=" + uuid;

                var input = document.createElement('input');
                input.type = 'hidden';
                input.name = "uuid";
                input.value = uuid;
                form.appendChild(input);
                form.submit();
            });

        function submitEditPassword(form) {
            let url = form.attr("action");
            let method = form.attr("method");
            return new Promise(function (resolve, reject) {
                $.ajax({
                    url: url,
                    method: method,
                    data: form.serialize(),
                    dataType: "json"
                }).done(function (data) {
                    resolve(data);
                }).fail(function (jqXHR, error) {
                    reject(jqXHR);
                });
            });
        }
    }
}