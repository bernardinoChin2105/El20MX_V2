var canvas = document.getElementById("signature-pad");
var clearButton = document.querySelector(".clear-signature");
var generatePdfButton = document.querySelector(".generate-pdf");
var generateJpegButton = document.querySelector(".generate-jpeg");
var signaturePad = new SignaturePad(canvas, {
  backgroundColor: 'rgb(255, 255, 255)'
});

function resizeCanvas() {
  var ratio = Math.max(window.devicePixelRatio || 1, 1);

  canvas.width = canvas.offsetWidth * ratio;
  canvas.height = canvas.offsetHeight * ratio;
  canvas.getContext("2d").scale(ratio, ratio);

  signaturePad.clear();
}

window.onresize = resizeCanvas;
resizeCanvas();

clearButton.addEventListener("click", function (event) {
  signaturePad.clear();
});

function createForm(action) {
  var dataURL = signaturePad.toDataURL();
  var form = document.createElement("form");
  var signature = document.createElement("input");
  form.action = action;
  form.method = "POST";

  signature.value = dataURL;
  signature.name = "signature";
  form.appendChild(signature);

  return form;
}

generatePdfButton.addEventListener("click", function (event) {
  if (signaturePad.isEmpty()) {
    alert("Firma primero");
  } else {

    var form = createForm("/PdfSignature/GeneratePdf");

    document.body.appendChild(form);

    form.submit();

    document.body.removeChild(form);
  }
});

generateJpegButton.addEventListener("click", function (event) {
  if (signaturePad.isEmpty()) {
    alert("Firma primero");
  } else {
    var form = createForm("/PdfSignature/GenerateJpeg");

    document.body.appendChild(form);

    form.submit();

    document.body.removeChild(form);
  }
});