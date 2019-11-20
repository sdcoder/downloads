/*globals angular, alert, console, navigator, $*/
/*jslint unparam: true */
// LoanApplication module & controller, handles all steps of the loan app process
angular.module('LoanAppModule', ['ngRoute', 'webStorageModule', 'ls.services', 'ls.filters', 'LightStreamDirectives'])
    .config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        'use strict';
        $routeProvider.caseInsensitiveMatch = true;
        $httpProvider.interceptors.push(['$injector', function ($injector) {
                var globalInterceptorFactory = $injector.get('globalInterceptorFactory');
                return {
                    request: globalInterceptorFactory.requests.lowercase
                };
            }]);
    }])
    .controller('LoanApplicationController', ['$scope', '$log', '$http', '$window', 'ratesService', 'stateService', 'loanAppSessionService', 'staticLookupService', 'validationService', '$rootScope', '$timeout', 'factHistory',
    function ($scope, $log, $http, $window, ratesService, stateService, loanAppSessionService, staticLookupService, validationService, $rootScope, $timeout, factHistory) {
        'use strict';
        var rateQueueCounter = 1;
        $scope.rateRequestQueue = [];
        $scope.rateResponseQueue = [];
        // private functions
        var populateState = function () {
            if ($scope.LoanApp && $scope.LoanApp.ZipCode && $scope.LoanApp.ZipCode.toString().length === 5) {
                stateService.getState($scope.LoanApp).then(function (data) {
                    if ($scope.LoanApplication && $scope.LoanApplication.ZipCode) {
                        $scope.LoanApplication.ZipCode.$setValidity('notFound', data.IsValid);
                        $scope.LoanApplication.ZipCode.$notFound = !data.IsValid;
                        $scope.LoanApp.UserSelectedState = !data.IsValid;
                        if (data.IsValid) {
                            if (data.IsMilitary) {
                                $scope.LoanApplication.ZipCode.$setValidity('military', false);
                                return;
                            }
                            $scope.LoanApplication.ZipCode.$setValidity('military', true);
                            $scope.LoanApp.State = data.State;
                            $scope.LoanApp.FloridaDocStampTax = data.FloridaDocStampTax;
                            // and default the primary applicant's information
                            $scope.LoanApp.PrePopulatedCity = data.City;
                            $scope.LoanApp.Applicants[0].Residence.Address.City = data.City;
                            $scope.LoanApp.Applicants[0].Residence.Address.State = data.State;
                            $scope.LoanApp.Applicants[0].Residence.Address.ZipCode = data.ZipCode;
                        }
                        else {
                            $scope.LoanApp.State = 'NotSelected';
                        }
                    }
                });
            }
        }, getTotalEmploymentIncome = function (loanApp) {
            var totalIncome = 0;
            if (loanApp && Array.isArray(loanApp.Applicants)) {
                angular.forEach(loanApp.Applicants, function (applicant) {
                    if (applicant.Occupation && applicant.Occupation.Employer && applicant.Occupation.Employer.GrossAnnualIncome) {
                        totalIncome += parseFloat(applicant.Occupation.Employer.GrossAnnualIncome);
                    }
                });
            }
            return totalIncome;
        }, getTotalApplicationOtherIncome = function (loanApp) {
            var totalOtherIncome = 0;
            if (loanApp && Array.isArray(loanApp.ApplicationOtherIncome) === true) {
                angular.forEach(loanApp.ApplicationOtherIncome, function (otherIncome) {
                    if (otherIncome.Amount) {
                        totalOtherIncome += parseFloat(otherIncome.Amount);
                    }
                });
            }
            return totalOtherIncome;
        }, getTotalIncome = function (loanApp) {
            var income = getTotalEmploymentIncome(loanApp);
            income += getTotalApplicationOtherIncome(loanApp);
            return income;
        }, areAllApplicantsRetired = function () {
            var allRetired = false, employedStatuses = ['Student', 'Homemaker', 'Retired', 'NotEmployed'];
            // Is primary applicant not employed?
            if ($scope.LoanApp.Applicants[0].Occupation.Type !== 'undefined'
                && $scope.LoanApp.Applicants[0].Occupation.Type !== 'NotSelected'
                && $.inArray($scope.LoanApp.Applicants[0].Occupation.Type, employedStatuses) !== -1) {
                allRetired = true;
            }
            if (allRetired === true && $scope.LoanApp.ApplicationType === 'Joint') {
                if ($scope.LoanApp.Applicants[1].Occupation.Type !== 'undefined'
                    && $scope.LoanApp.Applicants[1].Occupation.Type !== 'NotSelected'
                    && $.inArray($scope.LoanApp.Applicants[1].Occupation.Type, employedStatuses) !== -1) {
                    allRetired = true;
                }
                else {
                    allRetired = false;
                }
            }
            return allRetired;
        }, enableAllIncomeTypeOptions = function () {
            $('#OtherIncome0Type option, #OtherIncome1Type option, #OtherIncome2Type option, #OtherIncome3Type option').prop('disabled', false);
            $scope.updateWisconsinOtherIncomeType();
        }, clearApplicationOtherIncome = function () {
            var i;
            for (i = 0; i < 5; i += 1) {
                $scope.LoanApp.ApplicationOtherIncome[i] = {
                    Amount: null,
                    IncomeType: 'NotSelected'
                };
            }
            enableAllIncomeTypeOptions();
        }, 
        // Private that sets the state of the HasOtherAnnualIncome and OtherAnnualIncomeRequired flags
        // that control the Application Other Income UI.
        setOtherIncomeRequired = function () {
            // is other income required?
            if (areAllApplicantsRetired() === true) {
                $scope.LoanApp.OtherAnnualIncomeRequired = true;
                // Force non-employed applicants to enter income
                $scope.LoanApp.HasOtherAnnualIncome = 'Yes';
            }
            else {
                $scope.LoanApp.OtherAnnualIncomeRequired = false;
            }
            $scope.otherAnnualIncomeTypeChange();
        }, initOtherAnnualIncome = function () {
            setOtherIncomeRequired();
            // check if have other income amounts. If so, map values for frequency amounts (for retired applicants)
            // and default the HasOtherAnnualIncome model value to 'Yes'
            if (Array.isArray($scope.LoanApp.ApplicationOtherIncome)) {
                $.each($scope.LoanApp.ApplicationOtherIncome, function (idx, value) {
                    if (typeof value === 'object' && typeof value.Amount !== 'undefined' && Number(value.Amount) > 0) {
                        value.FrequencyAmount = value.Amount;
                        value.Frequency = (value.Frequency && value.Frequency.length > 0) ? value.Frequency : 'Annual';
                        $scope.LoanApp.HasOtherAnnualIncome = 'Yes';
                    }
                });
            }
            $scope.disableSelectedIncomeType();
        }, clearErrorFields = function () {
            $scope.LoanApp.ErrorMessage = null;
            $scope.LoanApp.ErrorFields = null;
            $scope.LoanApp.AlertMessage = null;
        }, clearInvalidFields = function () {
            // Check for invalid DOB, clear value if invalid
            var i;
            for (i = 0; i < $scope.LoanApp.Applicants.length; i++) {
                var dt = new Date($scope.LoanApp.Applicants[i].DateOfBirth);
                var years = ~~((Date.now() - dt) / (31557600000));
                if (years < 18) {
                    $scope.LoanApp.Applicants[i].DateOfBirth = '';
                }
            }
        }, 
        // "Same as residence address" logic
        copyPropertyValuesToHMDA = function () {
            $scope.LoanApp.HmdaComplianceProperty.Address.AddressLine = $scope.LoanApp.Applicants[0].Residence.Address.AddressLine;
            $scope.LoanApp.HmdaComplianceProperty.Address.City = $scope.LoanApp.Applicants[0].Residence.Address.City;
            $scope.LoanApp.HmdaComplianceProperty.Address.State = $scope.LoanApp.Applicants[0].Residence.Address.State;
            $scope.LoanApp.HmdaComplianceProperty.Address.ZipCode = $scope.LoanApp.Applicants[0].Residence.Address.ZipCode;
            $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit = {};
            if ($scope.LoanApp.Applicants[0].Residence.Address.SecondaryUnit) {
                $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Type = $scope.LoanApp.Applicants[0].Residence.Address.SecondaryUnit.Type;
                $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Value = $scope.LoanApp.Applicants[0].Residence.Address.SecondaryUnit.Value;
            }
            if ($scope.LoanApp.Applicants[0].Residence.Ownership === 'Own') {
                $scope.LoanApp.HmdaComplianceProperty.OccupancyType = 'OwnerOccupied';
            }
        }, 
        // for the rate calculator
        getRatesForRateTable = function (errorProcessing) {
            var queueId = getQueueId();
            startRatesResponseTimer();
            ratesService.getRates({
                QueueId: queueId,
                ApplicationId: $scope.LoanApp && ($scope.LoanApp.ApplicationId),
                PurposeOfLoan: $scope.PurposeOfLoan || $scope.LoanApp.PurposeOfLoan.Type,
                Discount: $scope.LoanApp && ($scope.LoanApp.Discount || ''),
                RateLockDate: $scope.LoanApp && ($scope.LoanApp.RateLockDate),
                State: $scope.LoanApp.State || '',
                CoApplicantState: $scope.LoanApp.Applicants[1].Residence.Address.State || '',
                IsSuntrustApplication: $scope.LoanApp.IsSuntrustApplication
            }).then(function (data) {
                $scope.rateResponseQueue.push({
                    queueId: data.queueId,
                    data: data,
                    postProcessing: function (data) {
                        $scope.rates = data;
                        $scope.$apply();
                    }
                });
            }, function (data) {
                $scope.rateResponseQueue.push({
                    queueId: data.queueId,
                    data: data,
                    errorProcessing: errorProcessing
                });
            });
        }, 
        // logic for determining if occupation fields are dirty
        haveAnyOccupationData = function (index) {
            var occupation = $scope.LoanApp.Applicants[index].Occupation;
            if (occupation.OccupationDescription) {
                return true;
            }
            if (occupation.Employer) {
                if (occupation.Employer.EmployerName) {
                    return true;
                }
                if (occupation.Employer.Address) {
                    if (occupation.Employer.Address.AddressLine) {
                        return true;
                    }
                    if (occupation.Employer.Address.SecondaryUnit && occupation.Employer.Address.SecondaryUnit.Type && occupation.Employer.Address.SecondaryUnit.Type !== 'NotSelected') {
                        return true;
                    }
                    if (occupation.Employer.Address.SecondaryUnit && occupation.Employer.Address.SecondaryUnit.Value) {
                        return true;
                    }
                    if (occupation.Employer.Address.City) {
                        return true;
                    }
                    if (occupation.Employer.Address.State && occupation.Employer.Address.State !== 'NotSelected') {
                        return true;
                    }
                    if (occupation.Employer.Address.ZipCode) {
                        return true;
                    }
                }
                if (occupation.Employer.PhoneNumber &&
                    (occupation.Employer.PhoneNumber.AreaCode ||
                        occupation.Employer.PhoneNumber.CentralOfficeCode ||
                        occupation.Employer.PhoneNumber.LineNumber ||
                        occupation.Employer.PhoneNumber.Extension)) {
                    return true;
                }
                if (occupation.Employer.TimeWithEmployer && occupation.Employer.TimeWithEmployer.Years) {
                    return true;
                }
                if (occupation.Employer.TimeWithEmployer && occupation.Employer.TimeWithEmployer.Months) {
                    return true;
                }
                if (occupation.Employer.GrossAnnualIncome && occupation.Employer.GrossAnnualIncome > 0) {
                    return true;
                }
            }
            return false;
        }, startRatesResponseTimer = function () {
            var intervalId = $window.setInterval(function () {
                $scope.processRatesQueue(intervalId);
            }, 100);
        }, getQueueId = function () {
            var queueId = rateQueueCounter++;
            $scope.rateRequestQueue.push(queueId);
            return queueId;
        };
        $scope.processRatesQueue = function (intervalId) {
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
        };
        var staticLookups = function () {
            staticLookupService.getStaticLookups()
                .then(function (data) {
                $scope.countries = data.countries;
            });
        }(); // self-executing function to get static lookups for controller on initialization
        var setDefaultResidency = function (applicant, newVal, oldVal) {
            var isUsCitizen = $scope.isNonUsCitizen(newVal) == false;
            var wasUsCitizen = $scope.isNonUsCitizen(oldVal) == false;
            var wasCleared = oldVal != null && oldVal.length == 0;
            if (isUsCitizen) {
                applicant.Residence.IsNonResident = "false";
            }
            else if (wasUsCitizen || wasCleared) {
                applicant.Residence.IsNonResident = null; // de-select
            }
        };
        // start of $scope or "public" variables and functons
        $scope.currentTabNumber = 0;
        $scope.isNonUsCitizen = function (selectedCitizenship) {
            if (selectedCitizenship == null || selectedCitizenship.length == 0) {
                return null;
            }
            return selectedCitizenship.indexOf("UnitedStates") == -1;
        };
        $scope.isCitizenshipSelected = function (applicant) {
            return applicant.Residence.Citizenships != null &&
                applicant.Residence.Citizenships.length > 0;
        };
        $scope.validateEmail = function (applicantIndex) {
            if (!$scope.LoanApp.Applicants[applicantIndex].EmailValidation) {
                $scope.LoanApp.Applicants[applicantIndex].EmailValidation = {};
            }
            if (applicantIndex === 0 && $scope.LoanApplication && $scope.LoanApplication.ApplicantEmailAddress) {
                $scope.LoanApplication.ApplicantEmailAddress.$setValidity("Invalid", true);
            }
            if (applicantIndex === 1 && $scope.LoanApplication && $scope.LoanApplication.CoApplicantEmailAddress) {
                $scope.LoanApplication.CoApplicantEmailAddress.$setValidity("Invalid", true);
            }
            // Regex validation
            var regEx = /^(?!.*([\.])\1{1})([0-9a-zA-Z]([-\.\+\w]*[0-9a-zA-Z_])*@((?!-)[A-Za-z0-9-\.]{1,63}\.){1}[a-zA-Z]{2,9})$/;
            if ($scope.LoanApp.Applicants[applicantIndex].EmailAddress
                && !regEx.test($scope.LoanApp.Applicants[applicantIndex].EmailAddress)) {
                $scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed = true;
                if (applicantIndex === 0 && $scope.LoanApplication && $scope.LoanApplication.ApplicantEmailAddress) {
                    $scope.LoanApplication.ApplicantEmailAddress.$setValidity("Invalid", false);
                }
                else if (applicantIndex === 1 && $scope.LoanApplication && $scope.LoanApplication.CoApplicantEmailAddress) {
                    $scope.LoanApplication.CoApplicantEmailAddress.$setValidity("Invalid", false);
                }
                return;
            }
            if ($scope.LoanApp.Applicants[applicantIndex].EmailAddress) {
                if (!$scope.LoanApp.Applicants[applicantIndex].EmailValidation.lastValidatedEmail) { // only validating once
                    $scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating = true;
                    validationService
                        .getEmailValidation($scope.LoanApp.Applicants[applicantIndex].EmailAddress)
                        .then(function success(response) {
                        $scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating = false;
                        if (response.isSuccessful && !response.isUnknown) {
                            $scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed = !response.isEmailValid;
                            $scope.LoanApp.Applicants[applicantIndex].EmailValidation.lastValidatedEmail = $scope.LoanApp.Applicants[applicantIndex].EmailAddress;
                        }
                    }, function error() {
                        $scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating = false;
                    });
                }
            }
            if ($scope.LoanApp.Applicants[applicantIndex].EmailValidation.lastValidatedEmail !=
                $scope.LoanApp.Applicants[applicantIndex].EmailAddress) {
                $scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed = false;
            }
        };
        $scope.validateReferralEmail = function () {
            if (!$scope.LoanApp.ReferralEmailValidation) {
                $scope.LoanApp.ReferralEmailValidation = {};
            }
            // Regex validation 
            if ($scope.LoanApplication && $scope.LoanApplication.ApplicantEmailAddress) {
                $scope.LoanApplication.ApplicantEmailAddress.$setValidity("Invalid", true);
            }
            var regEx = /^(?!.*([\.])\1{1})([0-9a-zA-Z]([-\.\+\w]*[0-9a-zA-Z_])*@((?!-)[A-Za-z0-9-\.]{1,63}\.){1}[a-zA-Z]{2,9})$/;
            if ($scope.LoanApp.ApplicantEmailAddress
                && !regEx.test($scope.LoanApp.ApplicantEmailAddress)) {
                $scope.LoanApp.ReferralEmailValidation.hasValidationFailed = true;
                if ($scope.LoanApplication && $scope.LoanApplication.ApplicantEmailAddress) {
                    $scope.LoanApplication.ApplicantEmailAddress.$setValidity("Invalid", false);
                }
                return;
            }
            if ($scope.LoanApp.ApplicantEmailAddress) {
                if (!$scope.LoanApp.ReferralEmailValidation.lastValidatedEmail) {
                    $scope.LoanApp.ReferralEmailValidation.isValidating = true;
                    validationService
                        .getEmailValidation($scope.LoanApp.ApplicantEmailAddress)
                        .then(function success(response) {
                        $scope.LoanApp.ReferralEmailValidation.isValidating = false;
                        if (response.isSuccessful && !response.isUnknown) {
                            $scope.LoanApp.ReferralEmailValidation.hasValidationFailed = !response.isEmailValid;
                            $scope.LoanApp.ReferralEmailValidation.lastValidatedEmail = $scope.LoanApp.ApplicantEmailAddress;
                        }
                    }, function error() {
                        $scope.LoanApp.ReferralEmailValidation.isValidating = false;
                    });
                }
            }
            if ($scope.LoanApp.ReferralEmailValidation.lastValidatedEmail != $scope.LoanApp.ApplicantEmailAddress) {
                $scope.LoanApp.ReferralEmailValidation.hasValidationFailed = false;
            }
        };
        $scope.canGetRates = function () {
            return $scope.LoanApp &&
                $scope.LoanApp.PurposeOfLoan &&
                $scope.LoanApp.PurposeOfLoan.Type &&
                $scope.LoanApp.PurposeOfLoan.Type !== 'NotSelected';
        };
        $scope.getRates = function (additionalProcessing, errorProcessing) {
            if ($scope.canGetRates()) {
                var queueId = getQueueId();
                startRatesResponseTimer();
                return ratesService.getRates({
                    QueueId: queueId,
                    PurposeOfLoan: $scope.LoanApp.PurposeOfLoan.Type,
                    LoanAmount: $scope.LoanApp.LoanAmount,
                    LoanTermMonths: $scope.LoanApp.LoanTermMonths,
                    PaymentMethod: $scope.LoanApp.LoanPaymentType || 'AutoPay',
                    ZipCode: $scope.LoanApp.ZipCode || '',
                    State: $scope.LoanApp.State || '',
                    CoApplicantState: $scope.LoanApp.Applicants[1].Residence.Address.State || '',
                    Discount: $scope.LoanApp.Discount || '',
                    ApplicationId: $scope.LoanApp.ApplicationId,
                    RateLockDate: $scope.LoanApp.RateLockDate,
                    IsSuntrustApplication: $scope.LoanApp.IsSuntrustApplication,
                    IsAddCoApp: $scope.LoanApp.IsAddCoApplicant
                }).then(function (data) {
                    $scope.rateResponseQueue.push({
                        queueId: data.queueId,
                        data: data,
                        postProcessing: function (data) {
                            $scope.LoanAppRates = data;
                            $scope.rates = data;
                            if (data && data.Rate && data.ProductRate) {
                                $scope.LoanApp.InterestRate = (data.ProductRate * 100).toPrecision(5);
                                $scope.LoanApp.InterestRateFrom = (data.Rate.Min * 100).toPrecision(5);
                                $scope.LoanApp.InterestRateTo = (data.Rate.Max * 100).toPrecision(5);
                                $scope.LoanApp.RateLockDate = data.RateLockDate;
                                $scope.LoanAmountInvalidated = false;
                                $scope.LoanTermInvalidated = false;
                            }
                            else if ($scope.LoanApp && $scope.LoanApp.LoanAmount && data && (data.MinLoanAmount > $scope.LoanApp.LoanAmount || data.MaxLoanAmount < $scope.LoanApp.LoanAmount)) {
                                $scope.LoanAmountInvalidated = true;
                            }
                            else if ($scope.LoanApp && $scope.LoanApp.LoanAmount && data && (data.MinTerm > $scope.LoanApp.LoanTermMonths || data.MaxTerm < $scope.LoanApp.LoanTermMonths)) {
                                $scope.LoanTermInvalidated = true;
                            }
                            $scope.$apply();
                            additionalProcessing();
                        }
                    });
                }, function (data) {
                    $scope.rateResponseQueue.push({
                        queueId: data.queueId,
                        data: data,
                        errorProcessing: errorProcessing
                    });
                });
            }
        };
        $scope.$watchCollection('[LoanApp.PurposeOfLoan.Type, LoanApp.LoanAmount, LoanApp.LoanTermMonths, LoanApp.LoanPaymentType, LoanApp.ZipCode, LoanApp.State, LoanApp.Discount, LoanApp.Applicants[0].Residence.Address.State, LoanApp.Applicants[1].Residence.Address.State]', function (is, was) {
            $scope.getRates(function () {
                //Added the below to fix bug 38568. What was happening was the min and max attributes get set based on the loan type
                //that is selected. If the loan amount value was outside of the min and max it was undefined. LoanApp.LoanAmount will only
                //have the loan amount value if it's in the valid range. You switch the loan type to something else, and even though the visible
                //loan amount is within the valid range, the value behind the scenes is undefined until it gets modified by the user.
                //Same problem here:
                //http://stackoverflow.com/questions/15656617/validation-not-triggered-when-data-binding-a-number-inputs-min-max-attributes
                if (is[0] !== was[0] && $scope.LoanApp.LoanAmount == undefined) {
                    var modelValue = $scope.LoanApplication.LoanAmount.$modelValue;
                    var viewValue = $scope.LoanApplication.LoanAmount.$viewValue;
                    if (modelValue)
                        $scope.LoanApp.LoanAmount = modelValue;
                    else if (viewValue)
                        $scope.LoanApp.LoanAmount = viewValue;
                }
            });
        });
        var rateDisclaimer = 'Rate is 0.50 higher with Invoice. AutoPay discount available only when selected prior to loan funding. With AutoPay, automated monthly payments can be made from an account with any bank or credit union in the United States, as long as the account permits ACH withdrawals.';
        $scope.rateDisclaimer = rateDisclaimer;
        $scope.$watch('LoanApp.LoanPaymentType', function (n, o) {
            if (n == 'Invoice')
                $scope.rateDisclaimer = 'Rate 0.50 pts higher with Invoice.';
            else
                $scope.rateDisclaimer = rateDisclaimer;
        });
        $scope.$watch('LoanApp.Applicants[0].Residence.Address.ZipCode', function (newVal, oldVal) {
            if (newVal === oldVal) {
                return;
            }
            if ($scope.LoanApp && newVal && newVal.length === 5) {
                stateService.getState({
                    ZipCode: $scope.LoanApp.Applicants[0].Residence.Address.ZipCode,
                    LoanAmount: $scope.LoanApp.LoanAmount
                }).then(function (data) {
                    if ($scope.LoanApplication && $scope.LoanApplication.ApplicantResidenceAddressZipCode) {
                        $scope.LoanApplication.ApplicantResidenceAddressZipCode.$setValidity('notFound', data.IsValid);
                        $scope.LoanApplication.ApplicantResidenceAddressZipCode.$notFound = !data.IsValid;
                        $scope.LoanApp.UserSelectedState = !data.IsValid;
                        if (data.IsValid) {
                            if (data.IsMilitary) {
                                $scope.LoanApplication.ApplicantResidenceAddressZipCode.$setValidity('military', false);
                                return;
                            }
                            $scope.LoanApplication.ApplicantResidenceAddressZipCode.$setValidity('military', true);
                            $scope.LoanApp.ZipCode = $scope.LoanApp.Applicants[0].Residence.Address.ZipCode;
                            $scope.LoanApp.State = data.State;
                            $scope.LoanApp.FloridaDocStampTax = data.FloridaDocStampTax;
                            // and default the primary applicant's information
                            if ($scope.LoanApp.Applicants[0].Residence.Address.City === $scope.LoanApp.PrePopulatedCity) {
                                $scope.LoanApp.Applicants[0].Residence.Address.City = data.City;
                            }
                            $scope.LoanApp.Applicants[0].Residence.Address.State = data.State;
                        }
                        else {
                            $scope.LoanApp.Applicants[0].Residence.Address.State = 'NotSelected';
                        }
                    }
                });
            }
        });
        // Getting the state on the Loan Information tab
        $scope.$watch('LoanApp.ZipCode', function (newVal, oldVal) {
            if (newVal === oldVal) {
                return;
            }
            populateState();
        });
        $scope.$watchCollection('[LoanApp.LoanAmount, LoanApp.Applicants[0].Residence.Address.State, LoanApp.Applicants[1].Residence.Address.State]', function () {
            if ($scope.LoanApp) {
                //consider the state from either applicant while calculating fees
                if ($scope.LoanApp.Applicants[0].Residence.Address.State === 'Florida' || ($scope.LoanApp.ApplicationType === 'Joint' && $scope.LoanApp.Applicants[1].Residence.Address.State === 'Florida')) {
                    stateService.getStateTax('Florida', $scope.LoanApp.LoanAmount).then(function (data) {
                        $scope.LoanApp.FloridaDocStampTax = data.FloridaDocStampTax;
                    });
                }
            }
        });
        $scope.$watch('LoanApp.State', function (newVal) {
            if (newVal && newVal !== 'NotSelected') {
                if ($scope.LoanApplication && $scope.LoanApplication.ZipCode) {
                    $scope.LoanApplication.ZipCode.$setValidity('notFound', true);
                }
            }
        });
        //////////////////////////////////////////////////////////////////////////////////
        // Application Other Income shenanigans!
        $scope.isWisconsin = function () {
            return $scope.LoanApp.State === 'Wisconsin' ||
                $scope.LoanApp.Applicants[0].Residence.Address.State === 'Wisconsin' ||
                $scope.LoanApp.Applicants[1].Residence.Address.State === 'Wisconsin';
        };
        $scope.updateWisconsinOtherIncomeType = function () {
            // Remove wisconsin specific option
            var $options = $('#OtherIncome0Type, #OtherIncome1Type, #OtherIncome2Type, #OtherIncome3Type').find('option[value=WisconsinSpouseIncome]');
            if ($scope.isWisconsin()
                && $('#OtherIncome0Type, #OtherIncome1Type, #OtherIncome2Type, #OtherIncome3Type').find('option[value=WisconsinSpouseIncome]:selected').length === 0) {
                $options.prop('disabled', false);
                $options.show();
            }
            else {
                $options = $('#OtherIncome0Type, #OtherIncome1Type, #OtherIncome2Type, #OtherIncome3Type').find('option[value=WisconsinSpouseIncome]:not(:selected)');
                $options.prop('disabled', true);
                $options.hide();
                $options.detach();
            }
        };
        $scope.otherAnnualIncomeFrequencyChange = function (index) {
            if ($scope.LoanApp.ApplicationOtherIncome[index].Frequency === "Monthly") {
                $scope.LoanApp.ApplicationOtherIncome[index].Amount = ($scope.LoanApp.ApplicationOtherIncome[index].FrequencyAmount * 12.0).toFixed(2);
            }
            else if ($scope.LoanApp.ApplicationOtherIncome[index].FrequencyAmount && $scope.LoanApp.ApplicationOtherIncome[index].FrequencyAmount > 0) {
                $scope.LoanApp.ApplicationOtherIncome[index].Amount = $scope.LoanApp.ApplicationOtherIncome[index].FrequencyAmount;
            }
            else {
                $scope.LoanApp.ApplicationOtherIncome[index].Amount = undefined;
            }
            return;
        };
        // Act when an other income type is selected
        $scope.otherAnnualIncomeTypeChange = function (index) {
            var i, selectedValue, $allIncomeTypes;
            // Make sure income type description empty if not type if not an 'other' type
            if (index >= 0) {
                selectedValue = $scope.LoanApp.ApplicationOtherIncome[index].IncomeType || 'NotSelected';
                if (selectedValue !== 'Other' && selectedValue !== 'OtherTaxExempt') {
                    $scope.LoanApp.ApplicationOtherIncome[index].OtherIncomeDescription = null;
                }
            }
            enableAllIncomeTypeOptions();
            $scope.disableSelectedIncomeType();
            $scope.updateWisconsinOtherIncomeType();
        };
        $scope.disableSelectedIncomeType = function (index) {
            var i, $allIncomeTypes;
            // Disable selected income type options in income type controls
            for (i = 0; i < 5; i += 1) {
                if ($scope.LoanApp.ApplicationOtherIncome && $scope.LoanApp.ApplicationOtherIncome[i]) {
                    if ($scope.LoanApp.ApplicationOtherIncome[i].IncomeType && $scope.LoanApp.ApplicationOtherIncome[i].IncomeType !== 'NotSelected') {
                        $allIncomeTypes = $('#OtherIncome0Type,#OtherIncome1Type,#OtherIncome2Type,#OtherIncome3Type');
                        $allIncomeTypes.find('option[value=' + $scope.LoanApp.ApplicationOtherIncome[i].IncomeType + ']').prop('disabled', true);
                    }
                }
            }
        };
        // Act when state of the 'yes to annual other income' checkbox is changed
        $scope.yesToOtherAnnualIncomeChange = function () {
            setOtherIncomeRequired();
        };
        $scope.otherAnnualIncomeChanged = function () {
            var bShowModal = false;
            if (Array.isArray($scope.LoanApp.ApplicationOtherIncome)) {
                $.each($scope.LoanApp.ApplicationOtherIncome, function (idx, value) {
                    if (typeof value === 'object' && (value.Amount || (value.IncomeType && value.IncomeType != 'NotSelected'))) {
                        bShowModal = true;
                        return false; //breakout of loop
                    }
                });
            }
            if (bShowModal) {
                $('#ConfirmDeleteOtherIncomeDataModal').foundation('open');
            }
            else {
                clearApplicationOtherIncome();
            }
        };
        $scope.cancelChangeOtherAnnualIncome = function () {
            $scope.LoanApp.HasOtherAnnualIncome = 'Yes';
        };
        $scope.confirmChangeOtherAnnualIncome = function () {
            clearApplicationOtherIncome();
        };
        $scope.areOtherIncomeFieldsDisabled = function () {
            return $scope.LoanApp.HasOtherAnnualIncome === undefined || $scope.LoanApp.HasOtherAnnualIncome === 'No';
        };
        $scope.isOtherIncomeAmountRequired = function () {
            if (typeof $scope.LoanApp.HasOtherAnnualIncome === 'undefined' || $scope.LoanApp.HasOtherAnnualIncome === 'No') {
                return false;
            }
            var bAmountRequired = true;
            if (Array.isArray($scope.LoanApp.ApplicationOtherIncome)) {
                $.each($scope.LoanApp.ApplicationOtherIncome, function (idx, value) {
                    if (typeof value === 'object' && typeof value.Amount !== 'undefined' && Number(value.Amount) > 0) {
                        bAmountRequired = false;
                        return false; //breakout of loop
                    }
                });
            }
            return bAmountRequired;
        };
        $scope.$watchCollection('[LoanApp.Applicants[0].Occupation.Employer.GrossAnnualIncome,LoanApp.Applicants[1].Occupation.Employer.GrossAnnualIncome]', function () {
            setOtherIncomeRequired();
        });
        $scope.$watchCollection('[LoanApp.Applicants[0].Residence.Address.State, LoanApp.Applicants[1].Residence.Address.State]', function (newVal, oldVal) {
            $scope.updateWisconsinOtherIncomeType();
        });
        $scope.$watchCollection('[LoanApp.Applicants[0].Occupation.Type, LoanApp.Applicants[1].Occupation.Type]', function (newTypes, oldTypes) {
            angular.forEach(oldTypes, function (oldType, index) {
                if (oldType) {
                    $scope.LoanApp.Applicants[index].PreviousOccupationType = oldType;
                }
            });
            setOtherIncomeRequired();
            // putting this in a timeout so it can execute after page has been rendered 
            // other income sources are in an ng-if and re-rendered based on selections
            $timeout(function () {
                $scope.updateWisconsinOtherIncomeType();
                $scope.disableSelectedIncomeType();
            });
        });
        // End application other income shenanigans
        //////////////////////////////////////////////////////////////////////////////////
        $scope.$watchCollection('[LoanApp.Applicants[0].Residence.Address.AddressLine,' +
            'LoanApp.Applicants[0].Residence.Address.City,' +
            'LoanApp.Applicants[0].Residence.Address.State,' +
            'LoanApp.Applicants[0].Residence.Address.ZipCode,' +
            'LoanApp.Applicants[0].Residence.Address.SecondaryUnit.Type,' +
            'LoanApp.Applicants[0].Residence.Address.SecondaryUnit.Value,' +
            'LoanApp.Applicants[0].Residence.Ownership]', function () {
            if ($scope.LoanApp.SubjectPropertySameAsResidentAddress) {
                copyPropertyValuesToHMDA();
            }
        });
        // handle "HMDA not submitted" change
        $scope.$watch('LoanApp.ApplicantHmdaData[0].NotSubmitted', function (newVal) {
            if (newVal) {
                $scope.LoanApp.ApplicantHmdaData[0].ApplicantRace = [];
                $scope.LoanApp.ApplicantHmdaData[0].Gender = null;
                $scope.LoanApp.ApplicantHmdaData[0].Ethnicity = null;
            }
        });
        $scope.$watch('LoanApp.ApplicantHmdaData[1].NotSubmitted', function (newVal) {
            if (newVal) {
                $scope.LoanApp.ApplicantHmdaData[1].ApplicantRace = [];
                $scope.LoanApp.ApplicantHmdaData[1].Gender = null;
                $scope.LoanApp.ApplicantHmdaData[1].Ethnicity = null;
            }
        });
        // Auto set residency flag for US citizens
        $scope.$watchCollection('LoanApp.Applicants[0].Residence.Citizenships', function (newVal, oldVal) {
            if (newVal) {
                setDefaultResidency($scope.LoanApp.Applicants[0], newVal, oldVal);
            }
        });
        $scope.$watchCollection('LoanApp.Applicants[1].Residence.Citizenships', function (newVal, oldVal) {
            if (newVal) {
                setDefaultResidency($scope.LoanApp.Applicants[1], newVal, oldVal);
            }
        });
        // handle "same as resident address" changes
        $scope.$watch('LoanApp.SubjectPropertySameAsResidentAddress', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (newVal) {
                    copyPropertyValuesToHMDA();
                }
                else if (newVal === false) {
                    $scope.LoanApp.HmdaComplianceProperty.Address.AddressLine = null;
                    $scope.LoanApp.HmdaComplianceProperty.Address.City = null;
                    $scope.LoanApp.HmdaComplianceProperty.Address.State = null;
                    $scope.LoanApp.HmdaComplianceProperty.Address.ZipCode = null;
                    $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit = {};
                    $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Type = 'NotSelected';
                    $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Value = null;
                }
            }
        });
        // mirror address if 'same address' box is checked
        $scope.$watch('LoanApp.CoApplicantSameAddress', function (newVal) {
            if (newVal) {
                $scope.LoanApp.Applicants[1].Residence.Address.AddressLine = $scope.LoanApp.Applicants[0].Residence.Address.AddressLine;
                $scope.LoanApp.Applicants[1].Residence.Address.City = $scope.LoanApp.Applicants[0].Residence.Address.City;
                $scope.LoanApp.Applicants[1].Residence.Address.State = $scope.LoanApp.Applicants[0].Residence.Address.State;
                $scope.LoanApp.Applicants[1].Residence.Address.ZipCode = $scope.LoanApp.Applicants[0].Residence.Address.ZipCode;
                $scope.LoanApp.Applicants[1].Residence.Address.SecondaryUnit = {};
                $scope.LoanApp.Applicants[1].Residence.Address.SecondaryUnit.Type = $scope.LoanApp.Applicants[0].Residence.Address.SecondaryUnit.Type;
                $scope.LoanApp.Applicants[1].Residence.Address.SecondaryUnit.Value = $scope.LoanApp.Applicants[0].Residence.Address.SecondaryUnit.Value;
            }
        });
        $scope.saveAndCheckValidity = function (currentStep, goToUrl) {
            //adding analytics for progress bar
            if (goToUrl.includes('personalinfo')) {
                SC.main.progressBar.personalInfo();
            }
            else if (goToUrl.includes('securityinfo')) {
                SC.main.progressBar.securityInfo();
            }
            else if (goToUrl.includes('confirm')) {
                SC.main.progressBar.confirmAndSubmit();
            }
            else {
                SC.main.progressBar.loanInfo();
            }
            // save the current status of this form. If any are skipped, we will redirect.
            if (!$scope.LoanApp.SkippedSteps) {
                $scope.LoanApp.SkippedSteps = [];
            }
            clearErrorFields();
            clearInvalidFields();
            // check if attempting to skip from the security info page to somewhere
            // else in the app without having first reserved the selected user id
            if (currentStep === 3 && $scope.LoanApp.UserCredentials.UserId === null) {
                $scope.LoanApp.SkippedSteps[currentStep] = false;
                $scope.LoanApp.UserIdNotReserved = true;
            }
            else {
                $scope.LoanApp.SkippedSteps[currentStep] = $scope.LoanApplication.$valid;
            }
            loanAppSessionService.save($scope.LoanApp);
            if (goToUrl) {
                $window.location.href = goToUrl;
                return false;
            }
            return true;
        };
        // save / load / session state
        $scope.saveLoanApp = function () {
            loanAppSessionService.save($scope.LoanApp);
            return true;
        };
        $scope.deleteLoanApp = function () {
            loanAppSessionService.remove();
            // start a new loan app session, preserve discount and purpose of loan
            loanAppSessionService.start(null, $scope.LoanApp.PurposeOfLoan.Type, null, null, null, $scope.LoanApp.Discount);
            return true;
        };
        $scope.loadLoanApp = function () {
            $scope.LoanApp = loanAppSessionService.load();
            if ($scope.LoanApp && $scope.LoanApp.ZipCode && !$scope.LoanApp.State) {
                populateState();
            }
        };
        $scope.initSuntrustApp = function (isReferralMode) {
            // for UI elements
            $scope.SunTrustApp = true;
            $scope.LoanApp.IsSuntrustApplication = true;
            if (isReferralMode !== undefined) {
                $scope.LoanApp.IsReferralMode = isReferralMode;
            }
            $scope.$watch('LoanApp.IsExistingClient', function (value) {
                $scope.LoanApp.SocialSecurityNumber = null;
            });
            // and validate the employee id on any change
            $scope.$watch('LoanApp.TeammateEmployeeId', function (newValue, oldValue) {
                // ignore if validating against officer id's
                if ($('#TeammateEmployeeId').attr('ls-officer-id')) {
                    return;
                }
                if (newValue && newValue !== oldValue) {
                    if ($scope.LoanApp.TeammateEmployeeId && $scope.LoanApp.TeammateEmployeeId.length > 5) {
                        // reset everything
                        $scope.EmployeeLookupWaiting = true;
                        $scope.LoanApp.TeammateEmployeeIdFound = false;
                        $scope.LoanApp.TeammateEmployeeIdLookupFail = false;
                        // hit the server to validate id. This might take a while
                        $http.post('/Services/ValidateEmployeeId', {
                            id: $scope.LoanApp.TeammateEmployeeId
                        }).success(function (result) {
                            // ensure it is not "stale"
                            if (result.EmployeeId !== $scope.LoanApp.TeammateEmployeeId) {
                                return;
                            }
                            $scope.EmployeeLookupWaiting = false;
                            $scope.LoanApp.TeammateIsContractorOrTemporary = null;
                            $scope.LoanApp.EmployeeVerificationResult = result.APIResult || 'Error';
                            if (result && result.Success) {
                                $scope.LoanApp.TeammateEmployeeIdLookupFail = false;
                                $scope.LoanApp.TeammateEmployeeIdFound = result.Found;
                                $scope.LoanApplication.TeammateEmployeeId.$setValidity('valid', result.Found);
                                if (!result.Found) {
                                    $timeout(function () {
                                        $('#TeammateEmployeeId').blur();
                                    });
                                }
                            }
                            else {
                                $scope.LoanApp.TeammateEmployeeIdLookupFail = true;
                                $scope.LoanApp.TeammateEmployeeIdFound = false;
                            }
                        }).error(function () {
                            $scope.EmployeeLookupWaiting = false;
                            $scope.LoanApp.EmployeeVerificationResult = 'Error';
                            $scope.LoanApp.TeammateEmployeeIdLookupFail = true;
                            $scope.LoanApp.TeammateEmployeeIdFound = false;
                        });
                    }
                }
            });
            $scope.$watch('LoanApp.TeammateIsContractorOrTemporary', function () {
                // ignore if validating against officer id's
                if ($('#TeammateEmployeeId').attr('ls-officer-id')) {
                    return;
                }
                if (!$scope.LoanApplication.TeammateEmployeeId) {
                    return;
                }
                if (!$scope.LoanApp.TeammateEmployeeIdLookupFail && !$scope.LoanApp.TeammateEmployeeIdFound && !$scope.LoanApp.TeammateIsContractorOrTemporary) {
                    $scope.LoanApplication.TeammateEmployeeId.$setValidity('valid', false);
                    return;
                }
                if ($scope.LoanApp.TeammateEmployeeIdLookupFail || $scope.LoanApp.TeammateEmployeeIdFound || $scope.LoanApp.TeammateIsContractorOrTemporary) {
                    $scope.LoanApplication.TeammateEmployeeId.$setValidity('valid', true);
                    return;
                }
            });
        };
        // logic for requiring employer fields
        $scope.employerFieldsRequired = function (index) {
            var occupation = $scope.LoanApp.Applicants[index].Occupation;
            if (occupation.Type === 'EmployedBySelf' || occupation.Type === 'EmployedByOther') {
                return true;
            }
            return false;
        };
        // logic for disabling employer fields
        $scope.employerFieldsDisabled = function (index) {
            var occupation = $scope.LoanApp.Applicants[index].Occupation;
            if (occupation.Type === 'Retired' || occupation.Type === 'Student' || occupation.Type === 'Homemaker' || occupation.Type === 'NotEmployed') {
                return true;
            }
        };
        // proceed to next step
        $scope.nextStep = function (nextPage, currentStep, skipRatesCheck) {
            clearErrorFields();
            if ($scope.LoanApp.SkippedSteps && $scope.LoanApplication) {
                $scope.LoanApp.SkippedSteps[currentStep] = $scope.LoanApplication.$valid;
            }
            if (!skipRatesCheck && (!$scope.LoanAppRates || $scope.LoanAppRates.PurposeOfLoan !== $scope.PurposeOfLoan)) {
                if (currentStep === 1) {
                    // We are on the first page.
                    $scope.getRates(function () {
                        if (!$scope.LoanAmountInvalidated && !$scope.LoanTermInvalidated)
                            $scope.nextStep(nextPage, currentStep);
                    }, function () {
                        $scope.nextStep(nextPage, currentStep, true);
                    });
                }
                else {
                    $scope.saveLoanApp();
                    $window.location.href = './gotostep?step=' + 1;
                }
                return;
            }
            if ($scope.LoanAmountInvalidated) {
                $scope.LoanApp.ErrorMessage = "Please check the loan amount before proceeding.";
                $scope.saveLoanApp();
                $window.location.href = './gotostep?step=' + 1;
                return;
            }
            else if ($scope.LoanTermInvalidated) {
                $scope.LoanApp.ErrorMessage = "Please check the loan term before proceeding.";
                $scope.saveLoanApp();
                $window.location.href = './gotostep?step=' + 1;
                return;
            }
            $scope.saveLoanApp();
            $window.location.href = nextPage;
        };
        $scope.workStatusChanged = function (index, idPrefix) {
            var occupation = $scope.LoanApp.Applicants[index].Occupation;
            if ((occupation.Type === 'Retired' || occupation.Type === 'Student' || occupation.Type === 'Homemaker' || occupation.Type === 'NotEmployed') && haveAnyOccupationData(index)) {
                $('#ConfirmDeleteEmploymentDataModal' + idPrefix).foundation('open');
            }
            if (occupation.Type === 'EmployedBySelf') {
                $scope.salaryDescription = '"Annual" (Net) Salary';
            }
            else {
                $scope.salaryDescription = '"Annual" Salary';
            }
        };
        $scope.salaryDescription = '"Annual" Salary';
        $scope.hasSelectedRetiredWorkStatus = function () {
            if ($scope.LoanApp.Applicants[0].Occupation.Type === 'Retired' || $scope.LoanApp.Applicants[1].Occupation.Type === 'Retired') {
                return true;
            }
            return false;
        };
        $scope.confirmChangeWorkStatus = function (index, prefix) {
            var occupationType = $scope.LoanApp.Applicants[index].Occupation.Type;
            $scope.LoanApp.Applicants[index].Occupation = {
                Type: occupationType
            };
            $('#ConfirmDeleteEmploymentDataModal' + prefix).foundation('close');
        };
        $scope.cancelChangeWorkStatus = function (index, prefix) {
            $scope.LoanApp.Applicants[index].Occupation.Type = $scope.LoanApp.Applicants[index].PreviousOccupationType;
            $('#ConfirmDeleteEmploymentDataModal' + prefix).foundation('close');
        };
        // proceed to next step, but save the app as well
        $scope.submitLoanApp = function (submitPage, nextPage, currentStep) {
            var retVal;
            var i;
            if (!currentStep) {
                currentStep = 4;
            }
            if ($scope.LoanApp.SkippedSteps) {
                $scope.LoanApp.SkippedSteps[currentStep] = $scope.LoanApplication.$valid;
            }
            $scope.saveLoanApp();
            // check for skipped steps
            if ($scope.LoanApp.SkippedSteps) {
                for (i = 0; i < 5; i += 1) {
                    if ($scope.LoanApp.SkippedSteps[i] !== undefined && $scope.LoanApp.SkippedSteps[i] === false) {
                        $scope.LoanApp.ErrorMessage = "Please complete this page before submitting your loan application";
                        $scope.saveLoanApp();
                        $window.location.href = './gotostep?step=' + i;
                        retVal = false;
                        return retVal;
                    }
                }
            }
            // if no skipped steps, show loading image, submit app
            if (!$scope.Loading) {
                $scope.Loading = true;
                clearErrorFields();
                $http.post(submitPage, $scope.LoanApp).success(function (result) {
                    $scope.Loading = false;
                    if (result.Success) {
                        // clear out session
                        $scope.deleteLoanApp();
                        //// change the 'back button' behavior, to prevent shenanigans
                        if ($window.history && $window.history.replaceState) {
                            $window.history.replaceState({}, 'Home Page', result.BackButtonLocation || '/');
                        }
                        factHistory.clearHistory();
                        // and redirect to the next / thank you page
                        $window.location.href = nextPage;
                    }
                    else {
                        $scope.LoanApp.ErrorMessage = result.ErrorMessage;
                        $scope.LoanApp.ErrorFields = result.ErrorFields;
                        $scope.saveLoanApp();
                        if (result.Redirect && result.Redirect !== 'None') {
                            $window.location.href = './' + result.Redirect;
                        }
                    }
                }).error(function (data, status, headers, config) {
                    console.log("error happened: status: " + status);
                    console.log("error happened: data: " + data);
                    $scope.Loading = false;
                    $scope.LoanApp.ErrorMessage = "We're sorry, but there was an error submitting your application.";
                });
            }
            return retVal;
        };
        // BEGIN - TEST METHODS
        $scope.testExport = function () {
            $scope.saveLoanApp();
            $('#LoanAppJSONImportButton').hide();
            $('#LoanAppJSON').val(JSON.stringify($scope.LoanApp, null, ' '));
            $('#LoanAppImportExportModal').foundation('open');
        };
        $scope.testImport = function () {
            $('#LoanAppJSONImportButton').show();
            $('#LoanAppImportExportModal').foundation('open');
        };
        $scope.testImportFromJSON = function () {
            var app, json = $('#LoanAppJSON').val();
            try {
                app = JSON.parse(json);
                if (app) {
                    if (app && app.UserCredentials) {
                        app.UserCredentials.UserName = null;
                        app.UserCredentials.UserId = null;
                    }
                    $scope.Discount = null; // error prone, when switching between Suntrust or Native apps
                    $scope.LoanApp = app;
                    $scope.LoanApp.InterestRate = null;
                    $scope.LoanApp.ApplicationId = null;
                    $scope.LoanApp.RateLockDate = null;
                    $scope.saveLoanApp();
                    $('#LoanAppImportExportModal').foundation('close');
                    return;
                }
            }
            catch (e) {
                alert("Parsing error: , " + e);
            }
        };
        // END TEST METHODS 
        // data for checkboxes
        $scope.ApplicantRaces = [
            {
                id: 'AmericanIndianAlaskaNative', text: 'American Indian or Alaska Native'
            },
            {
                id: 'Asian', text: 'Asian'
            },
            {
                id: 'BlackOrAfricanAmerican', text: 'Black or African American'
            },
            {
                id: 'NativeHawaiianOtherPacificIsle', text: 'Native Hawaiian Or Other Pacific Islander'
            },
            {
                id: 'White', text: 'White'
            }
        ];
        // reservation of user id, handled via a service
        $scope.reserveUserId = function (nextPage, currentStep) {
            $scope.saveLoanApp();
            // if we alreayd have a user id, we don't need to fetch it again
            if ($scope.LoanApp.UserCredentials.UserId || $scope.LoanApp.UserCredentials.IsTemporary) {
                $scope.nextStep(nextPage, currentStep);
                return;
            }
            // if null, hit the back end to get a user id
            $http.post('./reserveuserid', $scope.LoanApp.UserCredentials).success(function (result) {
                $scope.LoanApp.UserIdNotReserved = false;
                if (result.Success && result.UserId) {
                    $scope.LoanApp.UserCredentials.UserId = result.UserId;
                    $scope.LoanApp.UserCredentials.IsTemporary = false;
                    $scope.nextStep(nextPage, currentStep);
                }
                else {
                    if (result.TemporaryUserName !== null) {
                        $scope.LoanApp.UserCredentials.IsTemporary = true;
                        $scope.LoanApp.UserCredentials.UserName = result.TemporaryUserName;
                    }
                    else {
                        $scope.LoanApp.ErrorMessage = result.ErrorMessage;
                    }
                }
            }).error(function (data, status, headers, config) {
                if (!navigator.onLine) {
                    $scope.LoanApp.ErrorMessage = 'Your browser or device appears to be offline. Please re-connect and try again.';
                }
                else {
                    $scope.LoanApp.ErrorMessage = 'There was an error reserving your user name. Please try again';
                }
            });
        };
        // for disabling the form if it's not complete
        $scope.formShouldBeDisabled = function () {
            var i;
            if ($scope.currentTabNumber && $scope.LoanApp.SkippedSteps) {
                for (i = 0; i < $scope.currentTabNumber; i += 1) {
                    if ($scope.LoanApp.SkippedSteps[i] !== undefined && $scope.LoanApp.SkippedSteps[i] === false) {
                        return true;
                    }
                }
            }
            return false;
        };
        // for applicant race validation
        $scope.applicantRaceSelected = function (applicantId) {
            if (angular.isArray($scope.LoanApp.ApplicantHmdaData) && typeof $scope.LoanApp.ApplicantHmdaData[applicantId] === 'object') {
                var applicantRace = $scope.LoanApp.ApplicantHmdaData[applicantId].ApplicantRace;
                if (angular.isArray(applicantRace) && applicantRace.length > 0) {
                    return true;
                }
            }
            return false;
        };
        // for time at address / employer 
        $scope.timeFieldIsRequired = function (isRequired, otherField) {
            if (!$scope.$eval(isRequired)) {
                return false;
            }
            var otherValue = $scope.$eval(otherField);
            if (otherValue && otherValue > 0) {
                return false;
            }
            return true;
        };
        $scope.$watch('PurposeOfLoan', function (newVal, oldVal) {
            if (newVal && newVal !== oldVal) {
                getRatesForRateTable(null);
            }
        });
        $scope.$watch('Discount', function (newVal, oldVal) {
            if (newVal && newVal !== oldVal) {
                getRatesForRateTable(null);
            }
        });
        $scope.$watch('LoanApp.PurposeOfLoan.Type', function () {
            $scope.PurposeOfLoan = $scope.LoanApp.PurposeOfLoan.Type;
            var d = new Date();
            d.setTime(d.getTime() + 24 * 60 * 60 * 1000 * 30);
            document.cookie = "LoanPurpose = " + $scope.PurposeOfLoan + ";expires=" + d.toUTCString() + ";" + "path = /;";
            $scope.LoanApp.PurposeOfLoan.IsHomeImprovement = ($scope.LoanApp.PurposeOfLoan.Type === 'HomeImprovement');
        });
        // account services use case - different apps need to be loaded when the user selects a different template from the drop-down
        $scope.$watch('LoanApp.ReApplyApplicationType', function (newVal, oldVal) {
            if (newVal === undefined || newVal === oldVal) {
                return;
            }
            $http.post('/Apply/AccountServices/LoadApp', {
                ReApplyApplicationType: $scope.LoanApp.ReApplyApplicationType,
                ZipCode: $scope.LoanApp.ZipCode,
                PurposeOfLoan: $scope.LoanApp.PurposeOfLoan.Type,
                LoanAmount: $scope.LoanApp.LoanAmount,
                LoanTermMonths: $scope.LoanApp.LoanTermMonths,
                LoanPaymentType: $scope.LoanApp.LoanPaymentType
            }).then(function (response) {
                if (response.data.IsSignOut) {
                    $window.location.href = '/customer-sign-in';
                }
                else if (response.data && response.data.LoanApp) {
                    loanAppSessionService.cleanup(response.data.LoanApp);
                    $scope.LoanApp = response.data.LoanApp;
                    if ($scope.LoanApp.Discount) {
                        getRatesForRateTable(null);
                    }
                }
            });
        });
        $scope.$watch('LoanApp.ApplicationTakenOverPhone', function (newVal, oldVal) {
            if (newVal === undefined || newVal === oldVal) {
                return;
            }
        });
        // inquiry application use case - load a loan app model from the server
        $scope.loadInProgressApp = function (dataURL, ignoreLoanTermMonths, ignoreApplicationType) {
            if ($scope.LoanApp.LoanAppHasBeenPreviouslyLoaded === true) {
                return;
            }
            else {
                $http.post(dataURL).success(function (response) {
                    if (response && response.LoanApp) {
                        loanAppSessionService.cleanup(response.LoanApp);
                        $scope.LoanApp = response.LoanApp;
                        $scope.validateEmail(0);
                        if ($scope.LoanApp.Applicants[1].EmailAddress) {
                            $scope.validateEmail(1);
                        }
                        $scope.LoanApp.LoanAppHasBeenPreviouslyLoaded = true;
                        if (ignoreLoanTermMonths || $scope.LoanApp.LoanTermMonths == 0) {
                            $scope.LoanApp.LoanTermMonths = undefined;
                        }
                        if (ignoreApplicationType) {
                            $scope.LoanApp.ApplicationType = 'NotSelected';
                        }
                    }
                    else if (response && response.Success === false) {
                        $scope.LoanApp.ErrorMessage = response.ErrorMessage;
                        $scope.LoanApp.LoanAppHasBeenPreviouslyLoaded = false;
                    }
                }).error(function () {
                    $scope.LoanApp.ErrorMessage = 'Server error.';
                });
            }
        };
        // data initialization
        $scope.loadLoanApp();
        initOtherAnnualIncome();
        // any errors?
        $scope.broadcastValidate = function () {
            $timeout(function () {
                $rootScope.$broadcast('validate');
                $timeout(function () {
                    $('.ng-invalid').first().focus();
                }, 120);
            });
        };
        if ($scope.LoanApp.ErrorMessage) {
            $scope.broadcastValidate();
        }
        $(document).ready(function () {
            $(function () {
                'use strict';
                if ($scope.formShouldBeDisabled()) {
                    $('#LoanAppFieldset input').attr('disabled', 'disabled');
                    $('#LoanAppFieldset select').attr('disabled', 'disabled');
                }
            });
        });
    }]);
//# sourceMappingURL=loan-application.js.map