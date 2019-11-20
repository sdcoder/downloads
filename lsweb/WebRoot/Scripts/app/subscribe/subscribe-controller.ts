/*globals angular */
/*jslint unparam: true*/ // bug in VS + JSLint - does not properly allow unused params in callbacks
(function () {
    'use strict';

    angular
        .module('LightStreamApp')
        .controller('SubscribeController', ['$scope', '$sce', '$log', '$timeout', 'subscribeService', 'validationService', 'marketingService', SubscribeController]);

    function SubscribeController($scope, $sce, $log, $timeout, subscribeService, validationService, marketingService) {

        var clearForm = function () {
            $scope.subscriber = {};
            $scope.emailValidation = {};
            manuallyClear(); // clear invalid data
            resetFlags();
            $scope.subscribeForm.$setPristine();
            $('form[name="subscribeForm"]').removeClass('ng-submitted'); // this sucks, but this (=>) isn't available until angular 1.3 => $scope.subscribeForm.$setUntouched();
            $scope.$apply();
        };

        // another crappy thing I have to do because angular doesn't bind to invalid data by default
        // and ng-model-options which would allow bypassing this behavior isn't avaliable until 1.3
        // thus, simply clearing the bound model doesn't work if the data is invalid
        var manuallyClear = function () {
            $('#subscriber-email').val('');
            $('#subscriber-name').val('');
        }

        var getProductInterestTypes = function () {
            return subscribeService.getProductInterestTypes()
                                   .then(function (productInterestTypes) {
                                       $scope.productInterestTypes = productInterestTypes;
                                   });
        }(); // self execute

        var resetFlags = function () {
            $scope.hasError = false;
            $scope.subscriptionAdded = false;
            $scope.isDuplicateEmail = false;
        };

        var startDelayedClose = function () {
            $timeout(function () {
                $('#subscribe-form-modal').foundation('close');
            }, 2000);
        };

        $scope.emailValidation = {};

        $scope.subscribe = function () {

            if ($scope.subscribeForm.$valid) {
                $scope.isLoading = true;

                resetFlags();

                return subscribeService.saveSubscriber($scope.subscriber)
                                        .then(function (result) {
                                            $scope.isInMaintenanceMode = result.isInMaintenanceMode;
                                            $scope.hasError = result.success == false;
                                            $scope.subscriptionAdded = result.subscriptionAdded;
                                            $scope.emailValidation.isDuplicateEmail = result.success && !result.subscriptionAdded && !result.isInMaintenanceMode;
                                            $scope.isLoading = false;

                                            if ($scope.subscriptionAdded) {
                                                startDelayedClose();

                                                marketingService.t({
                                                    linkTrackVars: 'eVar11,eVar66,events',
                                                    linkTrackEvents: 'event5',
                                                    eVar11: 'LScom|KeepInTouchForm',
                                                    eVar66: $scope.subscriber.productInterest.label,
                                                    events: 'event5'
                                                });
                                            }
                                        });
            }

        };

        $(document).on('closed.fndtn.reveal', '#subscribe-form-modal[data-reveal]', function () {
            clearForm();
        });

        $(document).on('open.fndtn.reveal', '#subscribe-form-modal[data-reveal]', function () {
            subscribeService.checkMaintenanceMode()
                            .then(function (result) {
                                $scope.isInMaintenanceMode = result.isInMaintenanceMode;
                            });
        });
    };
})();