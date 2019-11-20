/*globals angular, $*/
// EnoticesController - responsible for loading and displaying ENotices, and user authentication for joint apps
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
(function () {
    'use strict';
    angular.module('ApplicationStatusModule')
        .controller('ENoticesController', ['$scope', '$sce', '$http', '$window', '$timeout', function ($scope, $sce, $http, $window, $timeout) {
            // ENotices
            $scope.getENotices = function (suppressAutoLoad, suppressENoticeType) {
                $http.post('/enotices/load', { SuppressType: suppressENoticeType }).success(function (result) {
                    $scope.ENotices = result;
                    $scope.ENotices.PassedChallengeResponse = [];
                    if ($scope.ENotices.Docs && !$scope.ENotices.HasMultipleApplicants && !suppressAutoLoad) {
                        $scope.ActiveENotice = 0;
                        $scope.displayDoc(0);
                        $scope.ENotices.ShowSecurityPrompt = false;
                    }
                });
            };
            $scope.displayDoc = function (index) {
                $http.get('/enotices/view/' + $scope.ENotices.Docs[index].EDocId).success(function (content) {
                    $scope.ENoticeContent = $sce.trustAsHtml(content);
                    $scope.ENotices.ShowSecurityPrompt = false;
                });
            };
            $scope.displayPDF = function (index) {
                $window.location = '/enotices/download/' + $scope.ENotices.Docs[index].PdfVersionEDocId;
            };
            $scope.viewEDoc = function (index) {
                if (!$scope.ENotices.HasMultipleApplicants || ($scope.ActiveENotice === index && $scope.ENotices.ShowSecurityPrompt === false)) {
                    $scope.displayDoc(index);
                }
                else {
                    $scope.ENotices.ShowSecurityPrompt = true;
                    $scope.ENoticeOnSuccess = $scope.displayDoc;
                }
                $scope.ActiveENotice = index;
            };
            $scope.$watch('ActiveENotice', function () {
                $timeout(function () {
                    $scope.SSN = null;
                    $scope.DateOfBirth = undefined;
                    $scope.ErrorMessage = null;
                });
            });
            $scope.$watch('ENotices.ShowSecurityPrompt', function (newVal, oldVal) {
                if (newVal) {
                    $timeout(function () {
                        $('#SSN').focus();
                        $scope.ENoticeContent = '';
                        $scope.SSN = null;
                        $scope.DateOfBirth = null;
                    });
                }
            });
            $scope.downloadEDoc = function (index) {
                if (!$scope.ENotices.HasMultipleApplicants || ($scope.ActiveENotice === index && $scope.ENotices.ShowSecurityPrompt === false)) {
                    if (!$scope.ENotices.ShowSecurityPrompt) {
                        $scope.displayPDF(index);
                    }
                }
                else {
                    $scope.ENotices.ShowSecurityPrompt = true;
                    $scope.ENoticeOnSuccess = $scope.displayPDF;
                }
                $scope.ActiveENotice = index;
            };
            $scope.validateApplicant = function () {
                $http.post('/enotices/validate', {
                    SSN: $scope.SSN,
                    DateOfBirth: $scope.DateOfBirth,
                    ApplicantId: $scope.ENotices.Docs[$scope.ActiveENotice].ApplicantId
                }).success(function (result) {
                    if (result && result.IsValid) {
                        $scope.ErrorMessage = null;
                        $scope.ENotices.PassedChallengeResponse.push($scope.ENotices.Docs[$scope.ActiveENotice].ApplicantId);
                        $scope.ENotices.ShowSecurityPrompt = false;
                        $scope.ENoticeOnSuccess($scope.ActiveENotice);
                    }
                    else {
                        $scope.ErrorMessage = result.ErrorMessage;
                    }
                });
            };
        }]);
}());
//# sourceMappingURL=ENoticesController.js.map