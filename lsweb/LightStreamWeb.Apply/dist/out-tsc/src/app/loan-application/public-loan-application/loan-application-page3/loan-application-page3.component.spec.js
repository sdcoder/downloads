import { async, TestBed } from '@angular/core/testing';
import { LoanApplicationPage3Component } from './loan-application-page3.component';
describe('LoanApplicationPage3Component', function () {
    var component;
    var fixture;
    beforeEach(async(function () {
        TestBed.configureTestingModule({
            declarations: [LoanApplicationPage3Component]
        })
            .compileComponents();
    }));
    beforeEach(function () {
        fixture = TestBed.createComponent(LoanApplicationPage3Component);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });
    it('should create', function () {
        expect(component).toBeTruthy();
    });
});
//# sourceMappingURL=loan-application-page3.component.spec.js.map