// loanAppSessionService
angular.module('ls.services').factory('loanAppSessionService', ['$http', 'webStorage', '$window', 'factHistory', function ($http, webStorage, $window, factHistory) {
        webStorage.order(['session', 'local']);
        // load from local or server session
        var _load = function () {
            var loanApp = null;
            if (!webStorage.isSupported) {
                $.ajax({
                    url: '/Services/Session/Get',
                    data: { key: 'LoanApplication' },
                    async: false,
                    success: function (data) {
                        loanApp = JSON.parse(data);
                    }
                });
            }
            else {
                //get from local storage
                loanApp = webStorage.get('LoanApplication');
                //if nothing in web storage, try to get from Service
                if (!loanApp) {
                    $.ajax({
                        url: '/Services/Session/Get',
                        data: { key: 'LoanApplication' },
                        async: false,
                        success: function (data) {
                            loanApp = JSON.parse(data);
                        }
                    });
                }
                if (loanApp) {
                    loanApp.FACTHistory = factHistory.getFactIds();
                }
            }
            // If nothing in WebStorage, get defaults
            if (!loanApp) {
                loanApp = {
                    PurposeOfLoan: {
                        Type: ($window.location.search.match(/purposeOfLoan=(.*?)($|\&)/i) || [])[1] || 'NotSelected'
                    },
                    LoanPaymentType: 'AutoPay',
                    ApplicationType: 'NotSelected',
                    CombinedFinancials: {},
                    Applicants: [{
                            Residence: {
                                Ownership: 'NotSelected',
                                Address: {}
                            },
                            Occupation: {},
                            EmailValidation: {}
                        }, {
                            Residence: {
                                Ownership: 'NotSelected',
                                Address: {}
                            },
                            Occupation: {},
                            EmailValidation: {}
                        }],
                    ApplicantHmdaData: [{ ApplicantRace: [] }, { ApplicantRace: [] }],
                    ApplicationOtherIncome: [{ IncomeType: 'NotSelected' }, { IncomeType: 'NotSelected' }, { IncomeType: 'NotSelected' }, { IncomeType: 'NotSelected' }],
                    HmdaComplianceProperty: {
                        Address: {
                            SecondaryUnit: {}
                        }
                    },
                    UserCredentials: {
                        IsTemporary: false
                    },
                    FACTData: {},
                    FACTHistory: factHistory.getFactIds(),
                    UserIdNotReserved: false,
                    ApplicationTakenOverPhone: false
                };
            }
            if (loanApp.ZipCode && !loanApp.Applicants[0].Residence.Address.ZipCode) {
                loanApp.Applicants[0].Residence.Address.ZipCode = loanApp.ZipCode;
            }
            if (loanApp.State && (!loanApp.Applicants[0].Residence.Address.State || loanApp.Applicants[0].Residence.Address.State === 'NotSelected')) {
                loanApp.Applicants[0].Residence.Address.State = loanApp.State;
            }
            angular.forEach(loanApp.ApplicationOtherIncome, function (otherIncome, key) {
                if (!otherIncome.IncomeType) {
                    otherIncome.IncomeType = 'NotSelected';
                }
            });
            var mData = $window.mData || {};
            mData.loanApplication = loanApp;
            return loanApp;
        }, _save = function (loanApp) {
            if (!webStorage.isSupported) {
                $.ajax({
                    url: '/Services/Session/Set',
                    data: {
                        key: 'LoanApplication',
                        value: JSON.stringify(loanApp)
                    },
                    async: false,
                    method: 'POST'
                });
            }
            else {
                webStorage.add('LoanApplication', loanApp);
            }
            var mData = $window.mData || {};
            mData.loanApplication = loanApp;
        }, 
        // override the purpose of loan
        // also accepts object of properies to map to new app
        _setPurposeOfLoan = function (purposeOfLoan, more) {
            var app = _load(), propName;
            app.PurposeOfLoan.Type = purposeOfLoan;
            if (more) {
                for (propName in more) {
                    if (more.hasOwnProperty(propName)) {
                        app[propName] = more[propName];
                    }
                }
            }
            _save(app);
        }, 
        // call when starting a new app from the rates page. Will not overwrite old application data
        _start = function (zipCode, purposeOfLoan, loanAmount, loanTerm, paymentType, discount, isSuntrustApplication) {
            var app = _load();
            app.PurposeOfLoan.Type = purposeOfLoan;
            app.LoanAmount = loanAmount;
            app.LoanTermMonths = loanTerm;
            app.Discount = discount || 'NoDiscount';
            if (!app.IsAccountServices) {
                app.ZipCode = zipCode;
            }
            app.LoanPaymentType = paymentType;
            app.IsSuntrustApplication = isSuntrustApplication;
            _save(app);
        }, _remove = function () {
            if (!webStorage.isSupported) {
                $.ajax({
                    url: '/Services/Session/Delete',
                    data: { key: 'LoanApplication' },
                    async: false
                });
            }
            else {
                webStorage.remove('LoanApplication');
            }
        }, _cleanupLoanApp = function (loanApp) {
            var i;
            // data cleanup
            for (i = 0; i < 2; i++) {
                if (loanApp.Applicants[i].Residence.TimeAtAddress.Months === 0 && loanApp.Applicants[i].Residence.TimeAtAddress.Years === 0) {
                    loanApp.Applicants[i].Residence.TimeAtAddress = {};
                }
                if (loanApp.Applicants[i].Occupation.Employer.TimeWithEmployer.Months === 0 && loanApp.Applicants[i].Occupation.Employer.TimeWithEmployer.Years === 0) {
                    loanApp.Applicants[i].Occupation.Employer.TimeWithEmployer = {};
                }
                if (loanApp.Applicants[i].DateOfBirth === '0001-01-01T00:00:00') {
                    loanApp.Applicants[i].DateOfBirth = null;
                }
                if (loanApp.Applicants[i].Residence.IsNonResident != null) {
                    loanApp.Applicants[i].Residence.IsNonResident = loanApp.Applicants[i].Residence.IsNonResident.toString();
                }
            }
            if (loanApp.LoanAmount === 0) {
                loanApp.LoanAmount = null;
            }
            // init 'Yes to other annual income' if any items have been supplied
            loanApp.YesToOtherAnnualIncome = !!(loanApp.ApplicationOtherIncome && loanApp.ApplicationOtherIncome.length);
            if (!loanApp.ApplicationOtherIncome) {
                loanApp.ApplicationOtherIncome = [];
            }
            while (loanApp.ApplicationOtherIncome.length < 4) {
                loanApp.ApplicationOtherIncome.push({});
            }
        }, _init = function (dataUrl, data) {
            var promise = $http
                .post(dataUrl, data || {})
                .then(function (response) {
                // if we have a loan app, save it to session storage
                if (response.data && response.data.LoanApp) {
                    _cleanupLoanApp(response.data.LoanApp);
                }
                return response.data;
            });
            return promise;
        };
        return {
            setPurposeOfLoan: _setPurposeOfLoan,
            start: _start,
            load: _load,
            save: _save,
            remove: _remove,
            init: _init,
            cleanup: _cleanupLoanApp
        };
    }]);
//# sourceMappingURL=loanAppSessionService.js.map