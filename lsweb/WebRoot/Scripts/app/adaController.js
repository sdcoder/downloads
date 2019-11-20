(function () {
    "use strict";
    // ada MODAL/NAGIGATION HANDELING
    var multipleCharIndex = 0;
    var keyCodeMap = Object.freeze({
        "TAB": 9,
        "RETURN": 13,
        "ESC": 27,
        "SPACE": 32,
        "PAGEUP": 33,
        "PAGEDOWN": 34,
        "END": 35,
        "HOME": 36,
        "LEFT": 37,
        "UP": 38,
        "RIGHT": 39,
        "DOWN": 40
    });
    // open submenu if Enter or Space is pressed on an item w/ aria-popup=true
    $("[aria-haspopup=\"true\"]").keydown(function (e) {
        switch (e.keyCode) {
            case keyCodeMap.RETURN:
            case keyCodeMap.SPACE:
            case keyCodeMap.DOWN:
                e.preventDefault();
                openSubMenu($(this));
                setTimeout(function () {
                    $(".m-dropdown.focus > li").first().find("a").focus();
                }, 50);
                break;
            case keyCodeMap.UP:
                e.preventDefault();
                openSubMenu($(this));
                setTimeout(function () {
                    $(".m-dropdown.focus > li").last().find("a").focus();
                }, 50);
                break;
            case keyCodeMap.ESC:
                $(".navbar-navigation").focus();
                break;
            default:
                break;
        }
    });
    $("a.subMenuItem").keydown(function (e) {
        var currentItenIndex = $(this).data("subMenuItemIndex");
        var menuItemTotal = $(this).parent().parent().parent().find("button").data("subMenuItemTotal");
        var thisElement = $(this)[0];
        switch (true) {
            case (e.keyCode === keyCodeMap.TAB):
                closeSubMenus();
                break;
            case (e.keyCode === keyCodeMap.SPACE):
            case (e.keyCode === keyCodeMap.RETURN):
                e.preventDefault();
                thisElement.click();
                break;
            case (e.keyCode === keyCodeMap.UP):
            case (e.keyCode === keyCodeMap.DOWN):
                var atEnd = false;
                if (menuItemTotal - currentItenIndex === 0) {
                    atEnd = true;
                }
                e.preventDefault();
                if (e.keyCode === 40) { // down arrow
                    if (!atEnd) {
                        $(this).parent().next().find("a").focus();
                    }
                    else if (atEnd) {
                        $(this).parent().parent().find("a").first().focus();
                    }
                }
                else if (e.keyCode === 38) { // up arrow
                    if (currentItenIndex === 0) {
                        $(this).parent().parent().find("a").last().focus();
                    }
                    else {
                        $(this).parent().prev().find("a").focus();
                    }
                }
                break;
            case (e.keyCode === keyCodeMap.ESC):
                e.preventDefault();
                closeSubMenus();
                break;
            case (e.keyCode >= 65 && e.keyCode <= 90):
                var charCode = String.fromCharCode(e.keyCode);
                var charItemSelector = $(this).parent().parent().find("[data-startingLetter=\"" + charCode + "\"]");
                var focusSelector = charItemSelector[0];
                var multiCharFocusSelector = charItemSelector[0 + multipleCharIndex];
                if (charItemSelector.length === 1) {
                    focusSelector.focus();
                }
                else if (charItemSelector.length > 1) {
                    multiCharFocusSelector.focus();
                    if (multipleCharIndex === (charItemSelector.length - 1)) {
                        multipleCharIndex = 0;
                    }
                    else {
                        multipleCharIndex++;
                    }
                }
                break;
            default:
                break;
        }
    });
    // init Submenu item data
    function setSubMenuIndexes() {
        $(".menu-container-item [aria-haspopup=\"true\"]").each(function (index) {
            $(this).data("subMenuIndex", index);
            $(this).data("subMenuItemTotal", ($(this).next().children().length - 1));
            $(this).next().children().each(function (subindex) {
                $(this).find("a").data("subMenuItemIndex", subindex);
                $(this).find("a").attr("data-startingLetter", $(this).text().replace(/\s/g, "").slice(0, 1));
            });
        });
    }
    setSubMenuIndexes();
    // open submenu function
    function openSubMenu(thisItem) {
        $(".m-dropdown").removeClass("focus");
        $(".m-dropdown").attr("aria-expanded", "false");
        thisItem.next(".m-dropdown").addClass("focus");
        thisItem.next(".m-dropdown").attr("aria-expanded", "true");
        thisItem.next(".m-dropdown").attr("aria-hidden", "false");
    }
    // close submenus function
    function closeSubMenus() {
        $("ul.m-dropdown > li > a:focus").parent().parent().prev("[aria-haspopup=\"true\"]").focus();
        $(".m-dropdown").removeClass("focus").attr("aria-hidden", "true");
    }
    // close submenues on window scroll
    $(window).scroll(function () {
        closeSubMenus();
    });
    // close submenues if focus is moved to the body
    $(":header").focusin(function () {
        closeSubMenus();
    });
    $('.cardBack').focusin(function () {
        $(this).parent().addClass('focusFlip');
    });
    $('.cardBack').focusout(function () {
        $(this).parent().removeClass('focusFlip');
    });
    // hide focus outlines when focusing with a mouse rather than keyboard
    $("body").on("mousedown", "*", function (e) {
        if (($(this).is(":focus") || $(this).is(e.target)) && $(this).css("outline-style") === "none") {
            $(this).parents().css("outline", "none");
            $(this).css("outline", "none").on("blur", function () {
                $(this).off("blur", function () {
                    $(this).parents().css("outline", "");
                }).css("outline", "");
            });
        }
    });
    $(".infographic-container .screen-reader-only").on("focus", function () {
        $(this).parent().addClass("focus");
    }).on("blur", function () {
        $(this).parent().removeClass("focus");
    });
}());
var AdaFunctions = {
    rateTableIndicatorChange: function (i) {
        $(".loanrates tr").removeClass("active");
        i.parent().addClass("active");
        var col = i.parent().children().index(i);
        $(".loanrates th").removeClass("active");
        $(".loanrates th:eq(" + (col + 1) + ")").addClass("active");
    },
    rateTableIndicatorClear: function () {
        $(".loanrates tr").removeClass("active");
        $(".loanrates th").removeClass("active");
    },
    hotlinkDisclosure: function (id) {
        $("#" + id).click();
    },
    hotlinkSelector: function (selector) {
        $(selector).click();
    },
    goBackToLink: function (originID) {
        $("#" + originID).focus();
        $("#" + originID + "BackLink").remove();
    }
};
// eND MODAL/NAGIGATION HANDELING
// sTART Angular Anchor Scroll
// smoothScroll Service
angular.module("LightStreamApp").service("anchorSmoothScroll", ["$document", "$window", function ($document, $window) {
        var document = $document[0];
        var window = $window;
        function getCurrentPagePosition(window, document) {
            // firefox, Chrome, Opera, Safari
            if (window.pageYOffset) {
                return window.pageYOffset;
            }
            // internet Explorer 6 - standards mode
            if (document.documentElement && document.documentElement.scrollTop) {
                return document.documentElement.scrollTop;
            }
            // internet Explorer 6, 7 and 8
            if (document.body.scrollTop) {
                return document.body.scrollTop;
            }
            return 0;
        }
        function getElementY(document, element) {
            var y = element.offsetTop;
            var node = element;
            while (node.offsetParent && node.offsetParent !== document.body) {
                node = node.offsetParent;
                y += node.offsetTop;
            }
            return y;
        }
        this.scrollDown = function (startY, stopY, speed, distance) {
            var timer = 0;
            var step = Math.round(distance / 25);
            var leapY = startY + step;
            for (var i = startY; i < stopY; i += step) {
                setTimeout("window.scrollTo(0, " + leapY + ")", timer * speed);
                leapY += step;
                if (leapY > stopY) {
                    leapY = stopY;
                }
                timer++;
            }
        };
        this.scrollUp = function (startY, stopY, speed, distance) {
            var timer = 0;
            var step = Math.round(distance / 25);
            var leapY = startY - step;
            for (var i = startY; i > stopY; i -= step) {
                setTimeout("window.scrollTo(0, " + leapY + ")", timer * speed);
                leapY -= step;
                if (leapY < stopY) {
                    leapY = stopY;
                }
                timer++;
            }
        };
        this.scrollToTop = function (stopY) {
            scrollTo(0, stopY);
        };
        this.scrollTo = function (elementId, speed) {
            var element = document.getElementById(elementId);
            var navBuffer = 210; // this is to create an offset for our static navbar, modify as needed
            if (element) {
                var startY = getCurrentPagePosition(window, document);
                var stopY = getElementY(document, element);
                var distance = stopY > startY ? stopY - startY : startY - stopY;
                if (distance < 100) {
                    this.scrollToTop(stopY - navBuffer);
                }
                else {
                    var defaultSpeed = Math.round(distance / 100);
                    speed = speed || (defaultSpeed > 20 ? 20 : defaultSpeed);
                    if (stopY > startY) {
                        this.scrollDown(startY, stopY - navBuffer, speed, distance);
                    }
                    else {
                        this.scrollUp(startY, stopY - navBuffer, speed, distance);
                    }
                }
            }
        };
    }]);
