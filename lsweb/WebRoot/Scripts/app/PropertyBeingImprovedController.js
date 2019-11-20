/*globals angular, $*/
// VINController - responsible for VIN entry, saving, query, display.....
(function () {
    'use strict';
    angular.module('ApplicationStatusModule')
        .controller('PropertyBeingImprovedController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
            // VIN entry
            $scope.setupPropertyInfo = function (data) {
                $scope.LoanApp = $.parseJSON(data) || {};
            };
            // "Same as residence address" logic
            var copyPropertyValuesToHMDA = function () {
                $scope.LoanApp.HmdaComplianceProperty.Address.AddressLine = $scope.LoanApp.Residence.Address.AddressLine;
                $scope.LoanApp.HmdaComplianceProperty.Address.City = $scope.LoanApp.Residence.Address.City;
                $scope.LoanApp.HmdaComplianceProperty.Address.State = $scope.LoanApp.Residence.Address.State;
                $scope.LoanApp.HmdaComplianceProperty.Address.ZipCode = $scope.LoanApp.Residence.Address.ZipCode;
                $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit = {};
                if ($scope.LoanApp.Residence.Address.SecondaryUnit) {
                    $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Type = $scope.LoanApp.Residence.Address.SecondaryUnit.Type;
                    $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Value = $scope.LoanApp.Residence.Address.SecondaryUnit.Value;
                }
                $scope.LoanApp.HmdaComplianceProperty.OccupancyType = $scope.LoanApp.Residence.OccupancyType;
            };
            $scope.$watchCollection('[LoanApp.Residence.Address.AddressLine,' +
                'LoanApp.Residence.Address.City,' +
                'LoanApp.Residence.Address.State,' +
                'LoanApp.Residence.Address.ZipCode,' +
                'LoanApp.Residence.Address.SecondaryUnit.Type,' +
                'LoanApp.Residence.Address.SecondaryUnit.Value,' +
                'LoanApp.Residence.Ownership]', function () {
                if ($scope.LoanApp.SubjectPropertySameAsResidentAddress) {
                    copyPropertyValuesToHMDA();
                }
            });
            // handle "same as resident address" changes
            $scope.$watch('LoanApp.SubjectPropertySameAsResidentAddress', function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    if (newVal) {
                        copyPropertyValuesToHMDA();
                    }
                    else if (newVal === false) {
                        $scope.LoanApp.HmdaComplianceProperty.Address.AddressLine = null;
                        $scope.LoanApp.HmdaComplianceProperty.Address.City = null;
                        $scope.LoanApp.HmdaComplianceProperty.Address.State = null;
                        $scope.LoanApp.HmdaComplianceProperty.Address.ZipCode = null;
                        $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit = {};
                        $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Type = 'NotSelected';
                        $scope.LoanApp.HmdaComplianceProperty.Address.SecondaryUnit.Value = null;
                    }
                }
            });
            $scope.savePropertyAddress = function () {
                if ($scope.Waiting) {
                    return;
                }
                $scope.Waiting = true;
                $http.post('/SubjectPropertyAddress/Save', {
                    address: $scope.LoanApp.HmdaComplianceProperty.Address,
                    sameAsResidenceAddress: $scope.LoanApp.SubjectPropertySameAsResidentAddress
                }).success(function (result) {
                    if (result && result.Success) {
                        // No requirements for error message
                        $scope.LoanApp.Received = true;
                        $scope.LoanApp.AddressLine1 = result.AddressLine1;
                        $scope.LoanApp.AddressLine2 = result.AddressLine2;
                    }
                    $scope.Waiting = false;
                });
                return false;
            };
        }]);
}());
//# sourceMappingURL=PropertyBeingImprovedController.js.map