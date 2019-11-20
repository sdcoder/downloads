/*globals angular */

(function () {
    'use strict';

    var securityInformationController = function ($scope, $location, $sce, preferences) {
        var initialize = function () {
            var existingAlert = $scope.Model.alert;

            $scope.Model = angular.copy(preferences.getSecurityInformation());
            $scope.Model.SecurityInformation = { SelectedApplications: [] };
            $scope.Model.ChangeUserIdType = null;
            $scope.Model.alert = existingAlert;

            $scope.Model.SourceAccount = null;
            $scope.Model.DestAccount = null;
            $scope.Model.AccountSyncSettings = {
                Account: [
                    {
                        SourceUserId: null,
                        DestUserId: null,
                        Email: null,
                        Address: null,
                        HomePhone: null,
                        WorkPhone: null
                    },
                    {
                        SourceUserId: null,
                        DestUserId: null,
                        Email: null,
                        Address: null,
                        HomePhone: null,
                        WorkPhone: null
                    }]
            };

            $scope.accountSettingsInvalid = true;
            $scope.isApplicationSelected = false;
            if ($scope.Model.Applications && $scope.Model.Applications.length === 1) {
                $scope.Model.SecurityInformation.SelectedApplications.push($scope.Model.Applications[0].ApplicationId);
                $scope.isApplicationSelected = true;
            }
        },
            onError = function () {
                $scope.Model.alert = { type: "alert", message: "There was an error updating your data. Please try again." };
                $scope.waiting = false;
            };

        $scope.cancel = function (form) {
            if (form) {
                form.$setPristine();
            }
            $location.path("/securityInformation");
        };

        $scope.closeAlertMessage = function () {
            $scope.Model.alert = null;
        };

        $scope.getSecurityHint = function () {
            return $sce.trustAsHtml($scope.Model.SecurityHint);
        };

        $scope.getPasswordHint = function () {
            return $sce.trustAsHtml($scope.Model.PasswordHint);
        };

        $scope.getUserIdHint = function () {
            return $sce.trustAsHtml($scope.Model.UserIdHint);
        };

        $scope.toggleApplicationSelected = function (applicationId) {
            var index = $scope.Model.SecurityInformation.SelectedApplications.indexOf(applicationId);

            if (index > -1) {
                $scope.Model.SecurityInformation.SelectedApplications.splice(index, 1);
            } else {
                $scope.Model.SecurityInformation.SelectedApplications.push(applicationId);
            }
            $scope.isApplicationSelected = $scope.Model.SecurityInformation.SelectedApplications.length > 0;
        };

        $scope.submitChangeUserIdType = function () {
            if ($scope.Model.ChangeUserIdType === "new") {
                $location.path("/securityInformation/changeUserId/new");
            } else if ($scope.Model.ChangeUserIdType === 'move') {
                $location.path("/securityInformation/changeUserId/move");
            }
        };

        $scope.saveCreateUser = function (form, userId, model) {
            $scope.Model.alert = null;
            $scope.waiting = true;

            var changeUserIdRequestData = {
                userId: userId,
                securityInformation: model
            };

            preferences.changeUserId(changeUserIdRequestData).then(function (data) {
                if (data.Success) {
                    $scope.Model.alert = { type: "success", message: "Your new user ID is " + $scope.Model.SecurityInformation.UserName + ". Please remember and secure it. Thank you." };
                    $scope.Model.UserIdGainedFocus = false;
                    form.$setPristine();
                    form.CurrentPassword.$clearTrackVisited();
                    form.NewUserName.$clearTrackVisited();
                    $scope.Model.SecurityInformation = {};
                    preferences.loadPreferenceData().then(initialize);
                } else {
                    $scope.Model.alert = { type: "alert", message: data.ErrorMessage };
                }
                $scope.waiting = false;
            }, onError);
        };

        $scope.saveMoveAccounts = function (form, userId, model) {
            $scope.Model.alert = null;
            $scope.waiting = true;

            var moveAccountsRequestData = {
                userId: userId,
                securityInformation: model
            };

            preferences.moveAccount(moveAccountsRequestData).then(function (data) {
                if (data.Success) {
                    $scope.Model.alert = { type: "success", message: "Your account has been moved to user ID " + $scope.Model.SecurityInformation.UserName + "." };
                    form.$setPristine();
                    form.EstablishedUserName.$clearTrackVisited();
                    form.EstablishedPassword.$clearTrackVisited();
                    $scope.Model.SecurityInformation = {};
                    preferences.loadPreferenceData().then(initialize);
                } else {

                    if (data.ContactInformationConflict) {
                        $scope.Model.ShowContactSyncForm = true;
                        $scope.getAccountSyncInfo();
                    }

                    $scope.Model.alert = { type: "alert", message: data.ErrorMessage };
                }
                $scope.waiting = false;
            }, onError);
        };

        $scope.savePassword = function (form, userId, model) {
            $scope.Model.alert = null;
            $scope.waiting = true;

            var updatePasswordRequestData = {
                userId: userId,
                securityInformation: model
            };

            preferences.updatePassword(updatePasswordRequestData).then(function () {
                $scope.Model.alert = { type: "success", message: "Your password has been changed. Thank you." };
                $scope.waiting = false;
                $scope.Model.PasswordGainedFocus = false;
                form.$setPristine();
                form.OldPassword.$clearTrackVisited();
                form.Password.$clearTrackVisited();
                form.PasswordConfirm.$clearTrackVisited();
                $scope.Model.SecurityInformation = {};
            }, onError);
        };

        $scope.saveSecurityQuestion = function (form, userId, model) {
            $scope.Model.alert = null;
            $scope.waiting = true;

            var securityInformationRequestData = {
                userId: userId,
                securityInformation: model
            };

            preferences.updateSecurityQuestionAndAnswer(securityInformationRequestData).then(function () {
                $scope.Model.alert = { type: "success", message: "Your security question and answer have been updated. Thank you." };
                $scope.waiting = false;
                $scope.Model.SecurityAnswerGainedFocus = false;
                form.$setPristine();
                form.Password.$clearTrackVisited();
                form.ConfirmSecurityQuestionAnswer.$clearTrackVisited();
                $scope.Model.SecurityInformation = {};
            }, onError);
        };

        //Account Sync
        $scope.getAccountSyncInfo = function () {

            var accountSyncRequestData = {
                newUserName: $scope.Model.SecurityInformation.UserName
            }

            $scope.Model.SourceAccount = [];
            $scope.Model.DestAccount = [];

            preferences.getAccountSyncData(accountSyncRequestData).then(data => {
                var count1 = Number(0);
                var count2 = Number(0);
                data.forEach(element => {
                    if (element.IsSource) {
                        $scope.Model.AccountSyncSettings.Account[count1].SourceUserId = element.UserId;
                        $scope.Model.SourceAccount.push(element);
                        $scope.SetDefaultValues(element, count1);
                        count1++;
                    } else {
                        $scope.Model.AccountSyncSettings.Account[count2].DestUserId = element.UserId;
                        $scope.Model.DestAccount.push(element);
                        $scope.SetDefaultValues(element, count2);
                        count2++;
                    }

                });
            });
        }

        $scope.SetDefaultValues = (element, index) => {

            if (!element.Differences.Email)
                $scope.Model.AccountSyncSettings.Account[index].Email = "2";
            if (!element.Differences.Address)
                $scope.Model.AccountSyncSettings.Account[index].Address = "2";
            if (!element.Differences.HomePhone)
                $scope.Model.AccountSyncSettings.Account[index].HomePhone = "2";
            if (!element.Differences.WorkPhone)
                $scope.Model.AccountSyncSettings.Account[index].WorkPhone = "2";
        }


        $scope.saveAccountSyncAndMove = function (form, userId, model) {

            //save settings make controller call
            $scope.Model.alert = null;
            $scope.waiting = true;

            var syncAndMoveData = {
                userId: userId,
                accountSettings: JSON.stringify($scope.Model.AccountSyncSettings),
                securityInformation: model
            };

            preferences.syncAndMoveAccount(syncAndMoveData).then(data => {
                if (data.Success) {
                    $scope.Model.alert = { type: "success", message: "Your account has been moved to user ID " + $scope.Model.SecurityInformation.UserName + "." };
                    form.$setPristine();
                    $scope.Model.SecurityInformation = {};
                    preferences.loadPreferenceData().then(initialize);
                } else {
                    $scope.Model.alert = { type: "alert", message: data.ErrorMessage };
                }
                $scope.waiting = false;
            }, onError);
        };

        $scope.handleRadioClick = (field, account, optionSelected) => {
            switch (field) {
                case "emailAddress":
                    $scope.Model.AccountSyncSettings.Account[account].Email = Number(optionSelected);
                    break;
                case "address":
                    $scope.Model.AccountSyncSettings.Account[account].Address = Number(optionSelected);
                    break;
                case "homePhone":
                    $scope.Model.AccountSyncSettings.Account[account].HomePhone = Number(optionSelected);
                    break;
                case "workPhone":
                    $scope.Model.AccountSyncSettings.Account[account].WorkPhone = Number(optionSelected);
                    break;
                default:
            }

            $scope.accountSettingsInvalid  = $scope.checkIfInvalid();

        };

        $scope.checkIfInvalid = () => {

            for (var i = 0; i < 2; i++) {

                if (!$scope.Model.AccountSyncSettings.Account[i].SourceUserId)
                    continue;

                if (!$scope.Model.AccountSyncSettings.Account[i].Email ||
                    !$scope.Model.AccountSyncSettings.Account[i].Address ||
                    !$scope.Model.AccountSyncSettings.Account[i].HomePhone ||
                    !$scope.Model.AccountSyncSettings.Account[i].WorkPhone) {
                    return true;
                }
            }

            return false;

        }
        
        $scope.Model = {
            alert: null,
            CanUpdate: true,
            Applications: null
        };

        preferences.loadPreferenceData().then(initialize);
    };

    angular.module("accountPreferences")
        .controller("securityInformationController", ["$scope", "$location", "$sce", "preferences", securityInformationController]);
}());