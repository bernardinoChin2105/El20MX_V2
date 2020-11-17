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
        selectLastAccount();
    }
};

function selectAccountModal() {
    $("#selectAccountModal").find('.modal-content').load(urls.urlSelect, function () {
        $("#selectAccountModal").modal({ backdrop: 'static', keyboard: false });
    });
    $("#selectAccountModal").on('shown.bs.modal', function () {
        $('.chosen-select').chosen({ width: '100%', no_results_text: "Sin resultados para " });
        $("#btnSelectAccount").on("click", function () {
            let form = document.createElement("form");
            form.setAttribute("method", "GET");
            form.setAttribute("action", "/Account/SetAccount");

            let hiddenClave = document.createElement("input");
            hiddenClave.setAttribute("type", "hidden");
            hiddenClave.setAttribute("name", "uuid");
            hiddenClave.setAttribute("value", $("#accountListItem").val());
            form.appendChild(hiddenClave);
            
            document.body.appendChild(form);
            form.submit();
        });
    });
}



function createAccountModal() {
    $("#selectAccountModal").modal('hide');
    $("#createAccountModal").find('.modal-content').load(urls.urlCreate, function () {
        $("#createAccountModal").modal({ backdrop: 'static', keyboard: false });
    });
    $("#createAccountModal").on('shown.bs.modal', function () {
        //$("#wizard").steps();   
        //if (!$("#wizard").hasClass("wizard-big wizard clearfix")) {
        //    $("#wizard").steps(settings);
        //    initCreate();
        //}
        initCreate();
    });
    //$("#createAccountModal").on('hidden.bs.modal', function () {
    //    selectAccountModal();
    //});
}

function selectLastAccount() {
    $.ajax({
        type: 'get',
        dataType: 'json',
        url: urls.urlSelectLastAccount,
        success: function (json) {
            if (json.Success) {
                $("#selectAccountModal").modal('hide');
                window.location = urls.urlSetAccount + "?uuid=" + json.uuid;
            } else {
                toastr[json.Type](json.Mensaje, null, { 'positionClass': 'toast-top-center' }); 
            }
        },
        error: function (xhr) {
            toastr['error']('Ocurrió un error, intente nuevamente', null, { 'positionClass': 'toast-top-center' }); 
        }
    });
}

function validate() {
    if (!$('#RegisterRFCForm').valid()) {
        return;
    }
}

function initCreate() {

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

    $("#btn-sat-cancel").on("click", function () {
        $("#createAccountModal").modal("hide");
        selectAccountModal();
    });

    $("#btn-sat").on("click", function () {
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

        $.ajax({
            url: urls.urlCredential,
            data: data,
            type: 'POST',
            dataType: 'json',
            success: function (json) {
                if (json.Success) {
                    $("#validacionSat").val(json.Success);
                    toastr[json.Type](json.Mensaje, null, { 'positionClass': 'toast-top-center' }); 
                    selectLastAccount();
                } else {
                    rfc.attr("disabled", false);
                    pass.attr("disabled", false);
                    btn.attr("disabled", false);
                    toastr[json.Type](json.Mensaje, null, { 'positionClass': 'toast-top-center' }); 
                }
            },
            error: function (xhr, status) {
                console.log(xhr, status, "error");
                btn.attr("disabled", false);
                toastr['error']('Ocurrió un error, intente nuevamente', null, { 'positionClass': 'toast-top-center' }); 
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
        }
    });
}
