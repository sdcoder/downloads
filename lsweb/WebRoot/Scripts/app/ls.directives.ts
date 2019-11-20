/*globals angular, ProperCase, window, $, jQuery, Modernizr */
/*jslint regexp: true */
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
/*jslint nomen: true */
/*jslint bitwise: true*/
interface String {
    lpad(padString: string, length: number): string;
}

(function () {
    "use strict";
    angular.module('LightStreamDirectives', []);

    String.prototype.lpad = function (padString, length) {
        var str = this;
        while (str.length < length) {
            str = padString + str;
        }
        return str.toString();
    };

    var NAVIGATION_KEY_CODES = [8, 9, 35, 36, 37, 38, 39, 40, 45, 46, 112, 113, 114, 115, 116, 118, 119, 120, 121, 122, 123, 229], //229 is a code that some android keyboards send
        EDIT_KEY_CODES = [8, 9, 35, 36, 37, 38, 39, 40, 45],
        NUMERIC_KEY_CODES = [48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105],
        DECIMAL_KEY_CODES = NUMERIC_KEY_CODES.concat(190, 110);

    // ls-blur
    angular.module('LightStreamDirectives').directive('lsBlur', ['$parse', function ($parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                console.log('ack');
                var fn = $parse(attr.lsBlur);
                element.bind('blur', function (event) {
                    scope.$apply(function () {
                        fn(scope, { $event: event });
                    });
                });
            }
        };
    }]);

    // ls-officer-id
    // validates against a list of valid id's
    angular.module('LightStreamDirectives').directive('lsOfficerId', ['$http', function ($http) {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.
                    var numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    // if 6 digits or more, validate against server
                    if (numericOnly.length >= 5) {
                        $http({
                            method: 'GET',
                            url: '/Services/ValidateOfficerId/' + inputValue,
                            cache: true
                        }).then(function (result) {
                            modelCtrl.$setValidity("notFound", result && result.data && result.data.Success);
                            modelCtrl.$setValidity("valid", result && result.data && result.data.Success);
                        });
                    }


                    return numericOnly;
                });
            }
        };
    }]);

    // ls-branch-cost-center
    // validates against a list of valid id's
    angular.module('LightStreamDirectives').directive('lsBranchCostCenter', ['$http', function ($http) {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.
                    var numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    // if 7 digits or more, validate against server
                    if (numericOnly.length >= 7) {
                        $http({
                            method: 'GET',
                            url: '/Services/ValidateBranchCostCenter/' + inputValue,
                            cache: true
                        }).then(function (result) {
                            modelCtrl.$setValidity("notFound", result && result.data && result.data.Success);
                        });
                    }


                    return numericOnly;
                });
            }
        };
    }]);

    // ls-zip-code
    // restricts textbox to numbers only, must be exactly 5 digits long
    // auto-formats when focus is lost
    angular.module('LightStreamDirectives').directive('lsZipCode', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                // element.bind("keydown") to intercept keystrokes for invalid values
                element.bind("keydown", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }
                    // if not a numeric key, or shift is pressed (SHIFT + '3' == '#'), prevent it
                    if ($.inArray(event.which, NUMERIC_KEY_CODES) === -1 || event.shiftKey) {
                        scope.$apply(function () {
                            event.preventDefault();
                        });
                        event.preventDefault();
                    }
                });

                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.
                    var numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    // stop typing at 5 digits
                    if (numericOnly.length > 5) {
                        numericOnly = numericOnly.substring(0, 5);
                        if (numericOnly !== inputValue) {
                            modelCtrl.$setViewValue(numericOnly);
                            modelCtrl.$render();
                        }
                    }

                    // validate - exactly 5 digits, and not all same number
                    modelCtrl.$setValidity("valid", numericOnly.length === 5);

                    return numericOnly;
                });
            }
        };
    });

    // ls-date-of-birth
    // auto-formats when focus is lost
    angular.module('LightStreamDirectives').directive('lsDateOfBirth', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {

                // this should handle "010190" or "01011990"
                var parseDataStringToNumeric = function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null) {
                        return NaN;
                    }

                    var dateStr,
                        result,
                        numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    if (numericOnly.length === inputValue.length) {
                        if (numericOnly.length === 4) {
                            dateStr = numericOnly.substring(0, 1) + '/' + numericOnly.substring(1, 2) + '/' + numericOnly.substring(2, 4);
                            return Date.parse(dateStr);
                        }
                        if (numericOnly.length === 6) {
                            if (numericOnly.substring(2, 4) === '19' || numericOnly.substring(2, 4) === '20') {
                                // odd "221971" = "2/2/1971" use case
                                dateStr = numericOnly.substring(0, 1) + '/' + numericOnly.substring(1, 2) + '/' + numericOnly.substring(2, 6);
                            } else {
                                dateStr = numericOnly.substring(0, 2) + '/' + numericOnly.substring(2, 4) + '/' + numericOnly.substring(4, 8);
                            }
                            try {
                                if (isNaN(Date.parse(dateStr)) || new Date(Date.parse(dateStr)).getFullYear() > new Date().getFullYear()) {
                                    dateStr = numericOnly.substring(0, 2) + '/' + numericOnly.substring(2, 4) + '/19' + numericOnly.substring(4, 8);
                                }
                            } catch (ignore) { }

                            return Date.parse(dateStr);
                        }
                        if (numericOnly.length === 8) {
                            dateStr = numericOnly.substring(0, 2) + '/' + numericOnly.substring(2, 4) + '/' + numericOnly.substring(4, 8);
                            return Date.parse(dateStr);
                        }
                    }

                    // IE8 use case - failes to parse ISO dates.
                    result = Date.parse(inputValue);
                    if (isNaN(result) && numericOnly.length === 14) { // ISO
                        dateStr = numericOnly.substring(6, 8) + '/' + numericOnly.substring(4, 6) + '/' + numericOnly.substring(0, 4);
                        result = Date.parse(dateStr);
                    }

                    if (new Date(result).getFullYear() > new Date().getFullYear()) {
                        var resultDate = new Date(result);
                        resultDate.setFullYear(1900 + resultDate.getFullYear() % 100);
                        if (!isNaN(resultDate.valueOf())) {
                            result = resultDate.valueOf();
                        }
                    }

                    return result;
                },
                    formatModel = function () {
                        var dateNumeric, dt;
                        if (scope.model && modelCtrl.$valid) {
                            dateNumeric = parseDataStringToNumeric(scope.model);
                            if (!isNaN(dateNumeric)) {
                                dt = new Date(dateNumeric);

                                // return in mm/dd/yyyy
                                return (dt.getMonth() + 1).toString().lpad('0', 2) + '/' + dt.getDate().toString().lpad('0', 2) + '/' + dt.getFullYear();
                            }
                        }

                        return modelCtrl.$viewValue;
                    };


                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    var dateNumeric, dt, years;

                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // parse the date, which will return "NaN" if it cannot be parsed
                    dateNumeric = parseDataStringToNumeric(inputValue);
                    if (isNaN(dateNumeric)) {
                        modelCtrl.$setValidity("parse", false);
                        return inputValue;
                    }
                    modelCtrl.$setValidity("parse", true);

                    dt = new Date(dateNumeric);

                    // compute the years difference
                    /*ignore jslint start*/
                    years = ~~((Date.now() - dt) / (31557600000));
                    /*ignore jslint end*/

                    // if the year is before 1800, or after 2100, it is invalid
                    if (dt.getFullYear() < 1800 || dt.getFullYear() > 2100 || years < 1) {
                        modelCtrl.$setValidity("year", false);
                        return inputValue;
                    } // else 
                    modelCtrl.$setValidity("year", true);

                    var submitDate = new Date();
                    submitDate.setHours(0, 0, 0, 0);

                    // compute the age. No minors
                    var dateTo18 = new Date(dateNumeric);
                    dateTo18.setFullYear(dateTo18.getFullYear() + 18);

                    if (dateTo18 > submitDate) {  //check if 18 or over on submit date
                      modelCtrl.$setValidity("age", false);
                        return inputValue;
                    } //else 
                    modelCtrl.$setValidity("age", true);

                    // Also, no old people (age over 120)
                    var dateTo120 = new Date(dateNumeric);
                    dateTo120.setFullYear(dateTo120.getFullYear() + 120);

                    if (dateTo120 < submitDate) {  //check if 120 or over on submit date
                        modelCtrl.$setValidity("maxAge", false);
                        return inputValue;
                    } //else 
                    modelCtrl.$setValidity("maxAge", true);

                    // ok
                    modelCtrl.$setValidity("valid", true);

                    return inputValue;
                });

                modelCtrl.$formatters.push(function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }
                    return formatModel();
                });

                //  on blur, if valid, format properly
                element.on('blur', function () {
                    if (modelCtrl.$valid) {
                        modelCtrl.$setViewValue(formatModel());
                        modelCtrl.$render();
                    }
                });
            }
        };
    });

    angular.module('LightStreamDirectives').directive('lsAccountNumber', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, elementModel) {
                var numericKeyCodes = NUMERIC_KEY_CODES.concat(109, 189); // '-' differs across browsers
                element.bind("keydown", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }
                    // if not a numeric key, or shift is pressed (SHIFT + '3' == '#'), prevent it
                    if ($.inArray(event.which, numericKeyCodes) === -1 || event.shiftKey) {
                        scope.$apply(function () {
                            event.preventDefault();
                        });
                        event.preventDefault();
                    }
                });
            }
        };
    });

    // ls-social-security-number
    // restricts textbox to numbers, spaces, dashes, or parens
    // auto-formats when focus is lost
    angular.module('LightStreamDirectives').directive('lsSocialSecurityNumber', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                // element.bind("keydown") to intercept keystrokes for invalid values
                var numericKeyCodes = NUMERIC_KEY_CODES.concat(109, 189), // '-' differs across browsers
                    formatModel = function () {
                        if (scope.model && modelCtrl.$valid) {
                            var ssn = scope.model.toString();
                            return ssn.substring(0, 3) + '-' + ssn.substring(3, 5) + '-' + ssn.substring(5, 9);
                        }

                        return modelCtrl.$viewValue;
                    };


                element.bind("keydown", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }
                    // if not a numeric key, or shift is pressed (SHIFT + '3' == '#'), prevent it
                    if ($.inArray(event.which, numericKeyCodes) === -1 || event.shiftKey) {
                        scope.$apply(function () {
                            event.preventDefault();
                        });
                        event.preventDefault();
                    }
                });

                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.
                    var numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    // stop typing at nine digits
                    if (numericOnly.length > 9) {
                        numericOnly = numericOnly.substring(0, 9);
                        if (numericOnly !== inputValue) {
                            modelCtrl.$setViewValue(numericOnly);
                            modelCtrl.$render();
                        }
                    }

                    // validate - exactly 9 digits, and not all same number
                    modelCtrl.$setValidity("valid", (numericOnly.length === 9 && !/^(.)\1+$/.test(numericOnly)) || (numericOnly.length === 0 && modelCtrl.$error.required !== true));

                    return numericOnly;
                });

                modelCtrl.$formatters.push(function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }
                    return formatModel();
                });

                //  on blur, if valid, format properly
                element.on('blur', function () {
                    if (modelCtrl.$valid) {
                        modelCtrl.$setViewValue(formatModel());
                        modelCtrl.$render();
                    }
                });
            }
        };
    });

    // ls-phone-number
    // restricts textbox to numbers, spaces, dashes, or parens
    // auto-formats when focus is lost
    angular.module('LightStreamDirectives').directive('lsPhoneNumber', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel',
                required: '&ngRequired'
            },
            link: function (scope, element, attrs, modelCtrl: any) {
                // default 
                modelCtrl.$validationMessage = 'Please enter a valid 10 digit phone number (456-456-4567)';
                modelCtrl.$mulitPhoneValidationMessage = 'Please enter at least one 10 digit phone number (456-456-4567)';

                // local variables, functions
                var numericKeyCodes = NUMERIC_KEY_CODES.concat(109, 189), // '-' differs across browsers
                    validate = function (numericOnly, validLength) {
                        if (attrs.ngRequired) {
                            if (scope.required() === false) {
                                modelCtrl.$setValidity("valid", (numericOnly.length === 0 || numericOnly.length === validLength) && numericOnly.substring(0, 1) !== '0');
                                return;
                            }
                        }

                        modelCtrl.$setValidity("valid", numericOnly.length === validLength && numericOnly.substring(0, 1) !== '0');
                        var prefix,
                            regex,
                            parseValue = (numericOnly.substring(0, 1) === '1') ? numericOnly.substring(1) : numericOnly;

                        if (modelCtrl.$valid) {
                            // extra validation, for "isSameDigit"
                            regex = new RegExp(/^(.)\1+$/);

                            if (regex.test(parseValue.substring(0, 3)) && regex.test(parseValue.substring(3, 6)) && regex.test(parseValue.substring(6, 11))) {
                                modelCtrl.$setValidity("valid", false);
                            } else {
                                // and for "prefix"
                                prefix = parseInt(parseValue.substring(3, 6), 10);
                                if (isNaN(prefix) || prefix < 100) {
                                    modelCtrl.$setValidity("valid", false);
                                }
                            }
                        }
                    },
                    formatModel = function () {
                        if (scope.model && modelCtrl.$valid && scope.model.AreaCode) {
                            return '(' + scope.model.AreaCode + ') ' + scope.model.CentralOfficeCode + '-' + scope.model.LineNumber;
                        }

                        return modelCtrl.$viewValue;
                    };

                // element.bind("keydown") to intercept keystrokes for invalid values
                element.bind("keydown", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }
                    // allow parens
                    if (event.which === 57 && event.shiftKey) {
                        return true;
                    }
                    if (event.which === 48 && event.shiftKey) {
                        return true;
                    }
                    // if not a numeric key, or shift is pressed (SHIFT + '3' == '#'), prevent it
                    if ($.inArray(event.which, numericKeyCodes) === -1 || event.shiftKey) {
                        scope.$apply(function () {
                            event.preventDefault();
                        });
                        event.preventDefault();
                    }
                });


                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.
                    var parseValue,
                        numericOnly = inputValue.toString().replace(/[^\d]/g, ''),
                        validLength = numericOnly.substring(0, 1) === '1' ? 11 : 10; // 1-123-456-7890

                    if (numericOnly.length > validLength) {
                        numericOnly = numericOnly.substring(0, validLength);
                    }
                    modelCtrl.$validLength = validLength;
                    modelCtrl.$validationMessage = (validLength === 11) ? 'Please enter a valid 11 digit phone number (1-456-456-4567)' : 'Please enter a valid 10 digit phone number (456-456-4567)';

                    validate(numericOnly, validLength);

                    parseValue = (numericOnly.substring(0, 1) === '1') ? numericOnly.substring(1) : numericOnly;

                    if (!modelCtrl.$valid) {
                        return undefined;
                    }

                    return {
                        AreaCode: parseValue.substring(0, 3),
                        CentralOfficeCode: parseValue.substring(3, 6),
                        LineNumber: parseValue.substring(6, 11)
                    };
                });

                modelCtrl.$formatters.push(function (inputValue) {
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }
                    if (typeof inputValue === 'object' && inputValue.AreaCode && inputValue.CentralOfficeCode && inputValue.LineNumber) {
                        return '(' + inputValue.AreaCode + ') ' + inputValue.CentralOfficeCode + '-' + inputValue.LineNumber;
                    }
                    if (inputValue.length < 10) {
                        return inputValue;
                    }
                    return formatModel();
                });

                //  on blur, if valid, format properly
                element.on('blur', function () {
                    if (modelCtrl.$valid) {
                        modelCtrl.$setViewValue(formatModel());
                        modelCtrl.$render();
                    }
                });
            }
        };
    });


    // common directives. Added to 'ng' namespace so all forms can share them
    // ls-track-visited
    angular.module('LightStreamDirectives').directive('lsTrackVisited', function () {
        return {
            restrict: 'A',
            require: '?ngModel',
            link: function (scope, elm, attr, ctrl: any) {
                if (!ctrl) {
                    return;
                }

                $(elm).on('keydown', function (event) {
                    if (event.which == 9) {
                        scope.$apply(function () {
                            ctrl.$hasFocus = false;
                            ctrl.$hasVisited = true;
                        });
                    }
                });

                elm.on('focus', function () {
                    elm.addClass('has-focus');
                    ctrl.$hasFocus = true;
                    ctrl.$gainedFocus = true;
                });

                elm.on('blur', function () {
                    elm.removeClass('has-focus');
                    elm.addClass('has-visited');

                    scope.$apply(function () {
                        ctrl.$hasFocus = false;
                        ctrl.$hasVisited = true;
                    });
                });

                $(elm).closest('form').on('submit', function () {
                    elm.addClass('has-visited');

                    scope.$apply(function () {
                        ctrl.$hasFocus = false;
                        ctrl.$hasVisited = true;
                    });
                });

                ctrl.$clearTrackVisited = function () {
                    this.$hasVisited = false;
                    elm.removeClass('has-visited');
                };
            }
        };
    });


    // ls-validate-on-submit
    // Set error-property to evaluate a property on load. if "truthy", submit() will be called
    // to highlight all required fields. Useful when redircting to a form to display 
    // missing values
    angular.module('LightStreamDirectives').directive('lsValidateOnSubmit', ['$timeout', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element, attributes) {
                var $element = angular.element(element),
                    checkForm = function () {
                        var focusElement,
                            form;

                        // Remove the class pristine from all form elements.
                        $element.find('.ng-pristine').removeClass('ng-pristine');
                        $element.addClass('ng-submitted');

                        // Get the form object.
                        form = scope[attributes.name];

                        form.$pristine = false;
                        form.$submitted = true;

                        // Set all the fields to dirty and apply the changes on the scope so that
                        // validation errors are shown on submit only.
                        angular.forEach(form, function (formElement, fieldName) {
                            // If the fieldname starts with a '$' sign, it means it's an Angular
                            // property or function. Skip those items.
                            if (fieldName[0] === '$') {
                                return;
                            }

                            if (scope.$$phase) {
                                formElement.$pristine = false;
                                formElement.$dirty = true;
                            } else {
                                scope.$apply(function () {
                                    formElement.$pristine = false;
                                    formElement.$dirty = true;
                                });
                            }
                        });

                        // Do not continue if the form is invalid.
                        if (form.$invalid) {
                            // Focus on the first field that is invalid
                            focusElement = $('.ng-invalid', $element).first();

                            if (Modernizr && (Modernizr as any).touch) {
                                $timeout(function () {
                                    $('html, body').animate({
                                        scrollTop: focusElement.closest('.row').offset().top - 100
                                    }, 200, function () {
                                        if (document.activeElement instanceof HTMLElement) {
                                            document.activeElement.blur();
                                        }
                                        setTimeout(function () {
                                            focusElement.focus();
                                        }, 500);
                                    });
                                }, 180);
                            } else {
                                if (document.activeElement instanceof HTMLElement) {
                                    document.activeElement.blur();
                                }
                                setTimeout(function () {
                                    focusElement.focus();
                                }, 500);
                            }
                            
                            return false;
                        }

                        return true;
                    };

                // Add novalidate to the form element.
                attributes.$set('novalidate', 'novalidate');

                $element.bind('submit', function (e) {
                    if (checkForm()) {
                        if (attributes.onSuccess) {
                            scope.$apply(attributes.onSuccess);
                            e.preventDefault();
                        }
                        return true;
                    }

                    e.preventDefault();
                    return false;
                });
                scope.$on('validate', function (e) {
                    checkForm();
                    return false;
                });

                if (attributes.errorProperty && scope.$eval(attributes.errorProperty)) {
                    checkForm();
                }
            }
        };
    }]);

    // ls-drop-down-with-not-selected
    angular.module('LightStreamDirectives').directive('lsDropDownWithNotSelected', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    return inputValue || 'NotSelected';
                });

                modelCtrl.$formatters.push(function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null || inputValue === 'NotSelected') {
                        return '';
                    }
                    return inputValue;
                });
            }
        };
    });

    // ls-prohibit-not-selected
    // sets the "required" error if the drop-down is at 'NotSelected'
    angular.module('LightStreamDirectives').directive('lsProhibitNotSelected', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                function validate(value) {
                    if (modelCtrl.$isEmpty(value) || value === 'NotSelected') {
                        modelCtrl.$setValidity('required', false);
                        return 'NotSelected';
                    }
                    modelCtrl.$setValidity('required', true);
                    return value;
                }

                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    validate(inputValue);
                    return inputValue;
                });

                scope.$watch(function () {
                    return modelCtrl.$viewValue;
                }, validate);
            }
        };
    });

    // ls-currency-only
    // directive for currency only input fields
    // "maxlength" refers to the number of digits, without formatting, in the field. i.e. $100,000.00 = maxlength of 8
    angular.module('LightStreamDirectives').directive('lsCurrencyOnly', ['$filter', function ($filter) {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                var maxLength = attrs.maxlength,
                    minValue = isNaN(parseFloat(attrs.min)) ? undefined : parseFloat(attrs.min),
                    maxValue = isNaN(parseFloat(attrs.min)) ? undefined : parseFloat(attrs.max),
                    filterName = attrs.filterName || 'currency',
                    isRequired = function () {
                        if (attrs.ngRequired) {
                            return scope.$eval(attrs.ngRequired);
                        }
                        if (attrs.required) {
                            return true;
                        }

                        return false;
                    },
                    validate = function (decimalFormat) {
                        var required = isRequired();
                        if (required || (decimalFormat && decimalFormat.length > 0)) {
                            if (minValue !== undefined && !isNaN(minValue) && decimalFormat !== undefined) {
                                modelCtrl.$setValidity('min', (parseFloat(decimalFormat) >= minValue) || (!required && decimalFormat === '0'));
                            }
                            if (maxValue !== undefined && !isNaN(maxValue)) {
                                modelCtrl.$setValidity('max', parseFloat(decimalFormat) <= maxValue);
                            }
                        } else {
                            modelCtrl.$setValidity('min', true);
                            modelCtrl.$setValidity('max', true);
                        }
                        modelCtrl.$setValidity('parse', true);
                        modelCtrl.$setValidity('pattern', true);
                    };

                // element.bind("keydown") to intercept keystrokes for invalid values
                element.bind("keydown", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }
                    // if not a numeric key, or shift is pressed (SHIFT + '3' == '#'), prevent it
                    if ($.inArray(event.which, DECIMAL_KEY_CODES) === -1 || event.shiftKey) {

                        scope.$apply(function () {
                            event.preventDefault();
                        });

                        event.preventDefault();
                    }
                });

                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    var transformedInput, decimalFormat;

                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    transformedInput = inputValue.toString().replace(/[^0-9\.\,\$]/g, '');
                    if (maxLength && transformedInput.length > maxLength) {
                        transformedInput = transformedInput.substring(0, maxLength);
                    }

                    if (transformedInput !== inputValue) {
                        modelCtrl.$setViewValue(transformedInput);
                        modelCtrl.$render();
                    }

                    decimalFormat = transformedInput.replace(/[^0-9\.]/g, '');
                    validate(decimalFormat);
                    if (!modelCtrl.$valid) {
                        //return undefined; 
                    }
                    return decimalFormat;
                });

                modelCtrl.$formatters.push(function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    return $filter(filterName)(modelCtrl.$modelValue);
                });

                element.on('blur', function () {
                    if (modelCtrl.$valid) {
                        if ((modelCtrl.$modelValue == null || modelCtrl.$modelValue === '') && !isRequired()) {
                            modelCtrl.$setViewValue('');
                        } else {
                            modelCtrl.$setViewValue($filter(filterName)(modelCtrl.$modelValue));
                        }
                        modelCtrl.$render();
                    }
                });

                // watch for any changes to min or max values
                attrs.$observe<string>('min', function (newVal) {
                    minValue = isNaN(parseFloat(newVal)) ? undefined : parseFloat(newVal);
                    if (scope.$eval(attrs.ngModel) === undefined && element.val()) {
                        validate(element.val().replace(/[^0-9\.]/g, ''));
                    } else {
                        validate(scope.$eval(attrs.ngModel));
                    }
                });
                attrs.$observe<string>('max', function (newVal) {
                    maxValue = isNaN(parseFloat(newVal)) ? undefined : parseFloat(newVal);
                    validate(scope.$eval(attrs.ngModel));
                });

                validate(scope.$eval(attrs.ngModel));
                scope.$watch(function () { return modelCtrl.$error.required; }, function (value) {
                    validate(scope.$eval(attrs.ngModel));
                });
            }
        };
    }]);


    // ls-proper-case
    // uses the lightstream "ProperCase" functions to capitalize words, with exceptions
    // use "properCase='upper' to transform to uppercase
    angular.module('LightStreamDirectives').directive('lsProperCase', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: true,
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                var properCase = function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }
                    var capitalized = inputValue;

                    // Either uppercase it if "upper" is specified, or use the default "ProperCase"
                    if (attrs.lsProperCase && attrs.lsProperCase === "upper") {
                        capitalized = inputValue.toUpperCase();
                    } else {
                        capitalized = ProperCase.transform(inputValue);
                    }

                    if (capitalized !== inputValue) {
                        modelCtrl.$setViewValue(capitalized);
                        modelCtrl.$render();
                    }
                    return capitalized;
                };

                //modelCtrl.$parsers.push(properCase);
                properCase(scope.model);  // capitalize initial value

                //  on blur, format properly
                element.on('blur', function () {
                    if (modelCtrl.$valid) {
                        modelCtrl.$setViewValue(properCase(modelCtrl.$modelValue));
                        modelCtrl.$render();
                    }
                });
            }
        };
    });

    // ls-disallow-spaces
    // will prevent input of spaces from keypress and pasting
    angular.module('LightStreamDirectives').directive('lsDisallowSpaces', function () {
        return {
            restrict: 'A',
            link: function ($scope, $element) {
                $element.bind('input', function () {
                    $(this).val($(this).val().replace(/ /g, ''));
                });
            }
        };
    });

    // ls-restrict-to-pattern
    // Will restrict a user from typing anything that isn't allowed by the "ng-pattern" 
    angular.module('LightStreamDirectives').directive('lsRestrictToPattern', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },

            link: function (scope, element, attrs, modelCtrl) {
                var pattern = attrs.ngPattern || attrs.lsRestrictToPattern,
                    regEx = new RegExp(pattern.replace(/^\/|\/$/g, '')), // strip off leading and trailing '/' char from the JS regex
                    validateCharString = function (charString, event) {
                        if (regEx.test(charString)) {
                            return true;
                        }
                        // else 
                        scope.$apply(function () {
                            if (event.preventDefault) {
                                event.preventDefault();
                            } else {
                                event.returnValue = false;
                            }
                        });
                        if (event.preventDefault) {
                            event.preventDefault();
                        } else {
                            event.returnValue = false;
                        }
                    };

                element.bind("keypress", function (event) {
                    var charString = String.fromCharCode(event.charCode || event.which || event.keyCode),
                        keyCode = event.which || event.keyCode;
                    
                    return validateCharString(charString, event);
                });

                element.bind("paste", function (event) {
                    var charString,
                        clipboardData = (((event as any) as ClipboardEvent).clipboardData || (window as any).clipboardData as DataTransfer);
                    if (clipboardData) {
                        charString = clipboardData.getData("text");
                        if (charString == null) {
                            charString = clipboardData.getData("text/plain");
                        }
                        if (charString !== null) {
                            return validateCharString(charString, event);
                        }
                    }
                    return true;
                });
            }
        };
    });

    // ls-months
    // Will restrict a user from typing anything over 11
    angular.module('LightStreamDirectives').directive('lsMonths', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: true,

            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                var regEx = new RegExp(/[0-9]/),
                    otherFieldIsZero = function () {
                        return Number(scope.$eval(attrs.ngModel.replace('Months', 'Years'))) === 0;
                    };

                element.bind("keypress", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which || event.keyCode, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }

                    var charString = String.fromCharCode(event.which);
                    if (regEx.test(charString)) {
                        return true;
                    } // else {
                    scope.$apply(function () {
                        event.preventDefault();
                    });
                    event.preventDefault();
                });

                scope.$watch(attrs.ngModel.replace('Months', 'Years'), function (newValue) {
                    modelCtrl.$setValidity('doubleZero', !(Number(newValue) === 0 && modelCtrl.$viewValue === '0'));
                });

                modelCtrl.$parsers.push(function (inputValue: string) {
                    if (inputValue === '0') {
                        if (otherFieldIsZero()) {
                            modelCtrl.$setValidity('doubleZero', false);
                        }
                    } else {
                        modelCtrl.$setValidity('doubleZero', true);
                    }
                    return inputValue;
                });

            }
        };
    });

    // ls-numbers-only
    // directive for integer only input fields. Decimals, commas, formatting not allowed
    angular.module('LightStreamDirectives').directive('lsNumbersOnly', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            scope: {
                model: '=ngModel'
            },
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                var maxLength = attrs.maxlength,
                    minLength = attrs.minlength,
                    minValue = isNaN(parseInt(attrs.min, 10)) ? undefined : parseInt(attrs.min, 10),
                    maxValue = isNaN(parseInt(attrs.min, 10)) ? undefined : parseInt(attrs.max, 10),
                    minLengthPassed = (minLength === undefined) ? true : false,
                    lostFocus = false,
                    validate = function (viewValue) {
                        if (attrs.ngRequired) {
                            if (!scope.$eval(attrs.ngRequired)) {
                                return true;
                            }
                        }
                        if (minLength) {
                            if ((viewValue && viewValue.toString().length >= minLength) || viewValue.toString().length == 0) {
                                minLengthPassed = true;
                                modelCtrl.$setValidity('minlength', true);
                            } else {
                                modelCtrl.$setValidity('minlength', false);
                            }

                        }

                        if (attrs.ngMin) {
                            minValue = scope.$eval(attrs.ngMin);
                        }
                        if (minValue !== undefined && (minLengthPassed || lostFocus)) {
                            modelCtrl.$setValidity('min', (parseInt(viewValue, 10) >= minValue) || isNaN(parseInt(viewValue, 10)) || viewValue == undefined);
                        }
                        if (maxValue !== undefined && (minLengthPassed || lostFocus)) {
                            modelCtrl.$setValidity('max', (parseInt(viewValue, 10) <= maxValue) || isNaN(parseInt(viewValue, 10)) || viewValue == undefined);
                        }
                    };

                // fix numeric / string data binding
                if (scope.model && typeof scope.model === 'string') {
                    scope.model = parseInt(scope.model, 10);
                }

                // element.bind("keydown") to intercept keystrokes for invalid values
                element.bind("keydown", function (event) {
                    // allow any "navigation" key codes such as tab, arrow keys, copy/paste, etc...
                    if ($.inArray(event.which, NAVIGATION_KEY_CODES) !== -1 || event.ctrlKey) {
                        return true;
                    }
                    // if not a numeric key, or shift is pressed (SHIFT + '3' == '#'), prevent it
                    if ($.inArray(event.which, NUMERIC_KEY_CODES) === -1 || event.shiftKey) {
                        scope.$apply(function () {
                            event.preventDefault();
                        });
                        event.preventDefault();
                    }
                });

                element.on('blur', function () {
                    lostFocus = true;
                });

                modelCtrl.$formatters.push(function (inputValue: string) {
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }
                    validate(inputValue);
                    return inputValue;
                });


                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    var transformedInput = inputValue.toString().replace(/[^0-9]/g, '');
                    if (maxLength && transformedInput.length > maxLength) {
                        transformedInput = transformedInput.substring(0, maxLength);
                    }

                    validate(transformedInput);
                    if (transformedInput !== inputValue) {
                        modelCtrl.$setViewValue(transformedInput);
                        modelCtrl.$render();
                    }

                    return transformedInput;
                });

                // watch for any changes to min or max values
                attrs.$observe<string>('min', function (newVal) {
                    minValue = isNaN(parseInt(newVal, 10)) ? undefined : parseInt(newVal, 10);
                    validate(element.val());
                });
                attrs.$observe<string>('max', function (newVal) {
                    maxValue = isNaN(parseInt(newVal, 10)) ? undefined : parseInt(newVal, 10);
                    validate(element.val());
                });
            }
        };
    });

    // ls-checklist-model
    // For more info on checklist-model - https://github.com/vitalets/checklist-model
    angular.module('LightStreamDirectives').directive('lsChecklistModel', ['$parse', '$compile', function ($parse, $compile) {
        // contains
        function contains(arr, item) {
            var i;
            if (angular.isArray(arr)) {
                for (i = 0; i < arr.length; i += 1) {
                    if (angular.equals(arr[i], item)) {
                        return true;
                    }
                }
            }
            return false;
        }

        // add
        function add(arr, item) {
            var i;
            arr = angular.isArray(arr) ? arr : [];
            for (i = 0; i < arr.length; i += 1) {
                if (angular.equals(arr[i], item)) {
                    return arr;
                }
            }
            arr.push(item);
            return arr;
        }

        // remove
        function remove(arr, item) {
            var i;
            if (angular.isArray(arr)) {
                for (i = 0; i < arr.length; i += 1) {
                    if (angular.equals(arr[i], item)) {
                        arr.splice(i, 1);
                        break;
                    }
                }
            }
            return arr;
        }

        // http://stackoverflow.com/a/19228302/1458162
        function postLinkFn(scope, elem, attrs) {
            // compile with `ng-model` pointing to `checked`
            $compile(elem)(scope);

            // getter / setter for original model
            var getter = $parse(attrs.lsChecklistModel),
                setter = getter.assign,

                // value added to list
                value = $parse(attrs.checklistValue)(scope.$parent) || attrs.checklistValue;

            // watch UI checked change
            scope.$watch('checked', function (newValue, oldValue) {
                if (newValue === oldValue) {
                    return;
                }
                var current = getter(scope.$parent);
                if (newValue === true) {
                    setter(scope.$parent, add(current, value));
                } else {
                    setter(scope.$parent, remove(current, value));
                }
            });

            // watch original model change
            scope.$parent.$watch(attrs.lsChecklistModel, function (newArr, oldArr) {
                scope.checked = contains(newArr, value);
            }, true);
        }

        return {
            restrict: 'A',
            priority: 1000,
            terminal: true,
            scope: true,
            compile: function (tElement, tAttrs) {
                if (tElement[0].tagName !== 'INPUT' || tElement.attr('type') !== 'checkbox') {
                    throw 'ls-checklist-model should be applied to `input[type="checkbox"]`.';
                }

                if (!tAttrs.checklistValue) {
                    throw 'You should provide `checklist-value`.';
                }

                // exclude recursion
                tElement.removeAttr('ls-checklist-model');

                // local scope var storing individual checkbox model
                tElement.attr('ng-model', 'checked');

                return postLinkFn;
            }
        };
    }]);

    // ls-password-match
    angular.module('LightStreamDirectives').directive('lsPasswordMatch', [function () {
        return {
            restrict: 'A',
            scope: true,
            require: 'ngModel',
            link: function (scope, elem, attrs, control: ng.INgModelController) {
                var checker = function () {
                    //get the value of the first password
                    var e1 = scope.$eval(attrs.ngModel),

                        //get the value of the other password  
                        e2 = scope.$eval(attrs.lsPasswordMatch);

                    return e1 === e2;
                };
                scope.$watch(checker, function (n) {

                    //set the form control to valid if both 
                    //passwords are the same, else invalid
                    control.$setValidity("match", n);
                });
            }
        };
    }]);

    // ls-does-not-match
    // can be used to fail validation when two fields (such as SSN) match
    angular.module('LightStreamDirectives').directive('lsDoesNotMatch', [function () {
        return {
            restrict: 'A',
            scope: true,
            require: 'ngModel',
            link: function (scope, elem, attrs, control: ng.INgModelController) {
                var checker = function () {

                    // skip if it's on file
                    if (scope.$eval(attrs.lsIsOnFile)) {
                        return true;
                    }
                    //get the value of the first 
                    var e1 = scope.$eval(attrs.ngModel),

                        //get the value of the other 
                        e2 = scope.$eval(attrs.lsDoesNotMatch);

                    return e1 !== e2;
                };
                scope.$watch(checker, function (n) {
                    control.$setValidity("match", n);
                });
            }
        };
    }]);

    // ls-purpose-of-loan-description
    // displays the purpose of loan description, based on the enum value
    angular.module('LightStreamDirectives').directive('lsPurposeOfLoanDescription', ['$http', function ($http) {
        return {
            template: "{{description}}",
            restrict: 'A',
            scope: {
                purposeOfLoan: "="
            },
            link: function (scope, elem, attrs, control) {
                $http.get('/Services/Lookups/PurposeOfLoan/' + scope.purposeOfLoan).success(function (data) {
                    scope.description = data.Result;
                });
            }
        };
    }]);

    // ls-two-letter-state-code
    angular.module('LightStreamDirectives').directive('lsTwoLetterStateCode', ['$http', function ($http) {
        return {
            template: "{{description}}",
            restrict: 'A',
            scope: {
                state: "="
            },
            link: function (scope, elem, attrs, control) {
                $http.get('/Services/Lookups/State/' + scope.state).success(function (data) {
                    scope.description = data.Result;
                });
            }
        };
    }]);

    // ls-is-on-file
    angular.module('LightStreamDirectives').directive('lsIsOnFile', [function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            replace: true,
            link: function (scope, element, attrs, modelCtrl) {
                if (scope.$eval(attrs.lsIsOnFile)) {
                    element.replaceWith('<div><input type="text" class="disabled" disabled="disabled" value="[On File]"  /></div>');
                }
            }
        };
    }]);

    // ls-signature
    // inspired by http://ericterpstra.com/2012/10/angularjs-signit-custom-directives-and-form-controls/
    angular.module('LightStreamDirectives').directive('lsSignature', ['$timeout', function ($timeout) {
        return {
            templateUrl: '/scripts/templates/signature.html',   // Use a template in an external file
            restrict: 'A',                      // Must use <sigpad> element to invoke directive
            scope: {
                ApplicantFullName: '@applicantFullName'
            },                       // Create a new scope for the directive
            require: 'ngModel',                 // Require the ngModel controller for the linking function
            link: function (scope, element, attr, ctrl: ng.INgModelController) {
                // Attach the Signature Pad plugin to the template and keep a reference to the signature pad as 'sigPadAPI'
                var sigPadAPI = $(element).signaturePad({
                    defaultAction: 'drawIt',
                    penColour: 'black',
                    lineColour: '#E57A00',
                    lineWidth: 1,
                    clear: '',
                    lineTop: $('.sigPad canvas').height() - 2
                });

                scope.useScriptFont = function (newValue) {
                    scope.UsingScriptFont = newValue;
                    scope.updateModel();
                    return false;
                };

                // Clear the canvas when the 'clear' button is clicked
                $(element).find('[href=\\#clear]').on('click', function (e) {
                    sigPadAPI.clearCanvas();
                    scope.updateModel();
                    return false;
                });
                $(element).find('.pad').on('touchend', function (obj) {
                    scope.updateModel();
                });

                // when the mouse is lifted from the canvas, set the signature pad data as the model value
                scope.updateModel = function () {
                    $timeout(function () {
                        ctrl.$setViewValue(sigPadAPI.getSignature());
                    });
                };

                // Render the signature data when the model has data. Otherwise clear the canvas.
                ctrl.$render = function () {
                    if (ctrl.$viewValue) {
                        sigPadAPI.regenerate(ctrl.$viewValue.Data);
                    } else {
                        // This occurs when signatureData is set to null in the main controller
                        sigPadAPI.clearCanvas();
                    }
                };

                // update the model
                ctrl.$parsers.unshift(function (viewValue) {
                    return {
                        ApplicantFullName: scope.ApplicantFullName,
                        UsingScriptFont: scope.UsingScriptFont,
                        Data: viewValue,
                        JSON: sigPadAPI.getSignatureString()
                    };
                });
            }
        };
    }]);


    /*ignore jslint start*/
    // https://github.com/angular-ui/ui-date
    angular.module('LightStreamDirectives')
        .constant('uiDateConfig', {})
        .directive('uiDate', ['uiDateConfig', function (uiDateConfig) {
            var options;
            options = {};
            angular.extend(options, uiDateConfig);
            return {
                require: '?ngModel',
                link: function (scope, element, attrs, controller: ng.INgModelController) {
                    var getOptions = function () {
                        return angular.extend({}, uiDateConfig, scope.$eval(attrs.uiDate));
                    },
                        initDateWidget = function () {
                            var showing = false,
                                opts = getOptions(),
                                _onSelect;

                            // If we have a controller (i.e. ngModelController) then wire it up
                            if (controller) {
                                // Set the view value in a $apply block when users selects
                                // (calling directive user's function too if provided)
                                _onSelect = opts.onSelect || angular.noop;
                                opts.onSelect = function (value, picker) {
                                    scope.$apply(function () {
                                        showing = true;
                                        controller.$setViewValue(element.datepicker("getDate"));
                                        _onSelect(value, picker);
                                        element.blur();
                                    });
                                };
                                opts.beforeShow = function () {
                                    showing = true;
                                };
                                opts.onClose = function (value, picker) {
                                    showing = false;
                                };
                                element.off('blur.datepicker').on('blur.datepicker', function () {
                                    if (!showing) {
                                        scope.$apply(function () {
                                            element.datepicker("setDate", element.datepicker("getDate"));
                                            controller.$setViewValue(element.datepicker("getDate"));
                                        });
                                    }
                                });

                                // Update the date picker when the model changes
                                controller.$render = function () {
                                    var date = controller.$viewValue;
                                    // *****************************************************************************
                                    // 2014-11-20: LightStream addition of " !isNaN(date) " to handle nullable dates
                                    // *****************************************************************************
                                    if (angular.isDefined(date) && date !== null && !isNaN(date) && !angular.isDate(date)) {
                                        throw new Error('ng-Model value must be a Date object - currently it is a ' + typeof date + ' - use ui-date-format to convert it from a string');
                                    }
                                    element.datepicker("setDate", date);
                                    if (date == null) {
                                        element.find('.ui-state-active').removeClass('ui-state-active');
                                        element.find('.ui-state-highlight').removeClass('ui-state-highlight');
                                    }
                                };
                            }
                            // If we don't destroy the old one it doesn't update properly when the config changes
                            element.datepicker('destroy');
                            // Create the new datepicker widget
                            element.datepicker(opts);
                            if (controller) {
                                // Force a render to override whatever is in the input text box
                                controller.$render();
                            }
                        };
                    // Watch for changes to the directives options
                    scope.$watch(getOptions, initDateWidget, true);
                }
            };
        }])

        .constant('uiDateFormatConfig', '')

        .directive('uiDateFormat', ['uiDateFormatConfig', function (uiDateFormatConfig) {
            var directive = {
                require: 'ngModel',
                link: function (scope, element, attrs, modelCtrl) {
                    var dateFormat = attrs.uiDateFormat || uiDateFormatConfig;
                    if (dateFormat) {
                        // Use the datepicker with the attribute value as the dateFormat string to convert to and from a string
                        modelCtrl.$formatters.push(function (value) {
                            if (angular.isString(value)) {
                                return jQuery.datepicker.parseDate(dateFormat, value);
                            }
                            return null;
                        });
                        modelCtrl.$parsers.push(function (value) {
                            if (value) {
                                return jQuery.datepicker.formatDate(dateFormat, value);
                            }
                            return null;
                        });
                    } else {
                        // Default to ISO formatting
                        modelCtrl.$formatters.push(function (value) {
                            if (angular.isString(value)) {
                                return new Date(value);
                            }
                            return null;
                        });
                        modelCtrl.$parsers.push(function (value) {
                            if (value) {
                                return value.toISOString();
                            }
                            return null;
                        });
                    }
                }
            };
            return directive;
        }]);
    /*ignore jslint end*/

    // ls-routing-number
    // validates against ABA routing number checksum
    angular.module('LightStreamDirectives').directive('lsRoutingNumber', [function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.
                    var i, n, numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    // if 9 digits or more, validate against checksum
                    // http://www.brainjar.com/js/validation/
                    if (numericOnly.length >= 9) {
                        n = 0;
                        for (i = 0; i < numericOnly.length; i += 3) {
                            n += parseInt(numericOnly.charAt(i), 10) * 3
                                + parseInt(numericOnly.charAt(i + 1), 10) * 7
                                + parseInt(numericOnly.charAt(i + 2), 10);
                        }

                        // If the resulting sum is an even multiple of ten (but not zero),
                        // the aba routing number is good.
                        if (n !== 0 && n % 10 === 0) {
                            modelCtrl.$setValidity("checksum", true);
                        } else {
                            modelCtrl.$setValidity("checksum", false);
                        }
                    } else {
                        modelCtrl.$setValidity("checksum", false);
                    }

                    return numericOnly;
                });
            }
        };
    }]);

    // ls-credit-card-number
    // validates against credit card number checksum
    angular.module('LightStreamDirectives').directive('lsCreditCardNumber', [function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, modelCtrl: ng.INgModelController) {
                // parser ensures that maxlength is enforced, and sets valid / invalid flags on the field accordingly
                modelCtrl.$parsers.push(function (inputValue: string) {
                    var numberValid = true,
                        nSum = 0,
                        nVal = 0,
                        aCardNumDigits,
                        numericOnly = inputValue.toString().replace(/[^\d]/g, '');

                    // this next if is necessary for when using ng-required on your input. 
                    // In such cases, when a letter is typed first, this parser will be called
                    // again, and the 2nd time, the value will be undefined
                    if (inputValue === undefined || inputValue == null) {
                        return '';
                    }

                    // "validateInput" block responsible for validation. Strips all non-numeric characters, and validates.

                    // if 16 digits or more, validate against checksum
                    if (numericOnly.length >= 16) {
                        // "5" MasterCard, "4" Visa
                        if (numericOnly.charAt(0) === "5" || numericOnly.charAt(0) === "4") {
                            // Check-sum (Luhn algorithm)
                            aCardNumDigits = numericOnly.split("").reverse();

                            $.each(aCardNumDigits, function (idx: number, digit: number) {
                                nVal = (digit * (idx % 2 === 0 ? 1 : 2));
                                nSum += (Math.floor(nVal / 10) + (nVal % 10));
                            });
                            numberValid = (nSum % 10 === 0);
                        } else {
                            numberValid = false;
                        }

                        modelCtrl.$setValidity("checksum", numberValid);
                    }

                    return numericOnly;
                });
            }
        };
    }]);

    // ls-add-co-applicant
    angular.module('LightStreamDirectives').directive('lsAddCoApplicant', ['loanAppSessionService', '$window', function (loanAppSessionService, $window) {
        return {
            template: '<a href="#" class="orange-button button" ng-disabled="Loading" ng-click="addCoApp()">Submit Joint Application</a>' +
            '<img ng-cloak src="/content/images/ajax-loader.gif" ng-show="Loading" alt=""/>' +
            '<div ng-cloak ng-show="ErrorMessage" class="errorMessage" >{{ErrorMessage}}</div>',
            restrict: 'E',
            transclude: true,
            scope: {},
            link: function (scope, element, attrs, modelCtrl) {
                scope.addCoApp = function () {
                    scope.Loading = true;
                    loanAppSessionService.init('/Apply/Joint/LoadApp').then(function (response) {
                        if (response.Success) {
                            loanAppSessionService.save(response.LoanApp);
                            scope.Loading = false;
                            $window.location.href = '/apply/joint';
                        } else {
                            scope.Loading = false;
                            scope.ErrorMessage = response.ErrorMessage;
                        }
                    });

                };
            }
        };
    }]);

    // ls-jump-page
    // angular version of the old "data-jump=true" attribute used by CMS. This version works well in views or other dynamic content
    angular.module('LightStreamDirectives').directive('lsJumpPage', [function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs, modelCtrl) {
                $(element).bind('click', function (event) {
                    event.preventDefault();
                    var $self = $(this);

                    var href = $(this).attr('href');
                    if (href.toLowerCase().indexOf('lightstream.com') === -1 && href.toLowerCase().indexOf('localhost') === -1) {
                        $('#JumpPageModal').foundation('open');
                        $('#JumpPageModalTarget').attr('href', $(this).attr('href'));
                        $('#JumpPageModalTarget').bind('click', function () {
                            $('#JumpPageModal').foundation('close');
                            // support pop-up windows (like the Norton badge)
                            if ($self.data('popup')) {
                                window.open($(this).attr('href'),
                                    $self.data('name'),
                                    'width=' + $self.data('width') + ',height=' + $self.data('height') + ',location=yes,status=no,scrollbars=no,resizable=yes', true);
                                $('#JumpPageModalTarget').unbind('click');
                                return false;
                            }
                        });
                        return false;
                    }
                });
            }
        };
    }]);

    angular.module('LightStreamDirectives').directive("lsAccordion", function () {
        return {
            restrict: 'A',
            scope: {
                collapsible: "="
            },
            link: function (scope, element) {
                scope.$watch("collapsible", function (value: boolean) {
                    element.accordion({
                        heightStyle: "content",
                        collapsible: value
                    });
                });
            }
        };
    });

    angular.module('LightStreamDirectives').directive('occupationAutocomplete', ['occupationService', function (occupationService) {
        return {
            restrict: 'A',
            scope: { occupationDescription: "=occupationAutocomplete" },
            link: function (scope, element, attribute, controller) {
                occupationService.success(function (data) {
                    element.autocomplete({
                        source: function (request, response) {
                            var results = $.ui.autocomplete.filter(data, request.term);

                            response(results.slice(0, 10));
                        },
                        open: function (event, ui) {
                            if (navigator.userAgent.match(/(iPod|iPhone|iPad)/)) {
                                $('.ui-autocomplete').off('menufocus hover mouseover');
                            }
                        },
                        select: function (event, ui) {
                            scope.$apply();  // silly hack for weird IE8 (autocomplete?) digest cycle behavior
                            scope.$apply(function () {
                                scope.occupationDescription = ui.item.label;
                            });
                        },

                        minLength: 2
                    });
                });
            }
        };
    }]);

    angular.module('LightStreamDirectives').directive('lsImageLoaded', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.bind('load', function (event) {
                    scope[attrs.lsImageLoaded](event);
                    scope.$apply();
                });
                element.bind('error', function (event) {
                    scope[attrs.lsImageLoaded](event);
                    scope.$apply();
                });
            }
        };
    });

    angular.module('LightStreamDirectives').directive('lsEnterPressedToggle', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                /*
                    takes care of custom expanders
                    example: mobile annual percentage rates (apr) table
                  */
                $(element).on('keypress', function (e) {
                    var charCode = e.charCode || e.keyCode;

                    if (charCode == 13) //enter key pressed
                        $(this).toggleClass('expanded');
                });
            }
        };
    });
}());