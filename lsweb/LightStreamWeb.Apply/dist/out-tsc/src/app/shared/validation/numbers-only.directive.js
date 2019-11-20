import * as tslib_1 from "tslib";
import { Directive, ElementRef, HostListener, Input, Renderer2 } from '@angular/core';
import { FormControl } from '@angular/forms';
import { KeyBoardValues } from '../keyboard-values';
var NumbersOnlyDirective = /** @class */ (function () {
    function NumbersOnlyDirective(el, renderer) {
        this.el = el;
        this.renderer = renderer;
    }
    NumbersOnlyDirective.prototype.input = function (event) {
        var e = event;
        var pattern = new RegExp('^[0-9]*$');
        var value = event.target.value;
        var validValue = value;
        // replace invalid characters
        if (!pattern.test(value)) {
            validValue = value.replace(/[^0-9]/g, ''); // $formatter equivalent
            e.preventDefault();
        }
        var maxLength = Number.parseInt(event.target.getAttribute('maxLength'), 10);
        // if the value is too long, cut it off
        if (!Number.isNaN(maxLength) && validValue.length > maxLength) {
            validValue = validValue.substring(0, maxLength);
        }
        // update the view (triggers write value)
        this.lsNumbersOnly.patchValue(validValue);
        // update the model
        this.onChange(validValue);
    };
    NumbersOnlyDirective.prototype.keydown = function (event) {
        var e = event;
        // TODO: also use keycode (deprecated) as secondary check?
        if (e.ctrlKey || KeyBoardValues.navigationKeys.includes(e.key)) {
            return true;
        }
        var number = Number.parseInt(e.key, 10);
        if (Number.isNaN(number) || e.shiftKey) {
            e.preventDefault();
        }
        return true;
    };
    NumbersOnlyDirective.prototype.writeValue = function (obj) {
        this.renderer.setProperty(this.el.nativeElement, 'value', obj);
    };
    NumbersOnlyDirective.prototype.registerOnChange = function (fn) {
        this.onChange = fn;
    };
    NumbersOnlyDirective.prototype.registerOnTouched = function (fn) {
        this.onTouched = fn;
    };
    var _a, _b, _c;
    tslib_1.__decorate([
        Input(),
        tslib_1.__metadata("design:type", typeof (_a = typeof FormControl !== "undefined" && FormControl) === "function" ? _a : Object)
    ], NumbersOnlyDirective.prototype, "lsNumbersOnly", void 0);
    tslib_1.__decorate([
        HostListener('input', ['$event']),
        tslib_1.__metadata("design:type", Function),
        tslib_1.__metadata("design:paramtypes", [Object]),
        tslib_1.__metadata("design:returntype", void 0)
    ], NumbersOnlyDirective.prototype, "input", null);
    tslib_1.__decorate([
        HostListener('keydown', ['$event']),
        tslib_1.__metadata("design:type", Function),
        tslib_1.__metadata("design:paramtypes", [Object]),
        tslib_1.__metadata("design:returntype", void 0)
    ], NumbersOnlyDirective.prototype, "keydown", null);
    NumbersOnlyDirective = tslib_1.__decorate([
        Directive({
            selector: '[lsNumbersOnly]',
            providers: [
            // {
            //   provide: NG_VALUE_ACCESSOR,
            //   useExisting: forwardRef(() => NumbersOnlyDirective),
            //   multi: true
            // }
            ]
        }),
        tslib_1.__metadata("design:paramtypes", [typeof (_b = typeof ElementRef !== "undefined" && ElementRef) === "function" ? _b : Object, typeof (_c = typeof Renderer2 !== "undefined" && Renderer2) === "function" ? _c : Object])
    ], NumbersOnlyDirective);
    return NumbersOnlyDirective;
}());
export { NumbersOnlyDirective };
// TODO: Double check on whether we still need this from old AngularJS directives
// watch min/max value changes?
//# sourceMappingURL=numbers-only.directive.js.map