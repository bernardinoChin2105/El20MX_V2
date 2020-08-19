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
            //console.log(json, "holas");                      
            if (!json.Success) {
                window.location = "/Register";
            } else {
                if (json.Url !== "") {
                    window.location = json.Url;
                }
            }
        },
        error: function (xhr, status) {
            console.log(xhr, status, "error");
            //alert('Disculpe, existió un problema');
            //$(document).ready(function () {
            //    $("body").addClass("no_scroll");
            //    $('#ModalError').show();                                    
            //});
        }
    });
}


function onSuccess(googleUser) {
    console.log('onSuccess!', googleUser);    
    var profile = googleUser.getBasicProfile();
    $("#FistName").val(profile.getGivenName());
    $("#LastName").val(profile.getFamilyName());
    $("#Email").val(profile.getEmail()).attr("readonly", true);
    $("#Password").val(profile.getId() + "Go.").attr("readonly", true);
    $("#ConfirmPassword").val(profile.getId() + "Go.").attr("readonly", true);
    document.getElementById('view-password').disabled = true;
    document.getElementById('view-password-confirm').disabled = true;

    $("#SocialId").val(profile.getId());
    $("#RedSocial").val(true);
    $("#TypeRedSocial").val("Google");
    $("#btn-social-networks").hide();
    $("#form-title").text("Completa tu registro");
}

function onCustomSuccess(googleUser) {
    //console.log('onCustomSignIn!', googleUser);
    login(googleUser);
}

function onFailure() {
    console.log("onFailure");
}

function signOut() {
    //var auth2 = gapi.auth2.getAuthInstance();
    //auth2.signOut().then(function () {
    //    console.log('User signed out.');
    //});

    gapi.load('auth2', function () {
        gapi.auth2.init().then(function () {
            var auth2 = gapi.auth2.getAuthInstance();
            auth2.signOut().then(function () {
                console.log('User signed out.');
            });
        });
    });
}

function onLoad() {
    gapi.load('auth2', function () {
        gapi.auth2.init().then(function () {            
            var auth2 = gapi.auth2.getAuthInstance();
            auth2.signOut().then(function () {
                console.log('User signed out.');
            }).then(function () {
                gapi.signin2.render('custom_signin_button', {
                    'onsuccess': onCustomSuccess,
                    'onfailure': onFailure
                });
            });
        });
    });

    //gapi.signin2.render('custom_signin_button', {
    //    'onsuccess': onCustomSuccess,
    //    'onfailure': onFailure
    //});
}