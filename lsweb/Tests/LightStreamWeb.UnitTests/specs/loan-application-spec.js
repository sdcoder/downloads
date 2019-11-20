/// <reference path="../../../webroot/scripts/polyfill/es6-shim.min.js" />
/// <reference path="../scripts/jasmine-2.3.4/jasmine.js" />
/// <reference path="../scripts/jasmine-2.3.4/boot.js" />
/// <reference path="../../../webroot/scripts/angular/angular.js" />
/// <reference path="../../../webroot/scripts/angular/angular-route.js" />
/// <reference path="../../../webroot/scripts/angular/angular-webstorage.js" />
/// <reference path="../scripts/angular/angular-mocks.js" />
/// <reference path="../../../webroot/scripts/jquery-3.4.1.js" />
/// <reference path="../../../webroot/scripts/foundation/foundation.js" />

/// <reference path="../../../webroot/scripts/app/loan-application.js" />
/// <reference path="../../../webroot/scripts/app/ls.filters.js" />
/// <reference path="../../../webroot/scripts/app/ls.directives.js" />
/// <reference path="../../../webroot/scripts/app/ls.services.js" />
/// <reference path="../../../webroot/scripts/polyfill/url-search-params.js" />
/// <reference path="../../../webroot/scripts/app/fact/FactHistory.js" />

