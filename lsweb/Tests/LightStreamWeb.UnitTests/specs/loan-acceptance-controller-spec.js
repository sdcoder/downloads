/// <reference path="../scripts/jasmine-2.3.4/jasmine.js" />
/// <reference path="../scripts/jasmine-2.3.4/jasmine-html.js" />
/// <reference path="../scripts/jasmine-2.3.4/boot.js" />
/// <reference path="../Scripts/angular/angular.js" />
/// <reference path="../Scripts/angular/angular-webstorage.js" />
/// <reference path="../Scripts/angular/angular-route.js" />
/// <reference path="../Scripts/angular/angular-mocks.js" />
/// <reference path="../../../webroot/scripts/jquery/jquery-2.2.4.min.js" />
/// <reference path="../../../webroot/scripts/jquery-plugins/jquery.signaturepad.js" />
/// <reference path="../../../webroot/scripts/foundation/foundation.js" />

/// <reference path="../../../webroot/scripts/angular-file-upload/angular-file-upload.js" />
/// <reference path="../../../webroot/scripts/app/ls.filters.js" />
/// <reference path="../../../webroot/scripts/app/ls.services.js" />
/// <reference path="../../../webroot/scripts/app/ls.directives.js" />
/// <reference path="../../../webroot/scripts/app/ApplicationStatusModule.js" />
/// <reference path="../../../webroot/scripts/app/LoanAcceptanceController.js" />

describe('When accepting a loan', function () {
    var scope, ctrl, httpBackend;

    beforeEach(module('ApplicationStatusModule'));

    beforeEach(inject(function ($rootScope, $controller, $httpBackend) {
        httpBackend = $httpBackend;
        scope = $rootScope.$new();

        ctrl = $controller('LoanAcceptanceController',
            {
                $scope: scope
            });
    }));

    beforeEach(function () {
        // these service calls are made during controller initialization
        httpBackend.whenPOST('/appstatus/loadloanacceptancedata').respond(200);
        httpBackend.whenGET('/services/getlatestloantermrequest/').respond(200);
        httpBackend.whenGET('TabReview').respond(200);

    });

    afterEach(function () {
        httpBackend.verifyNoOutstandingRequest();
        httpBackend.verifyNoOutstandingExpectation();
    });

    describe('initialiation', function () {

        it('should initialize with SignatureAcknowledgement properties as false', function () {
            expect(scope.LoanAcceptance).toEqual({
                RequiresApplicantSignatureAcknowledgement: false,
                RequiresCoApplicantSignatureAcknowledgement: false
            });
        });

        it('should default with SignatureAcknowledgement properties as false', function () {
            expect(scope.LoanAcceptance).toEqual({
                RequiresApplicantSignatureAcknowledgement: false,
                RequiresCoApplicantSignatureAcknowledgement: false
            });
        });

    });

});