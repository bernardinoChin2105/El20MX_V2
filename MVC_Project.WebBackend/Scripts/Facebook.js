(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/es_LA/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

window.fbAsyncInit = function () {
    FB.init({
        //appId: '246375746578693', //LOCAL                     
        appId: '2444149519209146', //DEV
        //appId: '', //QA
        //appId: '',//PROD

        //appId: '{your-app-id}',
        cookie: true,
        oauth: true,
        xfbml: true,
        //version: 'v2.8' // use graph api version 2.8
        version: 'v6.0',
        //version: 'v7.0.'
    });

    //FB.AppEvents.logPageView();

    FB.getLoginStatus(function (response) {
        //console.log("respuesta", response);
        //close();
        statusChangeCallback(response);
    });
};

function statusChangeCallback(response) {
    //console.log('statusChangeCallback', response);
    if (response.status === 'connected') {
        registerAPI();
    } else if (response.status === 'not_authorized') {
        //no se hace nada
        //console.log(response, "no autorizado");
        sessionStorage.removeItem("SetFacebook");
    } else {
        //console.log("log face");
        registerAPI();
    }
}

//function checkLoginState() {               // Called when a person is finished with the Login Button.
//    console.log("revisar que hace");
//    FB.getLoginStatus(function (response) {   // See the onlogin handler
//        statusChangeCallback(response);
//    });
//}

function registerAPI() {
    var form = $("form").attr("id");
    var session = sessionStorage.getItem("SetFacebook");
    if (form === "RegisterForm" && session) {
        register();
    } else {
        sessionStorage.removeItem("SetFacebook");
    }
}

function GetInfo(response) {
    //console.log(response);
    var profile = response;

    //Asignar información de facebook al registro
    $("#FistName").val(profile.first_name);
    $("#LastName").val(profile.last_name);
    $("#Email").val(profile.email).attr("readonly", true);
    $("#Password").val(profile.id + "Fe.").attr("readonly", true);
    $("#ConfirmPassword").val(profile.id + "Fe.").attr("readonly", true);

    $("#SocialId").val(profile.id);
    $("#RedSocial").val(true);
    $("#TypeRedSocial").val("Facebook");
    $("#btn-social-networks").hide();
    $("#form-title").text("Completa tu registro");

    //$("#Email").trigger("blur");
}

function close() {
    //console.log("entro al cierre");
    FB.logout(function (response) {
        // user is now logged out
        console.log("cerro", response);
    });
}

function register() {
    try {
        FB.login(function (response) {
            //console.log("respuesta", response);
            if (response.status === 'connected') {
                FB.api(
                    '/me',
                    'GET',
                    //{ "fields": "email,first_name,last_name,picture.width(500)" },
                    { "fields": "email,first_name,last_name,birthday, hometown" },
                    function (info) {
                        console.log(info, "información que obtuve");
                        //formulario de registro
                        GetInfo(info);
                    });

            } else {
                //console.log(response + "no hay");
                // The person is not logged into this app or we are unable to tell.
                //GOOGLE TAG MANAGER (PAGE 15)                
            }
        }, { scope: 'public_profile,email' }); //, whatsapp_business_management
    }
    catch (e) {
        // statements to handle any exceptions
        console.log(e, "hubo un error");
    }
}

function Loguear() {
    try {
        FB.login(function (response) {
            //console.log("respuesta", response);
            if (response.status === 'connected') {
                FB.api(
                    '/me',
                    'GET',
                    //{ "fields": "email,first_name,last_name,picture.width(500)" },
                    { "fields": "email,first_name,last_name,birthday, hometown" },
                    function (info) {
                        //console.log(info, "información que obtuve");
                        //validación de login
                        var data = {
                            Email: info.email,
                            Password: info.id,
                            RedSocial: true,
                            TypeRedSocial: "Facebook",
                            SocialId: info.id
                        };

                        $.ajax({
                            url: UrlLogin,
                            data: data,
                            type: 'POST',
                            dataType: 'json',
                            success: function (json) {
                                //console.log(json, "holas");
                                //debugger;
                                if (!json.Success) {
                                    sessionStorage.setItem("SetFacebook", true);
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
                            },
                        });
                    });
            } else {
                //console.log(response + "no hay");
                // The person is not logged into this app or we are unable to tell.
                //GOOGLE TAG MANAGER (PAGE 15)                
            }
        }, { scope: 'public_profile,email' }); //, whatsapp_business_management
    }
    catch (e) {
        // statements to handle any exceptions
        console.log(e, "hubo un error");
    }

}

//(function (d, s, id) {
//    var js, fjs = d.getElementsByTagName(s)[0];
//    if (d.getElementById(id)) { return; }
//    js = d.createElement(s); js.id = id;
//    js.src = "https://connect.facebook.net/en_US/sdk.js";
//    fjs.parentNode.insertBefore(js, fjs);
//}(document, 'script', 'facebook-jssdk'));



