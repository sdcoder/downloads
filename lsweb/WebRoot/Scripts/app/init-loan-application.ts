/*globals angular */

// LoanApplication module & controller, handles all steps of the loan app process
angular.module('InitializeLoanAppModule', ['webStorageModule', 'ls.services'])
    .controller('InitializeLoanApplicationController',
        function ($scope, $window, loanAppSessionService) {
            'use strict';

            $scope.init = function (dataUrl, nextPage, lastPage) {
                if (!dataUrl) {
                    return;
                }

                loanAppSessionService.init(dataUrl).then(function (response) {
                    if (response.Success) {
                        // change the 'back button' behavior, to prevent shenanigans
                        if (lastPage) {
                            $window.history.replaceState({}, 'Last Page', lastPage || '/');
                        }
                        loanAppSessionService.save(response.LoanApp);
                        // and redirect to the next page
                        if (nextPage) {
                            $window.location.href = nextPage;
                        }
                    } else {
                        $scope.ErrorMessage = response.ErrorMessage;
                    }
                });
            };
        });


