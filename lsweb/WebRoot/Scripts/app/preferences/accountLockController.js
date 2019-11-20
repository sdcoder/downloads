/*globals angular */
(function () {
    'use strict';
    var accountLockController = function ($scope, preferences) {
        var onError = function () {
            $scope.alert = { type: "alert", message: "There was an error updating your AccountLock status. Please try again." };
            $scope.waiting = false;
        };
        $scope.enableAccountLock = function (userId, accountLockStatus) {
            var newAccountLockStatus = !accountLockStatus;
            $scope.alert = null;
            $scope.waiting = true;
            preferences.updateAccountLock({ userId: userId, enableAccountLock: newAccountLockStatus }).then(function () {
                $scope.Model.AccountLock.IsEnabled = newAccountLockStatus;
                $scope.waiting = false;
            }, onError);
        };
        $scope.closeAlertMessage = function () {
            $scope.alert = null;
        };
        $scope.alert = null;
        $scope.Model = {};
        preferences.loadPreferenceData().then(function () {
            $scope.Model = angular.copy(preferences.getAccountLock());
        });
    };
    angular.module("accountPreferences")
        .controller("accountLockController", ["$scope", "preferences", accountLockController]);
}());
//# sourceMappingURL=accountLockController.js.map