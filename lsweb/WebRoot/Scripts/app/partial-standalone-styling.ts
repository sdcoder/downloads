/*global jQuery*/
/*jslint browser: true*/
/*jslint evil: true */
if (typeof jQuery == 'undefined') {
    if (document.getElementsByTagName('body')[0].classList && document.getElementsByTagName('body')[0].classList.length === 0) {
        var f = document.createElement('link');
        f.setAttribute('rel', 'stylesheet');
        f.setAttribute('href', 'https://cdn.jsdelivr.net/foundation/5.4.7/css/foundation.min.css');
        document.body.appendChild(f);

        var node = document.createElement('style');
        node.innerHTML = "body { font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin:2em; padding-top:120px; max-width: 800px; background: url(/content/images/logo_tm_color.jpg) no-repeat left top; } .close-reveal-modal { display:none; }";
        document.body.appendChild(node);
    } else if (!document.getElementsByTagName('body')[0].classList) {
        // IE8
        document.write("<style>body { font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin:2em; padding-top:120px; background: url(/content/images/logo_tm_color.jpg) no-repeat left top; } .close-reveal-modal { display:none; }</style>");
    }
}
