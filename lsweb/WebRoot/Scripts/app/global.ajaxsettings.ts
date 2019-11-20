/*global $, jQuery*/

(function ($) {
    'use strict';

    $.ajaxPrefilter(function (config: any, originalOpts: JQueryAjaxSettings, jqXHR: JQueryXHR) {
        if (config.url.substring(0, 1) === '/' || config.url.substring(0, 1) === '.') {
            //requesting an external resource
            if (config.url.substring(0, 2) === '//')
                return config;
            //requesting an internal resource

            //--don't lowercase query string
            config.url = config.url.split('?')[0].toLowerCase();

            //--don't lowercase jump link
            config.url = config.url.split('#')[0].toLowerCase();

            return config;
        }

        return config;
    });
}(jQuery));
