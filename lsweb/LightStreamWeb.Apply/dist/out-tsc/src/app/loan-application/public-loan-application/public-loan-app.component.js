import * as tslib_1 from "tslib";
import { Component } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { PaymentTypes, ApplicationTypes, LoanPurposes } from './loan-application';
import { ActivatedRoute, Router } from '@angular/router';
import { LocationStrategy } from '@angular/common';
import { WindowRef } from 'src/app/shared/native-window.service';
import { ValidatePhoneNumber } from 'src/app/shared/validation/validation';
var PublicLoanAppComponent = /** @class */ (function () {
    function PublicLoanAppComponent(formBuilder, locationStrategy, router, route, windowRef) {
        var _this = this;
        this.formBuilder = formBuilder;
        this.locationStrategy = locationStrategy;
        this.router = router;
        this.route = route;
        this.windowRef = windowRef;
        this.loanAppSessionStorageKey = 'loan-app';
        this.applicationTypes = [];
        this.loanPurposes = [];
        this.paymentTypes = [];
        // TODO: write generic function?
        Object.keys(ApplicationTypes).forEach(function (key) {
            _this.applicationTypes.push({ value: key, label: ApplicationTypes[key] });
        });
        Object.keys(LoanPurposes).forEach(function (key) {
            _this.loanPurposes.push({ value: key, label: LoanPurposes[key] });
        });
        Object.keys(PaymentTypes).forEach(function (key) {
            _this.paymentTypes.push({ value: key, label: PaymentTypes[key] });
        });
        var loanAppStorage = this.windowRef.getSessionStorage(this.loanAppSessionStorageKey);
        this.loanAppForm = this.getDefaultLoanForm();
        if (loanAppStorage) {
            this.applyFormValues(this.loanAppForm, JSON.parse(loanAppStorage));
        }
        else {
            this.loanAppForm = this.getDefaultLoanForm();
        }
    }
    Object.defineProperty(PublicLoanAppComponent.prototype, "page1", {
        get: function () {
            return this.loanAppForm.get('page1');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(PublicLoanAppComponent.prototype, "page2", {
        get: function () {
            return this.loanAppForm.get('page2');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(PublicLoanAppComponent.prototype, "page3", {
        get: function () {
            return this.loanAppForm.get('page3');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(PublicLoanAppComponent.prototype, "page4", {
        get: function () {
            return this.loanAppForm.get('page4');
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(PublicLoanAppComponent.prototype, "navigation", {
        get: function () {
            return this.loanAppForm.get('navigation');
        },
        enumerable: true,
        configurable: true
    });
    PublicLoanAppComponent.prototype.applyFormValues = function (group, formValues) {
        var _this = this;
        Object.keys(formValues).forEach(function (key) {
            var formControl = group.controls[key];
            if (formControl instanceof FormGroup) {
                _this.applyFormValues(formControl, formValues[key]);
            }
            else {
                formControl.setValue(formValues[key]);
                // todo: there has to be a better way?
                // build a generic method?
                // need to get the one that points to the instantiated instance
                var fromEnum = void 0;
                switch (key) {
                    case 'applicationType':
                        fromEnum = _this.applicationTypes.find(function (a) { return a.value === formValues[key].value; });
                        break;
                    case 'paymentType':
                        fromEnum = _this.paymentTypes.find(function (a) { return a.value === formValues[key].value; });
                        break;
                    case 'loanPurpose':
                        fromEnum = _this.loanPurposes.find(function (a) { return a.value === formValues[key].value; });
                        break;
                }
                if (fromEnum) {
                    formControl.setValue(fromEnum);
                }
            }
        });
    };
    PublicLoanAppComponent.prototype.ngOnInit = function () {
        this.onRouteParamChange();
        this.onPopState();
        this.onValueChanges();
    };
    PublicLoanAppComponent.prototype.continue = function () {
        var allowContinue = true;
        switch (this.currentPage) {
            case 3:
                allowContinue = this.page3.valid;
                break;
            case 2:
                allowContinue = this.page2.valid;
                break;
            case 1:
                allowContinue = this.page1.valid;
                break;
            default:
                break;
        }
        if (allowContinue) {
            this.setCurrentPage(this.currentPage + 1, true);
        }
        else {
            this.invalidContinue = true;
        }
    };
    PublicLoanAppComponent.prototype.evaluatePreviewState = function (pageNumber) {
        this.invalidContinue = false;
        switch (pageNumber) {
            case 4:
                this.setPreviewState(this.page4, this.page3);
                break;
            case 3:
                this.setPreviewState(this.page3, this.page2);
                break;
            case 2:
                this.setPreviewState(this.page2, this.page1);
                break;
            case 1:
                this.page1.enable();
                this.navigation.enable();
                break;
            default:
                break;
        }
    };
    PublicLoanAppComponent.prototype.getLoanApp = function () {
        this.windowRef.getSessionStorage(this.loanAppSessionStorageKey);
    };
    PublicLoanAppComponent.prototype.getDefaultLoanForm = function () {
        return this.formBuilder.group({
            page1: this.formBuilder.group({
                applicationType: ['', Validators.required],
                loanPurpose: [null, Validators.required],
                zipCode: ['', Validators.required],
                loanAmount: [
                    '',
                    [
                        Validators.required,
                        Validators.min(5000),
                        Validators.max(100000),
                        Validators.maxLength(12)
                    ]
                ],
                loanTermMonths: [
                    '',
                    [
                        Validators.required,
                        Validators.min(24),
                        Validators.max(144),
                        Validators.minLength(2),
                        Validators.maxLength(3),
                        Validators.pattern('^[0-9]*$')
                    ]
                ],
                paymentType: [this.paymentTypes[0], Validators.required]
            }),
            page2: this.formBuilder.group({
                firstName: ['', Validators.required],
                middleInitial: [''],
                lastName: ['', Validators.required],
                email: [
                    '',
                    [
                        Validators.required,
                        Validators.maxLength(50),
                        Validators.pattern('^(?!.*([\\.])\\1)[a-z0-9_\\-\\.\\+]{2,}@[a-z0-9_\\-\\.]{1,}\\.[a-z]{2,}$')
                    ]
                ],
                residencePhoneNumber: [
                    '',
                    [
                        Validators.required,
                        Validators.maxLength(14),
                        Validators.pattern('\\d*'),
                        ValidatePhoneNumber
                    ]
                ]
            }),
            page3: this.formBuilder.group({
                userId: ['', Validators.required],
                password: ['', Validators.required]
            }),
            navigation: this.formBuilder.group({
                continue: ['Continue'],
                submit: ['Submit']
            })
        });
    };
    PublicLoanAppComponent.prototype.onRouteParamChange = function () {
        var _this = this;
        this.route.params.subscribe(function (params) {
            _this.setCurrentPage(Number(params['pageId']), false);
        });
    };
    PublicLoanAppComponent.prototype.onPopState = function () {
        var _this = this;
        this.locationStrategy.onPopState(function (event) {
            if (event.state) {
                if (event.state.pageNumber) {
                    _this.setCurrentPage(event.state.pageNumber, false);
                }
                else if (event.state.navigationId) {
                    _this.setCurrentPage(Number(_this.route.params['pageId']), false);
                }
                else {
                    console.log('no valid event state parameters');
                }
            }
            else {
                console.log('no event state');
            }
        });
    };
    PublicLoanAppComponent.prototype.onValueChanges = function () {
        var _this = this;
        this.page1.valueChanges.subscribe(function (val) {
            _this.loanAppForm.value.page1 = val;
            _this.updateSessionStorage(val);
        });
        this.page2.valueChanges.subscribe(function (val) {
            _this.loanAppForm.value.page2 = val;
            _this.updateSessionStorage(val);
        });
        this.page3.valueChanges.subscribe(function (val) {
            _this.loanAppForm.value.page3 = val;
            _this.updateSessionStorage(val);
        });
        this.page4.valueChanges.subscribe(function (val) {
            _this.loanAppForm.value.page4 = val;
            _this.updateSessionStorage(val);
        });
    };
    PublicLoanAppComponent.prototype.setCurrentPage = function (pageNumber, shouldUpdatePushState) {
        switch (pageNumber) {
            case 4:
            case 3:
            case 2:
            case 1:
                this.currentPage = pageNumber;
                break;
            default:
                this.currentPage = 1;
                break;
        }
        console.log('setting current page: ' + this.currentPage);
        if (shouldUpdatePushState) {
            this.updatePushState(this.currentPage);
        }
        this.evaluatePreviewState(pageNumber);
    };
    PublicLoanAppComponent.prototype.setPreviewState = function (currentPageGroup, previousPageGroup) {
        if (previousPageGroup.invalid || previousPageGroup.disabled) {
            currentPageGroup.disable();
            this.navigation.disable();
        }
        else {
            currentPageGroup.enable();
            this.navigation.enable();
        }
    };
    PublicLoanAppComponent.prototype.updatePushState = function (currentPage) {
        var url = this.router.createUrlTree(['apply2/' + currentPage]).toString();
        this.locationStrategy.pushState({ pageNumber: currentPage }, '', url, '');
    };
    PublicLoanAppComponent.prototype.updateSessionStorage = function (val) {
        this.windowRef.setSessionStorage(this.loanAppSessionStorageKey, JSON.stringify(this.loanAppForm.value));
    };
    var _a, _b, _c, _d;
    PublicLoanAppComponent = tslib_1.__decorate([
        Component({
            selector: 'ls-public-loan-app',
            templateUrl: './public-loan-app.component.html',
            styleUrls: ['./public-loan-app.component.scss']
        }),
        tslib_1.__metadata("design:paramtypes", [typeof (_a = typeof FormBuilder !== "undefined" && FormBuilder) === "function" ? _a : Object, typeof (_b = typeof LocationStrategy !== "undefined" && LocationStrategy) === "function" ? _b : Object, typeof (_c = typeof Router !== "undefined" && Router) === "function" ? _c : Object, typeof (_d = typeof ActivatedRoute !== "undefined" && ActivatedRoute) === "function" ? _d : Object, WindowRef])
    ], PublicLoanAppComponent);
    return PublicLoanAppComponent;
}());
export { PublicLoanAppComponent };
//# sourceMappingURL=public-loan-app.component.js.map