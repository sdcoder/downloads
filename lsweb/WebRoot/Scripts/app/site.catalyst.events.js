/*jslint browser: true*/
/*globals SC, $ */
/*
    site.catalyst.events

    Use to hook up page actions (such as opening modals, clicking buttins, etc...) to Site Catalyst tracking evevents
*/
$(function () {
    'use strict';
    // hook up to all foundation modal reveal events
    $(document).on('open', '[data-reveal]', function () {
        if ($(this).attr('id') === 'PlantATreeModal') {
            SC.main.plantaTree();
        }
    });
});
//# sourceMappingURL=site.catalyst.events.js.map