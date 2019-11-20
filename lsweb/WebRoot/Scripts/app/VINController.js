/*globals angular, $*/
// VINController - responsible for VIN entry, saving, query, display.....
(function () {
    'use strict';
    angular.module('ApplicationStatusModule')
        .controller('VINController', ['$scope', '$http', '$window', function ($scope, $http, $window) {
            // VIN entry
            $scope.setupVINCollection = function (data) {
                $scope.VehicleInfo = $.parseJSON(data) || {};
            };
            $scope.validateVIN = function () {
                if (!$scope.VehicleInfo || !$scope.VehicleInfo.VIN) {
                    return;
                }
                if (/[ioq]/i.test($scope.VehicleInfo.VIN)) {
                    $('#VINConfirmModal').foundation('open');
                }
                $http.get('/VehicleInformation/Validate', {
                    params: {
                        vin: $scope.VehicleInfo.VIN
                    }
                }).success(function (result) {
                    var isValid = (result === 'true' || $scope.VehicleInfo.VIN === $scope.VehicleInfo.UserConfirmedVIN);
                    $scope.VINEntryForm.VIN.$setValidity('checksum', isValid);
                    if (!isValid) {
                        $('#VINConfirmModal').foundation('open');
                    }
                });
            };
            $scope.confirmCurrentVIN = function () {
                $scope.VehicleInfo.UserConfirmedVIN = $scope.VehicleInfo.VIN;
                $scope.VINEntryForm.VIN.$setValidity('checksum', true);
                $scope.closeVINConfirmation();
                return false;
            };
            $scope.closeVINConfirmation = function () {
                $('#VINConfirmModal').foundation('close');
                return false;
            };
            $scope.searchForVehicles = function () {
                $scope.VehicleInfo.Loading = true;
                $http.post('/VehicleInformation/Search', {
                    vin: $scope.VehicleInfo.VIN,
                    mileage: $scope.VehicleInfo.Mileage,
                    applicationId: $scope.VehicleInfo.ApplicationId
                }).success(function (results) {
                    $scope.VehicleInfo.Loading = false;
                    $scope.VehicleInfo.Vehicles = results;
                    if (!results || results.length === 0) {
                        $scope.VehicleInfo.NoneOfAboveSelected = true;
                    }
                });
                return false;
            };
            $scope.saveVehicleInfo = function (vehicle) {
                $scope.VehicleInfo.Loading = true;
                $http.post('/VehicleInformation/Save', {
                    VIN: $scope.VehicleInfo.VIN,
                    Make: vehicle.Make,
                    Model: vehicle.Model,
                    Year: vehicle.Year,
                    Description: vehicle.Description,
                    Mileage: $scope.VehicleInfo.Mileage,
                    PaidToDealer: $scope.VehicleInfo.TransactionAmount,
                    ApplicationId: $scope.VehicleInfo.ApplicationId
                }).success(function (result) {
                    $scope.VehicleInfo.Loading = false;
                    $scope.VehicleInfo.Received = true;
                    $scope.VehicleInfo.MakeAndModel = vehicle.Make + ' ' + vehicle.Model;
                    $scope.VehicleInfo.Year = vehicle.Year;
                    if (result && result.Success) {
                        if (result.AutoSatisfied) {
                            $scope.VehicleInfo.AutoSatisfied = result.AutoSatisfied;
                        }
                    }
                });
                return false;
            };
            $scope.$watch('VehicleInfo.VIN', function (newValue, oldValue) {
                if (newValue !== oldValue) {
                    $scope.VehicleInfo.NoneOfAboveSelected = false;
                    $scope.VehicleInfo.Vehicles = null;
                }
            });
            $scope.saveUserEnteredVehicle = function () {
                $scope.VehicleInfo.Loading = true;
                $http.post('/VehicleInformation/Save', {
                    VIN: $scope.VehicleInfo.VIN,
                    Make: $scope.VehicleInfo.Make,
                    Model: $scope.VehicleInfo.Model,
                    Year: $scope.VehicleInfo.Year,
                    Mileage: $scope.VehicleInfo.Mileage,
                    PaidToDealer: $scope.VehicleInfo.TransactionAmount,
                    ApplicationId: $scope.VehicleInfo.ApplicationId
                }).success(function () {
                    $scope.VehicleInfo.Loading = false;
                    $scope.VehicleInfo.Received = true;
                    $scope.VehicleInfo.MakeAndModel = $scope.VehicleInfo.Make + ' ' + $scope.VehicleInfo.Model;
                });
                return false;
            };
            $scope.selectVehicleFromAPI = function (vehicle) {
                $scope.VehicleInfo.Make = vehicle.Make;
                $scope.VehicleInfo.Model = vehicle.Model;
                $scope.VehicleInfo.Year = vehicle.Year;
                $scope.VehicleInfo.Description = vehicle.Description;
                $scope.VehicleInfo.Submitted = true;
            };
            $scope.selectVehicleFromUser = function () {
                $scope.VehicleInfo.Submitted = true;
            };
            $scope.unSubmit = function () {
                $scope.VehicleInfo.Submitted = false;
            };
            $scope.ceritfy = function (redirect) {
                try {
                    $("#vinCertifyError").hide();
                    $scope.VehicleInfo.Loading = true;
                    var data = {
                        VIN: $scope.VehicleInfo.VIN,
                        ApplicationId: $scope.VehicleInfo.ApplicationId,
                        UserCertified0: $scope.VehicleInfo.UserCertified0,
                        UserCertified1: $scope.VehicleInfo.UserCertified1,
                        Make: $scope.VehicleInfo.Make,
                        Model: $scope.VehicleInfo.Model,
                        Year: $scope.VehicleInfo.Year,
                        Description: $scope.VehicleInfo.Description,
                        Mileage: $scope.VehicleInfo.Mileage,
                        PaidToDealer: $scope.VehicleInfo.TransactionAmount
                    };
                    $http.post('/VehicleInformation/SaveCertified', data).success(function (result) {
                        if (result.Success) {
                            $window.location = redirect;
                        }
                        else {
                            $scope.VehicleInfo.Loading = false;
                            $("#vinCertifyError").show();
                            $("#vinCertifyError").html('There was an error submitting your request. Please <a href="/appstatus/refresh">click here</a> to log in and try again.');
                            $.post('/Error/LogJSError', {
                                msg: JSON.stringify(data)
                            });
                        }
                    }).error(function (data, status, headers, config) {
                        $("#vinCertifyError").show();
                        $("#vinCertifyError").html("The VIN submission failed with error code " + status);
                        $scope.VehicleInfo.Loading = false;
                    });
                }
                catch (err) {
                    $("#vinCertifyError").show();
                    $("#vinCertifyError").html("The VIN submission failed with the following error: " + err.message);
                    $scope.VehicleInfo.Loading = false;
                }
            };
        }]);
}());
//# sourceMappingURL=VINController.js.map