// disclosure Scroll Controller for Anchor Links
angular.module("LightStreamApp").controller("AdaCtrl", ["$scope", "$location", "anchorSmoothScroll",
    function ($scope, $location, anchorSmoothScroll) {
        $scope.gotoElement = function (eID) {
            // set the location.hash to the id of
            // the element you wish to scroll to.
            // $location.hash("bottom"); //disabled to avoid urlbar change
            // call $anchorScroll()
            anchorSmoothScroll.scrollTo(eID);
        };
        $scope.appendBackLink = function (eID, originID) {
            //var backClk: string = "javascript: angular.element(this).scope().goBackToLink( \'" + originID + "\' ); $(this).remove(); void 0;";
            var backClk = "javascript: AdaFunctions.goBackToLink( \'" + originID + "\' ); $(this).remove(); void 0;";
            var backTxt = "Back to content &#62;";
            var backAda = "Back to content.";
            var backID = originID + "BackLink";
            var backClass = "BackToContentLink";
            var backPt1 = "<a aria-label=\"" + backAda + "\" class=\"" + backClass + "\" id=\"" + backID + "\" href=\"#\" onClick=\"" + backClk + "\">";
            var backLink = backPt1 + backTxt + "</a>";
            $(".BackToContentLink").remove(); // remove any existing links to avoid duplicates
            // $(backLink).insertAfter("#" + eID);
            $("#" + eID).next().after(backLink);
            $scope.$apply();
        };
        $scope.goBackToLink = function (originID) {
            $scope.gotoElement(originID);
            $("#" + originID).focus();
        };
    }]);
