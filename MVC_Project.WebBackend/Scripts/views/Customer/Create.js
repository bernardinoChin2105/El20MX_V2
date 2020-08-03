﻿$(document).ready(function () {
    //$('.chosen-select').chosen({ width: '100%', "disable_search": true });
    $.validator.addMethod("RFCTrue",
        function (value, element) {
            return value.match(/^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z\d]{2})([A\d])$/);
        }, "Debe ser un RFC válido"
    );
    $.validator.addMethod("CURPTrue",
        function (value, element) {
            return value.match(/^([A-Z&]|[a-z&]{1})([AEIOU]|[aeiou]{1})([A-Z&]|[a-z&]{1})([A-Z&]|[a-z&]{1})([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1])([HM]|[hm]{1})([AS|as|BC|bc|BS|bs|CC|cc|CS|cs|CH|ch|CL|cl|CM|cm|DF|df|DG|dg|GT|gt|GR|gr|HG|hg|JC|jc|MC|mc|MN|mn|MS|ms|NT|nt|NL|nl|OC|oc|PL|pl|QT|qt|QR|qr|SP|sp|SL|sl|SR|sr|TC|tc|TS|ts|TL|tl|VZ|vz|YN|yn|ZS|zs|NE|ne]{2})([^A|a|E|e|I|i|O|o|U|u]{1})([^A|a|E|e|I|i|O|o|U|u]{1})([^A|a|E|e|I|i|O|o|U|u]{1})([0-9]{2})$/g);
        }, "Debe ser un CURP válido"
    );

    $("#CURP").keyup(function () {
        this.value = this.value.toUpperCase();
    });
    $("#RFC").keyup(function () {
        this.value = this.value.toUpperCase();
    });

    $("#CreateForm").validate({
        rules: {
            FistName: {
                required: true,
                maxlength: 50
            },
            LastName: {
                required: true,
                maxlength: 50
            },
            RFC: {
                required: true,
                RFCTrue: true,
                maxlength: 13
            },
            CURP: {
                CURPTrue: true,
                maxlength: 18
            },
            ZipCode: {
                required: true,
                maxlength: 5
            }
        }
        ,
        messages: {
            FistName: {
                required: "Campo obligatorio",
                maxlength: jQuery.validator.format("Ingrese no más de {0} caracteres.")
            },
            LastName: {
                required: "Campo obligatorio",
                maxlength: jQuery.validator.format("Ingrese no más de {0} caracteres.")
            },
            RFC: {
                required: "Campo obligatorio",
                maxlength: jQuery.validator.format("Ingrese no más de {0} caracteres.")
            },
            CURP: {
                //required: "Campo obligatorio",
                maxlength: jQuery.validator.format("Ingrese no más de {0} caracteres.")
            },
            ZipCode: {
                required: "Campo obligatorio",
                maxlength: jQuery.validator.format("Ingrese no más de {0} caracteres.")
            }
        }
    });
});

function validarDatos() {
    if (!$('#CreateForm').valid()) {
        return;
    }
    $('#CreateForm').submit();
}

var itemNumberEmail = 1;
var itemNumberPhone = 1;
var indexEmail = 1;
var indexPhone = 1;
$(".btn-add-email").click(function () {
    //console.log("que paso?")
    var item = '<div class="row">'+
        '<div class="col-12 col-md-10" > ' +
        '<label class="col-form-label control-label">Otro Email</label>' +
        '<input type="hidden" name="Emails[' + itemNumberEmail + '].TypeContact" value="EMAIL" />' +
        '<input type="email" class="form-control emails" name="Emails[' + itemNumberEmail + '].EmailOrPhone" />' +
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
