import * as tslib_1 from "tslib";
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LsComponentModule } from '@lightstream/web-components-marketing';
import { PublicLoanApplicationModule } from './loan-application/public-loan-application/public-loan-application.module';
import { PartnerLoanApplicationModule } from './loan-application/partner-loan-application/partner-loan-application.module';
var AppModule = /** @class */ (function () {
    function AppModule() {
    }
    AppModule = tslib_1.__decorate([
        NgModule({
            declarations: [
                AppComponent
            ],
            imports: [
                BrowserModule,
                AppRoutingModule,
                PublicLoanApplicationModule,
                PartnerLoanApplicationModule,
                LsComponentModule
            ],
            providers: [],
            bootstrap: [AppComponent]
        })
    ], AppModule);
    return AppModule;
}());
export { AppModule };
//# sourceMappingURL=app.module.js.map