/*globals angular, jQuery, $ */
/*jslint regexp: true */
/*jslint plusplus: true */
/*jslint nomen: true */
(function () {
    "use strict";
    angular.module('ls.services').factory('subscribeService', ['$http', function ($http) {
            return {
                getProductInterestTypes: function () {
                    var promise = $http.get('/Services/Lookups/LoanInterestLookups')
                        .then(function (response) {
                        return response.data.Result.LoanInterestTypes;
                    });
                    return promise;
                },
                saveSubscriber: function (subscriber) {
                    var promise = $http.post('/marketing/subscribe', subscriber)
                        .then(function success(response) {
                        return {
                            success: response.data.Success,
                            subscriptionAdded: response.data.SubscriptionAdded,
                            isInMaintenanceMode: response.data.IsInMaintenanceMode
                        };
                    }, function error(response) {
                        return {
                            success: false,
                            subscriptionAdded: false,
                            isInMaintenanceMode: false
                        };
                    });
                    return promise;
                },
                checkMaintenanceMode: function () {
                    var promise = $http.get('/maintenance-status/info')
                        .then(function (response) {
                        return {
                            isInMaintenanceMode: response.data.IsInMaintenanceMode
                        };
                    });
                    return promise;
                }
            };
        }]);
}());
//# sourceMappingURL=subscribe-service.js.map