describe('When filling out loan application', function () {
    var scope, ctrl, httpBackend, q, stateService, validationService, window;

    var mockLoanAppSessionService = {
        load: function () {
            var loanApp = {
                PurposeOfLoan: {
                    Type: 'NotSelected'
                },
                LoanPaymentType: 'AutoPay',
                ApplicationType: 'NotSelected',
                CombinedFinancials: {},
                Applicants: [{
                    Residence: {
                        Ownership: 'NotSelected',
                        Address: {}
                    },
                    Occupation: {}
                }, {
                    Residence: {
                        Ownership: 'NotSelected',
                        Address: {}
                    },
                    Occupation: {}
                }],
                ApplicantHmdaData: [{ ApplicantRace: [] }, { ApplicantRace: [] }],
                ApplicationOtherIncome: [{}, {}, {}, {}],
                HmdaComplianceProperty: {
                    Address: {
                        SecondaryUnit: {}
                    }
                },
                UserCredentials: {
                    IsTemporary: false
                },
                FACTHistory: [],
                FACTData: {},
                UserIdNotReserved: false,
                ApplicationTakenOverPhone: false
            };

            if (loanApp.ZipCode && !loanApp.Applicants[0].Residence.Address.ZipCode) {
                loanApp.Applicants[0].Residence.Address.ZipCode = loanApp.ZipCode;
            }
            if (loanApp.State && (!loanApp.Applicants[0].Residence.Address.State || loanApp.Applicants[0].Residence.Address.State === 'NotSelected')) {
                loanApp.Applicants[0].Residence.Address.State = loanApp.State;
            }

            return loanApp;
        },
        remove: function () { },
        save: function () { },
        start: function () { },
        cleanup: function () { }
    };

    beforeEach(module('LoanAppModule'));

    beforeEach(inject(function ($rootScope, $controller, $httpBackend, $q, $compile, _stateService_, _validationService_) {

        httpBackend = $httpBackend;
        q = $q;

        stateService = _stateService_;
        validationService = _validationService_;

        var mockStaticLookupsService = {
            getStaticLookups: function () {
                var deferred = q.defer();
                deferred.resolve({});
                return deferred.promise;
            }
        };

        scope = $rootScope.$new();

        window = {
            location: { href: '' },
            history: {
                replaceState: function () { }
            },
            setInterval: function () { }
        }

        ctrl = $controller('LoanApplicationController',
            {
                $scope: scope,
                $window: window,
                loanAppSessionService: mockLoanAppSessionService,
                staticLookupService: mockStaticLookupsService
            });
    }));

    beforeEach(function () {
        // This is needed for all tests bc initialization of loan app module makes service call to gets rates
        httpBackend.whenPOST('/services/rates').respond(200, { QueueId: 1 });
        httpBackend.whenGET('/lookups/ofaccountries').respond(200);
        httpBackend.whenGET('/services/getlatestloantermrequest/').respond(200);
    });

    afterEach(function () {
        httpBackend.verifyNoOutstandingRequest();
        httpBackend.verifyNoOutstandingExpectation();
    });

    describe('email', function () {

        describe('applicant validation', function () {

            beforeEach(function () {
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            EmailAddress: '',
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: "Retired" }
                        },
                        {
                            EmailAddress: '',
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: "Retired" }
                        }
                    ]
                }
            });

            it('should not validate email when there is no email to validate', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve({});
                    return deferred.promise;
                });

                var applicantIndex = 0;

                // act
                scope.validateEmail(applicantIndex);

                httpBackend.flush();
                scope.$digest();

                // assert
                expect(validationService.getEmailValidation.calls.count()).toBe(0);
            });

            it('should initialize email validation when it is not initialized', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation).toBeFalsy();

                // act
                scope.validateEmail(applicantIndex);

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation).toBeTruthy();

            });

            it('should not turn on validation failed flag when validation completes successfully and email is valid', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: true
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation).toBeUndefined();

                // act
                scope.validateEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBeFalsy();
            });

            it('should not turn on validation failed flag when validation completes successfully, email result is invalid, but certainty type is unknown', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: false,
                    isUnknown: true
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation).toBeUndefined();

                // act
                scope.validateEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBeFalsy();
            });

            it('should turn on validation failed flag when validation completes successfully and email is invalid', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation).toBeUndefined();

                // act
                scope.validateEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBeTruthy();
            });

            it('should turn off validation failed flag when email is updated', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';
                scope.LoanApp.Applicants[applicantIndex].EmailValidation = { lastValidatedEmail: 'blah-blah@blah.com', hasValidationFailed: true };

                // act
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBe(true);

                scope.validateEmail(applicantIndex);

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBe(false);
            });

            it('should not turn off validation failed flag when email is not updated', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';
                scope.LoanApp.Applicants[applicantIndex].EmailValidation = { lastValidatedEmail: 'blah@blah.com', hasValidationFailed: true };

                // act
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBe(true);

                scope.validateEmail(applicantIndex);

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.hasValidationFailed).toBe(true);
            });

            it('should set last validated email when validation completes successfully', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isValid: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation).toBeUndefined();

                // act
                scope.validateEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.lastValidatedEmail).toBe('blah@blah.com');
            });

            it('should not validate email if it has already been passed through validation', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                spyOn(validationService, 'getEmailValidation');
                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';
                scope.LoanApp.Applicants[applicantIndex].EmailValidation = { lastValidatedEmail: 'blah@blah.com' };

                // act
                scope.validateEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(validationService.getEmailValidation.calls.count()).toBe(0);
            });

            it('should turn on \"is validating\" flag while validating email and then off when validation is successful', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                // act
                scope.validateEmail(applicantIndex);

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating).toBe(true);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating).toBe(false);
            });

            it('should turn on \"is validating\" flag while validating email and then off when validation is not successful', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                // act
                scope.validateEmail(applicantIndex);

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating).toBe(true);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating).toBe(false);
            });

            it('should turn on \"is validating\" flag while validating email and then off when service errors out', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);


                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.reject();
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.Applicants[applicantIndex].EmailAddress = 'blah@blah.com';

                // act
                scope.validateEmail(applicantIndex);

                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating).toBe(true);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.Applicants[applicantIndex].EmailValidation.isValidating).toBe(false);
            });
        });

        describe('referral validation', function () {
            beforeEach(function () {
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            EmailAddress: '',
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: "Retired" }
                        },
                        {
                            EmailAddress: '',
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: "Retired" }
                        }
                    ]
                }
            });

            it('should not validate email when there is no email to validate', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve({});
                    return deferred.promise;
                });

                var applicantIndex = 0;

                // act
                scope.validateReferralEmail(applicantIndex);

                httpBackend.flush();
                scope.$digest();

                // assert
                expect(validationService.getEmailValidation.calls.count()).toBe(0);
            });

            it('should initialize email validation when it is not initialized', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.ReferralEmailValidation).toBeFalsy();

                // act
                scope.validateReferralEmail(applicantIndex);

                // assert
                expect(scope.LoanApp.ReferralEmailValidation).toBeTruthy();

            });

            it('should not turn on validation failed flag when validation completes successfully and email is valid', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: true
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.ReferralEmailValidation).toBeUndefined();

                // act
                scope.validateReferralEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBeFalsy();
            });

            it('should not turn on validation failed flag when validation completes successfully, email result is invalid, but certainty type is unknown', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: false,
                    isUnknown: true
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.ReferralEmailValidation).toBeUndefined();

                // act
                scope.validateReferralEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBeFalsy();
            });

            it('should turn on validation failed flag when validation completes successfully and email is invalid', function () {

                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.ReferralEmailValidation).toBeUndefined();

                // act
                scope.validateReferralEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBeTruthy();
            });

            it('should turn off validation failed flag when email is updated', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';
                scope.LoanApp.ReferralEmailValidation = { lastValidatedEmail: 'blah-blah@blah.com', hasValidationFailed: true };

                // act
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBe(true);

                scope.validateReferralEmail(applicantIndex);

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBe(false);
            });

            it('should not turn off validation failed flag when email is not updated', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';
                scope.LoanApp.ReferralEmailValidation = { lastValidatedEmail: 'blah@blah.com', hasValidationFailed: true };

                // act
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBe(true);

                scope.validateEmail(applicantIndex);

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.hasValidationFailed).toBe(true);
            })

            it('should set last validated email when validation completes successfully', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isValid: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                expect(scope.LoanApp.ReferralEmailValidation).toBeUndefined();

                // act
                scope.validateReferralEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.lastValidatedEmail).toBe('blah@blah.com');
            });

            it('should not validate email if it has already been passed through validation', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                spyOn(validationService, 'getEmailValidation');
                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';
                scope.LoanApp.ReferralEmailValidation = { lastValidatedEmail: 'blah@blah.com' };

                // act
                scope.validateReferralEmail(applicantIndex);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(validationService.getEmailValidation.calls.count()).toBe(0);
            });

            it('should turn on \"is validating\" flag while validating email and then off when validation is successful', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: true,
                    isEmailValid: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                // act
                scope.validateReferralEmail(applicantIndex);

                expect(scope.LoanApp.ReferralEmailValidation.isValidating).toBe(true);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.isValidating).toBe(false);
            });

            it('should turn on \"is validating\" flag while validating email and then off when validation is not successful', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);

                var fakeResponse = {
                    isSuccessful: false
                };

                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeResponse);
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                // act
                scope.validateReferralEmail(applicantIndex);

                expect(scope.LoanApp.ReferralEmailValidation.isValidating).toBe(true);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.isValidating).toBe(false);
            });

            it('should turn on \"is validating\" flag while validating email and then off when service errors out', function () {
                // arrange
                httpBackend.whenPOST('/services/validate-email').respond(200);


                spyOn(validationService, 'getEmailValidation').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.reject();
                    return deferred.promise;
                });

                var applicantIndex = 0;

                scope.LoanApp.ApplicantEmailAddress = 'blah@blah.com';

                // act
                scope.validateReferralEmail(applicantIndex);

                expect(scope.LoanApp.ReferralEmailValidation.isValidating).toBe(true);

                httpBackend.flush(); // ensure response has been processed
                scope.$digest();

                // assert
                expect(scope.LoanApp.ReferralEmailValidation.isValidating).toBe(false);
            });
        });

    });

    describe('residence', function () {
        it('should set hmda compliance address to resident address when subject property is same resident address', function () {

            httpBackend.whenPOST('/services/statelookup').respond(200);

            expect(scope.LoanApp.HmdaComplianceProperty.Address.AddressLine).toBeUndefined();
            expect(scope.LoanApp.HmdaComplianceProperty.Address.City).toBeUndefined();
            expect(scope.LoanApp.HmdaComplianceProperty.Address.State).toBeUndefined();
            expect(scope.LoanApp.HmdaComplianceProperty.Address.ZipCode).toBeUndefined();
            expect(scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Type).toBeUndefined();
            expect(scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Value).toBeUndefined();

            // Arrange
            var expectedAddress = 'Expected Address';
            var expectedCity = 'Expected City';
            var expectedState = 'Expected State';
            var expectedZipCode = 'Expected ZipCode';
            var expectedSecondaryUnitType = 'Expected Secondary Unit Type';
            var expectedSecondaryUnitValue = 'Expected Secondary Unit Value';

            scope.LoanApp = {
                ZipCode: 92108,
                PurposeOfLoan: { Type: "HomeImprovement" },
                HmdaComplianceProperty: { Address: {} },
                LoanAmount: 5000,
                Applicants: [
                    {
                        Occupation: {
                            Employer: { GrossAnnualIncome: 0 }
                        },
                        Residence: {
                            Address: {
                                AddressLine: expectedAddress,
                                City: expectedCity,
                                State: expectedState,
                                ZipCode: expectedZipCode,
                                SecondaryUnit: {
                                    Type: expectedSecondaryUnitType,
                                    Value: expectedSecondaryUnitValue
                                }
                            },
                            Ownership: 'Own'
                        }
                    },
                    {
                        Occupation: {
                            Employer: { GrossAnnualIncome: 0 }
                        },
                        Residence: {
                            Address: { State: '' }
                        }
                    }
                ]
            };

            httpBackend.flush();

            // Act
            scope.LoanApp.SubjectPropertySameAsResidentAddress = true;
            scope.$digest();

            // Assert
            expect(scope.LoanApp.HmdaComplianceProperty.Address.AddressLine).toBe(expectedAddress);
            expect(scope.LoanApp.HmdaComplianceProperty.Address.City).toBe(expectedCity);
            expect(scope.LoanApp.HmdaComplianceProperty.Address.State).toBe(expectedState);
            expect(scope.LoanApp.HmdaComplianceProperty.Address.ZipCode).toBe(expectedZipCode);
            expect(scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Type).toBe(expectedSecondaryUnitType);
            expect(scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Value).toBe(expectedSecondaryUnitValue);
        });

        describe('zip code', function () {

            beforeEach(inject(function ($compile) {
                var element = angular.element(
                    '<form name="LoanApplication">' +
                    '<input type="text" name="ZipCode" id="ZipCode" ng-model="LoanApp.ZipCode">' +
                    '<input type="text" name="ApplicantResidenceAddressZipCode" id="ApplicantResidenceAddressZipCode" ng-model="LoanApp.Applicants[0].Residence.Address.ZipCode">' +
                    '</form>');

                $compile(element)(scope);
            }));

            it('should populate state info when loan app zip code changes', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.$digest();

                var isValid = true;
                var expectedState = "AnyState";
                var expectedCity = "AnyCity";
                var expectedZipCode = "12345";

                var fakeStateReturnObject = {
                    IsValid: isValid,
                    State: expectedState,
                    City: expectedCity,
                    ZipCode: expectedZipCode
                }

                spyOn(stateService, 'getState').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeStateReturnObject);
                    return deferred.promise;
                });

                httpBackend.flush();

                // Act
                scope.LoanApp.ZipCode = expectedZipCode;
                scope.$digest();

                // Assert
                expect(scope.LoanApplication.ZipCode.$invalid).toBe(false);
                expect(scope.LoanApplication.ZipCode.$notFound).toBe(!isValid);

                expect(scope.LoanApp.UserSelectedState).toBe(!isValid);
                expect(scope.LoanApp.State).toBe(expectedState);
                expect(scope.LoanApp.FloridaDocStampTax).toBeUndefined();
                expect(scope.LoanApp.PrePopulatedCity).toBe(expectedCity);

                expect(scope.LoanApp.Applicants[0].Residence.Address.City).toBe(expectedCity);
                expect(scope.LoanApp.Applicants[0].Residence.Address.State).toBe(expectedState);
                expect(scope.LoanApp.Applicants[0].Residence.Address.ZipCode).toBe(expectedZipCode);
            });

            it('should invalidate zip code when loan zip code changes and is military', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.$digest();

                var anyZipCode = "12345";

                var fakeStateReturnObject = {
                    IsValid: true,
                    ZipCode: anyZipCode,
                    IsMilitary: true
                }

                spyOn(stateService, 'getState').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeStateReturnObject);
                    return deferred.promise;
                });

                expect(scope.LoanApplication.ZipCode.$invalid).toBe(false);  // Check for clean value before digesting

                httpBackend.flush();

                // Act 
                scope.LoanApp.ZipCode = anyZipCode;
                scope.$digest();

                // Assert
                expect(scope.LoanApplication.ZipCode.$invalid).toBe(true);
            });

            it('should set not set state when loan zip code changes and state service data is invalid', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.$digest();

                var anyZipCode = "12345";
                var expectedStateValue = "NotSelected";
                var fakeStateReturnObject = {
                    IsValid: false,
                    ZipCode: anyZipCode,
                }

                spyOn(stateService, 'getState').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeStateReturnObject);
                    return deferred.promise;
                });

                httpBackend.flush();

                // Act 
                scope.LoanApp.ZipCode = anyZipCode;
                scope.$digest();

                // Assert
                expect(scope.LoanApp.State).toBe(expectedStateValue);
            });

            it('should populate state info when primary applicant\'s zip code changes', function () {
                // Arrange

                scope.$digest();

                var isValid = true;
                var expectedState = "AnyState";
                var expectedCity = "AnyCity";
                var expectedZipCode = "12345";

                var fakeStateReturnObject = {
                    IsValid: isValid,
                    State: expectedState,
                    City: expectedCity,
                    ZipCode: expectedZipCode
                }

                spyOn(stateService, 'getState').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeStateReturnObject);
                    return deferred.promise;
                });

                httpBackend.flush();

                // Act
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: {
                                Address: { ZipCode: expectedZipCode }
                            }
                        },
                        {
                            Residence: {
                                Address: {}
                            }
                        }
                    ]
                };

                scope.$digest();

                //// Assert
                expect(scope.LoanApplication.ApplicantResidenceAddressZipCode.$invalid).toBe(false);
                expect(scope.LoanApplication.ApplicantResidenceAddressZipCode.$notFound).toBe(!isValid);

                expect(scope.LoanApp.UserSelectedState).toBe(!isValid);
                expect(scope.LoanApp.ZipCode).toBe(expectedZipCode);
                expect(scope.LoanApp.State).toBe(expectedState);
                expect(scope.LoanApp.FloridaDocStampTax).toBeUndefined();

                expect(scope.LoanApp.Applicants[0].Residence.Address.City).toBe(expectedCity);
                expect(scope.LoanApp.Applicants[0].Residence.Address.State).toBe(expectedState);
                expect(scope.LoanApp.Applicants[0].Residence.Address.ZipCode).toBe(expectedZipCode);
            });

            it('should invalidate zip code when primary applicant\'s zip code changes and is military', function () {


                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                var anyZipCode = "12345";

                var fakeStateReturnObject = {
                    IsValid: true,
                    ZipCode: anyZipCode,
                    IsMilitary: true
                }

                spyOn(stateService, 'getState').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeStateReturnObject);
                    return deferred.promise;
                });

                expect(scope.LoanApplication.ApplicantResidenceAddressZipCode.$invalid).toBe(false);  // Check for clean value before digesting

                httpBackend.flush();

                // Act 
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: {
                                Address: { ZipCode: anyZipCode }
                            }
                        }
                    ]
                };
                scope.$digest();

                // Assert
                expect(scope.LoanApplication.ApplicantResidenceAddressZipCode.$invalid).toBe(true);
            });

            it('should set not set state when primary applicant\'s zip code changes and state service data is invalid', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                var anyZipCode = "12345";
                var expectedStateValue = "NotSelected";
                var fakeStateReturnObject = {
                    IsValid: false,
                    ZipCode: anyZipCode,
                }

                spyOn(stateService, 'getState').and.callFake(function () {
                    var deferred = q.defer();
                    deferred.resolve(fakeStateReturnObject);
                    return deferred.promise;
                });

                httpBackend.flush();

                // Act 
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: {
                                Address: { ZipCode: anyZipCode }
                            }
                        },
                        {
                            Residence: {
                                Address: {
                                    ZipCode: anyZipCode
                                }
                            }
                        }
                    ]
                };
                scope.$digest();

                // Assert
                expect(scope.LoanApp.Applicants[0].Residence.Address.State).toBe(expectedStateValue);
            });
        });

        it('should get Florida document stamp tax when loan amount changes and primary applicant\'s resident state is Florida', function () {
            // Arrange
            httpBackend.whenPOST('/Services/StateLookup').respond(200);

            var expectedFloridaStampTax = 35.35;
            httpBackend.expectPOST('/services/statetaxlookup').respond(200, { FloridaDocStampTax: expectedFloridaStampTax });

            scope.LoanApp = {
                PurposeOfLoan: { Type: 'NotSelected' },
                Applicants: [
                    {
                        Residence: {
                            Address: { ZipCode: '12345', State: 'Florida' }
                        },
                        Occupation: { Type: '' }
                    },
                    {
                        Residence: {
                            Address: { ZipCode: '12345', State: 'California' }
                        },
                        Occupation: { Type: '' }
                    }
                ],
                LoanAmount: 10000
            };

            // Act
            httpBackend.flush();
            scope.$digest();

            // Assert
            expect(scope.LoanApp.FloridaDocStampTax).toBe(expectedFloridaStampTax);
        });

        it('should get Florida document stamp tax when loan amount changes and joint applicant\'s resident state is Florida', function () {
            // Arrange
            httpBackend.whenPOST('/services/statelookup').respond(200);

            var expectedFloridaStampTax = 35.35;
            httpBackend.expectPOST('/services/statetaxlookup').respond(200, { FloridaDocStampTax: expectedFloridaStampTax });

            scope.LoanApp = {
                PurposeOfLoan: { Type: 'NotSelected' },
                ApplicationType: 'Joint',
                Applicants: [
                    {
                        Residence: {
                            Address: { ZipCode: '12345' }
                        },
                        Occupation: { Type: '' }
                    },
                    {
                        Residence: {
                            Address: { ZipCode: '12345', State: 'Florida' }
                        },
                        Occupation: { Type: '' }
                    }
                ],
                LoanAmount: 10000
            };

            // Act
            httpBackend.flush();
            scope.$digest();

            // Assert
            expect(scope.LoanApp.FloridaDocStampTax).toBe(expectedFloridaStampTax);
        });

        it('should not get Florida document stamp tax when loan amount changes and neither applicant\'s residence is Florida', function () {
            // Arrange
            httpBackend.whenPOST('/services/statelookup').respond(200);

            scope.LoanApp = {
                PurposeOfLoan: { Type: 'NotSelected' },
                Applicants: [
                    {
                        Residence: {
                            Address: { ZipCode: '12345', State: 'California' }
                        },
                        Occupation: { Type: '' }
                    },
                    {
                        Residence: {
                            Address: { ZipCode: '12345', State: 'California' }
                        },
                        Occupation: { Type: '' }
                    }
                ],
                State: 'California',
                LoanAmount: 10000
            };

            // Act
            httpBackend.flush();
            scope.$digest();

            // Assert
            expect(scope.LoanApp.FloridaDocStampTax).toBeFalsy();
        });

        it('should show "non-applicant spouse\'s income" when state is Wisconsin', function () {
            // Arrange
            scope.$digest();
            httpBackend.flush();

            spyOn($.fn, 'prop').and.callFake(function () { });

            scope.LoanApp.State = 'Wisconsin';

            // Act
            scope.updateWisconsinOtherIncomeType();

            // Assert
            expect($.fn.prop).toHaveBeenCalledWith('disabled', false);
        });

        it('should not show "non-applicant spouse\'s income" when state is not Wisconsin', function () {
            scope.$digest();
            httpBackend.flush();

            spyOn($.fn, 'prop').and.callFake(function () { });

            scope.LoanApp.State = 'California';
            scope.updateWisconsinOtherIncomeType();

            expect($.fn.prop).toHaveBeenCalledWith('disabled', true);
        });

        it('should not be "non US citizen" when United States selected', function () {
            // Arrange
            var selectedCitizenship = ['UnitedStates', 'AnythingElse'];

            // Act
            var result = scope.isNonUsCitizen(selectedCitizenship);

            // Assert
            expect(result).toBe(false);

        });

        it('should be "non US citizen" when United States not selected', function () {
            // Arrange
            var selectedCitizenship = ['AnythingElse'];

            // Act
            var result = scope.isNonUsCitizen(selectedCitizenship);

            // Assert
            expect(result).toBe(true);
        });

        it('should make "non US citizen" undetermined  when no citizenship selected', function () {

            // Act
            var result = scope.isNonUsCitizen(null);

            // Assert
            expect(result).toBe(null);

            // Act
            result = scope.isNonUsCitizen(undefined);

            // Assert
            expect(result).toBe(null);

            // Act 
            result = scope.isNonUsCitizen([]);

            // Assert
            expect(result).toBe(null);
        });

        it('should indicate citizenship non selected when citizenships has not been set', function () {

            // Arrange
            var applicant = {
                Residence: {
                    Citizenships: null
                }
            };

            // Act
            var result = scope.isCitizenshipSelected(applicant);

            // Assert 
            expect(result).toBe(false);

            // Arrange
            applicant = {
                Residence: {
                    Citizenships: undefined
                }
            };

            // Act
            result = scope.isCitizenshipSelected(applicant);

            // Assert 
            expect(result).toBe(false);

            // Arrange
            applicant = {
                Residence: {
                    Citizenships: []
                }
            };

            // Act
            result = scope.isCitizenshipSelected(applicant);

            // Assert 
            expect(result).toBe(false);

        });

        it('should indicate citizenship selected when citizenships has been selected', function () {

            // Arrange
            var applicant = {
                Residence: {
                    Citizenships: ['AnySelection']
                }
            };

            // Act
            var result = scope.isCitizenshipSelected(applicant);

            // Assert 
            expect(result).toBe(true);

        });

        it('should set is non resident flag to false when citizenship selection changes and new selection includes US', function () {
            httpBackend.flush();

            /* Applicant 1 */

            // Arrange - Ensure defaults
            expect(scope.LoanApp.Applicants[0].Residence.Citizenships).toBe(undefined);
            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(undefined);

            // Act
            scope.LoanApp.Applicants[0].Residence.Citizenships = ["UnitedStates"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe('false');

            /* Applicant 2 */

            // Arrange - Ensure defaults
            expect(scope.LoanApp.Applicants[1].Residence.Citizenships).toBe(undefined);
            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe(undefined);

            // Act
            scope.LoanApp.Applicants[1].Residence.Citizenships = ["UnitedStates"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe('false');
        });

        it('should deselect is non resident flag when citizenship selection changes and previously included the US', function () {
            httpBackend.flush();

            /* Applicant 1 */

            // Arrange - Ensure defaults
            scope.LoanApp.Applicants[0].Residence.Citizenships = ["UnitedStates"];
            scope.$digest();

            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe('false');

            // Act
            scope.LoanApp.Applicants[0].Residence.Citizenships = ["AnythingElse"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(null);

            /* Applicant 2 */

            // Arrange - Ensure defaults
            scope.LoanApp.Applicants[1].Residence.Citizenships = ["UnitedStates"];
            scope.$digest();

            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe('false');

            // Act
            scope.LoanApp.Applicants[1].Residence.Citizenships = ["AnythingElse"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe(null);
        });

        it('should deselect is non resident flag when citizenship selection changes and citizenships were all previously deselected', function () {
            httpBackend.flush();

            /* Applicant 1 */

            // Arrange - Ensure defaults
            var anyNonResidentSelection = 'true';

            scope.LoanApp.Applicants[0].Residence.IsNonResident = anyNonResidentSelection;
            scope.LoanApp.Applicants[0].Residence.Citizenships = [];
            scope.$digest();

            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(anyNonResidentSelection);

            // Act
            scope.LoanApp.Applicants[0].Residence.Citizenships = ["AnythingElse"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(null);

            /* Applicant 2 */

            // Arrange - Ensure defaults
            anyNonResidentSelection = 'true';

            scope.LoanApp.Applicants[1].Residence.IsNonResident = anyNonResidentSelection;
            scope.LoanApp.Applicants[1].Residence.Citizenships = [];
            scope.$digest();

            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe(anyNonResidentSelection);

            // Act
            scope.LoanApp.Applicants[1].Residence.Citizenships = ["AnythingElse"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(null);
        });

        it('should leave is non resident flag untouched when selection changes between non-US citizen selections', function () {
            httpBackend.flush();

            /* Applicant 1 */

            // Arrange - Ensure defaults
            var anyNonResidentSelection = 'true';

            scope.LoanApp.Applicants[0].Residence.IsNonResident = anyNonResidentSelection;
            scope.LoanApp.Applicants[0].Residence.Citizenships = ["Anything"];
            scope.$digest();

            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(anyNonResidentSelection);

            // Act
            scope.LoanApp.Applicants[0].Residence.Citizenships = ["AnythingElse"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[0].Residence.IsNonResident).toBe(anyNonResidentSelection);

            /* Applicant 2 */

            // Arrange - Ensure defaults
            var anyNonResidentSelection = 'true';

            scope.LoanApp.Applicants[1].Residence.IsNonResident = anyNonResidentSelection;
            scope.LoanApp.Applicants[1].Residence.Citizenships = ["Anything"];
            scope.$digest();

            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe(anyNonResidentSelection);

            // Act
            scope.LoanApp.Applicants[1].Residence.Citizenships = ["AnythingElse"];
            scope.$digest();

            // Assert
            expect(scope.LoanApp.Applicants[1].Residence.IsNonResident).toBe(anyNonResidentSelection);

        });
    });

    describe('income', function () {


        describe('other annual income', function () {
            it('should require other annual income when primary applicant retired and not applying jointly', function () {


                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: "Retired" }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: "Retired" }
                        }
                    ]
                };

                httpBackend.flush();

                // Act
                scope.$digest();

                // Assert
                expect(scope.LoanApp.OtherAnnualIncomeRequired).toBe(true);
            });

            it('should not require other annual income when primary applicant not retired and not applying jointly', function () {


                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: "EmployedByOther" }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: "Retired" }
                        }
                    ]
                };

                httpBackend.flush();
                // Act
                scope.$digest();

                // Assert
                expect(scope.LoanApp.OtherAnnualIncomeRequired).toBe(false);
            });

            it('should require other annual income when all applicants are retired and applying jointly', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.LoanApp = {
                    PurposeOfLoan: { Type: 'NotSelected' },
                    ApplicationType: 'Joint',
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: 'Retired' }
                        },
                        {
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: 'Retired' }
                        }
                    ]
                };

                httpBackend.flush();

                // Act
                scope.$digest();

                // Assert
                expect(scope.LoanApp.OtherAnnualIncomeRequired).toBe(true);
            });

            it('should not require other annual income when all applicants are not retired and are applying jointly', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.LoanApp = {
                    PurposeOfLoan: { Type: 'NotSelected' },
                    ApplicationType: 'Joint',
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: 'Retired' }
                        },
                        {
                            Residence: { Address: { ZipCode: '12345' } },
                            Occupation: { Type: 'EmployedByOther' }
                        }
                    ]
                };

                httpBackend.flush();

                // Act
                scope.$digest();

                // Assert
                expect(scope.LoanApp.OtherAnnualIncomeRequired).toBe(false);
            });

            it('should force non-employed applicants to have other annual income', function () {

                httpBackend.whenPOST('/services/statelookup').respond(200);

                // Arrange
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: "Retired" }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: "Retired" }
                        }
                    ]
                };

                httpBackend.flush();

                // Act
                scope.$digest();

                // Assert
                expect(scope.LoanApp.HasOtherAnnualIncome).toBe("Yes");
            });

            it('should clear other annual income when "other annual income" change confirmed', function () {

                // Arrange
                for (var i = 0; i < 5; i += 1) {
                    scope.LoanApp.ApplicationOtherIncome[i] = {
                        Amount: 1
                    };
                }

                // Act
                scope.confirmChangeOtherAnnualIncome();

                // Assert
                for (var j = 0; j < 5; j += 1) {
                    expect(scope.LoanApp.ApplicationOtherIncome[j].Amount).toBe(null);
                }
            });

            describe('frequency', function () {
                var element;

                beforeEach(inject(function ($compile) {
                    element = angular.element(
                        '<form name="LoanApplication">' +
                        '<select id="OtherIncome0Frequency" name="OtherIncome0Frequency" ng-model="LoanApp.ApplicationOtherIncome[0].Frequency" ng-change="otherAnnualIncomeFrequencyChange(0)">' +
                        '<option></option>' +
                        '<option>Monthly</option>' +
                        '<option>Annual</option>' +
                        '<option>Whatever</option>' +
                        '</select>' +
                        '</form>');

                    $compile(element)(scope);
                }));

                it('should calculate annual "other income" amount from frequency amount when frequency changes to monthly', inject(function ($compile) {


                    scope.LoanApp = {
                        PurposeOfLoan: { Type: 'NotSelected' },
                        Applicants: [
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            },
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            }
                        ],

                        // Property under test
                        ApplicationOtherIncome: [{ FrequencyAmount: 1000 }]
                    };

                    element.find('select').val('Monthly').triggerHandler('change');
                    httpBackend.flush();
                    scope.$digest();

                    expect(scope.LoanApp.ApplicationOtherIncome[0].Amount).toBe('12000.00');
                }));

                it('should calculate annual "other income" amount from frequency amount when frequency is set to annual', inject(function ($compile) {
                    // Arrange


                    scope.LoanApp = {
                        PurposeOfLoan: { Type: 'NotSelected' },
                        Applicants: [
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            },
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            }
                        ],

                        // Property set for test
                        ApplicationOtherIncome: [{ FrequencyAmount: 12000 }]
                    };
                    httpBackend.flush();

                    // Act
                    element.find('select').val('Annual').triggerHandler('change');

                    // Assert
                    expect(scope.LoanApp.ApplicationOtherIncome[0].Amount).toBe(12000);
                }));

                it('should calculate annual "other income" amount from frequency amount and assume annual frequency when frequency is set to anything other than monthly', inject(function ($compile) {
                    // Arrange


                    scope.LoanApp = {
                        PurposeOfLoan: { Type: 'NotSelected' },
                        Applicants: [
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            },
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            }
                        ],

                        // Property set for test
                        ApplicationOtherIncome: [{ FrequencyAmount: 12000 }]
                    };
                    httpBackend.flush();

                    // Act
                    element.find('select').val('Whatever').triggerHandler('change');

                    // Assert
                    expect(scope.LoanApp.ApplicationOtherIncome[0].Amount).toBe(12000);
                }));

                it('should not calculate annual "other income" amount and when frequency amount is not set', inject(function ($compile) {
                    // Arrange
                    scope.LoanApp = {
                        PurposeOfLoan: { Type: 'NotSelected' },
                        Applicants: [
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            },
                            {
                                Residence: { Address: { ZipCode: '12345' } },
                                Occupation: { Type: '' }
                            }
                        ],

                        // Property set for test
                        ApplicationOtherIncome: [{ FrequencyAmount: null }]
                    };
                    httpBackend.flush();

                    // Act
                    element.find('select').val('Whatever').triggerHandler('change');

                    // Assert
                    expect(scope.LoanApp.ApplicationOtherIncome[0].Amount).toBeUndefined();
                }));
            });
        });
    });

    describe('employment', function () {

        describe('when changing work status to retired', function () {

            it('should show dialog when occupation description filled out', function () {
                // Arrange
                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                OccupationDescription: "AnyDescription"
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer name filled out', function () {

                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    EmployerName: "AnyEmployerName"
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address line filled out', function () {

                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    Address: {
                                        AddressLine: "AddressLine"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address secondary unit type filled out', function () {
                // Arrange

                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    Address: {
                                        SecondaryUnit: {
                                            Type: "AnyType"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address secondary unit value filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    Address: {
                                        SecondaryUnit: {
                                            Value: "AnyValue"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address city filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    Address: {
                                        City: "AnyCity"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address state filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    Address: {
                                        State: "AnyState"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address zip code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    Address: {
                                        ZipCode: "AnyZIP"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone area code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    PhoneNumber: {
                                        AreaCode: "AnyAreaCode"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone central office code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    PhoneNumber: {
                                        CentralOfficeCode: "AnyCentralOfficeCode"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone line number filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    PhoneNumber: {
                                        LineNumber: "AnyLineNumber"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone line extension filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    PhoneNumber: {
                                        Extension: "AnyLineNumber"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when years with employer filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    TimeWithEmployer: {
                                        Years: "AnyYears"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when months with employer filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    TimeWithEmployer: {
                                        Months: "AnyMonths"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when gross annual income filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Retired",
                                Employer: {
                                    GrossAnnualIncome: 1
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should not show dialog when no employment info has been filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: 'Retired' }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).not.toHaveBeenCalled();
            });
        });

        describe('when changing work status to student', function () {

            it('should show dialog when occupation description filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                OccupationDescription: "AnyDescription"
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer name filled out', function () {

                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    EmployerName: "AnyEmployerName"
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address line filled out', function () {

                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    Address: {
                                        AddressLine: "AddressLine"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address secondary unit type filled out', function () {
                // Arrange

                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    Address: {
                                        SecondaryUnit: {
                                            Type: "AnyType"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address secondary unit value filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    Address: {
                                        SecondaryUnit: {
                                            Value: "AnyValue"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address city filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    Address: {
                                        City: "AnyCity"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address state filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    Address: {
                                        State: "AnyState"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address zip code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    Address: {
                                        ZipCode: "AnyZIP"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone area code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    PhoneNumber: {
                                        AreaCode: "AnyAreaCode"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone central office code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    PhoneNumber: {
                                        CentralOfficeCode: "AnyCentralOfficeCode"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone line number filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    PhoneNumber: {
                                        LineNumber: "AnyLineNumber"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone line extension filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    PhoneNumber: {
                                        Extension: "AnyLineNumber"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when years with employer filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    TimeWithEmployer: {
                                        Years: "AnyYears"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when months with employer filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    TimeWithEmployer: {
                                        Months: "AnyMonths"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when gross annual income filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "Student",
                                Employer: {
                                    GrossAnnualIncome: 1
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should not show dialog when no employment info has been filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: 'Student' }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).not.toHaveBeenCalled();
            });
        });

        describe('when changing work status to homemaker', function () {

            it('should show dialog when occupation description filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                OccupationDescription: "AnyDescription"
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer name filled out', function () {

                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    EmployerName: "AnyEmployerName"
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address line filled out', function () {

                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    Address: {
                                        AddressLine: "AddressLine"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address secondary unit type filled out', function () {
                // Arrange

                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    Address: {
                                        SecondaryUnit: {
                                            Type: "AnyType"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address secondary unit value filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    Address: {
                                        SecondaryUnit: {
                                            Value: "AnyValue"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address city filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    Address: {
                                        City: "AnyCity"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address state filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    Address: {
                                        State: "AnyState"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address zip code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    Address: {
                                        ZipCode: "AnyZIP"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone area code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    PhoneNumber: {
                                        AreaCode: "AnyAreaCode"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone central office code filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    PhoneNumber: {
                                        CentralOfficeCode: "AnyCentralOfficeCode"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone line number filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    PhoneNumber: {
                                        LineNumber: "AnyLineNumber"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when employer address phone line extension filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    PhoneNumber: {
                                        Extension: "AnyLineNumber"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when years with employer filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    TimeWithEmployer: {
                                        Years: "AnyYears"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when months with employer filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    TimeWithEmployer: {
                                        Months: "AnyMonths"
                                    }
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should show dialog when gross annual income filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: {
                                Type: "NotEmployed",
                                Employer: {
                                    GrossAnnualIncome: 1
                                }
                            }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).toHaveBeenCalledWith('open');
            });

            it('should not show dialog when no employment info has been filled out', function () {
                // Arrange


                scope.LoanApp = {
                    PurposeOfLoan: { Type: "NotSelected" },
                    Applicants: [
                        {
                            Residence: { Address: { ZipCode: "12345" } },
                            Occupation: { Type: 'NotEmployed' }
                        },
                        {
                            Residence: { Address: { ZipCode: "12345" } }
                        }]
                };

                httpBackend.flush();
                scope.$digest();

                spyOn($.fn, 'foundation').and.callFake(function () { });

                // Act
                scope.workStatusChanged(0, 'Applicant');

                // Assert
                expect($.fn.foundation).not.toHaveBeenCalled();
            });
        });

    });

    describe('submitting loan app', function () {
        var element;

        beforeEach(inject(function ($compile) {
            element = angular.element(
                '<form name="LoanApplication">' +
                '</form>');

            $compile(element)(scope);
        }));

        it('should default current step to step 4 when current step is not set and then corresponding skipped step\'s validity', function () {

            // Arrange
            var expectedValue = scope.LoanApplication.$valid;
            scope.LoanApp.SkippedSteps = [{}, {}, {}, {}];
            scope.Loading = true;

            // Act
            scope.submitLoanApp();

            // Assert
            expect(scope.LoanApp.SkippedSteps[4]).toBe(expectedValue);
        });

        it('should set an error message if any step has been skipped', function () {
            // Arrange
            var expectedErrorMessage = 'Please complete this page before submitting your loan application';
            scope.LoanApp.SkippedSteps = [false, {}, {}, {}];
            scope.Loading = true;

            // Act
            expect(scope.LoanApp.ErrorMessage).toBeFalsy();
            scope.submitLoanApp();

            // Assert
            expect(scope.LoanApp.ErrorMessage).toBe(expectedErrorMessage);
        });

        it('should save loan app if any step has been skipped', function () {
            // Arrange
            scope.LoanApp.SkippedSteps = [false, {}, {}, {}];
            scope.Loading = true;

            // Act
            spyOn(scope, 'saveLoanApp');
            scope.submitLoanApp();

            // Assert
            expect(scope.saveLoanApp).toHaveBeenCalled();
            expect(scope.saveLoanApp.calls.count()).toBe(2);  // There may be an extra, unnecessary call here, but this is what the code does as of print time
        });

        it('should redirect if any step has been skipped', function () {
            // Arrange
            scope.LoanApp.SkippedSteps = [false, {}, {}, {}];
            scope.Loading = true;

            // Act
            scope.submitLoanApp();

            // Assert
            expect(window.location.href).toBe('./gotostep?step=0');
        });

        it('should return false if any step has been skipped', function () {
            // Arrange
            scope.LoanApp.SkippedSteps = [false, {}, {}, {}];
            scope.Loading = true;

            // Act
            var result = scope.submitLoanApp();

            // Assert
            expect(result).toBe(false);
        });

        it('should set loading to false after submitting loan app', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { Success: true });

            // Act
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.Loading).toBe(false);
        });

        it('should clear error fields after submitting loan app', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';
            scope.Loading = null;
            scope.LoanApp.ErrorMessage = 'ErrorMessage';
            scope.LoanApp.ErrorFields = 'ErrorFields';
            scope.LoanApp.AlertMessage = 'AlertMessage';

            httpBackend.expectPOST(aSubmitUri).respond(200, { Success: true });

            // Act
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.LoanApp.ErrorMessage).toBe(null);
            expect(scope.LoanApp.ErrorFields).toBe(null);
            expect(scope.LoanApp.AlertMessage).toBe(null);
        });

        it('should delete loan app after submitting loan app', function () {
            // Arrange
            var aSubmitUri = 'a/submit/uri';
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { Success: true });

            // Act
            spyOn(scope, 'deleteLoanApp');
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.deleteLoanApp).toHaveBeenCalled();
            expect(scope.deleteLoanApp.calls.count()).toBe(1);
        });

        it('should modify browser back navigation to send to home page after submitting loan app ', function () {
            // Arrange
            var aSubmitUri = 'a/submit/uri';
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { Success: true });

            // Act
            spyOn(window.history, 'replaceState');
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(window.history.replaceState).toHaveBeenCalledWith({}, 'Home Page', '/');
        });

        it('should modify browser back navigation to send to specified page after submitting loan app ', function () {
            // Arrange
            var aSubmitUri = 'a/submit/uri';
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { Success: true, BackButtonLocation: 'specificLocation' });

            // Act
            spyOn(window.history, 'replaceState');
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(window.history.replaceState).toHaveBeenCalledWith({}, 'Home Page', 'specificLocation');
        });

        it('should set error messages when submit result is not success', function () {
            // Arrange
            var aSubmitUri = 'a/submit/uri';
            var expectedErrorMessage = 'expectedErrorMessage';
            var expectedErrorFields = 'expectedErrorFields';

            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { ErrorMessage: expectedErrorMessage, ErrorFields: expectedErrorFields });

            // Act
            expect(scope.LoanApp.ErrorMessage).toBeFalsy();
            expect(scope.LoanApp.ErrorFields).toBeFalsy();

            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.LoanApp.ErrorMessage).toBe(expectedErrorMessage);
            expect(scope.LoanApp.ErrorFields).toBe(expectedErrorFields);
        });

        it('should save loan app when submit result is not success', function () {
            // Arrange
            var aSubmitUri = 'a/submit/uri';

            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, {});

            // Act
            spyOn(scope, 'saveLoanApp');
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.saveLoanApp).toHaveBeenCalled();
            expect(scope.saveLoanApp.calls.count()).toBe(2);
        });

        it('should redirect submit result is not success and redirect location specified', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';
            var expectedRedirectLocation = "some/redirect/location";
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { Redirect: expectedRedirectLocation });

            // Act
            expect(window.location.href).toBe('');

            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(window.location.href).toBe('./' + expectedRedirectLocation);
        });

        it('should not redirect when submit result is not success and redirect location is "None"', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';

            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, { Redirect: 'None' });

            // Act
            expect(window.location.href).toBe('');

            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(window.location.href).toBe('');
        });

        it('should not redirect when submit result is not success and redirect location not specified', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';

            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(200, {});

            // Act
            expect(window.location.href).toBe('');

            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(window.location.href).toBe('');
        });

        it('should set loading to false when error occurs on submission', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(500, {});

            // Act
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.Loading).toBe(false);
        });

        it('should set expected error message when error occurs on submission', function () {

            // Arrange
            var aSubmitUri = 'a/submit/uri';
            var expectedErrorMessage = 'We\'re sorry, but there was an error submitting your application.';
            scope.Loading = null;

            httpBackend.expectPOST(aSubmitUri).respond(500, {});

            // Act
            expect(scope.LoanApp.ErrorMessage).toBeFalsy();
            scope.submitLoanApp(aSubmitUri);
            httpBackend.flush();

            // Assert
            expect(scope.LoanApp.ErrorMessage).toBe(expectedErrorMessage);
        });
    });

});


describe('When getting rates', function () {
    var scope, ctrl, httpBackend, q, stateService, validationService, window;

    var mockLoanAppSessionService = {
        load: function () {
            var loanApp = {
                PurposeOfLoan: {
                    Type: 'NotSelected'
                },
                LoanPaymentType: 'AutoPay',
                ApplicationType: 'NotSelected',
                CombinedFinancials: {},
                Applicants: [{
                    Residence: {
                        Ownership: 'NotSelected',
                        Address: {}
                    },
                    Occupation: {}
                }, {
                    Residence: {
                        Ownership: 'NotSelected',
                        Address: {}
                    },
                    Occupation: {}
                }],
                ApplicantHmdaData: [{ ApplicantRace: [] }, { ApplicantRace: [] }],
                ApplicationOtherIncome: [{}, {}, {}, {}],
                HmdaComplianceProperty: {
                    Address: {
                        SecondaryUnit: {}
                    }
                },
                UserCredentials: {
                    IsTemporary: false
                },
                FACTHistory: [],
                FACTData: {},
                UserIdNotReserved: false,
                ApplicationTakenOverPhone: false
            };

            if (loanApp.ZipCode && !loanApp.Applicants[0].Residence.Address.ZipCode) {
                loanApp.Applicants[0].Residence.Address.ZipCode = loanApp.ZipCode;
            }
            if (loanApp.State && (!loanApp.Applicants[0].Residence.Address.State || loanApp.Applicants[0].Residence.Address.State === 'NotSelected')) {
                loanApp.Applicants[0].Residence.Address.State = loanApp.State;
            }

            return loanApp;
        },
        remove: function () { },
        save: function () { },
        start: function () { },
        cleanup: function () { }
    };

    beforeEach(module('LoanAppModule'));

    beforeEach(inject(function ($rootScope, $controller, $httpBackend, $q, $compile, _stateService_, _validationService_) {

        httpBackend = $httpBackend;
        q = $q;

        stateService = _stateService_;
        validationService = _validationService_;

        var mockStaticLookupsService = {
            getStaticLookups: function () {
                var deferred = q.defer();
                deferred.resolve({});
                return deferred.promise;
            }
        };

        scope = $rootScope.$new();

        window = {
            location: { href: '' },
            history: {
                replaceState: function () { }
            },
            setInterval: function () { }
        }

        ctrl = $controller('LoanApplicationController',
            {
                $scope: scope,
                $window: window,
                loanAppSessionService: mockLoanAppSessionService,
                staticLookupService: mockStaticLookupsService
            });
    }));

    beforeEach(function () {
        // This is needed for all tests bc initialization of loan app module makes service call to gets rates
        //httpBackend.whenPOST('/services/rates').respond(200, { QueueId: 1 });
        httpBackend.whenGET('/lookups/ofaccountries').respond(200);
        httpBackend.whenGET('/services/getlatestloantermrequest/').respond(200);
    });

    afterEach(function () {
        httpBackend.verifyNoOutstandingRequest();
        httpBackend.verifyNoOutstandingExpectation();
    });

    describe('rates', function () {


        // NOTE: I don't see any need for these default values.  
        // They cause strange issues with binding contributing to Bug 63427
        // Should we discover a need for this default amounts, I will investigate further.

        //it('should set default minimum loan amount to 5000', function () {
        //    expect(scope.rates.MinLoanAmount).toBe(5000);
        //});

        //it('should set default max loan amount to 100000', function () {
        //    expect(scope.rates.MaxLoanAmount).toBe(100000);
        //});

        //it('should set default min term amount to 24', function () {
        //    expect(scope.rates.MinTerm).toBe(24);
        //});

        //it('should set default max term amount to 144', function () {
        //    expect(scope.rates.MaxTerm).toBe(144);
        //});

        var processRatesQueue = function (scope) {
            scope.rateRequestQueue.forEach(function (queueItem) {
                scope.rateResponseQueue.push(queueItem);
                scope.processRatesQueue(1);
            });
        }

        it('should only be allowed to get rates when the loan purpose type is not unselected', function () {

            // Arrange
            httpBackend.whenPOST('/services/rates').respond(200, { QueueId: 1 });
            scope.LoanApp.PurposeOfLoan.Type = "NotUnSelected";

            // Act
            var result = scope.canGetRates();

            // Assert
            expect(result).toBe(true);

        });

        it('should not be allowed to get rates when the loan purpose type is not selected', function () {

            // Arrange
            httpBackend.whenPOST('/services/rates').respond(200, { QueueId: 1 });
            scope.LoanApp.PurposeOfLoan.Type = "NotSelected";

            // Act
            var result = scope.canGetRates();

            // Assert
            expect(result).toBe(false);
        });

        it('should set loan data when getting rates', function () {

            // Arrange
            var fakeResponseObject = {
                QueueId: 1,
                FirstAgainCodeTrackingId: 1234
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";

            httpBackend.flush();

            // Act 
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanAppRates.FirstAgainCodeTrackingId).toBe(1234);
            expect(scope.rates.FirstAgainCodeTrackingId).toBe(1234);
        });

        it('should set accurate interest rate when there is a rate and product rate', function () {

            // Arrange
            var fakeResponseObject = {
                Rate: {},
                ProductRate: 0.01750
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanApp.InterestRate).toBe("1.7500");
        });

        it('should set accurate "interest rate from" when there is a rate and product rate', function () {

            // Arrange
            var fakeResponseObject = {
                Rate: { Min: 0.01750 },
                ProductRate: 0.01750
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanApp.InterestRateFrom).toBe("1.7500");
        });

        it('should set accurate "interest rate to" when there is a rate and product rate', function () {

            // Arrange
            var fakeResponseObject = {
                Rate: { Max: 0.03290 },
                ProductRate: 0.01750
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanApp.InterestRateTo).toBe("3.2900");
        });

        it('should set set loan app lock rate with when there is not already a lock rate set', function () {

            // Arrange
            var anyResponseDate = new Date().toString();
            var fakeResponseObject = {
                Rate: {},
                ProductRate: 0.01750,
                RateLockDate: anyResponseDate
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";
            scope.LoanApp.RateLockDate = null;

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanApp.RateLockDate).toBe(anyResponseDate);
        });

        it('should not set loan app lock rate with when there is a lock rate already set', function () {
            // Arrange
            var anyResponseDate = new Date();
            var fakeResponseObject = {
                Rate: {},
                ProductRate: 0.01750,
                RateLockDate: anyResponseDate.toString()
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";

            var aPreviouslySetDate = new Date();
            aPreviouslySetDate.setDate(anyResponseDate.getDate() + 1);

            scope.LoanApp.RateLockDate = aPreviouslySetDate.toString();

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);
            // Assert
            expect(scope.LoanApp.RateLockDate).toBe(anyResponseDate.toString());
            expect(scope.LoanApp.RateLockDate).not.toBe(aPreviouslySetDate.toString());
        });

        it('should invalidate loan amount when loan amount is less than minimum loan amount (and there is no rate data)', function () {
            // Arrange
            var fakeResponseObject = {
                MinLoanAmount: "2"
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.LoanAmount = 1;
            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";
            scope.LoanAmountInvalidated = undefined;

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanAmountInvalidated).toBe(true);
        });

        it('should not change loan amount invalidation when loan amount meets minimum loan amount (and there is no rate data)', function () {
            // Arrange
            var fakeResponseObject = {
                MinLoanAmount: "1"
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.LoanAmount = 1;
            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";
            scope.LoanAmountInvalidated = undefined;

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanAmountInvalidated).toBe(undefined);
        });

        it('should not invalidate loan amount there is rate data', function () {
            // Arrange
            var fakeResponseObject = {
                Rate: {},
                ProductRate: 0.01750,
            };

            httpBackend.whenPOST('/services/rates').respond(200, fakeResponseObject);

            scope.LoanApp.PurposeOfLoan.Type = "AnySelectedLoanPurpose";
            scope.LoanAmountInvalidated = undefined;

            httpBackend.flush();

            // Act
            httpBackend.expectPOST('/services/rates').respond(200, fakeResponseObject);

            scope.getRates();

            httpBackend.flush();

            processRatesQueue(scope);

            // Assert
            expect(scope.LoanAmountInvalidated).toBe(false);
        });

        // TODO: Add tests for rate table data ?
    });

});

describe('When initiating loan application', function () {
    var scope, ctrl, httpBackend, q, window;

    var initialLoanApp;

    var mockLoanAppSessionService = {
        save: function () { },
        start: function () { },
        load: function () {
            return initialLoanApp;
        },

        cleanup: function () {
        }
    };

    var mockOfacCountriesService = {
        getOfacCountries: function () {
            var deferred = q.defer();
            deferred.resolve({});
            return deferred.promise;
        }
    };

    beforeEach(module('LoanAppModule'));

    beforeEach(function () {
        initialLoanApp = {
            PurposeOfLoan: {
                Type: 'NotSelected'
            },
            LoanPaymentType: 'AutoPay',
            ApplicationType: 'NotSelected',
            CombinedFinancials: {},
            Applicants: [{
                Residence: {
                    Ownership: 'NotSelected',
                    Address: {}
                },
                Occupation: {}
            }, {
                Residence: {
                    Ownership: 'NotSelected',
                    Address: {}
                },
                Occupation: {}
            }],
            ApplicantHmdaData: [{ ApplicantRace: [] }, { ApplicantRace: [] }],
            ApplicationOtherIncome: [{}, {}, {}, {}],
            HmdaComplianceProperty: {
                Address: {
                    SecondaryUnit: {}
                }
            },
            UserCredentials: {
                IsTemporary: false
            },
            FACTData: {},
            FACTHistory: [],
            UserIdNotReserved: false,
            ApplicationTakenOverPhone: false
        };

        if (initialLoanApp.ZipCode && !initialLoanApp.Applicants[0].Residence.Address.ZipCode) {
            initialLoanApp.Applicants[0].Residence.Address.ZipCode = initialLoanApp.ZipCode;
        }
        if (initialLoanApp.State && (!initialLoanApp.Applicants[0].Residence.Address.State || initialLoanApp.Applicants[0].Residence.Address.State === 'NotSelected')) {
            initialLoanApp.Applicants[0].Residence.Address.State = initialLoanApp.State;
        }
    });

    beforeEach(inject(function ($rootScope, $controller, $httpBackend, $q) {

        httpBackend = $httpBackend;
        q = $q;

        scope = $rootScope.$new();

        window = { location: { href: '' } }
    }));

    it('should set frequency amount when loan app is initiated with other income', inject(function ($controller) {

        // Arrange
        for (var i = 0; i < 5; i += 1) {
            initialLoanApp.ApplicationOtherIncome[i] = {
                Amount: 1
            };
        }

        // Act
        expect(scope.LoanApp).toBeUndefined();

        ctrl = $controller('LoanApplicationController',
            {
                $scope: scope,
                $window: window,
                loanAppSessionService: mockLoanAppSessionService,
                ofacCountriesService: mockOfacCountriesService
            });

        // Assert
        for (var j = 0; j < 5; j += 1) {
            expect(scope.LoanApp.ApplicationOtherIncome[j].FrequencyAmount).toBe(1);
        };

    }));

    it('should set frequency when loan app is initiated with other income', inject(function ($controller) {

        // Arrange
        for (var i = 0; i < 5; i += 1) {
            initialLoanApp.ApplicationOtherIncome[i] = {
                Amount: 1
            };
        }

        // Act
        expect(scope.LoanApp).toBeUndefined();

        ctrl = $controller('LoanApplicationController',
            {
                $scope: scope,
                $window: window,
                loanAppSessionService: mockLoanAppSessionService,
                ofacCountriesService: mockOfacCountriesService
            });

        // Assert
        for (var j = 0; j < 5; j += 1) {
            expect(scope.LoanApp.ApplicationOtherIncome[j].Frequency).toBe('Annual');
        };

    }));

    it('should set "has other annual income" flag when loan app is initiated with other income', inject(function ($controller) {

        // Arrange
        for (var i = 0; i < 5; i += 1) {
            initialLoanApp.ApplicationOtherIncome[i] = {
                Amount: 1
            };
        }

        // Act
        expect(scope.LoanApp).toBeUndefined();

        ctrl = $controller('LoanApplicationController',
            {
                $scope: scope,
                $window: window,
                loanAppSessionService: mockLoanAppSessionService,
                ofacCountriesService: mockOfacCountriesService
            });

        // Assert
        expect(scope.LoanApp.HasOtherAnnualIncome).toBe('Yes');
    }));
});