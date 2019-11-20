/*global angular, $ */
/*jslint browser: true*/
(function () {
    'use strict';

    // log (some) client side errors
    window.onerror = function (msg, url, line, columnNo, error) {
        debugger;
        $.post('/error/logjserror', {
            msg: msg,
            url: url,
            line: line
        });
        return true;
    };

    // angularjs
    angular.module('ls.common', []).config(function ($provide) {
        debugger;
        $provide.decorator("$exceptionHandler", function ($delegate) {
            return function (exception, cause) {
                $delegate(exception, cause);
                $.post('/error/logangularerror', {
                    exception: exception,
                    cause: cause
                });
            };
        });
    });

}());

