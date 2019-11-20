import * as tslib_1 from "tslib";
import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { BaseLoanApplicationComponent } from '../base-loan-application';
var LoanApplicationPage3Component = /** @class */ (function (_super) {
    tslib_1.__extends(LoanApplicationPage3Component, _super);
    function LoanApplicationPage3Component(location) {
        var _this = _super.call(this) || this;
        _this.location = location;
        return _this;
    }
    LoanApplicationPage3Component.prototype.ngOnInit = function () {
    };
    var _a;
    LoanApplicationPage3Component = tslib_1.__decorate([
        Component({
            selector: 'ls-loan-application-page3',
            templateUrl: './loan-application-page3.component.html',
            styleUrls: ['./loan-application-page3.component.scss']
        }),
        tslib_1.__metadata("design:paramtypes", [typeof (_a = typeof Location !== "undefined" && Location) === "function" ? _a : Object])
    ], LoanApplicationPage3Component);
    return LoanApplicationPage3Component;
}(BaseLoanApplicationComponent));
export { LoanApplicationPage3Component };
//# sourceMappingURL=loan-application-page3.component.js.map