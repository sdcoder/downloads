/*globals angular, console, window, jQuery, $ */
/*jslint bitwise: true */
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
(function () {
    'use strict';

    angular.module('ApplicationStatusModule')
        .controller('CustomerIdentificationController', ['$scope', '$http', function ($scope, $http) {
            var Section = {
                NONE: 0x00,
                INSTRUCTIONS: 0x01,
                PROCESSING: 0x02,
                MULTSELQ: 0x04,
                SGLSELQ: 0x08,
                COMPLETE: 0x10
            },
                activateSection = function (section) {
                    $scope.showInstructions = (section & Section.INSTRUCTIONS) > 0;
                    $scope.showProcessing = (section & Section.PROCESSING) > 0;
                    $scope.showSglSelQ = (section & Section.SGLSELQ) > 0;
                    $scope.showMultiSelQ = (section & Section.MULTSELQ) > 0;
                    $scope.showCompleted = (section & Section.COMPLETE) > 0;
                };

            $scope.customerIdentificationInit = function (applicantName, applicantType) {
                $scope.ApplicantName = applicantName;
                $scope.ApplicantType = applicantType;
                activateSection(Section.INSTRUCTIONS);
            };

            $scope.loadFirstQuestion = function() {
                // to hide the "start" button
                $scope['IdentityVerificationStarted' + $scope.ApplicantType] = true;

                activateSection(Section.PROCESSING);
                $http({
                    method: 'GET',
                    url: '/CustomerIdentification/GetFirstQuestion',
                    params: {
                        applicantType: $scope.ApplicantType
                    },
                    cache: false
                }).success(function (data, status) {
                        $scope.questionData = data;
                        // Check if a question has been returned
                        if (typeof $scope.questionData.Question === 'object') {
                            if ($scope.questionData.Question.Type === 1) { // 1 = single
                                activateSection(Section.SGLSELQ);
                            } else if ($scope.questionData.Question.Type === 2) { // 2 = multiple
                                activateSection(Section.MULTSELQ);
                            }
                        } else {
                            activateSection(Section.COMPLETE);
                        }
                    })
                    .error(function () {
                        activateSection(Section.COMPLETE);
                    });
            };


            $scope.getNextQuestion = function () {

                // if responding to a single select question, 
                // set Choice.Selected based on the choice id in the
                // ChoiceSelected prop
                if ($scope.showSglSelQ === true) {
                    angular.forEach($scope.questionData.Question.Choices, function (choice, key) {
                        if (choice.ChoiceId === Number($scope.questionData.Question.ChoiceSelected)) {
                            choice.Selected = "true";
                        }
                    });
                }

                activateSection(Section.PROCESSING);

                $http.post('/CustomerIdentification/GetNextQuestion', $scope.questionData, { cache: false })
                    .success(function (data, status) {
                        $scope.questionData = data;
                        // Check if a question has been returned
                        if (typeof $scope.questionData.Question === 'object') {
                            if ($scope.questionData.Question.Type === 1) { // 1 = single
                                activateSection(Section.SGLSELQ);
                            } else if ($scope.questionData.Question.Type === 2) { // 2 = multiple
                                activateSection(Section.MULTSELQ);
                            }
                        } else {
                            activateSection(Section.COMPLETE);
                        }
                    })
                    .error(function () {
                        activateSection(Section.COMPLETE);
                    });
            };

            $scope.gotoPage = function (url) {
                window.location.href = url;
            };

            $scope.haveAnswer = function () {
                var res = false;
                if ($scope.questionData && $scope.questionData.Question) {
                    // If single selected question.
                    if ($scope.questionData.Question.ChoiceSelected) {
                        res = true;
                    } else {
                        // For multi select question.
                        angular.forEach($scope.questionData.Question.Choices, function (choice, key) {
                            if (choice.Selected === "true") {
                                res = true;
                            }
                        });
                    }
                }
                return res;
            };

            activateSection(Section.INSTRUCTIONS);
        }]);
}());
