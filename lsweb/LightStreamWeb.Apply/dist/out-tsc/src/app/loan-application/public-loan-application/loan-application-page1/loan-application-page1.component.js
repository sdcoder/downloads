import * as tslib_1 from "tslib";
import { Component, Input } from '@angular/core';
import { BaseLoanApplicationComponent } from '../base-loan-application';
var LoanApplicationPage1Component = /** @class */ (function (_super) {
    tslib_1.__extends(LoanApplicationPage1Component, _super);
    function LoanApplicationPage1Component() {
        return _super.call(this) || this;
    }
    Object.defineProperty(LoanApplicationPage1Component.prototype, "applicationType", {
        get: function () {
            return this.form.get('page1').get('applicationType').value.value;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoanApplicationPage1Component.prototype, "loanTermMonthsControl", {
        get: function () {
            return this.form.get('page1').get('loanTermMonths');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoanApplicationPage1Component.prototype, "loanTermMonths", {
        get: function () {
            return this.form.get('page1').get('loanTermMonths').value;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoanApplicationPage1Component.prototype, "paymentType", {
        get: function () {
            return this.form.get('page1').get('paymentType').value.value;
        },
        enumerable: true,
        configurable: true
    });
    LoanApplicationPage1Component.prototype.ngOnInit = function () { };
    LoanApplicationPage1Component.prototype.updateLoanApp = function () { };
    tslib_1.__decorate([
        Input(),
        tslib_1.__metadata("design:type", Array)
    ], LoanApplicationPage1Component.prototype, "applicationTypes", void 0);
    tslib_1.__decorate([
        Input(),
        tslib_1.__metadata("design:type", Array)
    ], LoanApplicationPage1Component.prototype, "loanPurposes", void 0);
    tslib_1.__decorate([
        Input(),
        tslib_1.__metadata("design:type", Array)
    ], LoanApplicationPage1Component.prototype, "paymentTypes", void 0);
    LoanApplicationPage1Component = tslib_1.__decorate([
        Component({
            selector: 'ls-loan-application-page1',
            templateUrl: './loan-application-page1.component.html',
            styleUrls: ['./loan-application-page1.component.scss']
        }),
        tslib_1.__metadata("design:paramtypes", [])
    ], LoanApplicationPage1Component);
    return LoanApplicationPage1Component;
}(BaseLoanApplicationComponent));
export { LoanApplicationPage1Component };
//# sourceMappingURL=loan-application-page1.component.js.map