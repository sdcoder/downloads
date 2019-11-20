import * as tslib_1 from "tslib";
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoanApplicationPage1Component } from './loan-application-page1/loan-application-page1.component';
import { LoanApplicationPage2Component } from './loan-application-page2/loan-application-page2.component';
import { LoanApplicationPage3Component } from './loan-application-page3/loan-application-page3.component';
import { LoanApplicationPage4Component } from './loan-application-page4/loan-application-page4.component';
import { PublicLoanAppComponent } from './public-loan-app.component';
import { AppRoutingModule } from '../../app-routing.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { BaseLoanApplicationComponent } from './base-loan-application';
import { SharedModule } from 'src/app/shared/shared.module';
var PublicLoanApplicationModule = /** @class */ (function () {
    function PublicLoanApplicationModule() {
    }
    PublicLoanApplicationModule = tslib_1.__decorate([
        NgModule({
            imports: [
                CommonModule,
                AppRoutingModule,
                FormsModule,
                ReactiveFormsModule,
                SharedModule
            ],
            declarations: [
                PublicLoanAppComponent,
                BaseLoanApplicationComponent,
                LoanApplicationPage1Component,
                LoanApplicationPage2Component,
                LoanApplicationPage3Component,
                LoanApplicationPage4Component
            ],
            exports: [
                PublicLoanAppComponent,
                BaseLoanApplicationComponent,
                LoanApplicationPage1Component,
                LoanApplicationPage2Component,
                LoanApplicationPage3Component,
                LoanApplicationPage4Component
            ]
        })
    ], PublicLoanApplicationModule);
    return PublicLoanApplicationModule;
}());
export { PublicLoanApplicationModule };
//# sourceMappingURL=public-loan-application.module.js.map