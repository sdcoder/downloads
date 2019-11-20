import * as tslib_1 from "tslib";
import { Injectable } from '@angular/core';
function _window() {
    return window;
}
var WindowRef = /** @class */ (function () {
    function WindowRef() {
    }
    Object.defineProperty(WindowRef.prototype, "nativeWindow", {
        get: function () {
            return _window();
        },
        enumerable: true,
        configurable: true
    });
    WindowRef.prototype.setSessionStorage = function (key, value) {
        this.nativeWindow.window.sessionStorage.setItem(key, value);
    };
    WindowRef.prototype.getSessionStorage = function (key) {
        return this.nativeWindow.window.sessionStorage.getItem(key);
    };
    WindowRef = tslib_1.__decorate([
        Injectable()
    ], WindowRef);
    return WindowRef;
}());
export { WindowRef };
//# sourceMappingURL=native-window.service.js.map