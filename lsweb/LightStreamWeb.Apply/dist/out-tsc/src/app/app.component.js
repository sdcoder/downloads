import * as tslib_1 from "tslib";
import { Component } from '@angular/core';
import { RoutingStateService } from './shared/routing-state.service';
var AppComponent = /** @class */ (function () {
    function AppComponent(routingService) {
        this.title = 'lightstream-web-apply';
        routingService.loadRouting();
    }
    AppComponent.prototype.ngOnInit = function () {
        $(document).foundation();
    };
    AppComponent = tslib_1.__decorate([
        Component({
            selector: 'app-root',
            templateUrl: './app.component.html',
            styleUrls: ['./app.component.scss'],
            providers: [RoutingStateService]
        }),
        tslib_1.__metadata("design:paramtypes", [RoutingStateService])
    ], AppComponent);
    return AppComponent;
}());
export { AppComponent };
//# sourceMappingURL=app.component.js.map