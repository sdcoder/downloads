/*globals appstatus, angular*/
(function () {
    'use strict';
    angular.module('ApplicationStatusModule')
        .controller('DeclineReferralOptInController', ['$scope', '$http',
        function ($scope, $http) {
            // decline referral
            $scope.declineReferralOptIn = function () {
                $http.post('/AppStatus/DeclineReferralOptIn').success(function () {
                    $scope.DeclineReferralOptInSubmitted = true;
                });
            };
        }]);
}());
//# sourceMappingURL=DeclineReferralOptInController.js.map