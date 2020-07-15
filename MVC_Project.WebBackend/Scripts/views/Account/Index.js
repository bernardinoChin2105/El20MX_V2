$(document).ready(function () {
    selectAccountModal();
});

var settings = {
    headerTag: "h1",
    content: "div",
    enableContentCache: true,
    enableAllSteps: false,
    labels: {
        current: "paso actual:",
        pagination: "Paginación",
        finish: "Finalizar",
        next: "Siguente",
        previous: "Anterior",
        loading: "Cargando ..."
    },
    onStepChanging: function (event, currentIndex, newIndex) {
        console.log(event, currentIndex, newIndex, "cambiando");
        if (currentIndex === 0) {
            var valid = $("#validacionSat").val();
            console.log(valid, "que trajo");
            if (valid === "false") {
                validate();
                return false;
            }
        }
        return true;
    },
    onFinishing: function (event, currentIndex) {
        console.log(event, currentIndex, "finalizando");
        return true;
    },
    onFinished: function (event, currentIndex) {
        console.log(event, currentIndex, "finalizado");
        //$("#wizard").steps('reset');
        $("#createAccountModal").modal("hide");
    }
};

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
        //$("#wizard").steps();   
        if (!$("#wizard").hasClass("wizard-big wizard clearfix")) {
            $("#wizard").steps(settings);
            initCreate();
        }
    });
    $("#createAccountModal").on('hidden.bs.modal', function () {
        selectAccountModal();
    });
}

function validate() {
    if (!$('#RegisterRFCForm').valid()) {
        return;
    }
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
        var rfc = $("#rfc");
        var pass = $("#password");
        var btn = $(this);
       
        if (!$('#RegisterRFCForm').valid()) {           
            btn.attr("disabled", false);
            rfc.attr("disabled", false);
            pass.attr("disabled", false);
            return;
        }

        btn.attr("disabled", true);
        rfc.attr("disabled", true);
        pass.attr("disabled", true);

        var data = {
            rfc: rfc.val(),
            password: pass.val()
        };

        console.log("datos", data);

        $.ajax({
            url: urls.urlCredential,
            data: data,
            type: 'POST',
            dataType: 'json',
            success: function (json) {
                console.log(json, "respuesta");
                console.log(json.Success, !json.Success, "que respuesta trae");
                if (json.Success) {
                    $("#validacionSat").val(json.Success);
                    //toastr['success']('Cuenta registrada');                    
                    toastr[json.Type](json.Mensaje);   
                } else {
                    rfc.attr("disabled", false);
                    pass.attr("disabled", false);
                    btn.attr("disabled", false);
                    //toastr['error']('Ocurrió un error, intente nuevamente');
                    toastr[json.Type](json.Mensaje);   
                }
            },
            error: function (xhr, status) {
                //alert('Disculpe, existió un problema');
                console.log(xhr, status, "error");
                btn.attr("disabled", false);
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
