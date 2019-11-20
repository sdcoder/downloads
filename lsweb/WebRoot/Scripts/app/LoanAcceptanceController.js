/*globals angular, SC, document, jQuery, $ */
/*jslint browser: true */
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
/* Unit testing note: for proper unit testing, we'll need to remove the dependency on "SC" (site catalyst) or encapsulate that in a service */
// LoanAcceptanceController - controls al four phases of loan acceptance
angular.module('ApplicationStatusModule')
    .config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        'use strict';
        $routeProvider.caseInsensitiveMatch = true;
        $routeProvider
            .when('/LoanAgreement', { templateUrl: 'TabLoanAgreement' })
            .when('/Review', { templateUrl: 'TabReview' })
            .when('/Account', { templateUrl: 'TabAccount' })
            .when('/Verify', { templateUrl: 'TabVerify' })
            .when('/Processing', { templateUrl: 'TabProcessing' })
            .when('/ChangeLoanTerms', { templateUrl: 'TabChangeLoanTerms' })
            .when('/ConfirmChangeLoanTerms', { templateUrl: 'TabConfirmChangeLoanTerms' })
            .otherwise({ templateUrl: 'TabReview' });
    }]);
angular.module('ApplicationStatusModule')
    .controller('LoanAcceptanceController', ['$scope', '$http', '$route', 'ratesService', '$location', '$window', '$timeout', '$anchorScroll', '$sce', 'NLTRService',
    function ($scope, $http, $route, ratesService, $location, $window, $timeout, $anchorScroll, $sce, NLTRService) {
        'use strict';
        var SuspendWatchPaymentIsSameAsFunding = false;
        // START private functions
        var initCalendar = function () {
            // show two months
            $scope.dateOptions = {
                numberOfMonths: 2,
                setDate: 0,
                beforeShowDay: function (date) {
                    if (!$scope.LoanAcceptance || !$scope.LoanAcceptance.CalendarFundingDates) {
                        return [true];
                    }
                    var fd = $scope.LoanAcceptance.CalendarFundingDates[date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate()];
                    if (fd && fd === 'Wire') {
                        return [true, 'wire', ''];
                    }
                    if (fd && fd === 'ACH') {
                        return [true, 'ach', ''];
                    }
                    return [false, '', ''];
                }
            };
        }, checkForCompleteSignatures = function () {
            if ((!$scope.LoanAcceptance.IsJoint && $scope.LoanAcceptance.ApplicantSignature.Submitted) ||
                ($scope.LoanAcceptance.IsJoint && $scope.LoanAcceptance.ApplicantSignature.Submitted && $scope.LoanAcceptance.CoApplicantSignature.Submitted)) {
                $scope.Waiting = true;
                $http.post('/AppStatus/PersistLoanAgreement', { LoanTermsRequestId: $scope.LoanAcceptance.LoanTerms.LoanTermsRequestId }).success(function (result, status) {
                    if (result && !result.Success) {
                        $scope.ErrorMessage = $sce.trustAsHtml('There was an error submitting your loan agreement. Please <a href="/appstatus/refresh">click here</a> to log in and try again.');
                        $scope.Waiting = false;
                        return false;
                    }
                    $scope.LoanAcceptance.IsSigned = true;
                    $scope.LoanAcceptance.IsPartiallySigned = false;
                    $location.path('/Account');
                    $scope.Waiting = false;
                });
                return;
            }
            if ($scope.LoanAcceptance.IsJoint && ($scope.LoanAcceptance.ApplicantSignature.Submitted || $scope.LoanAcceptance.CoApplicantSignature.Submitted)) {
                $scope.LoanAcceptance.IsPartiallySigned = true;
            }
        }, submitSignature = function (sigData, isCoApp) {
            $http.post('/Signature/Submit', {
                applicationId: $scope.LoanAcceptance.ApplicationId,
                usingScriptFont: sigData.UsingScriptFont,
                signatureJSON: sigData.JSON,
                isCoApplicant: isCoApp,
                applicantFullName: sigData.ApplicantFullName
            }).success(function (response) {
                if (response.Success) {
                    sigData.Submitted = true;
                    sigData.TimeStamp = response.TimeStamp;
                    sigData.ImageURL = response.ImageURL;
                    checkForCompleteSignatures();
                }
                else {
                    $scope.ErrorMessage = 'There was an error saving your signature';
                }
            });
        };
        // END private functions
        // load data
        $scope.LoanAcceptance = {};
        $scope.LoanAcceptance.RequiresApplicantSignatureAcknowledgement = false;
        $scope.LoanAcceptance.RequiresCoApplicantSignatureAcknowledgement = false;
        $scope.LoanAgreementModel = {};
        $http.post('/AppStatus/LoadLoanAcceptanceData').success(function (result) {
            $scope.LoanAcceptance = result;
            $scope.LoanAcceptance.ChangeLoanTerms = {
                LoanAmountMinusFees: result.LoanTerms.LoanAmountMinusFees,
                PurposeOfLoan: result.LoanTerms.PurposeOfLoan,
                PaymentMethod: result.LoanTerms.PaymentMethod,
                LoanTerm: result.LoanTerms.LoanTerm,
                InterestRate: result.LoanTerms.InterestRate,
                MonthlyPayment: result.LoanTerms.MonthlyPayment,
                FloridaDocStampTax: result.LoanTerms.FloridaDocStampTax
            };
            // trigger date refresh
            initCalendar();
            // Rhode Island
            if ($scope.LoanAcceptance.RequiresAcknowledgements && $scope.LoanAcceptance.IsSecured) {
                $scope.$watch('LoanAgreementModel.CoApplicantAcknowledged', function (newValue) {
                    var value = newValue === undefined ? false : newValue;
                    $scope.ShowCoApplicantInitials = value;
                    $scope.LoanAcceptance.RequiresCoApplicantSignatureAcknowledgement = $scope.LoanAcceptance.IsJoint && !value;
                    // Wipe out signature if unchecked.
                    if (!value && $scope.LoanAcceptance.CoApplicantSignature !== undefined && newValue !== undefined && !$scope.LoanAcceptance.CoApplicantSignature.Submitted) {
                        $scope.LoanAcceptance.CoApplicantSignature.Data = null;
                        var sigPad = $("#sigCoApplicantSignature").signaturePad();
                        if (sigPad != null) {
                            sigPad.clearCanvas();
                        }
                    }
                });
                $scope.$watch('LoanAgreementModel.ApplicantAcknowledged', function (newValue) {
                    var value = newValue === undefined ? false : newValue;
                    $scope.ShowApplicantInitials = value;
                    $scope.LoanAcceptance.RequiresApplicantSignatureAcknowledgement = !value;
                    // Wipe out signature if unchecked.
                    if (!value && $scope.LoanAcceptance.ApplicantSignature !== undefined && newValue !== undefined && !$scope.LoanAcceptance.ApplicantSignature.Submitted) {
                        $scope.LoanAcceptance.ApplicantSignature.Data = null;
                        var sigPad = $("#sigApplicantSignature").signaturePad();
                        if (sigPad != null) {
                            sigPad.clearCanvas();
                        }
                    }
                });
                if ($scope.LoanAcceptance.ApplicantSignature.Submitted) {
                    $scope.LoanAgreementModel.ApplicantAcknowledged = true;
                }
                if ($scope.LoanAcceptance.CoApplicantSignature.Submitted) {
                    $scope.LoanAgreementModel.CoApplicantAcknowledged = true;
                }
            }
        });
        $scope.getCurrentRoute = function () {
            if (!$route || !$route.current || !$route.current.$$route) {
                return '/Review';
            }
            return $route.current.$$route.originalPath;
        };
        $scope.goToStep = function (url) {
            $location.path(url);
        };
        $scope.isStepActive = function (stepNumber) {
            var activeStep = 1;
            if ($route && $route.current && $route.current.$$route) {
                if ($route.current.$$route.originalPath === '/LoanAgreement') {
                    activeStep = 2;
                }
                if ($route.current.$$route.originalPath === '/Account') {
                    activeStep = 3;
                }
                if ($route.current.$$route.originalPath === '/Verify' || $route.current.$$route.originalPath === '/Processing') {
                    activeStep = 4;
                }
            }
            return stepNumber == activeStep;
        };
        // Navigation + Site Catalyst / Omniture tracking
        $scope.$on('$routeChangeSuccess', function (scope, newRoute, oldRoute) {
            if (!newRoute) {
                return;
            }
            // if we're navigating away from the loan agreement, it's a joint app, and it's partially signed. Persist.
            if (oldRoute && oldRoute.loadedTemplateUrl === 'TabLoanAgreement') {
                if ($scope.LoanAcceptance.IsJoint && $scope.LoanAcceptance.ApplicantSignature.Submitted !== $scope.LoanAcceptance.CoApplicantSignature.Submitted) {
                    if (!$scope.LoggingOut) {
                        $http.post('/Signature/PersistPartial', {
                            applicationId: $scope.LoanAcceptance.ApplicationId
                        });
                    }
                }
            }
            if (newRoute.loadedTemplateUrl === 'TabReview') {
                if ($scope.LoanAcceptance && $scope.LoanAcceptance.LoanTerms) {
                    SC.prefunding.reviewLoanTerms($scope.LoanAcceptance.LoanTerms.LoanAmount, $scope.LoanAcceptance.LoanTerms.LoanTerm, $scope.LoanAcceptance.LoanTerms.PurposeOfLoan, $scope.LoanAcceptance.LoanTerms.PaymentMethod);
                }
                else {
                    // LoanAcceptance.LoanTerms is not loaded yet, set a watch to fire once it is loaded
                    var listener = $scope.$watch('LoanAcceptance.LoanTerms', function () {
                        if ($scope.LoanAcceptance && $scope.LoanAcceptance.LoanTerms) {
                            SC.prefunding.reviewLoanTerms($scope.LoanAcceptance.LoanTerms.LoanAmount, $scope.LoanAcceptance.LoanTerms.LoanTerm, $scope.LoanAcceptance.LoanTerms.PurposeOfLoan, $scope.LoanAcceptance.LoanTerms.PaymentMethod);
                            listener(); // clears the watch
                        }
                    });
                }
            }
            if (newRoute.loadedTemplateUrl === 'TabAccount') {
                SC.prefunding.accountSetup();
                $timeout(function () {
                    $(document).foundation();
                });
            }
            if (newRoute.loadedTemplateUrl === 'TabVerify') {
                SC.prefunding.verification();
            }
            if (newRoute.loadedTemplateUrl === 'TabLoanAgreement') {
                SC.prefunding.signLoanAgreement();
            }
            if (newRoute.loadedTemplateUrl === 'TabChangeLoanTerms') {
                $timeout(function () {
                    $("ul.grid-cell-adjust").gridAdjust(); // because of IE8
                    $(document).foundation();
                    $('a[data-reveal-id=RatesModal]').on('click', function () {
                        $('#RatesModal').foundation('open');
                        return false;
                    });
                });
                SC.appStatus.changeLoanTermOffer();
            }
        });
        // NLTR
        NLTRService.setUp($scope, '/appstatus/nltrapproved', '/appstatus/refresh');
        $scope.goBack = function () {
            $location.path('/Review');
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
                        $window.location.href = '/appstatus/nltrapproved';
                        return;
                    }
                    if (result.IsAutoDeclined) {
                        $window.location.href = '/appstatus/refresh';
                        return;
                    }
                    // else
                    $scope.ChangeLoanTermsError = null;
                    $location.path('/ConfirmChangeLoanTerms');
                }
                else if (!result || result.IsStale) {
                    $scope.ChangeLoanTermsError = $sce.trustAsHtml('There was an error submitting your request. Please <a href="/appstatus/refresh">click here</a> to log in and try again.');
                }
                else {
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
                    $window.location.href = '/appstatus/refresh';
                }
                else {
                    $scope.ErrorMessage = result && (result.ErrorMessage || "We're sorry, but there was an error submitting your request");
                    $scope.Waiting = false;
                }
            });
        };
        $scope.LoanOffer = {};
        $http.get('/services/GetLatestLoanTermRequest/')
            .success(function (response) {
            $scope.LoanOffer.LoanAmount = response.MaxAmount;
            $scope.LoanOffer.TermMonths = response.TermMonths;
        });
        // Loan Agreement + signatures
        $scope.$watchCollection('[LoanAcceptance.IsPartiallySigned,LoanAcceptance.IsSigned]', function () {
            if ($scope.LoanAcceptance.IsPartiallySigned) {
                $('.navbar-container .navbar-navigation-header-actions .sign-out-action a').on('click', function (e) {
                    e.preventDefault();
                    $('#ConfirmSignOutPartialSignature').foundation('open');
                    return false;
                }).attr('href', '');
            }
            else if ($scope.LoanAcceptance.IsSigned) {
                $('.navbar-container .navbar-navigation-header-actions .sign-out-action a').on('click', function (e) {
                    e.preventDefault();
                    $('#ConfirmSignOut').foundation('open');
                    return false;
                }).attr('href', '');
            }
        });
        $scope.persistPartialSignature = function (e) {
            $scope.LoggingOut = true;
            $http.post('/Signature/PersistPartial', {
                applicationId: $scope.LoanAcceptance.ApplicationId
            }).success(function () {
                $window.location.href = '/signin/logout';
                $scope.LoggingOut = false;
            });
            return false;
        };
        $scope.provideSecondSignature = function () {
            $location.path('/LoanAgreement');
            $location.hash('Signatures');
            $anchorScroll();
        };
        $scope.allSignaturesAreEmpty = function () {
            var appSigned = $scope.LoanAcceptance.ApplicantSignature !== undefined && !$scope.LoanAcceptance.RequiresApplicantSignatureAcknowledgement &&
                ($scope.LoanAcceptance.ApplicantSignature.UsingScriptFont ||
                    $scope.LoanAcceptance.ApplicantSignature.Submitted ||
                    ($.isArray($scope.LoanAcceptance.ApplicantSignature.Data) && $scope.LoanAcceptance.ApplicantSignature.Data.length > 10)), coAppSigned = $scope.LoanAcceptance.CoApplicantSignature !== undefined && !$scope.LoanAcceptance.RequiresCoApplicantSignatureAcknowledgement &&
                ($scope.LoanAcceptance.CoApplicantSignature.UsingScriptFont ||
                    $scope.LoanAcceptance.CoApplicantSignature.Submitted ||
                    ($.isArray($scope.LoanAcceptance.CoApplicantSignature.Data) && $scope.LoanAcceptance.CoApplicantSignature.Data.length > 10));
            if ($scope.LoanAcceptance.IsJoint) {
                $scope.LoanAcceptance.IsPartiallySigned = (appSigned !== coAppSigned) && (appSigned || coAppSigned);
                if (appSigned && $scope.LoanAcceptance.ApplicantSignature.Submitted) {
                    return !coAppSigned;
                }
                if (coAppSigned && $scope.LoanAcceptance.CoApplicantSignature.Submitted) {
                    return !appSigned;
                }
                return !appSigned && !coAppSigned;
            }
            return !appSigned;
        };
        $scope.submitSignatures = function () {
            if ($scope.LoanAcceptance.ApplicantSignature && !$scope.LoanAcceptance.RequiresApplicantSignatureAcknowledgement &&
                ($scope.LoanAcceptance.ApplicantSignature.UsingScriptFont || ($.isArray($scope.LoanAcceptance.ApplicantSignature.Data) && $scope.LoanAcceptance.ApplicantSignature.Data.length > 0))) {
                submitSignature($scope.LoanAcceptance.ApplicantSignature, false);
            }
            if ($scope.LoanAcceptance.CoApplicantSignature && !$scope.LoanAcceptance.RequiresCoApplicantSignatureAcknowledgement &&
                ($scope.LoanAcceptance.CoApplicantSignature.UsingScriptFont || ($.isArray($scope.LoanAcceptance.CoApplicantSignature.Data) && $scope.LoanAcceptance.CoApplicantSignature.Data.length > 0))) {
                submitSignature($scope.LoanAcceptance.CoApplicantSignature, true);
            }
        };
        // account setup
        $scope.$watch('LoanAcceptance.FundingDate', function () {
            if (!$scope.LoanAcceptance || !$scope.LoanAcceptance.CalendarFundingDates) {
                return;
            }
            var date = new Date($scope.LoanAcceptance.FundingDate), fd = $scope.LoanAcceptance.CalendarFundingDates[date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate()];
            $scope.LoanAcceptance.IsWire = (fd === 'Wire');
        });
        $scope.$watch('LoanAcceptance.FundingAccount.RoutingNumber', function (newValue, oldValue) {
            if (newValue && newValue !== oldValue) {
                SuspendWatchPaymentIsSameAsFunding = false;
                $scope.LoanAcceptance.PaymentIsSameAsFunding = null;
                $http
                    .post('/services/BankingInformation', {
                    routingNumber: $scope.LoanAcceptance.FundingAccount.RoutingNumber
                })
                    .success(function (data, status, headers, config) {
                    if (data && data.BankName && $scope.LoanAcceptance.BrokerageAccount) {
                        $scope.LoanAcceptance.BrokerageAccount.BeneficiaryBankName = data.BankName.substring(0, 17);
                    }
                    if (data && data.RoutingNumberIsNotValidForACHDebits) {
                        $scope.LoanAcceptance.FundingRoutingNumberInvalidForDebits = true;
                    }
                    else {
                        $scope.LoanAcceptance.FundingRoutingNumberInvalidForDebits = false;
                    }
                });
            }
        });
        $scope.$watch('LoanAcceptance.PaymentAccount.RoutingNumber', function (newValue, oldValue) {
            if (newValue && newValue !== oldValue) {
                $http
                    .post('/services/BankingInformation', {
                    routingNumber: $scope.LoanAcceptance.PaymentAccount.RoutingNumber
                })
                    .success(function (data, status, headers, config) {
                    if (data && data.RoutingNumberIsNotValidForACHDebits) {
                        $scope.LoanAcceptance.PaymentAccount.IsRoutingNumberInvalid = true;
                    }
                    else {
                        $scope.LoanAcceptance.PaymentAccount.IsRoutingNumberInvalid = false;
                    }
                });
            }
        });
        $scope.$watch('LoanAcceptance.PaymentIsSameAsFunding', function (newValue, oldValue) {
            if (newValue === null || (newValue === false && SuspendWatchPaymentIsSameAsFunding === false)) {
                $scope.LoanAcceptance.PaymentAccount.IsRoutingNumberInvalid = false;
                $scope.LoanAcceptance.PaymentAccount.RoutingNumber = "";
                $scope.LoanAcceptance.PaymentAccount.AccountNumber = "";
                $scope.LoanAcceptance.PaymentAccount.ConfirmAccountNumber = "";
                $scope.LoanAcceptance.PaymentAccount.IsCheckingAccount = null;
            }
            else {
                if (SuspendWatchPaymentIsSameAsFunding === false) {
                    if ($scope.LoanAcceptance.FundingAccount) {
                        $scope.LoanAcceptance.PaymentAccount.AuthorizedSigner = $scope.LoanAcceptance.FundingAccount.AuthorizedSigner;
                        $scope.LoanAcceptance.PaymentAccount.AccountNumber = $scope.LoanAcceptance.FundingAccount.AccountNumber;
                        $scope.LoanAcceptance.PaymentAccount.ConfirmAccountNumber = $scope.LoanAcceptance.FundingAccount.ConfirmAccountNumber;
                        $scope.LoanAcceptance.PaymentAccount.IsCheckingAccount = $scope.LoanAcceptance.FundingAccount.IsCheckingAccount;
                    }
                    if ($scope.LoanAcceptance.PaymentAccount) {
                        if ($scope.LoanAcceptance.IsWire && $scope.LoanAcceptance.FundingRoutingNumberInvalidForDebits) {
                            $scope.LoanAcceptance.PaymentAccount.IsRoutingNumberInvalid = true;
                            $scope.LoanAcceptance.PaymentAccount.RoutingNumber = "";
                            SuspendWatchPaymentIsSameAsFunding = true;
                            $scope.LoanAcceptance.PaymentIsSameAsFunding = false;
                        }
                        else {
                            $scope.LoanAcceptance.PaymentAccount.IsRoutingNumberInvalid = false;
                            $scope.LoanAcceptance.PaymentAccount.RoutingNumber = $scope.LoanAcceptance.FundingAccount.RoutingNumber;
                            SuspendWatchPaymentIsSameAsFunding = false;
                        }
                    }
                }
            }
        });
        $scope.$watch('LoanAcceptance.FundingAccount.AuthorizedSigner', function (newValue, oldValue) {
            if (newValue && newValue !== oldValue) {
                $scope.LoanAcceptance.Verification.CardholderName = newValue;
            }
        });
        $scope.$watchCollection('[LoanAcceptance.FundingDate,LoanAcceptance.PaymentDayOfMonth]', function () {
            if ($scope.LoanAcceptance && $scope.LoanAcceptance.FundingDate && $scope.LoanAcceptance.PaymentDayOfMonth) {
                $http.post('/Services/BankingInformation/GetFirstPaymentDate', {
                    fundingDate: $scope.LoanAcceptance.FundingDate,
                    dayOfMonth: $scope.LoanAcceptance.PaymentDayOfMonth
                }).success(function (data) {
                    if (data && data.Success) {
                        $scope.LoanAcceptance.FirstPaymentDate = data.FirstPaymentDate;
                    }
                });
            }
        });
        $scope.submitAccountInfo = function () {
            // extra client-side validation
            if (!$scope.LoanAcceptance.FundingDate) {
                $('#FundingDateCalendar').addClass('ng-invalid');
                $location.hash('FundingDateCalendar');
                $anchorScroll();
                $scope.ErrorMessage = "Please select a funding date";
                return;
            }
            $('#FundingDateCalendar').removeClass('ng-invalid');
            $scope.ErrorMessage = null;
            $scope.Waiting = true;
            $http.post('/AppStatus/ValidateBankingInfo', $scope.LoanAcceptance).success(function (result) {
                if (result && result.Success) {
                    // if the skip CC is checked, skip Verify
                    if (result.SkipCreditCardVerification) {
                        $scope.submitFinal();
                        return;
                    }
                    $location.path('/Verify');
                    $scope.Waiting = false;
                    return;
                }
                $scope.Waiting = false;
                // highlight th eproblem field
                if (result && result.ErrorValue) {
                    //$scope.$eval('LoanAcceptance.' + result.ErrorValue + ' = null');
                    if ($('form[name=AccountSetupForm]').find('[ng-model="LoanAcceptance.' + result.ErrorValue + '"]').length) {
                        $('form[name=AccountSetupForm]').find('[ng-model="LoanAcceptance.' + result.ErrorValue + '"]').focus();
                    }
                    else {
                        $scope.$eval('LoanAcceptance.' + result.ErrorValue + ' = null');
                    }
                }
                $scope.ErrorMessage = $sce.trustAsHtml(result.ErrorMessage) || $sce.trustAsHtml('There was an error submitting your account information. Please <a href="/appstatus/refresh">click here</a> to log in and try again.');
            });
        };
        $scope.submitFinal = function () {
            $location.path('/Processing');
            $scope.Waiting = true;
            $scope.LoanAcceptance.NumberOfAttempts = ($scope.LoanAcceptance.NumberOfAttempts || 0) + 1;
            $http.post('/AppStatus/SubmitLoanContract', $scope.LoanAcceptance).success(function (result) {
                if (result && result.Success) {
                    $window.location.href = '/appstatus/refresh';
                    $scope.Waiting = false;
                    return;
                }
                if (result.SignOut) {
                    $window.location.href = '/signin/logout';
                    return;
                }
                $scope.Waiting = false;
                $scope.ErrorMessage = result.ErrorMessage;
                // clear out the problem field
                if (result.ErrorValue) {
                    $scope.$eval('LoanAcceptance.' + result.ErrorValue + ' = null');
                }
                // redirect to a tab, if needed
                if (result.Redirect) {
                    $location.path(result.Redirect);
                }
            });
        };
        $scope.$watch('Waiting', function () {
            $timeout(function () {
                $scope.Waiting = false;
            }, 20000); // after 20 seconds, let them try again if there was some error
        });
        // wisconsin
        $scope.nonApplicantSpouseDataIsRequired = function () {
            if (!$scope.LoanAcceptance || !$scope.LoanAcceptance.NonAppSpouseData || !$scope.LoanAcceptance.NonAppSpouseData.HasNonApplicantSpouseIncome) {
                return false;
            }
            var nonAppSpouseData = $scope.LoanAcceptance.NonAppSpouseData;
            return nonAppSpouseData.HasNonApplicantSpouseIncome &&
                ((nonAppSpouseData.IsJoint && nonAppSpouseData.Primary.HasNonApplicantSpouse === false && nonAppSpouseData.Secondary.HasNonApplicantSpouse === false)
                    ||
                        (!nonAppSpouseData.IsJoint && nonAppSpouseData.Primary.HasNonApplicantSpouse === false));
        };
        $scope.$watch('LoanAcceptance.NonAppSpouseData.Primary.SameAddress', function (newVal, oldVal) {
            if (newVal && newVal !== oldVal) {
                if ($scope.LoanAcceptance.NonAppSpouseData.Primary.SameAddress) {
                    $scope.LoanAcceptance.NonAppSpouseData.Primary.Address = $scope.LoanAcceptance.NonAppSpouseData.PrimaryApplicantAddresss;
                }
                else {
                    $scope.LoanAcceptance.NonAppSpouseData.Primary.Address = {};
                }
            }
        });
        $scope.$watch('LoanAcceptance.NonAppSpouseData.Secondary.SameAddress', function (newVal, oldVal) {
            if (newVal && newVal !== oldVal) {
                if ($scope.LoanAcceptance.NonAppSpouseData.Secondary.SameAddress) {
                    $scope.LoanAcceptance.NonAppSpouseData.Secondary.Address = $scope.LoanAcceptance.NonAppSpouseData.SecondaryApplicantAddresss;
                }
                else {
                    $scope.LoanAcceptance.NonAppSpouseData.Secondary.Address = {};
                }
            }
        });
        // verification
        $scope.$watch('LoanAcceptance.Verification.CardholderName', function (newVal, oldVal) {
            if (newVal && newVal !== oldVal) {
                if ($scope.LoanAcceptance.Verification.SecondaryAddress && newVal === 'SecondaryBorrowerName') {
                    $scope.LoanAcceptance.Verification.Address = $scope.LoanAcceptance.Verification.SecondaryAddress;
                }
                else {
                    $scope.LoanAcceptance.Verification.Address = $scope.LoanAcceptance.Verification.PrimaryAddress;
                }
            }
        });
        $scope.hasValidExpirationDate = function () {
            if ($scope.LoanAcceptance.Verification && $scope.LoanAcceptance.Verification.ExpirationMonth && $scope.LoanAcceptance.Verification.ExpirationYear) {
                $scope.LoanAcceptance.Verification.LastDayOfMonth = new Date($scope.LoanAcceptance.Verification.ExpirationYear, $scope.LoanAcceptance.Verification.ExpirationMonth, 0);
                if ($scope.LoanAcceptance.Verification.LastDayOfMonth > new Date()) {
                    return true;
                }
            }
            return false;
        };
        // requirements madness - to display fields twice. And mirror them. Really.
        $scope.$watch('LoanAcceptance.BrokerageAccount.BeneficiaryRoutingNumber', function (newValue, oldValue) {
            if (newValue !== oldValue && $scope.LoanAcceptance.BrokerageAccount && $scope.LoanAcceptance.FundingAccount) {
                $scope.LoanAcceptance.FundingAccount.RoutingNumber = newValue;
            }
        });
        $scope.$watch('LoanAcceptance.BrokerageAccount.BeneficiaryAccountNumber', function (newValue, oldValue) {
            if (newValue !== oldValue && $scope.LoanAcceptance.BrokerageAccount && $scope.LoanAcceptance.FundingAccount) {
                $scope.LoanAcceptance.FundingAccount.AccountNumber = newValue;
            }
        });
        $scope.$watch('LoanAcceptance.BrokerageAccount.BeneficiaryConfirmAccountNumber', function (newValue, oldValue) {
            if (newValue !== oldValue && $scope.LoanAcceptance.BrokerageAccount && $scope.LoanAcceptance.FundingAccount) {
                $scope.LoanAcceptance.FundingAccount.ConfirmAccountNumber = newValue;
            }
        });
        $scope.$watch('LoanAcceptance.FundingAccount.RoutingNumber', function (newValue, oldValue) {
            if (newValue !== oldValue && $scope.LoanAcceptance.BrokerageAccount && $scope.LoanAcceptance.FundingAccount) {
                $scope.LoanAcceptance.BrokerageAccount.BeneficiaryRoutingNumber = newValue;
            }
        });
        $scope.$watch('LoanAcceptance.FundingAccount.AccountNumber', function (newValue, oldValue) {
            if (newValue !== oldValue && $scope.LoanAcceptance.BrokerageAccount && $scope.LoanAcceptance.FundingAccount) {
                $scope.LoanAcceptance.BrokerageAccount.BeneficiaryAccountNumber = newValue;
            }
        });
        $scope.$watch('LoanAcceptance.FundingAccount.ConfirmAccountNumber', function (newValue, oldValue) {
            if (newValue !== oldValue && $scope.LoanAcceptance.BrokerageAccount && $scope.LoanAcceptance.FundingAccount) {
                $scope.LoanAcceptance.BrokerageAccount.BeneficiaryConfirmAccountNumber = newValue;
            }
        });
    }]);
// signatures - jQuery control
$(document).ready(function () {
    'use strict';
    $('div.sigPad').signaturePad({
        defaultAction: 'drawIt',
        penColour: 'black',
        lineColour: '#E57A00',
        lineWidth: 1,
        lineTop: $('.sigPad canvas').height() - 2
    });
    $('html.touch div.clearButton span').text('Draw above to sign');
    $('html.touch li.drawIt span').text(' if you prefer to sign.');
});
//# sourceMappingURL=LoanAcceptanceController.js.map