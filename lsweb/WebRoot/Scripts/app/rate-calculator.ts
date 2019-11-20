/*globals angular */
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
(function () {
    'use strict';

    angular.module('LightStreamApp');

    angular.module('LightStreamApp')
        .controller('StartApplicationController', ['$scope',
            function ($scope) {
                $scope.startApplication = function (applyPage) {
                    $scope.$emit('ls.emit.apply', { url: applyPage });
                };
            }]);

    angular.module('LightStreamApp')
        .controller('RateCalculator', ['$scope', '$window', 'ratesService', 'stateService', 'marketingService', 'loanAppSessionService',
            function ($scope, $window, ratesService, stateService, marketingService, loanAppSessionService) {
                var self = this;

                self.isLoanPurposeBanner = false;

                var rateQueueCounter = 1;
                $scope.rateRequestQueue = [];
                $scope.rateResponseQueue = [];

                var startRatesResponseTimer = function () {
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
                            if (!queuedResponse.errorProcessing) {
                                queuedResponse.postProcessing(queuedResponse.data);
                            }
                            else {
                                queuedResponse.errorProcessing();
                            }

                            // remove from both queues
                            var indexToRemove = $scope.rateResponseQueue.indexOf(queuedResponse);
                            $scope.rateResponseQueue.splice(indexToRemove, 1);

                            $scope.rateRequestQueue.shift();
                        }
                    }, 100);
                },
                    getQueueId = function () {
                        var queueId = rateQueueCounter++;
                        $scope.rateRequestQueue.push(queueId);

                        return queueId;
                    };

                $scope.getState = function () {
                    stateService.getState($scope).then(function (data) {
                        $scope.LoanCalculator.ZipCode.$setValidity('notFound', data.IsValid);
                        if (data.IsValid) {
                            $scope.State = data.State;
                            $scope.FloridaDocStampTax = data.FloridaDocStampTax;
                        }
                    });
                };

                $scope.$watchCollection('[ZipCode]', function () {
                    if ($scope.ZipCode && $scope.ZipCode.toString().length === 5) {
                        $scope.getState();
                    }
                });

                $scope.$watch('LoanAmount', function (n, o) {
                    if ($scope.ZipCode && $scope.ZipCode.toString().length === 5 && $scope.State && $scope.State === 'Florida') {
                        $scope.getState();
                    }
                });
                $scope.$watchCollection('[PurposeOfLoan, LoanAmount, LoanTermMonths, PaymentMethod]', function () {

                    var request = $scope;
                    request.QueueId = getQueueId();

                    startRatesResponseTimer();

                    ratesService.getRates(request)
                        .then(function (data) {

                            $scope.rateResponseQueue.push(
                                {
                                    queueId: data.queueId,
                                    data: data,
                                    postProcessing: function (data) {
                                        $scope.rates = data;

                                        $scope.$apply();
                                    }
                                }
                            );
                        });
                });


                $scope.$watch('PurposeOfLoan', function (newValue, oldValue) {
                    if (newValue && oldValue != newValue) {
                        /*
                            It's hacky, but since the banner and rate calc dropdown share the same property,
                            need a way to determine if the loan purpose selection is coming from banner or
                            rate calc dropdown.    
                        */
                        if (self.isLoanPurposeBanner)
                            marketingService.toolusage({
                                linkTrackVars: 'eVar45,prop4,events',
                                linkTrackEvents: 'event4',
                                prop4: 'Lightstream Auto Dropdown',
                                eVar45: 'LScom|AllAuto|' + newValue
                            });
                        else
                            marketingService.toolusage({
                                linkTrackVars: 'eVar45,prop4,events',
                                linkTrackEvents: 'event4',
                                prop4: 'Lightstream Rate Calculator',
                                eVar45: 'LScom|RateTerms|CurrentRatesCalculator|' + newValue
                            });
                    }
                });

                $scope.setPurposeOfLoan = function (purposeOfLoan) {
                    $scope.init(purposeOfLoan);
                };

                var autoPurposeValues = [
                    'NotSelected',
                    'NewAutoPurchase',
                    'UsedAutoPurchase',
                    'PrivatePartyPurchase',
                    'LeaseBuyOut',
                    'AutoRefinancing',
                    'MotorcyclePurchase'];

                var autoPurposes = [];

                $scope.autoLoanPurposes = function () {
                    if (autoPurposes.length > 0)
                        return autoPurposes;
                }

                function isAutoPurpose(purpose) {
                    var result = false;
                    angular.forEach(autoPurposeValues, function (value) {
                        if (purpose == value)
                            result = true;
                    });
                    return result;
                }

                function populateAutoPurposes() {
                    if (autoPurposes.length > 0)
                        return;
                    var ps = $scope.rates.LoanPurposes;
                    angular.forEach(ps, function (purpose) {
                        if (isAutoPurpose(purpose.Value))
                            autoPurposes.push(purpose);
                    });
                }

                $scope.init = function (purposeOfLoan, discount, isSuntrustApplication, customRateDisclosureContent, typeOfCalculator) {
                    $scope.TypeOfCalculator = typeOfCalculator;

                    if (!purposeOfLoan || purposeOfLoan === 0 || purposeOfLoan === '0')
                        purposeOfLoan = 'NotSelected';

                    $scope.PurposeOfLoan = purposeOfLoan;

                    $scope.Discount = discount;
                    $scope.IsSuntrustApplication = isSuntrustApplication || false;
                    $scope.CustomRateDisclosureContent = customRateDisclosureContent;

                    var request = $scope;
                    request.QueueId = getQueueId();
                    startRatesResponseTimer();

                    ratesService.getRates(request)
                        .then(function (data) {

                            $scope.rateResponseQueue.push(
                                {
                                    queueId: data.queueId,
                                    data: data,
                                    postProcessing: function (data) {

                                        $scope.rates = data;
                                        populateAutoPurposes();

                                        $scope.$apply();
                                    }
                                }
                            );

                        });
                };

                // on "apply"
                $scope.apply = function () {
                    loanAppSessionService.start($scope.ZipCode, $scope.PurposeOfLoan, $scope.LoanAmount, $scope.LoanTermMonths, $scope.PaymentMethod, $scope.Discount, $scope.IsSuntrustApplication);
                    marketingService.button({
                        linkTrackVars: 'eVar45,events',
                        linkTrackEvents: 'event23',
                        eVar45: 'LScom|RatesTerms|CurrentRatesCalculator|ApplyButton',
                        events: 'event23'
                    });

                    $scope.$emit('ls.ratecalculator.start', $scope);
                };

                // handle broadcast messages
                $scope.$on('ls.broadcast.apply', function (event, args) {
                    $scope.apply(args.url);
                });

                $scope.isLoanPurposeBanner = function (isBanner) {
                    self.isLoanPurposeBanner = isBanner;
                };

                // initialize 
                $scope.PurposeOfLoan = 'NotSelected';
            }]);

    angular.module('LightStreamApp')
        .controller('WidgetController', ['$rootScope', '$log',
            function ($rootScope, $log) {
                $rootScope.$on('ls.ratecalculator.start', function (event, calculatorScope) {

                    var action = $('#LoanCalculator').attr('action');

                    var params = location.search.substring(1).split('&');
                    var fact, lcid;

                    angular.forEach(params, function (item) {
                        var pair = item.split('=');
                        var key = pair[0];
                        var val = pair[1];

                        switch (key.toLowerCase()) {
                            case 'fact':
                                fact = val;
                                break;
                            case 'lcid':
                                lcid = val
                                break;
                        }
                    });

                    switch (calculatorScope.PurposeOfLoan) {
                        case 'HomeImprovement':
                            if (!fact) {
                                fact = 16026
                            };
                            if (!lcid) {
                                lcid = 'CS|HIL|home_improvement|L2|P|apply|' + fact;
                            }

                            break;
                        case 'NewAutoPurchase':
                            if (!fact) {
                                fact = 16011
                            };
                            if (!lcid) {
                                lcid = 'CS|AL|new_auto|L3|P|apply|' + fact;
                            }

                            break;
                        case 'UsedAutoPurchase':
                            if (!fact) {
                                fact = 16008
                            };
                            if (!lcid) {
                                lcid = 'CS|AL|used_auto|L3|P|apply|' + fact;
                            }

                            break;
                        case 'AutoRefinancing':
                            if (!fact) {
                                fact = 16014
                            };
                            if (!lcid) {
                                lcid = 'CS|AL|auto_refi|L3|P|apply|' + fact;
                            }

                            break;
                        case 'MotorcyclePurchase':
                            if (!fact) {
                                fact = 16027
                            };
                            if (!lcid) {
                                lcid = 'CS|RL|motorcycle|L2|apply|' + fact;
                            }

                            break;
                    }

                    action = '/Apply';

                    if (fact) {
                        action += ('?fact=' + fact)
                    };

                    if (lcid) {
                        action += ('&lcid=' + lcid)
                    }

                    $('#LoanCalculator').attr('action', action);
                });
            }
        ]);
}());