$(document).ready(function () {
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
                maxlength: 20
            },
            CURP: {
                CURPTrue: true,
                maxlength: 20
            },
            ZipCode: {
                required: true
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
                required: "Campo obligatorio"
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
$(".btn-add-email").click(function () {
    var item = '<div class="col-12 col-md-10">' +
        '<label class="col-form-label control-label">Email ' + (itemNumberEmail+1) + '</label>' +
        '<input type="hidden" name="Emails[' + itemNumberEmail+'].TypeContact" value="EMAIL" />' +
        '<input type="email" class="form-control emails" name="Emails[' + itemNumberEmail +'].EmailOrPhone" />' +
        '</div>';
    $("#ListEmails").append(item);
    itemNumberEmail++;
});

$(".btn-add-phone").click(function () {
    var item = '<div class="col-12 col-md-10">' +
        '<label class="col-form-label control-label">Teléfono ' + (itemNumberPhone+1) + '</label>' +
        '<input type="hidden" name="Phones[' + itemNumberPhone +'].TypeContact" value="PHONE)" />' +
        '<input type="text" class="form-control phones" name="Phones[' + itemNumberPhone +'].EmailOrPhone" data_mask="9999-99-99-99" removeMaskOnSubmit="true" greedy="false" />' +
        '</div>';
    $("#ListPhones").append(item);
    itemNumberPhone++;
});

$("#ZipCode").blur(function () {
    console.log("perdio el focus");
    var value = $(this).val();
    var cmbColony = $("#Colony");
    var cmbMunicipality = $("#Municipality");
    var cmbState = $("#State");
    console.log("valor", value);

    $.ajax({
        type: 'Get',
        async: true,
        data: { zipCode: value },
        url: urlGetLocation,
        success: function (json) {
            console.log(json, "respuesta");
            if (json.Data.success) {
                var datos = json.Data.data;
                if (datos.length > 0) {
                    console.log(datos, "que esta aquí");
                    cmbState.val(datos[0].stateId);

                    //Llenado de municipios
                    console.log(datos[0], "registro 0");
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
                    toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
                }


            } else {
                toastr["error"]("El registro de Código Postal no se encontró en la base de datos");
            }

        },
        error: function (xhr) {
            console.log('error');
        }
    });
});
