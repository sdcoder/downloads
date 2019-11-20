/*globals angular, window, jQuery, $*/
(function () {
    'use strict';
    angular.module('AccountServicesModule', ['ls.services', 'ls.filters', 'LightStreamDirectives', 'ngRoute'])
        .config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
            'use strict';
            $routeProvider.caseInsensitiveMatch = true;
            $httpProvider.interceptors.push(['$injector', function ($injector) {
                    var globalInterceptorFactory = $injector.get('globalInterceptorFactory');
                    return {
                        request: globalInterceptorFactory.requests.lowercase,
                        response: globalInterceptorFactory.responses.maintenanceMode
                    };
                }]);
        }])
        .controller('AccountServicesController', ['$scope', '$http', '$sce', '$window', '$compile', function ($scope, $http, $sce, $window, $compile) {
            var modal = new LightStreamWeb.Frontend.PrintableAjaxModalController("#PrintableModal", "#PrintButton");
            $scope.Model = {};
            $scope.Loading = true;
            // private functions
            var updateApplicationById = function (applicationId, appData, isActive) {
                if (isActive === void 0) { isActive = false; }
                var i;
                for (i = 0; i < $scope.Model.Applications.length; i = i + 1) {
                    if ($scope.Model.Applications[i].ApplicationId === applicationId) {
                        if (appData.PaymentAccountMessage && typeof appData.PaymentAccountMessage !== 'object') {
                            appData.PaymentAccountMessage = $sce.trustAsHtml(appData.PaymentAccountMessage);
                        }
                        $scope.Model.Applications[i] = appData;
                        $scope.Model.Applications[i].Active = isActive;
                        return;
                    }
                }
            }, 
            // refreshAccount: updates data for an account from the server, also updating server-side session objects,
            // and reducing concurrency errors.
            //
            // account - the particular account that just changed
            //
            // successCallback - a callback function for access to the updated account.
            // use it to set properties (typically status messages) before the angular model is updated from server data
            refreshAccount = function (account, successCallback) {
                // and refresh the payment info for this one account
                $http.post('/Account/RefreshAccount', account)
                    .success(function (result) {
                    if (result && result.Success) {
                        if (successCallback) {
                            successCallback(result.FundedAccount);
                        }
                        updateApplicationById(account.ApplicationId, result.FundedAccount, account.Active);
                    }
                })
                    .error(function () {
                    window.location.href = '/appstatus/refresh';
                });
            };
            // accordian
            $scope.setActiveApplication = function (account) {
                account.Active = !account.Active;
                return false;
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // amortization
            $('#PrintAmortizationScheduleModal').on('click', 'a', function () {
                var format = $(this).data('format');
                if (format) {
                    var w;
                    //don't open the new tab for IE when downloading PDF
                    if (format === 'html') {
                        w = $window.open();
                    }
                    else {
                        if (!$window.navigator.msSaveOrOpenBlob) {
                            w = $window.open();
                        }
                    }
                    $http.post('/Account/PrintAmortizationSchedule', {
                        Items: $scope.PrintableAmortizationSchedule,
                        Format: format,
                        ApplicationId: $scope.AmortizationScheduleApplicationId
                    }).success(function (result) {
                        if (format === 'html') {
                            $(w.document.body).html(result);
                        }
                        else {
                            var blob = $scope.dataToBlob(result);
                            if ($window.navigator && $window.navigator.msSaveOrOpenBlob) {
                                $window.navigator.msSaveOrOpenBlob(blob, "AmortizationSchedule.pdf"); //for IE
                            }
                            else {
                                var blobURL = URL.createObjectURL(blob);
                                w.location.href = blobURL;
                            }
                        }
                    }).error(function () { return w.close(); });
                }
            });
            $scope.dataToBlob = function (data) {
                var byteCharacters = atob(data);
                var byteNumbers = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumbers);
                var blob = new Blob([byteArray], {
                    type: 'application/pdf'
                });
                return blob;
            };
            $scope.printAmortizationSchedule = function (data, applicationId) {
                if (typeof data === "string") {
                    data = JSON.parse(data);
                }
                $scope.PrintableAmortizationSchedule = data;
                $scope.AmortizationScheduleApplicationId = applicationId;
                $('#PrintAmortizationScheduleModal').foundation('open');
            };
            $scope.displayMonthlyPaymentAmortizationSchedule = function (account) {
                if (!account.NewMonthlyPaymentAmount) {
                    return;
                }
                $http.post('/Account/AmortizationSchedule', {
                    ctx: account.Ctx,
                    paymentAmount: account.NewMonthlyPaymentAmount
                }).success(function (result) {
                    $('#AmortizationScheduleModal').html(result).foundation('open');
                    $compile($('#AmortizationScheduleModal').contents())($scope);
                    $('#AmortizationScheduleModal a[data-close').on('click', function () {
                        $('#AmortizationScheduleModal').foundation('close');
                    });
                });
            };
            $scope.displayExtraPaymentAmortizationSchedule = function (account) {
                if (!account.ScheduledPaymentAmount) {
                    return;
                }
                $http.post('/Account/AmortizationSchedule', {
                    ctx: account.Ctx,
                    extraPaymentAmount: account.ScheduledPaymentAmount,
                    extraPaymentEffectiveDate: account.ScheduledPaymentDate,
                    paymentAmount: account.NextPaymentAmount
                }).success(function (result) {
                    $('#AmortizationScheduleModal').html(result).foundation('open');
                    $compile($('#AmortizationScheduleModal').contents())($scope);
                    $('#AmortizationScheduleModal a[data-close').on('click', function () {
                        $('#AmortizationScheduleModal').foundation('close');
                    });
                });
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // account nickname
            $scope.editNickname = function (account) {
                if (account.Nickname) {
                    $scope.NicknameEditMode = 'Update';
                    $scope.NewNickname = account.Nickname;
                }
                else {
                    $scope.NicknameEditMode = 'Create';
                }
                $('#EditNicknameModal').data('account', account).foundation('open');
            };
            $scope.submitNickname = function () {
                // stash the active account with the modal
                var $modal = $('#EditNicknameModal'), account = $modal.data('account');
                //Clear existing errors
                $scope.NicknameErrorMessage = '';
                if (!$scope.NewNickname) {
                    $scope.NicknameErrorMessage = 'Please enter a nickname for this account.';
                    return;
                }
                $scope.NicknameWaiting = true;
                $http.post('/Account/UpdateNickname', {
                    ApplicationId: account.ApplicationId,
                    NewNickname: $scope.NewNickname
                }).success(function (result) {
                    if (result.Success) {
                        account.Nickname = $sce.trustAsHtml($scope.NewNickname);
                        updateApplicationById(account.ApplicationId, account);
                        $('#EditNicknameModal').data('account', null).foundation('close');
                    }
                    else {
                        account.ErrorMessage = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                        $scope.NicknameErrorMessage = account.ErrorMessage;
                    }
                    $scope.NicknameWaiting = false;
                }).error(function () {
                    account.ErrorMessage = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    $('#EditNicknameModal').data('account', null).foundation('close');
                    $scope.NicknameWaiting = false;
                });
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // e-Notices
            $scope.submitENotices = function (account) {
                account.EnoticesWaiting = true;
                account.EnoticesMessage = null;
                account.EnoticesError = null;
                $http.post('/Account/SubmitENotices', account)
                    .success(function (result) {
                    if (result.Success) {
                        account.EnoticesMessage = 'Your eNotices preferences have been updated.';
                    }
                    else {
                        account.EnoticesError = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    }
                    account.EnoticesWaiting = false;
                })
                    .error(function () {
                    account.EnoticesError = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    account.EnoticesWaiting = false;
                });
                return false;
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // documents
            $scope.showSecurityPromptModal = function (doc, format) {
                $scope.SecurityPrompt = {
                    ApplicantId: doc.ApplicantId,
                    Doc: doc,
                    Format: format
                };
                $('#ENoticesSecurityPromptModal').foundation('open');
            };
            //convert to modal
            $scope.displayDoc = function (doc) {
                modal.setUrl("/enotices/html/" + doc.EDocId);
                modal.open();
            };
            //convert to modal
            $scope.displayLoanAgreement = function (doc) {
                modal.setUrl("/enotices/viewloanagreement/" + doc.EDocId);
                modal.open();
            };
            //convert to modal
            $scope.displayLoanAgreement = function (doc, appId) {
                modal.setUrl("/enotices/viewloanagreement/" + doc.EDocId + "?appId=" + appId);
                modal.open();
            };
            //no need to change
            $scope.displayPDF = function (doc) {
                $window.location = '/enotices/download/' + doc.PdfVersionEDocId;
            };
            $scope.viewEDoc = function (account, doc) {
                if (account.IsJoint) {
                    $scope.showSecurityPromptModal(doc, 'html');
                }
                else {
                    $scope.displayDoc(doc);
                }
            };
            $scope.downloadEDoc = function (account, doc) {
                if (account.IsJoint) {
                    $scope.showSecurityPromptModal(doc, 'pdf');
                }
                else {
                    $scope.displayPDF(doc);
                }
            };
            $scope.validateApplicant = function () {
                $http.post('/enotices/validate', {
                    SSN: $scope.SecurityPrompt.SSN,
                    DateOfBirth: $scope.SecurityPrompt.DateOfBirth,
                    ApplicantId: $scope.SecurityPrompt.ApplicantId
                }).success(function (result) {
                    if (result && result.IsValid) {
                        $('#ENoticesSecurityPromptModal').foundation('close');
                        if ($scope.SecurityPrompt.Format === 'pdf') {
                            $scope.displayPDF($scope.SecurityPrompt.Doc);
                        }
                        else {
                            $scope.displayDoc($scope.SecurityPrompt.Doc);
                        }
                    }
                    else {
                        $scope.SecurityPrompt.ErrorMessage = result.ErrorMessage;
                    }
                });
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // AutoPay Info
            $scope.autoPayNextStep = function (account) {
                if (account.AutoPayNextStep) {
                    account.AutoPayCurrentStep = account.AutoPayNextStep;
                }
            };
            // switch to auto pay
            $scope.switchToAutoPay = function (account) {
                account.AutoPayInfoWaiting = true;
                account.AutoPayInfoMessage = null;
                account.AutoPayInfoDate = null;
                account.AutoPayInfoError = null;
                $http.post('/Account/SwitchToAutoPay', account)
                    .success(function (result) {
                    if (result.DisplayACHMessage) {
                        var nextPaymentDateString = result.NextPaymentDate;
                        refreshAccount(account, function (result) {
                            result.SwitchedMessage = result.AutoPayInfoMessage = 'Your account has been switched to the AutoPay payment method.';
                            result.AutoPayInfoDate = ' The AutoPay feature will take effect for your next billing payment on ' + nextPaymentDateString + '. If you need to schedule a payment for the current billing period, please schedule the payment under the Extra Payments and Payoffs tab';
                        });
                    }
                    else if (result.Success) {
                        refreshAccount(account, function (result) {
                            result.SwitchedMessage = result.AutoPayInfoMessage = 'Your account has been switched to the AutoPay payment method.';
                        });
                    }
                    else {
                        account.AutoPayInfoError = result.ErrorMessage;
                        account.AutoPayInfoWaiting = false;
                    }
                })
                    .error(function () {
                    account.AutoPayInfoError = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    account.AutoPayInfoWaiting = false;
                });
                return false;
            };
            // auto pay - monthly payment
            $scope.updateMonthlyPayment = function (account) {
                account.AutoPayInfoWaiting = true;
                account.AutoPayInfoMessage = null;
                account.AutoPayInfoError = null;
                $http.post('/Account/UpdateMonthlyPayment', account)
                    .success(function (result) {
                    if (result.Success) {
                        account.AutoPayInfoMessage = 'Thank you, Your monthly payment has been changed effective with your ' + $sce.trustAsHtml(result.EffectiveDate) + ' payment.';
                        refreshAccount(account, function (result) {
                            result.AutoPayInfoMessage = account.AutoPayInfoMessage;
                            result.AutoPayCurrentStep = 'MonthlyPaymentThankYou';
                        });
                    }
                    else {
                        account.AutoPayInfoError = result.ErrorMessage;
                        account.AutoPayInfoWaiting = false;
                    }
                })
                    .error(function () {
                    account.AutoPayInfoError = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    account.AutoPayInfoWaiting = false;
                });
                return false;
            };
            // auto pay - update bank account
            $scope.isExceptionAndIsSameAccount = function (account) {
                if (account.HasPendingAutoDebitPaymentException) {
                    return account.DebitRoutingNumber === account.RoutingNumber && account.DebitAccountNumber === account.AccountNumber;
                }
                return false;
            };
            $scope.updatePaymentAccount = function (account) {
                account.AutoPayInfoWaiting = true;
                account.AutoPayInfoMessage = null;
                account.AutoPayInfoError = null;
                $http.post('/Account/UpdatePaymentAccount', account)
                    .success(function (result) {
                    if (result.Success) {
                        account.AutoPayInfoMessage = 'Thank you, your account information has been changed and will be used for any future payments that may be due on, ' +
                            'or after the effective date of ' + $sce.trustAsHtml(result.EffectiveDate);
                        refreshAccount(account, function (result) {
                            result.AutoPayInfoMessage = account.AutoPayInfoMessage;
                            result.AutoPayCurrentStep = 'ThankYou';
                        });
                    }
                    else {
                        account.AutoPayInfoError = result.ErrorMessage;
                        account.AutoPayInfoWaiting = false;
                    }
                })
                    .error(function () {
                    account.AutoPayInfoError = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    account.AutoPayInfoWaiting = false;
                });
                return false;
            };
            $scope.canelUpdateAccount = function (account) {
                account.AccountNumber = null;
                account.AutoPayCurrentStep = null;
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // extra payments & payoffs
            $scope.extraPaymentGoHome = function (account) {
                account.ExtraPaymentErrorMessage = null;
                account.ExtraPaymentInfoMessage = null;
                account.ExtraPaymentCurrentStep = null;
            };
            $scope.displayPayOffInvoice = function (account) {
                if (!account.PayOffByInvoiceDate) {
                    return false;
                }
                $window.open("/Account/DisplayPayOffInvoice?Ctx=" + account.Ctx + "&SelectedDate=" + account.PayOffByInvoiceDate);
            };
            $scope.paymentByACH = function (account) {
                account.ExtraPaymentWaiting = true;
                $http.post('/Account/PaymentByACH', account)
                    .success(function (result) {
                    if (result && result.Success) {
                        refreshAccount(account, function (result) {
                            result.IsThankYouMessage = true;
                        });
                    }
                    else {
                        account.ExtraPaymentWaiting = false;
                        account.ExtraPaymentErrorMessage = result && (result.ErrorMessage || 'We are sorry but an error has occurred and we are unable to process your request. Please try again.');
                    }
                });
            };
            $scope.payOffByACH = function (account) {
                account.ExtraPaymentWaiting = true;
                $http.post('/Account/PayOffByACH', account)
                    .success(function (result) {
                    if (result && result.Success) {
                        refreshAccount(account);
                    }
                    else {
                        account.ExtraPaymentWaiting = false;
                        account.ExtraPaymentErrorMessage = result && (result.ErrorMessage || 'We are sorry but an error has occurred and we are unable to process your request. Please try again.');
                    }
                });
            };
            $scope.cancelPayoff = function (account) {
                // stash the active account with the modal
                $('#CancelPayoffModal').data('account', account).foundation('open');
            };
            $scope.confirmCancelPayoff = function () {
                // stash the active account with the modal
                var $modal = $('#CancelPayoffModal'), account = $modal.data('account');
                $scope.CancelPayoffWaiting = true;
                account.ExtraPaymentErrorMessage = null;
                $http.post('/Account/CancelPayoff', account)
                    .success(function (result) {
                    if (result.Success) {
                        account.ExtraPaymentWaiting = true;
                        refreshAccount(account, function (result) {
                            result.ExtraPaymentInfoMessage = 'Your payoff has been cancelled. To schedule a new date, please select a payoff date ' +
                                'from the calendar and provide the account information requested below. ' +
                                'Funds will be withdrawn from your designated account on that date.';
                            result.ExtraPaymentCurrentStep = 'PayOffByACH';
                            $scope.CancelPayoffWaiting = false;
                            $('#CancelPayoffModal').data('account', null).foundation('close');
                        });
                    }
                    else {
                        account.ExtraPaymentErrorMessage = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                        $scope.CancelPayoffWaiting = false;
                        $('#CancelPayoffModal').data('account', null).foundation('close');
                    }
                })
                    .error(function () {
                    account.ExtraPaymentErrorMessage = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    $scope.CancelPayoffWaiting = false;
                    $('#CancelPayoffModal').data('account', null).foundation('close');
                });
            };
            $scope.extraPaymentNextStep = function (account) {
                if (!account.ExtraPaymentNextStep) {
                    account.ExtraPaymentErrorMessage = 'Please make a selection before clicking Next.';
                }
                account.ExtraPaymentErrorMessage = null;
                account.ExtraPaymentInfoMessage = null;
                account.ExtraPaymentCurrentStep = account.ExtraPaymentNextStep;
            };
            $scope.cancelExtraPayment = function (account) {
                // stash the active account with the modal
                $('#CancelExtraPaymentModal').data('account', account).foundation('open');
            };
            $scope.confirmCancelExtraPayment = function () {
                // stash the active account with the modal
                var $modal = $('#CancelExtraPaymentModal'), account = $modal.data('account');
                $scope.CancelExtraPaymentWaiting = true;
                account.ExtraPaymentErrorMessage = null;
                $http.post('/Account/CancelExtraPayment', account)
                    .success(function (result) {
                    if (result.Success) {
                        refreshAccount(account, function (result) {
                            result.ExtraPaymentInfoMessage = 'Your payment has been cancelled. To schedule a new payment, please select a date from the calendar, ' +
                                'enter the amount of your payment and provide the account information requested below.';
                            result.ExtraPaymentCurrentStep = 'PaymentByACH';
                            $scope.CancelExtraPaymentWaiting = false;
                            $('#CancelExtraPaymentModal').data('account', null).foundation('close');
                        });
                    }
                    else {
                        account.ExtraPaymentErrorMessage = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                        $('#CancelExtraPaymentModal').data('account', null).foundation('close');
                    }
                })
                    .error(function () {
                    account.ExtraPaymentErrorMessage = 'We are sorry but an error has occurred and we are unable to process your request. Please try again.';
                    $scope.CancelExtraPaymentWaiting = false;
                    $('#CancelExtraPaymentModal').data('account', null).foundation('close');
                });
            };
            $scope.extraPaymentNextStep = function (account) {
                if (!account.ExtraPaymentNextStep) {
                    account.ExtraPaymentErrorMessage = 'Please make a selection before clicking Next.';
                }
                account.ExtraPaymentInfoMessage = null;
                account.ExtraPaymentErrorMessage = null;
                account.ExtraPaymentCurrentStep = account.ExtraPaymentNextStep;
            };
            $scope.allAccountsAreInvoice = function () {
                var i;
                if (!$scope.Model || !$scope.Model.Applications) {
                    return false;
                }
                for (i = 0; i < $scope.Model.Applications.length; i = i + 1) {
                    if ($scope.Model.Applications[i].PaymentType === 'AutoPay') {
                        return false;
                    }
                }
                return true;
            };
            $scope.anyAccountsAreInvoice = function () {
                var i;
                if (!$scope.Model || !$scope.Model.Applications) {
                    return false;
                }
                for (i = 0; i < $scope.Model.Applications.length; i = i + 1) {
                    if ($scope.Model.Applications[i].PaymentType === 'Invoice') {
                        return true;
                    }
                }
                return false;
            };
            ///////////////////////////////////////////////////////////////////////////////////////////
            // initial load of all the data
            $http.post('/Account/Load').success(function (result) {
                var i;
                $scope.Model = result;
                for (i = 0; i < $scope.Model.Applications.length; i = i + 1) {
                    // sanitize HTML content,
                    $scope.Model.Applications[i].PaymentAccountMessage = $sce.trustAsHtml($scope.Model.Applications[i].PaymentAccountMessage);
                }
                // init the calendar(s)
                $scope.invoiceDateOptions = {
                    numberOfMonths: 2,
                    setDate: 0,
                    beforeShowDay: function (date) {
                        if (!$scope.Model.PayoffDatesViaInvoice) {
                            return [false];
                        }
                        var fd = $scope.Model.PayoffDatesViaInvoice[date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate()];
                        if (fd) {
                            return [true, 'ach', ''];
                        }
                        return [false, '', ''];
                    }
                };
                $scope.autoPayDateOptions = {
                    numberOfMonths: 2,
                    setDate: 0,
                    beforeShowDay: function (date) {
                        if (!$scope.Model.PayoffDatesViaAutoPay) {
                            return [false];
                        }
                        var fd = $scope.Model.PayoffDatesViaAutoPay[date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate()];
                        if (fd) {
                            return [true, 'ach', ''];
                        }
                        return [false, '', ''];
                    }
                };
                $scope.Loading = false;
            });
        }]);
    // mini-controller for watches on variables for each account
    angular.module('AccountServicesModule')
        .controller('AccountServicesAccountWatchController', ['$scope', '$http',
        function ($scope, $http) {
            //PayOffByInvoiceDate
            $scope.$watch('account.PayOffByInvoiceDate', function (newValue, oldValue) {
                if (newValue && newValue !== oldValue) {
                    var account = $scope.$parent.account;
                    account.PayOffQuote = null;
                    $http.post('/Account/GetPayoffQuote', {
                        ApplicationId: account.ApplicationId,
                        PaymentType: 'Invoice',
                        SelectedDate: newValue
                    }).success(function (result) {
                        if (result && result.Success) {
                            account.PayOffQuote = result.PayOffQuote;
                        }
                    });
                }
            });
            // PayOffByAutoPayDate
            $scope.$watch('account.PayOffByAutoPayDate', function (newValue, oldValue) {
                if (newValue && newValue !== oldValue) {
                    var account = $scope.$parent.account;
                    account.PayOffQuote = null;
                    $http.post('/Account/GetPayoffQuote', {
                        ApplicationId: account.ApplicationId,
                        PaymentType: 'AutoPay',
                        SelectedDate: newValue
                    }).success(function (result) {
                        if (result && result.Success) {
                            account.PayOffQuote = result.PayOffQuote;
                            account.PayOffIncludedACHPayment = result.PayOffIncludedACHPayment;
                            account.PayOffIncludedACHPaymentDate = result.PayOffIncludedACHPaymentDate;
                        }
                    });
                }
            });
            $scope.$watch('account.ScheduledPaymentDate', function (newValue, oldValue) {
                if (newValue && newValue !== oldValue) {
                    var account = $scope.$parent.account;
                    $http.post('/Account/GetPayoffQuote', {
                        ApplicationId: account.ApplicationId,
                        PaymentType: 'AutoPay',
                        SelectedDate: newValue
                    }).success(function (result) {
                        if (result && result.Success) {
                            account.PayOffQuote = result.PayOffQuote;
                            account.PayOffIncludedACHPayment = result.PayOffIncludedACHPayment;
                            account.PayOffIncludedACHPaymentDate = result.PayOffIncludedACHPaymentDate;
                        }
                    });
                }
            });
        }]);
}());
//# sourceMappingURL=AccountServicesController.js.map