// disclosureLink Directive for Anchor Links
angular.module("LightStreamApp").directive("sup", function () {
    return {
        restrict: "E",
        controller: "AdaCtrl",
        link: function (scope, elm, attrs) {
            if (attrs.content !== "" && (elm.attr("class") !== "footerSup" &&
                elm.attr("class") !== "noLinkSup") && document.documentElement.clientWidth > 725 &&
                elm.text().charCodeAt(0) != 174 && elm.text().charCodeAt(0) != 169) {
                elm.html("<a tabindex=\"0\" role=\"link\">" + elm.html() + "</a>");
                elm.find("a").attr("class", "disclosureAnchorLink");
                var ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var ID_LENGTH = 5;
                var generateID = function () {
                    var rtn = "";
                    for (var i = 0; i < ID_LENGTH; i++) {
                        rtn += ALPHABET.charAt(Math.floor(Math.random() * ALPHABET.length));
                    }
                    return rtn;
                };
                elm.find("a").attr("id", generateID());
                elm.find("a").attr("linkedAnchor", getLinkedAnchor(elm.text()));
                if (elm.find("a").text() === "\u2020") {
                    elm.find("a").attr("aria-label", "Click to review same day funding disclosure.");
                    elm.find("a").attr("linkedAnchor", "SameDayFundingDisclosure");
                }
                else if (elm.find("a").text() === "\u2021") {
                    elm.find("a").attr("aria-label", "Click to review AAA discount disclosure.");
                    elm.find("a").attr("linkedAnchor", "footerdisclosure-dbldagger");
                }
                else if (elm.find("a").text() === "*") {
                    elm.find("a").attr("aria-label", "Click to review APR.");
                    elm.find("a").attr("linkedAnchor", "footerdisclosure-apr");
                }
                else if (elm.find("a").text() === "1" || elm.find("a").text() === "2" || elm.find("a").text() === "3") { //
                    elm.find("a").attr("aria-label", "Click to review disclosure " + elm.find("a").text() + ".");
                }
                else {
                    elm.find("a").attr("aria-label", "Click to review conditions for same-day funding.");
                }
                elm.find("a").on("click", relocateMe);
                elm.find("a").bind("keydown keypress", function (event) {
                    if (event.which === 13) {
                        $(this).click();
                        event.preventDefault();
                    }
                });
                if (!!elm.parent().closest(':header').length && elm.parents("#feature-bullets").length != 1 && !!elm.parent().not("span").length && elm[0].className != 'discounted-rate') {
                    elm.parent().css("display", "inline-block");
                    elm.parent().after(elm.parent().clone().html("").removeAttr('tabindex'));
                    elm.appendTo(elm.parent().next());
                }
                if (elm.parent().parent().attr("id") === "ctaDescriptions") {
                    var supStringTxt = elm.find("a").text();
                    var supStringClick = "tabindex=\"-1\" onClick=\"AdaFunctions.hotlinkDisclosure(\'" + elm.find("a").attr("id") + "\');\"";
                    var supStringWrap = "<sup class=\"footerSup\"" + supStringClick + ">" + supStringTxt + "</sup>";
                    elm.before(supStringWrap);
                    elm.parent().after(elm);
                    elm.focusin(function () {
                        $(this).prev().find("sup.footerSup").css("text-decoration", "underline");
                    }).focusout(function () {
                        $(this).prev().find("sup.footerSup").css("text-decoration", "none");
                    });
                }
            }
            else if (document.documentElement.clientWidth < 725) {
                if (elm.hasClass("footerSup")) {
                    elm.attr("class", "");
                }
            }
            function getLinkedAnchor(supText) {
                return "footerdisclosure-" + escape(supText).replace("%", "");
            }
            function relocateMe() {
                var linkedAnchor = elm.find("a").attr("linkedAnchor");
                var myID = elm.find("a").attr("id");
                var myContent = elm.find("a").text();
                if ((myContent === "*" || myContent === "apr" || myContent === "1" ||
                    myContent === "2" || myContent === "3" || myContent === "4" ||
                    myContent === "\u2020" || myContent === "\u2021") &&
                    !$(this).parent().hasClass("ada-container")) {
                    scope.appendBackLink(linkedAnchor, myID); // append link
                    scope.gotoElement(linkedAnchor); // scroll to anchor
                    setTimeout(function () {
                        $("#" + linkedAnchor).focus(); // move Focus
                    }, 500); // wait 0.5s before moving focus to avoid scroll glitch
                }
            }
        }
    };
});
// end Angular Anchor Scroll
$(function () {
    $(".applyInMinutesBig sup.screen-reader-only a.disclosureAnchorLink").focusin(function () {
        $(".lightstream-arrow-description sup.footerSup").css("text-decoration", "underline");
    }).focusout(function () {
        $(".lightstream-arrow-description sup.footerSup").css("text-decoration", "initial");
    });
    // reformat disclosures to prevent auto-readback from screen reader when focus changes
    $(".ada-container").each(function () {
        var disclosureType;
        if ($(this).attr('id') == 'SameDayFundingDisclosure') {
            disclosureType = 'Same Day Funding Disclosure';
            $(this).after('<div class="ada-container">' + $(this).html() + '</div><br>').removeClass('ada-container').attr('aria-label', disclosureType).css('padding-bottom', '0px');
        }
        else {
            if ($(this).attr('id') == 'footerdisclosure-apr') {
                disclosureType = 'APR Disclosure';
            }
            else {
                disclosureType = 'Disclosure ' + $(this).find('sup').text();
            }
            $(this).after('<div class="ada-container">' + $(this).html() + '</div>').html('').removeClass('ada-container').addClass('screen-reader-only').text(disclosureType);
        }
    });
    setTimeout(function () { $('#SameDayFundingDisclosure').html(''); }, 300);
    // skip To Content Link Fix
    $("#skiptocontentlink").on("click", function (e) {
        var headerSelector = $("header :header:first-child");
        if (headerSelector.length > 1) {
            headerSelector.focus();
        }
        else {
            $("header").next().find(":header:first").focus();
        }
        e.preventDefault();
    });
    $("#skiptocontentlink").bind("keydown keypress", function (e) {
        if (e.which === 13) {
            $("#skiptocontentlink").click();
            e.preventDefault();
        }
    });
    if ($("sup:contains('\u2020')").length) {
        $("#SameDayFundingDisclosure").show().load("/components/samedayfundingdisclosure");
    }
});
//# sourceMappingURL=adaController.js.map