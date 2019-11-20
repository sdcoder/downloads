/*globals angular */

// module + controller, dependent upon rates
angular.module('LightStreamApp')
    .controller('LandingPageController', [ '$scope', '$window', 'loanAppSessionService',
        function ($scope, $window, loanAppSessionService) {
            'use strict';

            $scope.applyFor = function (purposeOfLoan) {
                loanAppSessionService.setPurposeOfLoan(purposeOfLoan);
                return false;
            };

            $scope.setPurposeOfLoan = function (purposeOfLoan) {
                $scope.PurposeOfLoan = purposeOfLoan;
            };
        }]);