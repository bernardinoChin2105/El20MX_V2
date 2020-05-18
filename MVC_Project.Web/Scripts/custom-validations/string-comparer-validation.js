    (function ($) {
        $.validator.addMethod('stringcomparer', function (value, element, params) {
            console.log("entro");
            var stringToCompare = $(params.propertyId).val();
            if ('=' == params.restriction) {
                return value == stringToCompare;
            }
            if ('=' != params.restriction) {
                return value != stringToCompare;
            }
            return false;
        }, '');

        $.validator.unobtrusive.adapters.add('stringcomparer', ['restriction', 'property'], function (options) {
            console.log("string comparer", options);
            options.rules["stringcomparer"] = {
                propertyId: '#' + options.params.property,
                restriction: options.params.restriction
            };
            options.messages['stringcomparer'] = options.message;
        });
    })(jQuery);