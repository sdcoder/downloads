import * as tslib_1 from "tslib";
import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
var BaseLoanApplicationComponent = /** @class */ (function () {
    function BaseLoanApplicationComponent() {
    }
    Object.defineProperty(BaseLoanApplicationComponent.prototype, "page1", {
        get: function () {
            return this.form.get('page1');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BaseLoanApplicationComponent.prototype, "page2", {
        get: function () {
            return this.form.get('page2');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BaseLoanApplicationComponent.prototype, "page3", {
        get: function () {
            return this.form.get('page3');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BaseLoanApplicationComponent.prototype, "page4", {
        get: function () {
            return this.form.get('page4');
        },
        enumerable: true,
        configurable: true
    });
    BaseLoanApplicationComponent.prototype.ngOnInit = function () {
    };
    var _a;
    tslib_1.__decorate([
        Input(),
        tslib_1.__metadata("design:type", typeof (_a = typeof FormGroup !== "undefined" && FormGroup) === "function" ? _a : Object)
    ], BaseLoanApplicationComponent.prototype, "form", void 0);
    BaseLoanApplicationComponent = tslib_1.__decorate([
        Component({
            selector: 'ls-base-loan-appliation',
            template: ''
        }),
        tslib_1.__metadata("design:paramtypes", [])
    ], BaseLoanApplicationComponent);
    return BaseLoanApplicationComponent;
}());
export { BaseLoanApplicationComponent };
//# sourceMappingURL=base-loan-application.js.map