/*globals angular */

(function () {
    'use strict';

    var app = angular.module('accountPreferences', ['ngRoute', 'ls.services', 'ls.filters', 'webStorageModule', 'LightStreamDirectives']);

    app.config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        $httpProvider.interceptors.push(['$injector', function ($injector) {
            var globalInterceptorFactory = $injector.get('globalInterceptorFactory');

            return {
                request: globalInterceptorFactory.requests.lowercase
            }
        }]);

        $routeProvider.caseInsensitiveMatch = true;
        $routeProvider.when('/contactInformation', {
                controller: 'contactInformationController',
                templateUrl: 'scripts/templates/contactInformation-index.html'
            }).when('/contactInformation/updateEmail', {
                controller: 'contactInformationController',
                templateUrl: 'scripts/templates/contactInformation-updateEmail.html'
            }).when('/accountLock', {
                controller: "accountLockController",
                templateUrl: 'scripts/templates/accountLock-index.html'
            }).when('/privacyPreferences', {
                controller: "privacyPreferencesController",
                templateUrl: 'scripts/templates/privacyPreferences-index.html'
            }).when('/securityInformation', {
                controller: "securityInformationController",
                templateUrl: 'scripts/templates/securityInformation-index.html'
            }).when('/securityInformation/changeUserId', {
                controller: 'securityInformationController',
                templateUrl: 'scripts/templates/securityInformation/changeUserId-index.html'
            }).when('/securityInformation/changeUserId/new', {
                controller: 'securityInformationController',
                templateUrl: 'scripts/templates/securityInformation/changeUserId-new.html'
            }).when('/securityInformation/changeUserId/move', {
                controller: 'securityInformationController',
                templateUrl: 'scripts/templates/securityInformation/changeUserId-move.html'
            }).when('/securityInformation/changeUserId/accountsync', {
                controller: 'securityInformationController',
                templateUrl: 'scripts/templates/securityInformation/changeUserId-accountsync.html'
            }).when('/securityInformation/changePassword', {
                controller: 'securityInformationController',
                templateUrl: 'scripts/templates/securityInformation/changePassword-index.html'
            }).when('/securityInformation/changeSecurityQuestion', {
                controller: 'securityInformationController',
                templateUrl: 'scripts/templates/securityInformation/changeSecurityQuestion-index.html'
            }).otherwise({ redirectTo: '/' });
        }]);

    app.controller("accountPreferencesController", ["$scope", "$location", function ($scope, $location) {
        $scope.isCurrentRoute = function (routeName) {
            return $location.$$path.indexOf(routeName) === 1;
        };

        $scope.navigate = function (hash) {
            $location.path(hash);
        };

        $scope.validateEmail = function (model, errorModel) {
            var regEx = /^(?!.*([\.])\1{1})([0-9a-zA-Z]([-\.\+\w]*[0-9a-zA-Z_])*@((?!-)[A-Za-z0-9-\.]{1,63}\.){1}[a-zA-Z]{2,9})$/;

            if (!regEx.test(model)) {
                errorModel.$setValidity("pattern", false);
            }
            else {
                errorModel.$setValidity("pattern", true);
            } 
        }
    }]);
}());