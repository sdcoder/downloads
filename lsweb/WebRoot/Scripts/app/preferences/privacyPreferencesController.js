/*globals angular */
(function () {
    'use strict';
    var privacyPreferencesController = function ($scope, $location, $sce, preferences) {
        var onError = function () {
            $scope.alert = { type: "alert", message: "There was an error updating your data. Please try again." };
            $scope.waiting = false;
        };
        $scope.$watch('Model.AreApplicantPreferencesTheSame', function (newValue) {
            if (newValue === undefined) {
                return;
            }
            if (newValue) {
                $scope.Model.Applicant.PrivacyPreferences = angular.copy($scope.Model.CoApplicant.PrivacyPreferences);
                $scope.Model.Applicant.EmailPreferences = angular.copy($scope.Model.CoApplicant.EmailPreferences);
            }
            else {
                $scope.Model.Applicant.PrivacyPreferences = angular.copy($scope.Model.OriginalApplicant.PrivacyPreferences);
                $scope.Model.Applicant.EmailPreferences = angular.copy($scope.Model.OriginalApplicant.EmailPreferences);
            }
        });
        $scope.$watch('Model.AreCoApplicantPreferencesTheSame', function (newValue) {
            if (newValue === undefined) {
                return;
            }
            if (newValue) {
                $scope.Model.CoApplicant.PrivacyPreferences = angular.copy($scope.Model.Applicant.PrivacyPreferences);
                $scope.Model.CoApplicant.EmailPreferences = angular.copy($scope.Model.Applicant.EmailPreferences);
            }
            else {
                $scope.Model.CoApplicant.PrivacyPreferences = angular.copy($scope.Model.OriginalCoApplicant.PrivacyPreferences);
                $scope.Model.CoApplicant.EmailPreferences = angular.copy($scope.Model.OriginalCoApplicant.EmailPreferences);
            }
        });
        $scope.cancel = function (form) {
            form.$setPristine();
            $location.path("/");
        };
        $scope.save = function (form, userId, model) {
            $scope.alert = null;
            $scope.waiting = true;
            var privacyPreferencesSaveRequestData = {
                userId: userId,
                applicantId: model.ApplicantId,
                applicantPreferences: {
                    PrivacyPreferences: model.PrivacyPreferences,
                    EmailPreferences: model.EmailPreferences
                }
            };
            preferences.updatePrivacyPreferences(privacyPreferencesSaveRequestData).then(function () {
                $scope.alert = { type: "success", message: "Thank you, your information has been updated." };
                $scope.waiting = false;
                form.$setPristine();
            }, onError);
        };
        $scope.closeAlertMessage = function () {
            $scope.alert = null;
        };
        $scope.alert = null;
        $scope.Model = {};
        preferences.loadPreferenceData().then(function () {
            $scope.Model = angular.copy(preferences.getPrivacyPreferences());
            $scope.Model.OriginalApplicant = angular.copy($scope.Model.Applicant);
            if ($scope.Model.CoApplicant) {
                $scope.Model.OriginalCoApplicant = angular.copy($scope.Model.CoApplicant);
            }
            $scope.Model.PrivacyOptions = {
                "NoSharePersonalInfo": {
                    text: "No Share",
                    description: $sce.trustAsHtml("Please do not share my personal information as outlined in the <a href=\"/modals/privacy\" data-popup=\"true\" class=\"no-track\">LightStream Privacy Policy</a>. This means LightStream will not share my personal information for any marketing purpose by LightStream, SunTrust or SunTrust affliliates; or for everyday business purposes by SunTrust affliliates.")
                },
                "NoDirectMail": {
                    text: "No Direct Mail",
                    description: $sce.trustAsHtml("Please do not mail promotional materials to me. I understand I will continue to receive mailings required to properly service my accounts, such as account statements, that may contain notice of special offers.")
                },
                "NoTelemarketing": {
                    text: "No Telemarketing",
                    description: $sce.trustAsHtml("Please do not call me with information regarding special promotional offers. I understand that I will continue to receive calls necessary to properly service my account. (MUST include phone number in application)")
                },
                "LimitedEmail": {
                    text: "Limited Email",
                    description: $sce.trustAsHtml("Please do not send me <strong>SunTrust</strong> promotional emails. However, I wish to receive <strong>LightStream</strong> promotional emails. I understand that I will continue to receive account-based emails for which I have previously enrolled. (MUST include email address in application)")
                },
                "NoEmail": {
                    text: "No Email",
                    description: $sce.trustAsHtml("Please do not send me <strong>SunTrust, SunTrust-affliated companies,</strong> or <strong>LightStream</strong> promotional emails. I understand that I will continue to receive account-based emails for which I have previously enrolled. (MUST include email address in application)")
                }
            };
            $scope.Model.PrivacyPreferenceOptions = {
                "AcceptElectronicDisclosures": {
                    text: $sce.trustAsHtml("Accept electronic disclosures and notices")
                }
            };
        });
    };
    angular.module("accountPreferences")
        .controller("privacyPreferencesController", ["$scope", "$location", "$sce", "preferences", privacyPreferencesController]);
}());
//# sourceMappingURL=privacyPreferencesController.js.map