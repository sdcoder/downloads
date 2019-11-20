/*globals angular */
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
(function () {
    'use strict';
    angular.module('LightStreamApp', ['ngRoute', 'webStorageModule', 'ls.services', 'ls.filters', 'LightStreamDirectives'])
        .config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
            'use strict';
            $routeProvider.caseInsensitiveMatch = true;
            $httpProvider.interceptors.push(['$injector', function ($injector) {
                    var globalInterceptorFactory = $injector.get('globalInterceptorFactory');
                    return {
                        request: globalInterceptorFactory.requests.lowercase
                    };
                }]);
        }])
        .run(['$rootScope', function ($rootScope) {
            $rootScope.$on('ls.emit.apply', function (event, args) {
                $rootScope.$broadcast('ls.broadcast.apply', args);
            });
        }]);
}());
//# sourceMappingURL=app.js.map