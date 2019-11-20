import { async, TestBed } from '@angular/core/testing';
import { LoanApplicationPage4Component } from './loan-application-page4.component';
describe('LoanApplicationPage4Component', function () {
    var component;
    var fixture;
    beforeEach(async(function () {
        TestBed.configureTestingModule({
            declarations: [LoanApplicationPage4Component]
        })
            .compileComponents();
    }));
    beforeEach(function () {
        fixture = TestBed.createComponent(LoanApplicationPage4Component);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });
    it('should create', function () {
        expect(component).toBeTruthy();
    });
});
//# sourceMappingURL=loan-application-page4.component.spec.js.map