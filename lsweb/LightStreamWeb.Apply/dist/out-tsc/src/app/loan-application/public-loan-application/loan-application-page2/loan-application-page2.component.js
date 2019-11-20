import * as tslib_1 from "tslib";
import { Component } from '@angular/core';
import { Location, LocationStrategy } from '@angular/common';
import { BaseLoanApplicationComponent } from '../base-loan-application';
import { Router } from '@angular/router';
var LoanApplicationPage2Component = /** @class */ (function (_super) {
    tslib_1.__extends(LoanApplicationPage2Component, _super);
    function LoanApplicationPage2Component(location, locationStrategy, router) {
        var _this = _super.call(this) || this;
        _this.location = location;
        _this.locationStrategy = locationStrategy;
        _this.router = router;
        return _this;
    }
    Object.defineProperty(LoanApplicationPage2Component.prototype, "phoneNumberControl", {
        get: function () {
            return this.form.get('page2').get('residencePhoneNumber');
        },
        enumerable: true,
        configurable: true
    });
    LoanApplicationPage2Component.prototype.ngOnInit = function () { };
    LoanApplicationPage2Component.prototype.validateEmail = function () {
        console.log('TODO: experian email validation to make sure email exists.');
    };
    var _a, _b, _c;
    LoanApplicationPage2Component = tslib_1.__decorate([
        Component({
            selector: 'ls-loan-application-page2',
            templateUrl: './loan-application-page2.component.html',
            styleUrls: ['./loan-application-page2.component.scss']
        }),
        tslib_1.__metadata("design:paramtypes", [typeof (_a = typeof Location !== "undefined" && Location) === "function" ? _a : Object, typeof (_b = typeof LocationStrategy !== "undefined" && LocationStrategy) === "function" ? _b : Object, typeof (_c = typeof Router !== "undefined" && Router) === "function" ? _c : Object])
    ], LoanApplicationPage2Component);
    return LoanApplicationPage2Component;
}(BaseLoanApplicationComponent));
export { LoanApplicationPage2Component };
//# sourceMappingURL=loan-application-page2.component.js.map