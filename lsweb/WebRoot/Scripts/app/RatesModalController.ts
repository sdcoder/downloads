/*globals angular*/
(function () {
    'use strict';

    // EnoticesController - responsible for loading and displaying ENotices, and user authentication for joint apps
    angular.module('ApplicationStatusModule')
        .controller('RatesModalController', ['$scope', '$window', '$log', 'ratesService',
            function ($scope, $window, $log, ratesService) {

                var rateQueueCounter = 1;
                $scope.rateRequestQueue = [];
                $scope.rateResponseQueue = [];

                var getQueueId = function () {
                    var queueId = rateQueueCounter++;
                    $scope.rateRequestQueue.push(queueId);

                    return queueId;
                }, startRatesResponseTimer = function () {
                    var intervalId = $window.setInterval(function () {

                        var nextInRequestQueue = $scope.rateRequestQueue[0];

                        // did we get the response yet?
                        var queuedResponse = $scope.rateResponseQueue.find(function (queueItem) {
                            return queueItem.queueId === nextInRequestQueue;
                        });

                        if (queuedResponse) {
                            // stop timer
                            clearInterval(intervalId);

                            // process response
                            queuedResponse.postProcessing(queuedResponse.data);

                            // remove from both queues
                            var indexToRemove = $scope.rateResponseQueue.indexOf(queuedResponse);
                            $scope.rateResponseQueue.splice(indexToRemove, 1);

                            $scope.rateRequestQueue.shift();
                        }
                    }, 100);
                };

                // Rates: for the rates grid modal
                $scope.loadApplicationRates = function (applicationId, purposeOfLoan, applicationStatus) {
                    $scope.ApplicationId = applicationId;
                    $scope.ApplicationStatus = applicationStatus;

                    var queueId = getQueueId();
                    startRatesResponseTimer();

                    ratesService.getRates({
                        QueueId: queueId,
                        ApplicationId: applicationId,
                        PurposeOfLoan: purposeOfLoan,
                        ApplicationStatus: applicationStatus
                    }).then(function (data) {

                        $scope.rateResponseQueue.push({
                            queueId: data.queueId,
                            data: data,
                            postProcessing: function (data) {
                                $scope.rates = data;
                                $scope.PurposeOfLoan = data.PurposeOfLoan;
                                $scope.PurposeOfLoanEnabled = !data.DisableLoanPurposeChange;

                                $scope.$apply();
                            }
                        });
                    });
                };
                $scope.loadRatesGrid = function (purposeOfLoan, disablePurposeOfLoanChange) {

                    var queueId = getQueueId();
                    startRatesResponseTimer();

                    ratesService.getRates({
                        QueueId: queueId,
                        PurposeOfLoan: purposeOfLoan
                    }).then(function (data) {
                        $scope.rateResponseQueue.push({
                            queueId: data.queueId,
                            data: data,
                            postProcessing: function (data) {
                                $scope.rates = data;
                                $scope.PurposeOfLoan = data.PurposeOfLoan;
                                $scope.PurposeOfLoanEnabled = !disablePurposeOfLoanChange;

                                $scope.$apply();
                            }
                        });
                    });
                };
                $scope.$watch('PurposeOfLoan', function (newValue, oldValue) {
                    if (newValue !== oldValue) {

                        var queueId = getQueueId();
                        startRatesResponseTimer();

                        ratesService.getRates({
                            QueueId: queueId,
                            PurposeOfLoan: newValue,
                            ApplicationId: $scope.ApplicationId,
                            ApplicationStatus: $scope.ApplicationStatus
                        }).then(function (data) {
                            $scope.rateResponseQueue.push({
                                queueId: data.queueId,
                                data: data,
                                postProcessing: function (data) {
                                    $scope.rates = data;
                                    $scope.PurposeOfLoan = data.PurposeOfLoan;
                                    $scope.PurposeOfLoanEnabled = !data.DisableLoanPurposeChange;

                                    $scope.$apply();
                                }
                            });
                        });
                    }
                });
            }]);
}());
