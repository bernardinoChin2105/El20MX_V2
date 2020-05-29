function onSignIn(googleUser) {
    console.log(googleUser, "login");
    var profile = googleUser.getBasicProfile();
    console.log(profile, "usuario perfil");
    console.log('ID: ' + profile.getId()); // Do not send to your backend! Use an ID token instead.
    console.log('Name: ' + profile.getName());
    console.log('Image URL: ' + profile.getImageUrl());
    console.log('Email: ' + profile.getEmail()); // This is null if the 'email' scope is not present.

}
function onSuccess(googleUser) {
    console.log(googleUser, "usuario");
    console.log('Logged in as: ' + googleUser.getBasicProfile().getName());
    var profile = googleUser.getBasicProfile();
    //console.log(profile, "usuario perfil");
    //console.log('ID: ' + profile.getId()); // Do not send to your backend! Use an ID token instead.
    //console.log('Name: ' + profile.getName());
    //console.log('Full Name: ' + profile.getName());
    //console.log('Given Name: ' + profile.getGivenName());
    //console.log('Family Name: ' + profile.getFamilyName());
    //console.log('Image URL: ' + profile.getImageUrl());
    //console.log('Email: ' + profile.getEmail()); // This is null if the 'email' scope is not present.


    $("#FistName").val(profile.getGivenName());
    $("#LastName").val(profile.getFamilyName());
    $("#Email").val(profile.getEmail()).attr("readonly", true);
    $("#Password").val(profile.getId()).attr("disabled", true);
    $("#ConfirmPassword").val(profile.getId()).attr("disabled", true);    

    $("#SocialId").val(profile.getId());
    $("#RedSocial").val(true);
    $("#TypeRedSocial").val("Google");
}
function onFailure(error) {
    console.log(error, "Error");
}
function renderButton() {    
    gapi.signin2.render('my-signin2', {
        'scope': 'profile email',
        //'width': '100%',
        //'height': 50,
        'longtitle': true,
        'theme': 'dark',
        'onsuccess': onSuccess,
        'onfailure': onFailure
    });
}

//function login() {
//    $.ajax({
//        url: '@Url.Action("ConvertirJson","Login")',
//        data: { user: info.email, pass: info.id, facebook: true, rutaFace: info.picture.data.url },
//        type: 'POST',
//        dataType: 'json',
//        success: function (json) {
//            console.log(json)
//            if (json.facebook === "0") {
//                //redireccionar al registro del usuario
//                //window.location = "/registro-facebook";
//            }
//            if (json.ID_CLIENTE > 0) {
//                //redireccionar al sitio con ya la cuenta iniciadad
//                //window.location = "/cuenta";
//            }
//        },
//        error: function (xhr, status) {
//            //alert('Disculpe, existió un problema');
//            $(document).ready(function () {
//                //Mensaje de error
//                //$("body").addClass("no_scroll");
//                //$('#ModalError').show();                     
//            });
//        },
//    });
//}

function signOut() {
    var auth2 = gapi.auth2.getAuthInstance();
    auth2.signOut().then(function () {
        console.log('User signed out.');
    });
}
