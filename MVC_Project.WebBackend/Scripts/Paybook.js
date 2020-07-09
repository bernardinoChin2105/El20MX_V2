
/*
* 1. Set up and configure it:
*/
var tokenPaybook = "";
var params = {
    // [REQUIRED] A session token obtained from the Sync API
    token: tokenPaybook,
    // [REQUIRED] A DOM element identifier
    // IMPORTANT: This HAS TO MATCH the "id" of the HTML element
    element: "#widget",
    // [OPTIONAL] you can instantiate the widget in strict mode passing the JSON Web Keys
    // JWK(s) as provided in the session object obtained from the Sync API:
    //strict: {
    //    authorization,
    //    body
    //},
    // [OPTIONAL] you can provide a function that will be automatically used by the widget
    // to refresh it's token when this is expired. This function will must return a promise
    // with the new token value.
    refreshTokenFunction: () => {
        //... return some promise with a new token
        console.log("prueba");
    },
    // [REQUIRED] A valid WidgetConfig object
    config: {
        // Set up the language to use:
        locale: 'es',
        entrypoint: {
            // Set up the country to start:
            country: 'MX'
            // Set up the site organization type to start:
            //siteOrganizationType: '56cf4f5b784806cf028b4568'
        },
        navigation: {
            displayStatusInToast: true
        }
    },
};

/*
* 2. Instantiate the widget using the SyncWidget constructor:
*/
syncWidget = new SyncWidget(params);

/*
* 3. If desired, open it right away:
*/
syncWidget.open();


function openPaybook() {

}

