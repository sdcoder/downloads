var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var LightStreamWeb;
(function (LightStreamWeb) {
    var Frontend;
    (function (Frontend) {
        var lightstreamAppModule = angular.module('ls.services');
        var $ = jQuery;
        function addDays(date, days) {
            var result = new Date(date.valueOf());
            result.setDate(result.getDate() + days);
            return result;
        }
        var FactHistory = /** @class */ (function () {
            function FactHistory($http, $window) {
                this.$http = $http;
                this.$window = $window;
                this.FACT_HISTORY_KEY = "FactHistory";
                this._isLocalStorageSupported = this._supportsLocalStorage();
            }
            FactHistory.prototype._isFactIDTopOfList = function (fact, factHistory) {
                if (factHistory.length === 0)
                    return false;
                var last = factHistory[factHistory.length - 1];
                return last && last.ID === fact;
            };
            FactHistory.prototype._getItem = function (key) {
                if (this._isLocalStorageSupported)
                    return localStorage.getItem(key);
                else
                    return null;
            };
            FactHistory.prototype._getServerTimeInfoAsync = function () {
                return this.$http.get('/api/servertimeinfo', {
                    transformResponse: function (data, headersGetter) {
                        var serverData = JSON.parse(data);
                        var currentDate = new Date(serverData.Now);
                        return {
                            now: currentDate,
                            expirationDate: addDays(currentDate, serverData.ExpirationTime)
                        };
                    }
                }); // node types differ from typescript types and thus this cast
            };
            FactHistory.prototype._setItem = function (key, data) {
                if (this._isLocalStorageSupported)
                    localStorage.setItem(key, data);
            };
            FactHistory.prototype._supportsLocalStorage = function () {
                // Handle local storage not exisiting.
                if (typeof window.localStorage === 'undefined')
                    return false;
                // Handle Safari in private mode.
                try {
                    localStorage.setItem('test', 'testValue');
                    localStorage.removeItem('test');
                    return true;
                }
                catch (e) {
                    return false;
                }
            };
            FactHistory.prototype.addFactIdAsync = function (fact) {
                return __awaiter(this, void 0, void 0, function () {
                    var serverTimeInfo, factHistory;
                    return __generator(this, function (_a) {
                        switch (_a.label) {
                            case 0: return [4 /*yield*/, this._getServerTimeInfoAsync()];
                            case 1:
                                serverTimeInfo = (_a.sent()).data;
                                return [4 /*yield*/, this.getFactIdsRemovingExpiredAsync(serverTimeInfo)];
                            case 2:
                                factHistory = _a.sent();
                                // We don't want to add the fact if it was the last one we inserted.
                                if (!this._isFactIDTopOfList(fact, factHistory)) {
                                    factHistory.push({
                                        ID: fact,
                                        Timestamp: serverTimeInfo.now,
                                        ExpirationDate: serverTimeInfo.expirationDate
                                    });
                                    // Remove from end.
                                    if (factHistory.length > 10)
                                        factHistory.splice(0, 1);
                                    localStorage.setItem(this.FACT_HISTORY_KEY, JSON.stringify(factHistory));
                                }
                                return [2 /*return*/];
                        }
                    });
                });
            };
            FactHistory.prototype.clearHistory = function () {
                localStorage.removeItem(this.FACT_HISTORY_KEY);
            };
            FactHistory.prototype.getFactIdFromQueryString = function () {
                var search = new URLSearchParams(this.$window.location.search);
                return parseInt(search.get('fact') || search.get('?fact') || '-1');
            };
            FactHistory.prototype.getFactIds = function () {
                if (!Array.prototype.map) {
                    return null;
                }
                var factHistory = (JSON.parse(this._getItem(this.FACT_HISTORY_KEY)) || [])
                    .map(function (element) {
                    return {
                        ID: element.ID,
                        // Number of seconds since 1/1/1970
                        Timestamp: new Date(element.Timestamp),
                        ExpirationDate: new Date(element.ExpirationDate)
                    };
                });
                // Get an array with the newest fact IDs first
                return factHistory
                    .sort(function (a, b) { return a.Timestamp.valueOf() - b.Timestamp.valueOf(); })
                    .reverse();
            };
            FactHistory.prototype.getFactIdsRemovingExpiredAsync = function (serverTimeInfo) {
                return __awaiter(this, void 0, void 0, function () {
                    var factHistory, currentDate;
                    return __generator(this, function (_a) {
                        switch (_a.label) {
                            case 0:
                                if (!(typeof serverTimeInfo === 'undefined')) return [3 /*break*/, 2];
                                return [4 /*yield*/, this._getServerTimeInfoAsync()];
                            case 1:
                                serverTimeInfo = (_a.sent()).data;
                                _a.label = 2;
                            case 2:
                                factHistory = this.getFactIds();
                                currentDate = serverTimeInfo.now.valueOf();
                                return [2 /*return*/, factHistory
                                        .filter(function (f) { return f.ExpirationDate.valueOf() > currentDate; })
                                        .sort(function (a, b) { return a.Timestamp.valueOf() - b.Timestamp.valueOf(); })];
                        }
                    });
                });
            };
            return FactHistory;
        }());
        // TypeScript compiles classes down to a constructor function.
        lightstreamAppModule.service('factHistory', ['$http', '$window', FactHistory]);
        lightstreamAppModule.run(['factHistory', function (factHistory) {
                var fact = factHistory.getFactIdFromQueryString();
                if (fact !== -1)
                    factHistory.addFactIdAsync(fact);
            }]);
    })(Frontend = LightStreamWeb.Frontend || (LightStreamWeb.Frontend = {}));
})(LightStreamWeb || (LightStreamWeb = {}));
//# sourceMappingURL=FactHistory.js.map