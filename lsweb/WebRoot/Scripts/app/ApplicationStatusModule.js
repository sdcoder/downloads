/*globals appstatus, angular*/
angular.module('ApplicationStatusModule', ['ls.services', 'ls.filters', 'LightStreamDirectives', 'angularFileUpload', 'ngRoute']).config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        'use strict';
        $routeProvider.caseInsensitiveMatch = true;
        $httpProvider.interceptors.push(['$injector', function ($injector) {
                var globalInterceptorFactory = $injector.get('globalInterceptorFactory');
                return {
                    request: globalInterceptorFactory.requests.lowercase,
                    response: globalInterceptorFactory.responses.maintenanceMode
                };
            }]);
    }]);
//# sourceMappingURL=ApplicationStatusModule.js.map