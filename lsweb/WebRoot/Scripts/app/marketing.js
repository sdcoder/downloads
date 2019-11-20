///*globals s, window, document */
//(function () {
//    'use strict';

//    // disable super-slow omniture click tracking on some links
//    var old = s.mr;
//    s.mr = function (sess, q, rs, ta, u) {
//        if (window.event && window.event.toElement && window.event.toElement.classList.contains("no-track")) {
//            return;
//        }
//        old.call(this, sess, q, rs, ta, u);
//    };
//}());


/*global tracking*/
(function ($, SC) {
    var _setupVimeoTracking = function () {
        $('iframe[src*="player.vimeo.com"]').each(function (idx, iframe) {
            var vimeoPlayer = new Vimeo.Player(iframe);

            var vimeoTracker = new $.AdobeVimeoTracking({
                vimeoPlayer: vimeoPlayer
            });

            vimeoTracker.videoStarted().then(SC.main.vimeoVideo);
            vimeoTracker.video25PercentWatched().then(SC.main.vimeoVideo);
            vimeoTracker.video50PercentWatched().then(SC.main.vimeoVideo);
            vimeoTracker.video75PercentWatched().then(SC.main.vimeoVideo);
            vimeoTracker.videoCompleted().then(SC.main.vimeoVideo);
        });
    }

    var _onDocumentReady = function () {
        if (typeof Vimeo == 'undefined')
            return;

        _setupVimeoTracking();
    }

    var _init = function () {
        $(document).ready(_onDocumentReady);
    }

    _init();
})(jQuery, SC);