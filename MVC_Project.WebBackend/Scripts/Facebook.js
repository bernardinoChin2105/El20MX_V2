window.fbAsyncInit = function () {
    FB.init({
        appId: '246375746578693', //LOCAL 
        //appId: '675014006312332', //DEV 
        //appId: '737753873052692', //QA
        //appId: '1702488126725250',//PROD

        //appId: '{your-app-id}',
        cookie: true,
        xfbml: true,
        version: 'v6.0',
        //version: 'v7.0.'
    });

    FB.AppEvents.logPageView();

    //FB.getLoginStatus(function (response) {
    //    console.log("respuesta", response);
    //    close();
    //    //statusChangeCallback(response);
    //});
};

function close() {
    //FB.logout(function (response) {
    //    // user is now logged out
    //    console.log("cerro", response);
    //});
}

function Loguear() {
    try {
        FB.login(function (response) {
            console.log("respuesta", response);
            if (response.status === 'connected') {
                FB.api(
                    '/me',
                    'GET',
                    { "fields": "email,first_name,last_name,picture.width(500)" },
                    function (info) {
                        console.log(info, "información que obtuve");
                        //$.ajax({
                        //    url: '@Url.Action("ConvertirJson","Login")',
                        //    data: { user: info.email, pass: info.id, facebook: true, rutaFace: info.picture.data.url },
                        //    type: 'POST',
                        //    dataType: 'json',
                        //    success: function (json) {
                        //        console.log(json);

                        //        if (json.facebook === "0") {                                    
                        //            window.location = "/registro-facebook";
                        //        }
                        //        if (json.ID_CLIENTE > 0) {                                    
                        //            window.location = "/cuenta";
                        //        }
                        //    },
                        //    error: function (xhr, status) {
                        //        console.log(xhr, status, "error");
                        //        //alert('Disculpe, existió un problema');
                        //        //$(document).ready(function () {
                        //        //    $("body").addClass("no_scroll");
                        //        //    $('#ModalError').show();                                    
                        //        //});
                        //    },
                        //});
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
        //$(document).ready(function () {

        //    $("body").addClass("no_scroll");
        //    $('#ModalError').show();
        //});

    }

}

//function checkLoginState() {
//    FB.getLoginStatus(function (response) {
//        statusChangeCallback(response);
//    });
//}

//(function (d, s, id) {
//    var js, fjs = d.getElementsByTagName(s)[0];
//    if (d.getElementById(id)) { return; }
//    js = d.createElement(s); js.id = id;
//    js.src = "https://connect.facebook.net/en_US/sdk.js";
//    fjs.parentNode.insertBefore(js, fjs);
//}(document, 'script', 'facebook-jssdk'));

(function (d, s, id) {
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) { return; }
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/es_LA/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));


