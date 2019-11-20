import * as tslib_1 from "tslib";
import { NgModule } from '@angular/core';
import { NumbersOnlyDirective } from './numbers-only.directive';
import { PhoneNumberDirective } from './phone-number.directive';
var ValidationModule = /** @class */ (function () {
    function ValidationModule() {
    }
    ValidationModule = tslib_1.__decorate([
        NgModule({
            imports: [],
            declarations: [NumbersOnlyDirective, PhoneNumberDirective],
            exports: [NumbersOnlyDirective, PhoneNumberDirective]
        })
    ], ValidationModule);
    return ValidationModule;
}());
export { ValidationModule };
//# sourceMappingURL=validation.module.js.map