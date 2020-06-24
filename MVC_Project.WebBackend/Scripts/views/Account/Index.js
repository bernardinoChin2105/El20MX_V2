$(document).ready(function () {
    selectAccountModal();
});

function selectAccountModal() {
    $("#selectAccountModal").find('.modal-content').load(urls.urlSelect, function () {
        $("#selectAccountModal").modal({ backdrop: 'static', keyboard: false });
    });
}
function createAccountModal() {
    $("#selectAccountModal").modal('hide');
    $("#createAccountModal").find('.modal-content').load(urls.urlCreate, function () {
        $("#createAccountModal").modal({ backdrop: 'static', keyboard: false });
    });
    $("#createAccountModal").on('shown.bs.modal', function () {
        $("#wizard").steps();
        initCreate();
    });
    $("#createAccountModal").on('hidden.bs.modal', function () {
        selectAccountModal();
    });
}

function initCreate() {

    //$(".viewpass").on("click", function () {
    //    var input = $(".viewpass-input");
    //    console.log(input.val(), "valor");
    //    if (input.val() !== "") {
    //        var view = input.data("view");
    //        !view ? input.attr('type', 'text').data("view", true) : input.attr('type', 'password').data("view", false);
    //    }
    //});

    var input = $(".viewpass-input");
    $(".viewpass")
        .mouseup(function () {
            input.attr('type', 'password').data("view", false);
        })
        .mousedown(function () {
            if (input.val() !== "") {
                input.attr('type', 'text').data("view", true);
            }
        });

    $("#btn-sat").on("click", function () {
        console.log("estoy en el evento");

        if (!$('#RegisterRFCForm').valid()) {
            return;
        }

        var data = {
            rfc: $("#rfc").val(),
            password: $("#password").val()
        };

        console.log("datos", data);

        $.ajax({
            url: urls.urlCredential,
            data: data,
            type: 'POST',
            dataType: 'json',
            success: function (json) {
                console.log(json, "respuesta");
                //if (!json.Success) {
                //    window.location = "/Register";
                //} else {
                //    if (json.Url !== "") {
                //        window.location = json.Url;
                //    }
                //}
            },
            error: function (xhr, status) {
                console.log(xhr, status, "error");
                //alert('Disculpe, existió un problema');
                toastr['error']('Ocurrió un error, intente nuevamente');
            }
        });
    });

    $.validator.addMethod("RFCTrue",
        function (value, element) {
            return value.match(/^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z\d]{2})([A\d])$/);
        }, "Debe ser un RFC válido"
    );

    $("#RegisterRFCForm").validate({
        rules: {
            rfc: {
                required: true,
                RFCTrue: true
            },
            password: {
                required: true
                //minlength: 8
            }
        },
        messages: {
            rfc: {
                required: "Campo obligatorio"
            },
            password: {
                required: "Campo obligatorio"
                //minlength: "La contraseña debe ser mínimo de 8 caracteres",
            }
        }
    });
}
