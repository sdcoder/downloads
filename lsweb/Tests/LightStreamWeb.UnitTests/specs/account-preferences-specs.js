/// <reference path="../scripts/jasmine-2.3.4/jasmine.js" />
/// <reference path="../scripts/jasmine-2.3.4/boot.js" />
/// <reference path="../scripts/angular/angular.js" />
/// <reference path="../scripts/angular/angular-route.js" />
/// <reference path="../scripts/angular/angular-mocks.js" />
/// <reference path="../Scripts/angular/angular-webstorage.js" />
/// <reference path="../../../webroot/scripts/app/ls.directives.js" />
/// <reference path="../../../webroot/scripts/app/ls.services.js" />
/// <reference path="../../../webroot/scripts/app/ls.filters.js" />
/// <reference path="../../../webroot/scripts/app/accountpreferences.js" />
/// <reference path="../../../webroot/scripts/app/preferences/preferences.js" />
/// <reference path="../../../webroot/scripts/app/preferences/contactInformationController.js" />
/// <reference path="../../../webroot/scripts/app/preferences/accountLockController.js" />
/// <reference path="../../../webroot/scripts/app/preferences/privacyPreferencesController.js" />
/// <reference path="../../../webroot/scripts/app/preferences/securityInformationController.js" />

describe('account preferences', function () {

    beforeEach(module('accountPreferences'));

    it('should map routes to controllers', function () {
        inject(function($route) {
            expect($route.routes['/contactInformation'].templateUrl).toEqual('scripts/templates/contactInformation-index.html');

            expect($route.routes['/contactInformation/updateEmail'].templateUrl).toEqual('scripts/templates/contactInformation-updateEmail.html');

            expect($route.routes['/accountLock'].templateUrl).toEqual('scripts/templates/accountLock-index.html');

            expect($route.routes['/privacyPreferences'].templateUrl).toEqual('scripts/templates/privacyPreferences-index.html');

            expect($route.routes['/securityInformation'].templateUrl).toEqual('scripts/templates/securityInformation-index.html');

            expect($route.routes['/securityInformation/changeUserId'].templateUrl).toEqual('scripts/templates/securityInformation/changeUserId-index.html');

            expect($route.routes['/securityInformation/changeUserId/new'].templateUrl).toEqual('scripts/templates/securityInformation/changeUserId-new.html');

            expect($route.routes['/securityInformation/changeUserId/move'].templateUrl).toEqual('scripts/templates/securityInformation/changeUserId-move.html');

            expect($route.routes['/securityInformation/changeUserId/accountsync'].templateUrl).toEqual('scripts/templates/securityInformation/changeUserId-accountsync.html');

            expect($route.routes['/securityInformation/changePassword'].templateUrl).toEqual('scripts/templates/securityInformation/changePassword-index.html');

            expect($route.routes['/securityInformation/changeSecurityQuestion'].templateUrl).toEqual('scripts/templates/securityInformation/changeSecurityQuestion-index.html');

            //default route
            expect($route.routes[null].redirectTo).toEqual('/');
        });
    });

    describe('accountPreferencesController', function () {
        var controller;
        var scope;

        beforeEach(inject(function ($rootScope, $controller) {
            scope = $rootScope.$new();
            controller = $controller('accountPreferencesController', { '$scope': scope });
        }));

        it("is bar", function () {
            expect(true).toBeTruthy();
        });
    });

    describe("security information controller", function() {
        var controller;
        var scope;

        beforeEach(inject(function($rootScope, $controller) {
            scope = $rootScope.$new();
            controller = $controller("securityInformationController", { "$scope": scope });
        }));

        it("should close alert message", function() {
            scope.Model = { alert: { type: "foo", message: "bar" } };
            scope.closeAlertMessage();
            expect(scope.Model.alert).toBeNull();
        });
    });

    describe('preferences service', function () {
        var preferences, httpBackend;

        beforeEach(inject(function(_preferences_, $httpBackend) {
            preferences = _preferences_;
            httpBackend = $httpBackend;
        }));

        it("should load account preferences data", function () {
            httpBackend.whenGET("/profile/load").respond(200, { Applicant: {}, CoApplicant: {}, Applications: [] });
            preferences.loadPreferenceData().then(function (data) {
                expect(data).toEqual(true);
            });

            httpBackend.flush();
        });

        it("should save contact information", function() {
            var contactInformationData = {
                userId: 1234,
                applicantId: 5678,
                applicantModel: {}
            };

            httpBackend.expectPOST("/profile/updatecontactinformation", contactInformationData).respond(200, { success: "true" });
            preferences.updateContactInformation(contactInformationData).then(function (data) {
                expect(data).toEqual({ success: "true" });
            });

            httpBackend.flush();
        });

        it("should show error if failure received when saving contact information", function () {
            var contactInformationData = {
                userId: 1234,
                applicantId: 5678,
                applicantModel: {}
            };

            httpBackend.expectPOST("/profile/updatecontactinformation", contactInformationData).respond(500);
            preferences.updateContactInformation(contactInformationData).then(null, function(response) {
                expect(response.status).toEqual(500);
            });

            httpBackend.flush();
        });

        afterEach(function() {
            httpBackend.verifyNoOutstandingRequest();
            httpBackend.verifyNoOutstandingExpectation();
        });
    });
});