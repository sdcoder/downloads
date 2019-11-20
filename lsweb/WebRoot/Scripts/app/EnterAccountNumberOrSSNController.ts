/*globals angular */
angular.module('LightStreamApp')
    .controller('EnterAccountNumberOrSSNController', function ($scope) {
        'use strict';

        $scope.SelectedType = "SocialSecurityNumber";
        $scope.onSelectedTypeChanged = function (formName) {
            $scope.AccountNumber = null;
            $scope.$parent[formName]['AccountNumberOrSSN.AccountNumber'].$setPristine();
            $scope.$parent[formName]['AccountNumberOrSSN.AccountNumber'].$clearTrackVisited();
            $scope.$parent[formName]['AccountNumberOrSSN.AccountNumber'].$setValidity("valid", true);

            $scope.SocialSecurityNumber = null;
            $scope.$parent[formName]['AccountNumberOrSSN.SocialSecurityNumber'].$setPristine();
            $scope.$parent[formName]['AccountNumberOrSSN.SocialSecurityNumber'].$clearTrackVisited();
            $scope.$parent[formName]['AccountNumberOrSSN.SocialSecurityNumber'].$setValidity("valid", true);
        };
    });