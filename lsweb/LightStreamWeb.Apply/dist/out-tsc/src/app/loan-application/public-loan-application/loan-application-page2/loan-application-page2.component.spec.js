import { async, TestBed } from '@angular/core/testing';
import { LoanApplicationPage2Component } from './loan-application-page2.component';
describe('LoanApplicationPage2Component', function () {
    var component;
    var fixture;
    beforeEach(async(function () {
        TestBed.configureTestingModule({
            declarations: [LoanApplicationPage2Component]
        })
            .compileComponents();
    }));
    beforeEach(function () {
        fixture = TestBed.createComponent(LoanApplicationPage2Component);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });
    it('should create', function () {
        expect(component).toBeTruthy();
    });
});
//# sourceMappingURL=loan-application-page2.component.spec.js.map