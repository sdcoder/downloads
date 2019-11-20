import * as tslib_1 from "tslib";
import { Injectable } from '@angular/core';
import { Router, NavigationStart } from '@angular/router';
import { filter } from 'rxjs/operators';
var RoutingStateService = /** @class */ (function () {
    function RoutingStateService(router) {
        this.router = router;
        this.history = [];
    }
    RoutingStateService.prototype.loadRouting = function () {
        var _this = this;
        this.router.events
            .pipe(filter(function (event) { return event instanceof NavigationStart; }))
            .subscribe(function (_a) {
            var url = _a.url;
            _this.history = _this.history.concat([url]);
        });
    };
    RoutingStateService.prototype.getHistory = function () {
        return this.history;
    };
    RoutingStateService.prototype.getPreviousUrl = function () {
        return this.history[this.history.length - 2] || '/index';
    };
    var _a;
    RoutingStateService = tslib_1.__decorate([
        Injectable(),
        tslib_1.__metadata("design:paramtypes", [typeof (_a = typeof Router !== "undefined" && Router) === "function" ? _a : Object])
    ], RoutingStateService);
    return RoutingStateService;
}());
export { RoutingStateService };
//# sourceMappingURL=routing-state.service.js.map