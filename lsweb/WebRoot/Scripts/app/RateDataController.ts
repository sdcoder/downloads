interface RateDataControllerScope {
    autoMinRate: number;
    autoMaxRate: number;
    autoPurposes: string[];
    rateData: RateData[];
    rateDataByPurpose: (purposeName: string) => RateData;
    errorMessage: string;
}

interface RateData {
    purpose: string;
    minRate: number;
    maxRate: number;
}

(function () {
    'use strict';

    angular.module('LightStreamApp')
        .controller('RateDataController', ['$scope', '$http', function ($scope: RateDataControllerScope, $http: angular.IHttpService) {
            var self = this;

            $scope.autoMinRate = 0;
            $scope.autoMaxRate = 0;

            var autoPurposes = [
                'NewAutoPurchase',
                'UsedAutoPurchase',
                'PrivatePartyPurchase',
                'LeaseBuyOut',
                'AutoRefinancing',
                'MotorcyclePurchase',
                'Other'];

            $scope.rateData =
                [
                    { purpose: 'HomeImprovement', minRate: 0, maxRate: 0 },
                    { purpose: 'CreditCardConsolidation', minRate: 0, maxRate: 0 },
                    { purpose: 'MedicalExpense', minRate: 0, maxRate: 0 },
                    { purpose: 'TimeSharePurchase', minRate: 0, maxRate: 0 },
                    { purpose: 'BoatRvPurchase', minRate: 0, maxRate: 0 },
                    { purpose: 'Other', minRate: 0, maxRate: 0 }
                ]

            for (var i = 0; i < autoPurposes.length; i++) {
                var purposeObject = { purpose: autoPurposes[i], minRate: 0, maxRate: 0 };
                $scope.rateData.push(purposeObject);
            }

            $scope.rateDataByPurpose = function (purposeName) {
                for (var i = 0; i < $scope.rateData.length; i++) {
                    var purpose = $scope.rateData[i];
                    if (purpose.purpose == purposeName)
                        return purpose;
                }
            }

            self.getRateData = function () {
                $scope.errorMessage = '';
                for (var i = 0; i < $scope.rateData.length; i++) {
                    var purpose = $scope.rateData[i];
                    self.rateService(purpose);
                }
            }

            function setAutoMinMax() {
                var minRate = 100;
                var maxRate = 0;
                for (var i = 0; i < $scope.rateData.length; i++) {
                    var purpose = $scope.rateData[i];
                    if (isAutoPurpose(purpose.purpose)) {
                        var rateData = $scope.rateDataByPurpose(purpose.purpose);
                        if (rateData.minRate < minRate)
                            minRate = rateData.minRate;
                        if (rateData.maxRate > maxRate)
                            maxRate = rateData.maxRate;
                    }
                }
                $scope.autoMinRate = minRate;
                $scope.autoMaxRate = maxRate;
            }

            function isAutoPurpose(purposeName) {
                for (var i = 0; i < autoPurposes.length; i++) {
                    if (autoPurposes[i] == purposeName)
                        return true;
                }
                return false;
            }

            var ratesRetrieved = 0;

            self.rateService = function (purpose) {
                $http.post('/services/rates', { 'PurposeOfLoan': purpose.purpose }).then(function (response: angular.IHttpPromiseCallbackArg<{ MinRate: number, MaxRate: number }>) {
                    purpose.minRate = response.data.MinRate;
                    purpose.maxRate = response.data.MaxRate;
                    ratesRetrieved += 1;
                    if (ratesRetrieved == $scope.rateData.length)
                        setAutoMinMax();
                }, function (response) {
                    console.log('There was a problem loading the rates data in RateDataController: ' + response.data.ExceptionMessage);
                });
            }

            self.getRateData();
        }])

        .filter('percentage', ['$filter', function ($filter) {
            return function (input, decimals) {
                return $filter('number')(input * 100, decimals);
            };
        }]);
}());
