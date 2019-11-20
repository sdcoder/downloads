import { async, TestBed } from '@angular/core/testing';
import { PartnerLoanApplicationComponent } from './partner-loan-application.component';
describe('PartnerLoanApplicationComponent', function () {
    var component;
    var fixture;
    beforeEach(async(function () {
        TestBed.configureTestingModule({
            declarations: [PartnerLoanApplicationComponent]
        })
            .compileComponents();
    }));
    beforeEach(function () {
        fixture = TestBed.createComponent(PartnerLoanApplicationComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });
    it('should create', function () {
        expect(component).toBeTruthy();
    });
});
//# sourceMappingURL=partner-loan-application.component.spec.js.map