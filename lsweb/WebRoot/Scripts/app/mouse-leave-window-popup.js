//cross browser event handler
function addEvent(obj, evt, fn) {
    if (obj.addEventListener) {
        obj.addEventListener(evt, fn, false);
    }
    else if (obj.attachEvent) {
        obj.attachEvent("on" + evt, fn);
    }
}

// check if element or any ancestor has a class
function getParents(e, className) {
    var result = [];
    for (var p = e; p; p = p.parentElement) {
        
        // element.matches is not supported in IE11, use plain js code
        var classes = p.className.split(' ');
        var i = 0;
        while (i < classes.length) {
            if (classes[i] == className) {
                result.push(p);
                break;
            }
            else ++i;
        }
    }
    return result;
}

// check if cookie exists by cookie name
function check_cookie_name(name) {
    var match = document.cookie.match(new RegExp("(^| )" + name + "=([^;]+)"));
    if (match) {
        return true;
    }
    return false;
}

// Commented out for next sprint, since Opinion Lab code is not ready yet. 
addEvent(window, "load", function (e) {
    addEvent(document, "mouseout", function (e) {
        e = e ? e : window.event;
        var to = e.relatedTarget || e.toElement;
        var from = e.target;

        //only trigger when mouse move to top bar
        if (e.clientY < 10) {

            if (!check_cookie_name("survey_invite_shown") &&
                !check_cookie_name("TeammateReferralToken") &&
                !check_cookie_name("ApplicationCompleted")) {
                
                //ignore link clicks, and popups
                if (from &&
                    (from.localName != "a" || (from.parentElement && from.parentElement.localName != "a")) &&
                    getParents(from, "reveal-overlay").length == 0
                ) {

                    if (!to || to.nodeName == "HTML") {
                        
                        // Launch Opinion Lab Survey
                        OOo.abandonEntryShow();
                        
                        document.cookie = "survey_invite_shown=1";
                    }
                }
            }
        }
    });
});