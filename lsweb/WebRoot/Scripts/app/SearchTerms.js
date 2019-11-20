/*jslint browser: true*/
/*jslint plusplus: true */
/*jslint regexp: true */
/*global unescape, $ */ // this is widely supported
$(function () {
    'use strict';
    var i, qs, qsa, qsip, wordstring, ref = document.referrer;
    if (ref.indexOf('?') === -1) {
        return;
    }
    qs = ref.substr(ref.indexOf('?') + 1);
    qsa = qs.split('&');
    for (i = 0; i < qsa.length; i++) {
        qsip = qsa[i].split('=');
        if (qsip.length > 1) {
            if (qsip[0] === 'q' || qsip[0] === 'p') { // q= for Google / Bing, p= for Yahoo
                wordstring = decodeURI(qsip[1].replace(/\+/g, ' '));
                $.cookie('SearchTerms', wordstring);
                $.cookie('SearchEngine', ref.match(/:\/\/(.[^\/]+)/)[1]);
            }
        }
    }
});
//# sourceMappingURL=SearchTerms.js.map