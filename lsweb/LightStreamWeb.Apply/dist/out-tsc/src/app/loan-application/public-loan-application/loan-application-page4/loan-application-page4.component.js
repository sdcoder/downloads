import * as tslib_1 from "tslib";
import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { BaseLoanApplicationComponent } from '../base-loan-application';
var LoanApplicationPage4Component = /** @class */ (function (_super) {
    tslib_1.__extends(LoanApplicationPage4Component, _super);
    function LoanApplicationPage4Component(location) {
        var _this = _super.call(this) || this;
        _this.location = location;
        return _this;
    }
    LoanApplicationPage4Component.prototype.ngOnInit = function () {
    };
    var _a;
    LoanApplicationPage4Component = tslib_1.__decorate([
        Component({
            selector: 'ls-loan-application-page4',
            templateUrl: './loan-application-page4.component.html',
            styleUrls: ['./loan-application-page4.component.scss']
        }),
        tslib_1.__metadata("design:paramtypes", [typeof (_a = typeof Location !== "undefined" && Location) === "function" ? _a : Object])
    ], LoanApplicationPage4Component);
    return LoanApplicationPage4Component;
}(BaseLoanApplicationComponent));
export { LoanApplicationPage4Component };
//# sourceMappingURL=loan-application-page4.component.js.map