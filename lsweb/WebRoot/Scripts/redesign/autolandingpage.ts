$(document).ready(function () {
    $('#purposeOfLoanDesktop').appendTo('#autoFormDesktop');
    $('#purposeOfLoanMobile').appendTo('#autoFormMobile');
    $("#purposeOfLoanDesktop,#purposeOfLoanMobile,#PurposeOfLoan").bind("DOMSubtreeModified", function () {
        checkBlankOption(this.id);
    });
    checkBlankOption('purposeOfLoanDesktop');
    checkBlankOption('purposeOfLoanMobile');
    checkBlankOption('PurposeOfLoan');


    $('#purposeOfLoanDesktop, #purposeOfLoanMobile').click(function () {
        hideCtaBars();
    });
});

function checkBlankOption(selectElementId) {
    var opts = $('#' + selectElementId).find('option');
    $.each(opts, function (index, value) {
        if (value.value == 'NotSelected' && value.text != 'Select an Auto Loan')
            value.text = 'Select an Auto Loan';
    });
}

function hideCtaBars() {
    $('#ctaDescriptions').hide();
    $('#ctaDescriptionsMobile').hide();
}
