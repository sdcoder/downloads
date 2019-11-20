import * as tslib_1 from "tslib";
import { NgModule } from '@angular/core';
import { ValidationModule } from './validation/validation.module';
import { WindowRef } from './native-window.service';
var SharedModule = /** @class */ (function () {
    function SharedModule() {
    }
    SharedModule = tslib_1.__decorate([
        NgModule({
            imports: [ValidationModule],
            declarations: [],
            providers: [WindowRef],
            exports: [ValidationModule]
        })
    ], SharedModule);
    return SharedModule;
}());
export { SharedModule };
//# sourceMappingURL=shared.module.js.map