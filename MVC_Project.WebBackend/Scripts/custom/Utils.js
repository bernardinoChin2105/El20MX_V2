

function Utils() {
    var Geral = this;

    //Método que devuelve si una cadena o valor (input) contiene un valor o no.
    function InputContainerValue(input) {
        if (input != null && $.trim(input) != '') {
            return true;
        }
        else {
            return false;
        }
    }
    this.actualizarValidaciones = function (form) {
        form.removeData('validator');
        form.removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(form);
    };

    this.esVacio = function (val) {
        if (typeof (val) !== 'undefined' && val !== null && val != '') {
            return false;
        }
        return true;
    };

    this.confirmarAccion = function (confirmCallback) {
        swal({
            title: "Confirma",
            text: "¿Desea realizar la acción?",
            type: "info",
            animation: "slide-from-top",
            showCancelButton: true,
            confirmButtonText: "Aceptar",
            cancelButtonText: "Cancelar",
            closeOnConfirm: true
        }, function (guardarSolicitud) {
            if (guardarSolicitud) {
                confirmCallback();
            }
        });
    };
    this.DataToUserInterface = function DataToUserInterface(form, obj, disparar) {
        $.each(form.serializeArray(), function (i, field) {
            try {
                var val = eval("obj." + field.name);
                field.name = field.name.replace(/\./g, "\\.");
                var input = $(form).find('[name=' + field.name + ']');
                if (input.hasClass("date")) {
                    if ($(input).data('datepicker')) {
                        var date = new Date(parseInt(val.substr(6)));
                        $(input).datepicker('setDate', date);
                    }
                    else
                        if ($(input).data('DateTimePicker')) {
                            var date = moment(getJsonDate(val));
                            $(input).data("DateTimePicker").date(date);
                        }
                } else {
                    if (disparar != undefined && disparar != true)
                        input.val(val).change();
                    else
                        input.val(val);
                }
            } catch (ex) {
                console.log(ex);
            }
        })
    }

    this.CleanValidationMessage = function CleanValidationMessage(form) {
        try {
            var $validator = $(form.selector).validate();
            $validator.resetForm();
            $(form.selector + ' .form-group').removeClass('has-error');
        } catch (ex) { }
    }

    this.CleanForm = function CleanForm(form) {
        $(form).find(".form-control").val("").change();
        $(form).find(".field-validation-error").text("");
        Geral.CleanValidation(form);
    }

    this.UserInterfaceToData = function UserInterfaceToData(form) {
        var obj = new Object();
        $.each(form.serializeArray(), function (i, field) {
            try {
                //field.name = field.name.replace(".", "\\.");
                try {
                    obj[field.name] = field.value.trim();
                } catch (ex) {
                    obj[field.name] = field.value;
                }
                var $input = $(form).find('input[name="' + field.name + '"]');
                if ($input.hasClass("date") && field.value != "") {
                    if ($input.data('datepicker')) {
                        var d = field.value.split("/");
                        obj[field.name] = d[2] + "-" + d[1] + "-" + d[0];
                    }
                    else
                        if ($input.data('DateTimePicker')) {
                            obj[field.name] = $input.data("DateTimePicker").date().startOf('day').toISOString();
                        }
                }

            } catch (ex) {
                console.log(ex);
            }
        })
        return obj;
    }

    //Función que retorna los mensajes de validación cuando se envia por Ajax
    this.ValidateErrors = function ValidateErrors(form, resp) {
        Geral.CleanValidationMessage(form);
        if (resp.success == false) {
            $.each(resp.errors, function (key, value) {
                if (value != null) {
                    $(form).find('span[data-valmsg-for="' + value.propertyName.replace("model.", "") + '"]').closest(".form-group").removeClass("input-validation-valid").addClass('has-error');
                    $(form).find('span[data-valmsg-for="' + value.propertyName.replace("model.", "") + '"]').html(value.errorMessage).addClass("help-block").css("display", "block");

                } else {
                    $(form).find('span[data-valmsg-for="' + value.propertyName.replace("model.", "") + '"]').closest(".form-group").addClass("input-validation-valid").removeClass('has-error');
                    $(form).find('span[data-valmsg-for="' + value.propertyName.replace("model.", "") + '"]').removeClass("help-block").css("display", "none");
                }
            });
        }
        return resp.errors != undefined && resp.errors != null ? resp.errors.length : 0;
    }

    this.ValidateErrorsField = function ValidateErrors($field, error, form) {

        if (error == null || error == "") {
            $field.closest(".form-group").addClass("input-validation-valid").removeClass('has-error');
            $field.removeClass("help-block").css("display", "none");
            return;
        }
        $field.closest(".form-group").removeClass("input-validation-valid").addClass('has-error');
        $field.text(error).addClass("help-block").css("display", "block");

    }

    ////Habilitar el bloqueo de pantalla
    this.AJAXBeforeSend = function AJAXBeforeSend() {
        App.blockUI({ animate: !0 });
    }

    //DesHabilitar el bloqueo de pantalla
    this.AJAXComplete = function AJAXComplete(Mensaje) {
        App.unblockUI();
        if (Mensaje != undefined && Mensaje.Estado == 1) {
            ValidaError(Mensaje);
            return false;
        }
        return true;
    }

    this.AJAXError = function (jqXHR, textStatus, errorThrown) {
        console.log(jqXHR);
        console.log(textStatus);
        console.log(errorThrown);
        App.unblockUI();
        if (jqXHR.status == 400)
            AlertToastr("error", "Mensaje sintácticamente incorrecto", "error", 10000);
        if (jqXHR.status == 404)
            AlertToastr("error", "El enlace al que desea acceder no existe o ha cambiado de lugar", "error", 10000);
        else if (jqXHR.status == 403 || jqXHR.status == 401) {
            AlertToastr("error", 'No tiene los permisos suficientes para acceder al enlace seleccionado', "error", 10000);
        }
        else if (jqXHR.status == 408)
            AlertToastr("error", 'Tiempo de espera agotado', "error", 10000);
        else if (jqXHR.status == 503)
            AlertToastr("error", 'Servidor no disponible', "error", 10000);
        else if (textStatus === 'parsererror')
            AlertToastr("error", 'Requested JSON parse failed', "error", 10000);
        else if (textStatus === 'timeout')
            AlertToastr("error", 'Time out error', "error", 10000);
        else if (textStatus === 'abort')
            AlertToastr("error", 'Ajax request aborted', "error", 10000);
        else {
            AlertToastr("error", 'Ocurrió un error en el servidor. Código. ' + jqXHR.status + " Info. " + errorThrown, "error", 10000);
        }
    }

    this.LoadSelect = function LoadSelect(url, idPadre, $select, nombreOption, idOption, addEmpty, idHijo, callback, args) {
        var band = false;
        if ($select.hasClass("select2-hidden-accessible")) {
            band = true;
            try { $select.select2("destroy"); } catch (ex) { }
        }
        var obj = { PadreId: idPadre };
        $.ajax({
            url: url,
            data: JSON.stringify(obj),
            type: "POST", dataType: "JSON", contentType: "application/json",
            //beforeSend: this.AJAXBeforeSend(),
            //error: function (jqXHR, textStatus, errorThrown) { Geral.AJAXError(jqXHR, textStatus, errorThrown) },
            success: function (data) {
                if (!Geral.AJAXComplete(data.Mensaje)) { return; }
                $select.empty();
                if (addEmpty != undefined && data.DatoExtra.length > 0) {
                    if (addEmpty == true)
                        $select.append(new Option("Seleccione", ""));
                    else if (addEmpty == false) {

                    }
                    else {
                        $select.append(new Option(addEmpty, ""));
                    }
                }
                for (x = 0; x < data.DatoExtra.length; x++) {

                    $('<option/>', {
                        text: data.DatoExtra[x][nombreOption],
                        value: data.DatoExtra[x][idOption],
                        data: {
                            element: data.DatoExtra[x],
                        }
                    }).appendTo($select);
                }

                if (band)
                    try { $select.select2({ language: 'es', width: '100%', theme: "bootstrap" }); } catch (ex) { }
                if (idHijo != undefined) {

                    $select.val(idHijo).change().promise().done(function () {
                        if (callback != undefined)
                            callback(args);
                    });

                } else {
                    if (callback != undefined)
                        callback(args);

                }
            }
        });
    }

    this.LoadAutoComplete = function LoadAutoComplete($select, url, id, formatRepo, formatRepoSelection) {
        try {
            $select.select2({
                ajax: {
                    language: 'es', width: '100%', theme: "bootstrap",
                    url: url,
                    dataType: 'json',
                    delay: 250,
                    allowclear: true,
                    placeholder: 'Seleccione',
                    type: 'POST',
                    data: function (params) {
                        return {
                            term: params.term,
                            page: params.page
                        };
                    },
                    processResults: function (data, params) {

                        if (data.DatoExtra) {
                            data.DatoExtra.forEach(function (d, i) {
                                d.id = d[id];
                            })
                            params.page = params.page || 1;
                            return {
                                results: data.DatoExtra,
                                pagination: {
                                    more: (params.page * 30) < data.total_count
                                }
                            }
                        } else {

                            return {
                                results: data.Mensaje.Contenido,
                                pagination: {
                                    more: 0
                                }
                            }

                        }

                    },
                    cache: true
                },
                escapeMarkup: function (markup) { return markup; },
                minimumInputLength: 1,
                templateResult: formatRepo,
                templateSelection: formatRepoSelection
            });
        } catch (ex) {
            console.log($select);
            console.log(ex);
        }
    }

    this.AddEmpty = function AddEmpty($select, display, value) {
        value = value === undefined ? 0 : value;
        $select.prepend(new Option(display, value));
        $select.val(value);
    }

    this.IsInt = function IsInt(cad) {
        if (cad.trim().length > 0) {
            var exp = new RegExp("^(\\+|-)?[0-9]+$");
            return exp.test(cad);
        }
        return true;
    }


    function ValidaError(Msj) {
        switch (Msj.Estado) {
            case 1: {
                AlertToastr("error", Msj.Contenido, Msj.Titulo, 10000);
            } break;
        }
    }

    this.InputDate = function InputDate() {
        $.validator.methods.date = function (value, element) { return true; };
    }

    this.RenderDate = function RenderDate(value) {
        if (value === null || value == undefined) return "";
        var pattern = /Date\(([^)]+)\)/;
        var results = pattern.exec(value);
        var dt = new Date(parseFloat(results[1]));
        return dt.getDate() + "/" + (dt.getMonth() + 1) + "/" + dt.getFullYear();
    }

    this.StringToDate = function (sdate) {
        if (sdate == null)
            return "";
        var sd = sdate.split("-");
        var months = { "Ene": 0, "Feb": 1, "Mar": 2, "Abr": 3, "May": 4, "Jun": 5, "Jul": 6, "Ago": 7, "Sep": 8, "Oct": 9, "Nov": 10, "Dic": 11 };
        return new Date(sd[2], months[parseInt(sd[1])], sd[0]);
    }
    this.IntToString = function (sdate) {
        if (sdate == null || sdate == 0)
            return "";

        var months = { 1: "Enero", 2: "Febrero", 3: "Marzo", 4: "Abril", 5: "Mayo", 6: "Junio", 7: "Julio", 8: "Agosto", 9: "Septiembre", 10: "Octubre", 11: "Noviembre", 12: "Diciembre" };
        return (months[parseInt(sdate)]);
    }

    this.StringToDate2 = function (sdate) {
        if (sdate == null)
            return "";
        var sd = sdate.split("/");
        return new Date(sd[2], parseInt(sd[1]), sd[0]);
    }

    this.FormatDate = function (sdate) {
        if (sdate == null)
            return "";
        var sd = sdate.split("-");
        var months = { "Ene": "01", "Feb": "02", "Mar": "03", "Abr": "04", "May": "05", "Jun": "06", "Jul": "07", "Ago": "08", "Sep": "09", "Oct": "10", "Nov": "11", "Dic": "12" };
        return sd[2] + '-' + months[sd[1]] + '-' + sd[0];
    },

        this.resetSelect = function (controlName) {
            var $select = $(controlName);
            $select.empty();
            $('<option>', {
                value: ""
            }).html("Seleccionar").appendTo($select);
        }

    this.BloqueaPaginado = function (nombreGrid) {
        $(nombreGrid + "_paginate").hide();
    }
    this.obtenerDateDeUnixTimeStamp = function (timestamp) {
        var date = new Date(timestamp * 1000);	// Convert the passed timestamp to milliseconds
        var utcDate = moment.utc(date);
        return utcDate;
    }

    this.DesbloqueaPaginado = function (nombreGrid) {
        $(nombreGrid + "_paginate").show();
    }//,

    //this.BlockContent = function ($target) {
    //    App.blockUI({
    //        target: $target, 'textOnly': true,
    //        message: '<div class="block-spinner-bar" style="margin-left: -70px"><div class="bounce1"></div><div class="bounce2"></div><div class="bounce3"></div></div>',
    //    });
    //},

    //this.UnblockContent = function ($target) {
    //    App.unblockUI({
    //        target: $target
    //    });
    //}
}


