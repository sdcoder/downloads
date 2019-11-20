import { async, TestBed } from '@angular/core/testing';
import { LoanApplicationPage1Component } from './loan-application-page1.component';
describe('LoanApplicationPage1Component', function () {
    var component;
    var fixture;
    beforeEach(async(function () {
        TestBed.configureTestingModule({
            declarations: [LoanApplicationPage1Component]
        })
            .compileComponents();
    }));
    beforeEach(function () {
        fixture = TestBed.createComponent(LoanApplicationPage1Component);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });
    it('should create', function () {
        expect(component).toBeTruthy();
    });
});
//# sourceMappingURL=loan-application-page1.component.spec.js.map