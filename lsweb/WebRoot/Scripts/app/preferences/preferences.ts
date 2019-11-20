/*globals angular */
(function () {
    'use strict';

    var preferences = function ($http, $q) {
        var preferenceData = null,

            createContactInformationModel = function () {
                var contactInformationModel = {
                    UserId: preferenceData.UserId,
                    CanUpdate: preferenceData.CanUpdate,
                    Applicant: {
                        ApplicantId: preferenceData.Applicant.ApplicantId,
                        ContactInformation: preferenceData.Applicant.ContactInformation
                    },
                    CoApplicant: undefined,
                    HasCoApplicant: preferenceData.HasCoApplicant,
                    States: preferenceData.States,
                    SecondaryUnitTypes: preferenceData.SecondaryUnitTypes
                };

                if (contactInformationModel.Applicant.ContactInformation.UnitType == 'NotSelected') {
                    contactInformationModel.Applicant.ContactInformation.UnitType = '';
                }

                if (contactInformationModel.HasCoApplicant) {
                    contactInformationModel.CoApplicant = {
                        ApplicantId: preferenceData.CoApplicant.ApplicantId,
                        ContactInformation: preferenceData.CoApplicant.ContactInformation
                    };

                    if (contactInformationModel.CoApplicant.ContactInformation.UnitType == 'NotSelected') {
                        contactInformationModel.CoApplicant.ContactInformation.UnitType = '';
                    }
                }

                return contactInformationModel;
            },

            createPrivacyPreferencesModel = function () {
                var privacyPreferencesModel = {
                    UserId: preferenceData.UserId,
                    Applicant: {
                        ApplicantId: preferenceData.Applicant.ApplicantId,
                        FullName: preferenceData.Applicant.ContactInformation.FullName,
                        PrivacyPreferences: preferenceData.Applicant.PrivacyPreferences,
                        EmailPreferences: preferenceData.Applicant.EmailPreferences
                    },
                    CoApplicant: undefined,
                    HasCoApplicant: preferenceData.HasCoApplicant
                };

                if (privacyPreferencesModel.HasCoApplicant) {
                    privacyPreferencesModel.CoApplicant = {
                        ApplicantId: preferenceData.CoApplicant.ApplicantId,
                        FullName: preferenceData.CoApplicant.ContactInformation.FullName,
                        PrivacyPreferences: preferenceData.CoApplicant.PrivacyPreferences,
                        EmailPreferences: preferenceData.CoApplicant.EmailPreferences
                    };
                }

                return privacyPreferencesModel;
            },

            onSuccess = function (response) {
                preferenceData = null;
                return response.data;
            },

            changeUserId = function (changeUserIdData) {
                return $http.post("/Profile/ChangeUserId", changeUserIdData).then(onSuccess);
            },

            loadPreferenceData = function () {
                if (!preferenceData) {
                    return $http.get("/Profile/Load").then(function (response) {
                        preferenceData = response.data;
                        return true;
                    });
                } //else {

                var deferred = $q.defer(),
                    promise = deferred.promise;

                deferred.resolve();
                return promise.then(function () { return false; });
            },

            getContactInformation = function () {
                if (preferenceData) {
                    return createContactInformationModel();
                } //else {
                return {
                    CanUpdate: true,
                    Applicant: null
                };
            },

            getAccountLock = function () {
                if (preferenceData) {
                    return {
                        UserId: preferenceData.UserId,
                        AccountLock: {
                            IsEnabled: preferenceData.IsAccountLockEnabled
                        }
                    };
                } //else {

                return {};
            },

            getPrivacyPreferences = function () {
                if (preferenceData) {
                    return createPrivacyPreferencesModel();
                } //else {

                return {};
            },

            getSecurityInformation = function () {
                if (preferenceData) {
                    return {
                        UserId: preferenceData.UserId,
                        CanUpdate: preferenceData.CanUpdate,
                        Applications: preferenceData.Applications,
                        SecurityQuestionTypes: preferenceData.SecurityQuestionTypes,
                        SecurityHint: preferenceData.SecurityHint,
                        PasswordHint: preferenceData.PasswordHint,
                        UserIdHint: preferenceData.UserIdHint,
                        PasswordErrorHint: preferenceData.PasswordErrorHint,
                        PasswordNoMatchHint: preferenceData.PasswordNoMatchHint
                    };
                } //else {

                return {
                    CanUpdate: true,
                    Applications: null
                };
            },

            updateContactInformation = function (contactInformationData) {
                return $http.post("/Profile/UpdateContactInformation", contactInformationData).then(onSuccess);
            },

            updateAccountLock = function (accountLockData) {
                return $http.post("/Profile/UpdateAccountLock", accountLockData).then(onSuccess);
            },

            updatePrivacyPreferences = function (privacyPreferencesData) {
                return $http.post("/Profile/UpdatePrivacyPreferences", privacyPreferencesData).then(onSuccess);
            },

            updateSecurityQuestionAndAnswer = function (securityQuestionAndAnswerData) {
                return $http.post('/Profile/UpdateSecurityQuestionAndAnswer', securityQuestionAndAnswerData).then(function (response) { return response.data; });
            },

            updatePassword = function (passwordData) {
                return $http.post('/Profile/UpdatePassword', passwordData).then(function (response) { return response.data; });
            },

            moveAccount = function (moveAccountData) {
                return $http.post('/Profile/MoveAccount', moveAccountData).then(onSuccess);
            },

            getAccountSyncData = function (accountSyncData) {
                return $http.post('/Profile/GetAccountSync', accountSyncData).then(onSuccess);
            },

            syncAndMoveAccount = function(syncAndMoveData) {
                return $http.post('/Profile/SyncAndMoveAccount', syncAndMoveData).then(onSuccess);
            };

        

        return {
            changeUserId: changeUserId,
            loadPreferenceData: loadPreferenceData,
            getAccountLock: getAccountLock,
            getContactInformation: getContactInformation,
            getPrivacyPreferences: getPrivacyPreferences,
            getSecurityInformation: getSecurityInformation,
            moveAccount: moveAccount,
            updateAccountLock: updateAccountLock,
            updateContactInformation: updateContactInformation,
            updatePassword: updatePassword,
            updatePrivacyPreferences: updatePrivacyPreferences,
            updateSecurityQuestionAndAnswer: updateSecurityQuestionAndAnswer,
            getAccountSyncData: getAccountSyncData,
            syncAndMoveAccount: syncAndMoveAccount
        };
    };

    angular.module("accountPreferences").factory("preferences", ["$http", "$q", preferences]);
}());