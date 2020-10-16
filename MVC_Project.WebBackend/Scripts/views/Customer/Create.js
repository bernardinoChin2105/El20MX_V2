$(document).ready(function () {
    //$('.chosen-select').chosen({ width: '100%', "disable_search": true });
    var FistName = $("#FistName");
    var LastName = $("#LastName");
    var BusinessName = $("#BusinessName");
    var taxRegime = $("#taxRegime");

    $.validator.addMethod("RFCTrue",
        function (value, element) {
            const re = /^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z\d]{2})([A\d])$/;
            var valid = value.match(re);

            if (valid === null)
                return false;

            var str = value.slice(0, 4);

            //Si pasa la validación entonces es un             
            if (value.length === 12 && str.match(/^([A-ZÑ&]{3})[0-9]$/g) !== null) {
                $(".businessName").removeClass("hidden");
                $(".FistName").html("Nombre(s) Contacto");
                $(".LastName").html("Apellido(s) Contacto");
                taxRegime.val("PERSONA_MORAL");
                FistName.rules("add", {
                    required: false,
                    //messages: {
                    //    required: "Campo obligatorio"
                    //}
                });
                LastName.rules("add", {
                    required: false,
                    //messages: {
                    //    required: "Campo obligatorio"
                    //}
                });
                BusinessName.rules("add", {
                    required: true,
                    messages: {
                        required: "Campo obligatorio"
                    }
                });
            } else {
                $(".businessName").addClass("hidden");
                $(".FistName").html("Nombre(s)");
                $(".LastName").html("Apellido(s)");
                taxRegime.val("PERSONA_FISICA");
                FistName.rules("add", {
                    required: true,
                    messages: {
                        required: "Campo obligatorio"
                    }
                });
                LastName.rules("add", {
                    required: true,
                    messages: {
                        required: "Campo obligatorio"
                    }
                });
                BusinessName.rules("add", {
                    required: false,
                    //messages: {
                    //    required: "Campo obligatorio"
                    //}
                });
            }

            return true;
        }, "Debe ser un RFC válido"
    );

    $.validator.addMethod("CURPTrue",
        function (value, element) {
            if (!value)
                return true;

            return value.match(/^([A-Z&]|[a-z&]{1})([AEIOU]|[aeiou]{1})([A-Z&]|[a-z&]{1})([A-Z&]|[a-z&]{1})([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1])([HM]|[hm]{1})([AS|as|BC|bc|BS|bs|CC|cc|CS|cs|CH|ch|CL|cl|CM|cm|DF|df|DG|dg|GT|gt|GR|gr|HG|hg|JC|jc|MC|mc|MN|mn|MS|ms|NT|nt|NL|nl|OC|oc|PL|pl|QT|qt|QR|qr|SP|sp|SL|sl|SR|sr|TC|tc|TS|ts|TL|tl|VZ|vz|YN|yn|ZS|zs|NE|ne]{2})([^A|a|E|e|I|i|O|o|U|u]{1})([^A|a|E|e|I|i|O|o|U|u]{1})([^A|a|E|e|I|i|O|o|U|u]{1})([0-9]{2})$/g);
        }, "Debe ser un CURP válido"
    );

    $("#CURP").keyup(function () {
        this.value = this.value.toUpperCase();
    });
    $("#RFC").keyup(function () {
        this.value = this.value.toUpperCase();
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
            FistName: {
                required: true,
                Alphabetic: true
            },
            LastName: {
                required: true,
                Alphabetic: true
            },
            RFC: {
                required: true,
                RFCTrue: true,
                Alphanumeric: true,
            },
            CURP: {
                CURPTrue: true
            },
            ZipCode: {
                required: true,
                Numeric: true
            },
            Street: {
                Alphanumeric: true,
            },
            OutdoorNumber: {
                Alphanumeric: true,
            },
            InteriorNumber: {
                Alphanumeric: true,
            },
            BusinessName: {
                Alphanumeric: true
            }
        }
    });
});

function validarDatos() {
    if (!$('#CreateForm').valid()) {
        return;
    }
    $("#taxRegime").prop("disabled", false);
    $('#CreateForm').submit();
}

var itemNumberEmail = 1;
var itemNumberPhone = 1;
var indexEmail = 1;
var indexPhone = 1;

$(".btn-add-email").click(function () {    
    var item = '<div class="row">'+
        '<div class="col-12 col-md-10" > ' +
        '<label class="col-form-label control-label">Otro Email</label>' +
        '<input type="hidden" name="Emails[' + itemNumberEmail + '].TypeContact" value="EMAIL" />' +
        '<input type="email" class="form-control emails" maxlength="250" name="Emails[' + itemNumberEmail + '].EmailOrPhone" />' +
        '</div>' +
        '<div class="col-12 col-md-2">' +
        '<label class="col-form-label control-label heightLabel"></label>' +
        ' <button type="button" class="btn btn-color btn-remove-phone btn-remove" data-element="EMAIL" value="' + itemNumberEmail + '"><i class="fa fa-trash"></i></button>' +
        '</div>' +
        '</div>';
    $("#ListEmails").append(item);
    itemNumberEmail++;
    indexEmail++;
});

$(".btn-add-phone").click(function () {
    var item = '<div class="row"><div class="col-12 col-md-10">' +
        '<label class="col-form-label control-label">Otro Teléfono</label>' +
        '<input type="hidden" name="Phones[' + itemNumberPhone + '].TypeContact" value="PHONE" />' +
        '<input type="text" class="form-control phones" name="Phones[' + itemNumberPhone + '].EmailOrPhone" data-mask="9999-99-99-99" removeMaskOnSubmit="true" greedy="false" />' +
        '</div>' +
        '<div class="col-12 col-md-2">' +
        '<label class="col-form-label control-label heightLabel"></label>' +
        ' <button type="button" class="btn btn-color btn-remove-phone btn-remove" data-element="PHONE" value="' + itemNumberPhone + '"><i class="fa fa-trash"></i></button>' +
        '</div>' +
        '</div>';
        $("#ListPhones").append(item);
    itemNumberPhone++;
    indexPhone++;
});

var itemsRemoveP = [];
var itemsRemoveE = [];
$('.listContacts').on('click', '.btn-remove', function () {
    var removePhone = $("#indexPhone");
    var removeEmail = $("#indexEmail");
    //console.log(this, "elementos");

    var item = $(this).val();
    var data = $(this).data("element");
    //console.log(item, item.trim(), "valor del elemento");

    if (data === "PHONE") {
        itemsRemoveP.push(item);
        removePhone.val(itemsRemoveP);
        indexPhone--;
    } else {
        itemsRemoveE.push(item);
        removeEmail.val(itemsRemoveE);
        indexEmail--;
    }

    //console.log($(this).parent().parent(), "valor del elemento");
    $(this).parent().parent().addClass("hidden");
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
        url: urlGetLocation,
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
