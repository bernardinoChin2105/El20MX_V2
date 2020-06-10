//function onSignIn(googleUser) {
//    console.log(googleUser, "login onclick");
//    var profile = googleUser.getBasicProfile();
//    console.log(profile, "usuario perfil");
//    console.log('ID: ' + profile.getId()); // Do not send to your backend! Use an ID token instead.
//    console.log('Name: ' + profile.getName());
//    console.log('Image URL: ' + profile.getImageUrl());
//    console.log('Email: ' + profile.getEmail()); // This is null if the 'email' scope is not present.
//}

//function onSuccess(googleUser) {
//    console.log(googleUser, "usuario");
//    console.log('Logged in as: ' + googleUser.getBasicProfile().getName());
//    var profile = googleUser.getBasicProfile();
//    //console.log(profile, "usuario perfil");
//    //console.log('ID: ' + profile.getId()); // Do not send to your backend! Use an ID token instead.
//    //console.log('Name: ' + profile.getName());
//    //console.log('Full Name: ' + profile.getName());
//    //console.log('Given Name: ' + profile.getGivenName());
//    //console.log('Family Name: ' + profile.getFamilyName());
//    //console.log('Image URL: ' + profile.getImageUrl());
//    //console.log('Email: ' + profile.getEmail()); // This is null if the 'email' scope is not present.

//    var form = $("form").attr("id");
//    if (form === "Login") {
//        //validación de login
//        login(profile);

//    } else {
//        $("#FistName").val(profile.getGivenName());
//        $("#LastName").val(profile.getFamilyName());
//        $("#Email").val(profile.getEmail()).attr("readonly", true);
//        $("#Password").val(profile.getId()+ "Go.").attr("readonly", true);
//        $("#ConfirmPassword").val(profile.getId() + "Go.").attr("readonly", true);

//        $("#SocialId").val(profile.getId());
//        $("#RedSocial").val(true);
//        $("#TypeRedSocial").val("Google");
//    }
//}
//function onFailure(error) {
//    console.log(error, "Error");
//}
//function renderButton() {
//    gapi.signin2.render('my-signin2', {
//        'scope': 'profile email',
//        //'width': '100%',
//        //'height': 50,
//        'longtitle': true,
//        'theme': 'dark',
//        'onsuccess': onSuccess,
//        'onfailure': onFailure
//    });

//    //var button = document.querySelector('#my-signin2');
//    //button.addEventListener('click', function () {
//    //    console.log("hias");
//    //    gapi.signIn();
//    //});

//    //gapi.load('auth2', function () {
//    //    gapi.auth2.init({
//    //        client_id: "765148629644-ehqbkj60uuotg1tb7ofe66jjf7j8gaut.apps.googleusercontent.com",
//    //        scope: "profile email", // this isn't required
//    //    }).then(function (auth2) {
//    //        console.log("signed in: " + auth2.isSignedIn.get());
//    //        //onSuccess();
//    //        auth2.isSignedIn.listen(onSignIn);
//    //        var button = document.querySelector('#signInButton');
//    //        button.addEventListener('click', function () {
//    //            auth2.signIn();
//    //        });
//    //    });
//    //});
//    var auth2 = gapi.auth2.getAuthInstance();
//    auth2.isSignedIn.listen(onSignIn);

//    function onSignIn(googleUser) {
//        console.log("signedin");
//        // Useful data for your client-side scripts:
//        var profile = googleUser.getBasicProfile();
//        console.log("Name: " + profile.getName());
//    };
//}

//function onSign() {}

function login(googleUser) {
    var profile = googleUser.getBasicProfile();
    var data = {
        Email: profile.getEmail(),
        Password: profile.getId(),
        RedSocial: true,
        TypeRedSocial: "Google",
        SocialId: profile.getId()
    };

    $.ajax({
        url: UrlLogin,
        data: data,
        type: 'POST',
        dataType: 'json',
        success: function (json) {
            console.log(json, "holas");
            debugger;
            if (json.Success === false) {
                var form = $("form").attr("id");
                if (form === "Login") {
                    //validación de login
                    //login(profile);
                    //Redireccionar a home
                    window.location = "/Register";
                } else {
                    $("#FistName").val(profile.getGivenName());
                    $("#LastName").val(profile.getFamilyName());
                    $("#Email").val(profile.getEmail()).attr("readonly", true);
                    $("#Password").val(profile.getId() + "Go.").attr("readonly", true);
                    $("#ConfirmPassword").val(profile.getId() + "Go.").attr("readonly", true);

                    $("#SocialId").val(profile.getId());
                    $("#RedSocial").val(true);
                    $("#TypeRedSocial").val("Google");
                    $("#btn-social-networks").hide();
                    $("#form-title").text("Completa tu registro");
                }
            } else {
                window.location = json.Url;
            }
            //if (json. > 0) {
            //    window.location = "/cuenta";
            //}
        },
        error: function (xhr, status) {
            console.log(xhr, status, "error");
            //alert('Disculpe, existió un problema');
            //$(document).ready(function () {
            //    $("body").addClass("no_scroll");
            //    $('#ModalError').show();                                    
            //});
        },
    });
}

//function signOut() {
//    var auth2 = gapi.auth2.getAuthInstance();
//    auth2.signOut().then(function () {
//        console.log('User signed out.');
//    });
//}


function onSuccess(googleUser) {
    console.log('onSuccess!', googleUser);
    login(googleUser);
}

function onCustomSuccess(googleUser) {
    console.log('onCustomSignIn!', googleUser);
    login(googleUser);
}

function signOut() {
    var auth2 = gapi.auth2.getAuthInstance();
    auth2.signOut().then(function () {
        console.log('User signed out.');
    });
}

function onLoad() {
    gapi.signin2.render('custom_signin_button', {
        'onsuccess': onCustomSuccess
    });
}