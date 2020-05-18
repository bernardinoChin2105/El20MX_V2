(function ($) {

    $.validator.addMethod("passwordsecured",
        function (value, element, params) {
            return value.match(/^(?=.*\d)(?=.*\W+)(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$/);
        }, ""
    );

    $.validator.unobtrusive.adapters.addBool("passwordsecured");
})(jQuery);