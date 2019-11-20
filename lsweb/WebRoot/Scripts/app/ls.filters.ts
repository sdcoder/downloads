/*globals angular */
(function () {
    'use strict';

    // filters, for UI layout and display
    angular.module('ls.filters', []).filter('rateRange', ['$sce', '$filter', function ($sce, $filter) {
        return function (input, purposeOfLoan) {

            if (input === undefined || input === null || input.Min === null) {
                if (purposeOfLoan === 32700) { // NotSelected
                    return null;
                }
                return $sce.trustAsHtml("N/A");
            }

            if (input.Min === input.Max) {
                return $sce.trustAsHtml($filter('number')(input.Min * 100, 2) + '%');
            }
            return $sce.trustAsHtml($filter('number')(input.Min * 100, 2) + '% - ' + $filter('number')(input.Max * 100, 2) + '%');
        };
    }]).filter('trusted', ['$sce', function ($sce) {
        return function (text) {
            return $sce.trustAsHtml(text);
        };
    }]).filter('lsPaymentDayOfMonth', ['$sce', function ($sce) {
        return function (input) {
            if (input) {
                if (input == '99') {
                    return 'last';
                }
                else {
                    let num = Number(input);
                    let display = num.toString();

                    if (num >= 10 && num <= 20) {
                        display = num + "th";
                    }
                    else {
                        switch (num % 10) {
                            case 1:
                                display = num + "st";
                                break;
                            case 2:
                                display = num + "nd";
                                break;
                            case 3:
                                display = num + "rd";
                                break;
                            default:
                                display = num + "th";
                                break;
                        }
                    }

                    return display;
                }
            }
        };

    }]).filter('lsLoanAmountMin', ['$sce', '$filter', function ($sce, $filter) {
        return function (input) {
            if (input === undefined || input === null) {
                return $sce.trustAsHtml('');
            }
            if (input === '100000.01' || input === 100000.01) {
                return $sce.trustAsHtml('Above $100,000');
            }

            return $sce.trustAsHtml('$' + $filter('number')(input));
        };
    }]).filter('lsLoanAmountMax', ['$sce', '$filter', function ($sce, $filter) {
        return function (input) {
            if (input === undefined || input === null) {
                return $sce.trustAsHtml('');
            }

            if ((input % 1).toFixed(2) === '0.99') {
                return $sce.trustAsHtml('$' + $filter('number')(input - 0.99));
            }

            return $sce.trustAsHtml('$' + $filter('number')(input));
        };
    }])
        .filter('incomeSourceFilter', function () {
            return function (incomeSources, isWisconsin, currentSelection) {
                var filteredIncomeSources = [];
                var wisconsinValue = 'WisconsinSpouseIncome'

                angular.forEach(incomeSources, function (value) {
                    if (value.id == wisconsinValue && currentSelection != wisconsinValue) { // don't filter WI value if they've already selected it.
                        if (isWisconsin) {
                            filteredIncomeSources.push(value);
                        }
                    }
                    else {
                        filteredIncomeSources.push(value);
                    }
                });

                return filteredIncomeSources;
            }
        })
        .filter('removeNotSelected', function () {
            return function (items) {
                var filteredItems = [];
                angular.forEach(items, function (value) {
                    if (value.id != "" && value.id != "NotSelected") {
                        filteredItems.push(value);
                    }
                });
                return filteredItems;
            }
        })
        .filter('hideNotSelectedInList', ['$sce', function ($sce) {
            return function (items) {
                var filteredItems = [];
                angular.forEach(items, function (value) {
                    if (value === 'NotSelected') {
                        filteredItems.push('');
                    }
                    else {
                        filteredItems.push(value);
                    }
                });

                return filteredItems;
            };
        }])
        .filter('relevantErrors', function () {
            return function (errors, id) {
                if (!errors) {
                    return [];
                }

                return errors.filter(function (error) {
                    return error.id == id;
                });;
            };
        });

    // not creating a new module here....
    angular.module('ls.filters').filter('hideNotSelected', ['$sce', function ($sce) {
        return function (input) {
            if (input === undefined || input === null || input === 32700 || input === 'NotSelected') {
                return $sce.trustAsHtml('');
            }

            return $sce.trustAsHtml(input);
        };
    }]);

}());
