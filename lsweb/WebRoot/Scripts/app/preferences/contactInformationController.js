/*globals angular */
(function () {
    'use strict';
    var contactInformationController = function ($scope, $location, preferences, webStorage) {
        var onError = function () {
            $scope.alert = { type: "alert", message: "There was an error updating your data. Please try again." };
            $scope.waiting = false;
        };
        var onSuccess = function () {
            webStorage.remove('LoanApplication');
            $.ajax({
                url: '/Services/Session/Delete',
                data: { key: 'ApplicationInfo' },
                async: false
            });
            $scope.alert = { type: "success", message: "Thank you, your information has been updated." };
            $scope.waiting = false;
            //form.$setPristine();
        };
        $scope.$watch("Model.CoApplicantSameAddress", function (newValue) {
            if (newValue) {
                $scope.Model.CoApplicant.ContactInformation.Address = $scope.Model.Applicant.ContactInformation.Address;
                $scope.Model.CoApplicant.ContactInformation.UnitType = $scope.Model.Applicant.ContactInformation.UnitType;
                $scope.Model.CoApplicant.ContactInformation.UnitValue = $scope.Model.Applicant.ContactInformation.UnitValue;
                $scope.Model.CoApplicant.ContactInformation.City = $scope.Model.Applicant.ContactInformation.City;
                $scope.Model.CoApplicant.ContactInformation.State = $scope.Model.Applicant.ContactInformation.State;
                $scope.Model.CoApplicant.ContactInformation.ZipCode = $scope.Model.Applicant.ContactInformation.ZipCode;
            }
            else {
                if ($scope.Model && $scope.Model.CoApplicantTemp) {
                    $scope.Model.CoApplicant.ContactInformation.Address = angular.copy($scope.Model.CoApplicantTemp.Address);
                    $scope.Model.CoApplicant.ContactInformation.UnitType = angular.copy($scope.Model.CoApplicantTemp.UnitType);
                    $scope.Model.CoApplicant.ContactInformation.UnitValue = angular.copy($scope.Model.CoApplicantTemp.UnitValue);
                    $scope.Model.CoApplicant.ContactInformation.City = angular.copy($scope.Model.CoApplicantTemp.City);
                    $scope.Model.CoApplicant.ContactInformation.State = angular.copy($scope.Model.CoApplicantTemp.State);
                    $scope.Model.CoApplicant.ContactInformation.ZipCode = angular.copy($scope.Model.CoApplicantTemp.ZipCode);
                }
            }
        });
        $scope.cancel = function (form) {
            form.$setPristine();
            $location.path("/");
        };
        $scope.save = function (form, userId, model) {
            $scope.alert = null;
            $scope.waiting = true;
            var contactInformationSaveRequestData = {
                userId: userId,
                applicantId: model.ApplicantId,
                applicantModel: model.ContactInformation
            };
            preferences.updateContactInformation(contactInformationSaveRequestData).then(onSuccess, onError);
        };
        $scope.closeAlertMessage = function () {
            $scope.alert = null;
        };
        $scope.alert = null;
        $scope.Model = {
            CanUpdate: true,
            Applicant: null
        };
        preferences.loadPreferenceData().then(function () {
            $scope.Model = angular.copy(preferences.getContactInformation());
            $scope.Model.OriginalApplicant = angular.copy($scope.Model.Applicant);
            if ($scope.Model.CoApplicant) {
                $scope.Model.OriginalCoApplicant = angular.copy($scope.Model.CoApplicant);
                $scope.Model.CoApplicantTemp = angular.copy($scope.Model.CoApplicant.ContactInformation);
            }
        });
    };
    angular.module("accountPreferences")
        .controller("contactInformationController", ["$scope", "$location", "preferences", "webStorage", contactInformationController]);
}());
//# sourceMappingURL=contactInformationController.js.map