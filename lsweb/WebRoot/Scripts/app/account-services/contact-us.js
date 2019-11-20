$(function () {
    $('#ContactUsForm').on('submit', function (result) {
        $.ajax({
            type: 'POST',
            url: '/modals/account-services-contact-us',
            data: {
                Name: $('#Name').val(),
                EmailAddress: $('#EmailAddress').val(),
                Subject: $('#Subject option:selected').text(),
                Message: $('#Message').val(),
                ApplicationId: $('#Subject').val()
            }
        }).done(function (result) {
            if (result.Success) {
                $('#FormContent').fadeOut(200, function () {
                    $('#ThankYouMessage').show();
                });
            }
            else {
                $('#FormContent .error-message').text(result.ErrorMessage).show();
            }
        });
        return false;
    });
    setTimeout(function () {
        $('#ContactUsForm input[name=Name]').focus();
    }, 200);
});
//# sourceMappingURL=contact-us.js.map