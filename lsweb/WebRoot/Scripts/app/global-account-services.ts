/*globals $, angular, Foundation */
/*jslint browser:true */

interface JQuery {
    gridAdjust(): void;
}

$(function () {
    'use strict';

    //$('.datepicker').datepicker();

    // adding a gridAdjust plugin to jquery
    $.fn.gridAdjust = function () {
        var grid$ = $(this),
            li$ = grid$.find("li");

        if (!$('html').hasClass('ie8')) {
            li$.css("height", "auto");
        }

        li$.each(function () {
            var p$ = $(this).prev(),
                n$ = $(this).next();
            if (p$.length && p$.offset().top === $(this).offset().top) {
                if (p$.outerHeight() !== $(this).outerHeight()) {
                    if (p$.outerHeight() > $(this).outerHeight()) {
                        $(this).css("height", p$.outerHeight() + "px");
                    } else {
                        p$.css("height", $(this).outerHeight() + "px");
                    }
                }
            }
            if (n$.length && n$.offset().top === $(this).offset().top) {
                if (n$.outerHeight() !== $(this).outerHeight()) {
                    if (n$.outerHeight() > $(this).outerHeight()) {
                        $(this).css("height", n$.outerHeight() + "px");
                    } else {
                        n$.css("height", $(this).outerHeight() + "px");
                    }
                }
            }
        });
        return this;
    };

    // adjusting counter offer grids
    if ($("ul.grid-cell-adjust").length) {
        setTimeout(function () {
            $("ul.grid-cell-adjust").gridAdjust();
        }, 0);
        $(window).resize(function () {
            $("ul.grid-cell-adjust").gridAdjust();
        });
    }

    $(document).ready(function () {
        if ($.cookie('LS-FirstTimeSignedIn')) {
            $.removeCookie('LS-FirstTimeSignedIn', { path: '/' }); // => true
            SC.main.signedIn();
        }
    });
});