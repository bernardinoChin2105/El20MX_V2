var recurlyPromotionCoupons = (function () {
  var $codeInput = $('#rcoupon-code-input');

  function redeemCode() {
    setValid();

    var code = $codeInput.val();
    if (code.trim() === "") {
      setInvalid('Campo requerido.');
      return;
    }

    El20Utils.mostrarCargador();
    $.post("MyAccount/RedeemCoupon", { couponCode: code }, function (data) {
      if (data.status) {
        $('#promotion-coupon-modal').modal("hide");
        toastr['success'](data.message, null, { 'positionClass': 'toast-top-center', 'preventDuplicates': false, 'newestOnTop': true, 'showDuration': '5000' });
      } else {
        setInvalid(data.error);
      }
    }).fail(function () {
      setInvalid("El cupón no pudo ser canjeado. Verifique e intentelo más tarde.");
    }).always(function () {
      El20Utils.ocultarCargador();
    });
  }

  function resetInput() {
    $codeInput.val('');
    $codeInput.removeClass('is-invalid');
    $codeInput.siblings('.invalid-feedback').text('');
  }

  function setValid() {
    $codeInput.removeClass('is-invalid');
    $codeInput.siblings('.invalid-feedback').text('');
  }

  function setInvalid(message) {
    $codeInput.addClass('is-invalid');
    $codeInput.siblings('.invalid-feedback').text(message);
  }

  $('#promotion-coupon-modal').on('hidden.bs.modal', function (e) {
    resetInput();
  });

  return {
    redeemCode: redeemCode
  }
})();