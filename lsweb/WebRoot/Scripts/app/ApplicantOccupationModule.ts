/*globals angular, jQuery, $*/
(function () {
    'use strict';
    angular.module('OccupationModule', ['ng', 'ls.services'])
    .controller('OccupationController', ['$scope',
        function ($scope) {
            $scope.occupationDescription = "";
        }]);
})();