var _tools = function () {
    return {
        resetForm: function (Geralorm) {
            var $validator = $(Geralorm).validate();
            if ($validator)
                $validator.resetForm();
            $(Geralorm + ' .form-group').removeClass('error');
        },

        //Valida un rango de fechas para no permitir rangos incorrectos (para Content\plugins\bootstrap-datetimepicker)
        _validateDateRange: function ($fechaIni, $fechaFin, inputChanged) { //inputChanged: 0:$fechaIni, 1:$fechaFin
            if (inputHasValue($fechaIni.val()) && inputHasValue($fechaFin.val())) {
                var fechaIni = $fechaIni.data("DateTimePicker").date();
                var fechaFin = $fechaFin.data("DateTimePicker").date();
                if (fechaFin < fechaIni) {
                    if (inputChanged === 1)
                        $fechaIni.data("DateTimePicker").date(fechaFin);
                    else
                        $fechaFin.data("DateTimePicker").date(fechaIni);
                }
            }
        },

        isNumeric: function (num) {
            return !isNaN(num)
        },

        getFormattedValue: function (value, format, vDefault) {
            var newValue = '';
            if (inputHasValue(value)) {
                newValue = value == '.' ? 0 : value;

                if (newValue == '-') {
                    return '';
                }
                else {
                    if (inputHasValue(format)) {
                        if (format.length >= 2) {
                            var aFormat = format.split('');
                            var zeros = this.isNumeric(aFormat[1]) ? '.' + '0'.repeat(aFormat[1]) : '';
                            switch (aFormat[0].toUpperCase()) {
                                case 'N':
                                    return numeral(newValue).format('0,0' + zeros);
                                    break;
                                case 'C':
                                    return numeral(newValue).format('$0,0' + zeros);
                                    break;
                                case 'P':
                                    return numeral(newValue).format('0,0' + zeros + '%');
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            if (inputHasValue(vDefault)) {
                return vDefault;
            }

            return newValue;
        },

        //Deshabilita la tecla de <Enter> en el input especificado.
        avoidLineBreakOnKeypress: function (selector) {
            $(selector).keypress(function (event) {
                if (event.which == '13') {
                    return false;
                }
            });
        },

        //Remueve los enter y espacios en blanco extra de los textos de entrada en el input seleccionado.
        removeExtraBlankSpacesOnBlur: function (selector) {
            $(selector).blur(function (event) {
                var text = $(this).val().replace(/\n/g, "").replace(/[ ]{2,}/gi, " ").replace(/^\s*|\s*$/g, '');
                $(this).val(text);
            });
        },

        validateForm: function (formSel) {
            return $(formSel).validate({
                errorElement: "span",
                errorClass: "help-block help-block-error",
                highlight: function (element) {
                    $(element).closest('.form-group').addClass('error');
                },
                success: function (element) {
                    element.closest('.form-group').removeClass('error');
                    element.closest('.form-group span.help-block').remove();
                }
            });
        }
    };
}();

//Limpia el selector. Se requiere que se le envie el nombre. Ejemplo: #SelectorPais
function resetSelect(controlName) {
    var $select = $(controlName);
    $select.empty();
    $('<option>', {
        value: ""
    }).html("Seleccionar").appendTo($select);
}

function InputHasValue(input) {
    if (input != null && $.trim(input) != '') {
        return true;
    }
    else {
        return false;
    }
}

function getFiltros(form) {
    var $inputs = $(form + ' [filtro="true"]');
    var nFiltros = $inputs.length;
    var filtros = [];
    for (i = 1; i <= nFiltros; i++) {
        var input = $.grep($inputs, function (item) { return $(item).attr('filtro-order') == i; });
        filtros.push($(input).val());
    }

    return JSON.stringify(filtros);
}

var El20Utils = El20Utils || {};

(function (x, $) {

    //DataTables.
    x.GRID_TOTAL_ITEMS = [50, 100, 200, 300, 400, 500];
    x.PAGE_LENGTH_INIT = 50;
    x.GRID_LANG_SPANISH = {
        "sProcessing": "Procesando...",
        "sLengthMenu": "Mostrar _MENU_ registros",
        "sZeroRecords": "No se encontraron resultados",
        "sEmptyTable": "Ningún dato disponible en esta tabla",
        "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
        "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
        "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
        "sInfoPostFix": "",
        "sSearch": "Buscar:",
        "sUrl": "",
        "sInfoThousands": ",",
        "sLoadingRecords": "Cargando...",
        "oPaginate": {
            "sFirst": "Primero",
            "sLast": "Último",
            "sNext": "Siguiente",
            "sPrevious": "Anterior"
        },
        "oAria": {
            "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
            "sSortDescending": ": Activar para ordenar la columna de manera descendente"
        },
        "buttons": {
            "copy": "Copiar",
            "colvis": "Visibilidad"
        }
    };


    //Date.
    x.DATE_FORMAT = 'DD/MM/YYYY';

    x.lenguajeTabla = function (messages) {
        return {
            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": messages.emptyTable ? messages.emptyTable : "Ningún dato disponible en esta tabla",
            "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
            "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
            "sInfoPostFix": "",
            "sSearch": "Buscar:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Cargando...",
            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            },
            "oAria": {
                "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
                "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            },
            "buttons": {
                "copy": "Copiar",
                "colvis": "Visibilidad"
            }
        }
    };

    //Methods
    x.mostrarCargador = function () {
        $('#CargadorPantalla').modal('show');
    };
    x.mostrarCargadorModal = function () {
        $('#CargadorPantalla').addClass('modalzindex');
        $('#CargadorPantalla').modal('show');
    };

    x.ocultarCargador = function () {
        $('#CargadorPantalla').modal('hide');
    };
    x.ocultarCargadorModal = function () {
        $('#CargadorPantalla').removeClass('modalzindex');
        $('#CargadorPantalla').modal('hide');
    };

    x.formatoMoneda = function (num) {
        return "$" + num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    };
    x.formatoNumero = function (num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    };

})(El20Utils, jQuery);