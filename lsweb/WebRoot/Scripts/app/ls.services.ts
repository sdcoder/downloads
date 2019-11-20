/*globals angular, jQuery, $ */
/*jslint regexp: true */
/*jslint plusplus: true */
/*jslint nomen: true */

(function () {
    "use strict";

    angular.module('ls.services', ['ngRoute', 'webStorageModule'])
        .config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
            'use strict';
            $routeProvider.caseInsensitiveMatch = true;
            $httpProvider.interceptors.push(['$injector', function ($injector) {
                var globalInterceptorFactory = $injector.get('globalInterceptorFactory');

                return {
                    request: globalInterceptorFactory.requests.lowercase
                }
            }]);
        }]);

    angular.module('ls.services').factory('marketingService', ['$window', function ($window) {

        return {
            t: function (o) {
                if ($window.SC) {
                    $window.SC._t(o);
                }
            },
            tl: function (o) {
                if ($window.SC) {
                    $window.SC._tl(o);
                }
            },
            toolusage: function (o) {
                if ($window.SC) {
                    $window.SC._toolusage(o);
                }
            },
            button: function (o) {
                if ($window.SC) {
                    $window.SC._button(o);
                }
            }
        };
    }]);

    // ratesService
    angular.module('ls.services').factory('ratesService', ['$http', '$window', function ($http, $window) {
        return {
            getRates: function (request) {
                var mData = $window.mData || {};

                mData.ratesRequest = {
                    PurposeOfLoan: request.PurposeOfLoan,
                    LoanAmount: request.LoanAmount,
                    LoanTermMonths: request.LoanTermMonths,
                    PaymentMethod: request.PaymentMethod
                };

                var queueId = request.QueueId;

                var promise = $http
                    .post('/Services/Rates', {
                        ApplicationId: request.ApplicationId,
                        PurposeOfLoan: request.PurposeOfLoan,
                        LoanAmount: request.LoanAmount,
                        LoanTermMonths: request.LoanTermMonths,
                        PaymentMethod: request.PaymentMethod,
                        ZipCode: request.ZipCode,
                        State: request.State,
                        CoApplicantState: request.CoApplicantState,
                        Discount: request.Discount,
                        ApplicationStatus: request.ApplicationStatus,
                        RateLockDate: request.RateLockDate,
                        IsSuntrustApplication: request.IsSuntrustApplication,
                        CustomRateDisclosureContent: request.CustomRateDisclosureContent,
                        TypeOfCalculator: request.TypeOfCalculator,
                        IsAddCoApp: request.IsAddCoApp
                    }).then(function (response) {
                        response.data.queueId = queueId;
                        return response.data;
                    }, function (error) {
                        error.queueId = queueId;
                        return error;
                    });
                return promise;
            }
        };
    }]);

    // stateService
    angular.module('ls.services').factory('stateService', ['$http', function ($http) {
        return {
            getState: function (request) {
                var promise = $http
                    .post('/Services/StateLookup', {
                        ZipCode: request.ZipCode,
                        LoanAmount: request.LoanAmount
                    })
                    .then(function (response) {
                        return response.data;
                    });

                return promise;
            },
            getStateTax: function (state, loanAmount) {
                var promise = $http
                    .post('/Services/StateTaxLookup', {
                        State: state,
                        LoanAmount: loanAmount
                    })
                    .then(function (response) {
                        return response.data;
                    });

                return promise;
            }
        };
    }]);

    angular.module('ls.services').factory('staticLookupService', ['$http', 'factHistory', function ($http, factHistory) {
        return {
            getStaticLookups: function () {
                var promise = $http.get('/Lookups/StaticLookups')
                    .then(function (response) {
                        return response.data.Result;
                    });
                return promise;
            }
        };
    }]);

    angular.module('ls.services').factory('contactUsService', ['$http', function ($http) {
        return {
            sendEmail: function (request) {
                var promise = $http.post('/modals/contact-us', {
                    Name: request.Name,
                    EmailAddress: request.EmailAddress,
                    Message: request.Message
                    })
                    .then(function (response) {
                        return response.data;
                    });
                return promise;
            }
        };
    }]);

    // NLTRService
    // Expects scope to have a model named 'LoanAcceptance', with a State, LoanTerms, and ChangeLoanTerms objects
    angular.module('ls.services').factory('NLTRService', ['$http', 'stateService', 'ratesService', '$location', '$window', '$sce', '$log',
        function ($http, stateService, ratesService, $location, $window, $sce, $log) {
            return {
                setUp: function ($scope, onAutoApproved, onNLTRInProcess, onAutoDeclined) {

                    var rateQueueCounter = 1;
                    $scope.rateRequestQueue = [];
                    $scope.rateResponseQueue = [];

                    var getQueueId = function () {
                        var queueId = rateQueueCounter++;
                        $scope.rateRequestQueue.push(queueId);

                        return queueId;
                    };

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
                                queuedResponse.postProcessing(queuedResponse.data);

                                // remove from both queues
                                var indexToRemove = $scope.rateResponseQueue.indexOf(queuedResponse);
                                $scope.rateResponseQueue.splice(indexToRemove, 1);

                                $scope.rateRequestQueue.shift();
                            }
                        }, 100);
                    };

                    $scope.$watch('LoanAcceptance.ChangeLoanTerms.LoanAmountMinusFees', function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            if ($scope.LoanAcceptance.IsEligibleForFloridaDocStampTax) {
                                stateService.getStateTax({
                                    State: 'Florida',
                                    LoanAmount: newVal
                                }).then(function (data) {
                                    $scope.LoanAcceptance.ChangeLoanTerms.FloridaDocStampTax = data.FloridaDocStampTax;
                                });
                            }
                        }
                    });

                    $scope.$watchCollection('[LoanAcceptance.ChangeLoanTerms.PaymentMethod, LoanAcceptance.ChangeLoanTerms.LoanTerm, LoanAcceptance.ChangeLoanTerms.LoanAmountMinusFees]', function () {
                        if (!$scope.LoanAcceptance.LoanTerms || !$scope.LoanAcceptance.ChangeLoanTerms) {
                            return;
                        }

                        var queueId = getQueueId();
                        startRatesResponseTimer();

                        ratesService.getRates({
                            QueueId: queueId,
                            ApplicationId: $scope.LoanAcceptance.ApplicationId,
                            ApplicationStatus: 'Approved',
                            PurposeOfLoan: $scope.LoanAcceptance.LoanTerms.PurposeOfLoan,
                            LoanAmount: !$scope.LoanAcceptance.IsEligibleForFloridaDocStampTax ? $scope.LoanAcceptance.ChangeLoanTerms.LoanAmountMinusFees
                                : ($scope.LoanAcceptance.ChangeLoanTerms.LoanAmountMinusFees + $scope.LoanAcceptance.ChangeLoanTerms.FloridaDocStampTax),
                            LoanTermMonths: $scope.LoanAcceptance.ChangeLoanTerms.LoanTerm,
                            PaymentMethod: $scope.LoanAcceptance.ChangeLoanTerms.PaymentMethod || 'AutoPay',
                            ZipCode: $scope.LoanAcceptance.ZipCode || '',
                            Discount: $scope.LoanAcceptance.Discount || '',
                            RateLockDate: $scope.LoanAcceptance.RateLockDate
                        }).then(function (data) {
                            $scope.rateResponseQueue.push(
                                {
                                    queueId: data.queueId,
                                    data: data,
                                    postProcessing: function (data) {
                                        $scope.LoanTermRates = data;
                                        if (data && data.ProductRate) {
                                            $scope.LoanAcceptance.ChangeLoanTerms.InterestRate = (data.ProductRate * 100);
                                            $scope.LoanAcceptance.ChangeLoanTerms.MonthlyPayment = (data.ProductMonthlyPayment);
                                        } else {
                                            $scope.LoanAcceptance.ChangeLoanTerms.InterestRate = null;
                                            $scope.LoanAcceptance.ChangeLoanTerms.MonthlyPayment = null;
                                        }

                                        $scope.$apply();
                                    }
                                }
                            );
                        });
                    });

                    $scope.changeLoanTermsHasInFactNotChanged = function () {
                        if ($scope.LoanAcceptance.LoanTerms && $scope.LoanAcceptance.ChangeLoanTerms) {
                            if (
                                (parseFloat($scope.LoanAcceptance.LoanTerms.LoanAmount).toFixed(2) === parseFloat($scope.LoanAcceptance.ChangeLoanTerms.LoanAmountMinusFees).toFixed(2)) &&
                                ($scope.LoanAcceptance.LoanTerms.LoanTerm === $scope.LoanAcceptance.ChangeLoanTerms.LoanTerm) &&
                                ($scope.LoanAcceptance.LoanTerms.PaymentMethod === $scope.LoanAcceptance.ChangeLoanTerms.PaymentMethod)
                            ) {
                                return true;
                            }
                        }

                        return false;
                    };

                    $scope.submitChangeLoanTermsRequest = function () {
                        if ($scope.Waiting) {
                            return;
                        }
                        if ($scope.changeLoanTermsHasInFactNotChanged()) {
                            $scope.ChangeLoanTermsError = 'If you wish to submit a change, please modify your existing terms. Thank You.';
                            return;
                        }
                        $scope.Waiting = true;

                        $http.post('/AppStatus/SubmitChangeLoanTermsRequest', $scope.LoanAcceptance).success(function (result) {
                            if (result && result.Success) {
                                if (result.IsAutoApproved) {
                                    $window.location.href = onAutoApproved;
                                    return;
                                }
                                if (result.IsAutoDeclined) {
                                    $window.location.href = onAutoDeclined || '/appstatus/refresh';
                                    return;
                                }
                                // else
                                $scope.ChangeLoanTermsError = null;
                                $location.path('/ConfirmChangeLoanTerms');
                            } else if (!result || result.IsStale) {
                                $scope.ChangeLoanTermsError = $sce.trustAsHtml('There was an error submitting your request. Please <a href="/appstatus/refresh">click here</a> to log in and try again.');
                            } else {
                                $scope.ChangeLoanTermsError = (result && $sce.trustAsHtml(result.ErrorMessage)) || $sce.trustAsHtml('There was an error submitting your request. Please <a href="/appstatus/refresh">click here</a> to log in and try again.');
                                if (result && result.MaxLoanAmount) {
                                    $scope.LoanTermRates.MaxLoanAmount = result.MaxLoanAmount;
                                }
                            }
                            $scope.Waiting = false;
                        });
                    };
                    $scope.confirmLoanTermsChangeRequest = function () {
                        if ($scope.Waiting) {
                            return;
                        }
                        if ($scope.changeLoanTermsHasInFactNotChanged()) {
                            $scope.ErrorMessage = 'If you wish to submit a change, please modify your existing terms. Thank You.';
                            $location.path('/ChangeLoanTerms');
                            return;
                        }
                        $scope.Waiting = true;

                        $http.post('/AppStatus/ConfirmChangeLoanTermsRequest', $scope.LoanAcceptance).success(function (result) {
                            if (result && result.Success) {
                                $window.location.href = onNLTRInProcess;
                            } else {
                                $scope.ErrorMessage = result && (result.ErrorMessage || "We're sorry, but there was an error submitting your request");
                                $scope.Waiting = false;
                            }
                        });
                    };

                }
            };

        }]);

    angular.module('ls.services').factory('occupationService', ['$http', function ($http) {
        return $http.get('/Services/Autosuggest/Occupation');
    }]);

    angular.module('ls.services').factory('validationService', ['$http', '$log', function ($http, $log) {
        return {
            getEmailValidation: function (email) {

                var data = { email: email };
                var promise = $http.post('/services/validate-email', data)
                    .then(function success(response) {
                        if (response && response.data) {
                            return response.data;
                        }
                        else {
                            return { isSuccessful: false };
                        }
                    },
                    function error() {
                        return { isSuccessful: false };
                    });
                return promise;
            }
        };
    }]);

    angular.module('ls.services').factory('globalInterceptorFactory', ['$window', function ($window) {
        return {
            requests: {
                lowercase: function (config) {
                    if (config.url.substring(0, 1) === '/' || config.url.substring(0, 1) === '.') {
                        //requesting an external resource
                        if (config.url.substring(0, 2) === '//')
                            return config;

                        //requesting an internal resource

                        //--don't lowercase query string
                        config.url = config.url.split('?')[0].toLowerCase();

                        //--don't lowercase jump link
                        config.url = config.url.split('#')[0].toLowerCase();

                        return config;
                    }

                    return config;
                }
            },
            responses: {
                maintenanceMode: function (res) {
                    if (res.headers('X-In-Maintenance-Mode') !== null && res.headers('X-In-Maintenance-Mode') === 'true')
                        $window.location.href = '/customer-sign-in';

                    return res;
                }
            }
        }
    }]);
}());