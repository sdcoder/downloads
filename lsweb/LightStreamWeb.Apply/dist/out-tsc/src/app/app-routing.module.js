import * as tslib_1 from "tslib";
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PartnerLoanApplicationComponent } from './loan-application/partner-loan-application/partner-loan-application.component';
import { PublicLoanAppComponent } from './loan-application/public-loan-application/public-loan-app.component';
var routes = [
    {
        path: 'partner/apply',
        component: PartnerLoanApplicationComponent
    },
    {
        path: 'apply2',
        redirectTo: 'apply2/1',
        pathMatch: 'full'
    },
    {
        path: 'apply2/:pageId',
        component: PublicLoanAppComponent
    },
    {
        path: '',
        redirectTo: 'apply2/1',
        pathMatch: 'full'
    }
];
var AppRoutingModule = /** @class */ (function () {
    function AppRoutingModule() {
    }
    AppRoutingModule = tslib_1.__decorate([
        NgModule({
            imports: [RouterModule.forRoot(routes)],
            exports: [RouterModule]
        })
    ], AppRoutingModule);
    return AppRoutingModule;
}());
export { AppRoutingModule };
//# sourceMappingURL=app-routing.module.js.map