function resetPassModal() {
    $("#modal-containerRecupera").find('.modal-content').load(UrlReset, function () {
        $("#modal-containerRecupera").modal("show");
        $("#btn-RecuperaCE").on("click",
            function (e) {
                e.preventDefault();
                var validate = $('form#recupera-cuenta-form').validate();
                console.log($('#recupera-cuenta-form').valid(), "respuesta");                
                if (!$('#recupera-cuenta-form').valid()) {
                    //$("form#recupera-cuenta-form").find('span').removeClass("help-block").css("display", "none");
                    return;
                } else {
                    $("#modal-containerRecupera form#recupera-cuenta-form button#btn-RecuperaCE").hide();
                    $('#recupera-cuenta-form').submit();
                }
            });
    });
}
function MessageAlert(d) {
    var rslt = $.parseJSON(d.responseText);
    if (rslt.statusCode == 200) {
        $().toastmessage("showSuccessToast", rslt.status);
    }
    else {
        $().toastmessage("showErrorToast", rslt.status);
    }
}