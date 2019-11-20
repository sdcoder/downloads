/*globals $, jQuery */
$(function () {
    'use strict';
    $('#FAQSearch').on('keyup', function () {
        $('.faq-content').unhighlight();
        if ($(this).val()) {
            $('.faq-content').highlight($(this).val());
            $('.faq-content .highlight').closest('div.content').show();
            $('.faq-content .highlight').closest('div.content').closest('li').addClass('active');
            $('.faq-content li:not(:has(span.highlight))').removeClass('active');
            $('.faq-content li:not(:has(span.highlight)) > div.content').hide();
        }
    });
    $(document).ready(function () {
        var hash = location.hash;
        if (hash.replace('#', '') !== '') {
            var item = $(hash);
            if (item && !item.parent().hasClass('active')) {
                item.trigger('click');
            }
        }
    });
});
//# sourceMappingURL=faq.js.map