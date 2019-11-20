/*globals jQuery, $ */
$(function () {
    'use strict';
    $('a:contains("Excellent & Substantial Credit")').text('Good to Excellent Credit');
    $('a:contains("Good Credit")').text('Good to Excellent Credit');
    $('form .disclaimer.forminfo').addClass('teammate-alert').removeClass('forminfo').find('p:first-of-type').prepend('<i class="fa fa-comment"></i>  <strong>Please Read to Client:</strong><br />');
    $('form .add-teammate-alert').addClass('teammate-alert').find('p:first-of-type').prepend('<i class="fa fa-comment"></i>  <strong>Please Read to Client:</strong><br />');
    $('.business-hours').html($('.business-hours').text().replace('and', 'and<br/>'));
});
//# sourceMappingURL=suntrust-process-replacement.js.map