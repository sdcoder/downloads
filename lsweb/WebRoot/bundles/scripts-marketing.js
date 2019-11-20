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
/*!
* jQuery Cookie Plugin v1.3.1
* https://github.com/carhartl/jquery-cookie
*
* Copyright 2013 Klaus Hartl
* Released under the MIT license
*/
(function (factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as anonymous module.
        define(['jquery'], factory);
    } else {
        // Browser globals.
        factory(jQuery);
    }
} (function ($) {

    var pluses = /\+/g;

    function raw(s) {
        return s;
    }

    function decoded(s) {
        return decodeURIComponent(s.replace(pluses, ' '));
    }

    function converted(s) {
        if (s.indexOf('"') === 0) {
            // This is a quoted cookie as according to RFC2068, unescape
            s = s.slice(1, -1).replace(/\\"/g, '"').replace(/\\\\/g, '\\');
        }
        try {
            return config.json ? JSON.parse(s) : s;
        } catch (er) { }
    }

    var config = $.cookie = function (key, value, options) {

        // write
        if (value !== undefined) {
            options = $.extend({}, config.defaults, options);

            if (typeof options.expires === 'number') {
                var days = options.expires, t = options.expires = new Date();
                t.setDate(t.getDate() + days);
            }

            value = config.json ? JSON.stringify(value) : String(value);

            return (document.cookie = [
				config.raw ? key : encodeURIComponent(key),
				'=',
				config.raw ? value : encodeURIComponent(value),
				options.expires ? '; expires=' + options.expires.toUTCString() : '', // use expires attribute, max-age is not supported by IE
				options.path ? '; path=' + options.path : '',
				options.domain ? '; domain=' + options.domain : '',
				options.secure ? '; secure' : ''
			].join(''));
        }

        // read
        var decode = config.raw ? raw : decoded;
        var cookies = document.cookie.split('; ');
        var result = key ? undefined : {};
        for (var i = 0, l = cookies.length; i < l; i++) {
            var parts = cookies[i].split('=');
            var name = decode(parts.shift());
            var cookie = decode(parts.join('='));

            if (key && key === name) {
                result = converted(cookie);
                break;
            }

            if (!key) {
                result[name] = converted(cookie);
            }
        }

        return result;
    };

    config.defaults = {};

    $.removeCookie = function (key, options) {
        if ($.cookie(key) !== undefined) {
            // Must not alter options, thus extending a fresh object...
            $.cookie(key, '', $.extend({}, options, { expires: -1 }));
            return true;
        }
        return false;
    };

}));
/*jslint browser: true*/
/*jslint nomen: true */
/*global $, jQuery, console*/

(function ($, win) {
    'use strict';

    // site catalyst object, must exist on page before this script is called
    var s = win.s,
        SC;

    /* helper function that clears any previously set values and sets the "s" object's properties
    
    ex.
    var o = { pageName: 'Home' }; // object literal notation, short hand for var o = new Object();
    setObj(o);
    */
    function setObj(o) {
        // white list of approved properties the "s" object cares about, refer to site catalyst documentation and update as needed
        var prop, propertyWhiteList = /pageName|server|channel|pageType|prop[\d]+|campaign|state|zip|events|products|xact|purchaseID|transactionID|eVar[\d]+|hier[\d]+/;

        s.clearVars();

        // set properties
        for (prop in o) {
            if (o.hasOwnProperty(prop)) {
                if (propertyWhiteList.test(prop)) {
                    s[prop] = o[prop];
                }
            }
        }
    }

    function setValues(o, name) {
        //setObj(o);
        if (console && JSON && JSON !== undefined) {
            console.log(name + JSON.stringify(o));
        }
        window.dataLayer = o;
    }

    // SC is global object that will contain tracker and event calls
    SC = win.SC = {
        isAcctSvc: false,

        // increments page view
        _t: function (o) {
            setValues(o, "_t");
            //waits for Ensighten files to finish loading
            Bootstrapper.bindPageSpecificCompletion(function () {
                Bootstrapper.ensEvent.trigger("LSPageLoad");
            });
        },
        // does not increment page view count, used to track custom events
        _tl: function (o) {
            //setObj(o);
            if (console && JSON && JSON !== undefined) {
                console.log('SiteCatalyst._tl: ' + JSON.stringify(o));
            }
            window.dataLayer = o;
            //s.tl(); 
        },
        _sendLoan: function (o) {
            setValues(o, "_sendLoan");
            Bootstrapper.ensEvent.trigger("LSSendLoanInfo");
        },
        _button: function (o) {
            setValues(o, "_button");
            Bootstrapper.ensEvent.trigger("LSButtonClick");
        },
        _video: function (o) {
            setValues(o, "_video");
            Bootstrapper.ensEvent.trigger("LSVideoCall");
        },
        _toolusage: function (o) {
            setValues(o, "_toolusage");
            Bootstrapper.ensEvent.trigger("LSToolsUsage");
        }
    };

    // MAIN HOME PAGE
    SC.main = {
        homePage: function () {
            // simple tracker
            SC._t({
                pageName: 'LScom|Home',
                hier1: 'LScom'
            });
        },

        homeRedesign: function () {
            SC._t({
                pageName: 'LScom|Home',
                hier1: 'LScom|Home'
            });
        },

        autoLanding: function () {
            SC._t({
                pageName: 'LScom|LandingPage|AllAutoLoans|MainPage',
                hier1: 'LScom|LandingPage'
            });
        },

        howItWorks: function () {
            SC._t({
                pageName: 'LScom|Application|HowItWorksInfo',
                hier1: 'LScom|Application'
            });
        },

        plantaTree: function () {
            SC._t({
                pageName: 'LScom|PlantATree|PlantATreeInfo',
                hier1: 'LScom|PlantATree'
            });
        },

        privacyPolicy: function () {
            SC._t({
                pageName: 'LScom|Privacy&Security|PrivacyPolicy',
                hier1: 'LScom|Privacy&Security'
            });
        },

        onlinePrivacyPractices: function () {
            SC._t({
                pageName: 'LScom|Privacy&Security|OnlinePrivacyPractices',
                hier1: 'LScom|Privacy&Security'
            });
        },

        securityPolicy: function () {
            SC._t({
                pageName: 'LScom|Privacy&Security|SecurityPolicy',
                hier1: 'LScom|Privacy&Security'
            });
        },

        aboutUs: function () {
            SC._t({
                pageName: 'LScom|AboutUs|Mission',
                hier1: 'LScom|AboutUs'
            });
        },

        licensing: function () {
            SC._t({
                pageName: 'LScom|AboutUs|Licensing',
                hier1: 'LScom|AboutUs'
            });
        },

        customerGuarantee: function () {
            SC._t({
                pageName: 'LScom|CustomerGuarantee|CustomerGuaranteeInfo',
                hier1: 'LScom|CustomerGuarantee'
            });
        },

        ratingsReviews: function () {
            SC._t({
                pageName: 'LScom|Reviews',
                hier1: 'LScom|Reviews'
            });
        },

        ourTeam: function () {
            SC._t({
                pageName: 'LScom|AboutUs|OurTeam',
                hier1: 'LScom|AboutUs'
            });
        },

        theAnythingLoan: function () {
            SC._t({
                pageName: 'LScom|AboutUs|TheAnythingLoan',
                hier1: 'LScom|AboutUs'
            });
        },

        whoWeAre: function () {
            SC._t({
                pageName: 'LScom|AboutUs|WhoWeAre',
                hier1: 'LScom|AboutUs'
            });
        },

        customerTestimonials: function () {
            SC._t({
                pageName: 'LScom|AboutUs|CustomerTestimonials',
                hier1: 'LScom|AboutUs'
            });
        },

        affiliateProgram: function () {
            SC._t({
                pageName: 'LScom|AboutUs|AffiliateProgram',
                hier1: 'LScom|AboutUs'
            });
        },

        mediaRoom: function () {
            SC._t({
                pageName: 'LScom|AboutUs|MediaRoom',
                hier1: 'LScom|AboutUs|MediaRoom'
            });
        },

        pressReleases: function () {
            SC._t({
                pageName: 'LScom|AboutUs|MediaRoom|PressReleases',
                hier1: 'LScom|AboutUs|MediaRoom'
            });
        },

        pressKit: function () {
            SC._t({
                pageName: 'LScom|AboutUs|MediaRoom|PressKit',
                hier1: 'LScom|AboutUs|MediaRoom'
            });
        },

        businessPartners: function () {
            SC._t({
                pageName: 'LScom|AboutUs|BusinessPartners',
                hier1: 'LScom|AboutUs'
            });
        },

        lowRatePromise: function () {
            SC._t({
                pageName: 'LScom|LowRatePromise|LowRatePromiseInfo',
                hier1: 'LScom|LowRatePromise'
            });
        },

        excellentCredit: function () {
            SC._t({
                pageName: 'LScom|ExcellentCredit',
                hier1: 'LScom|ExcellentCredit'
            });
        },

        goodCredit: function () {
            SC._t({
                pageName: 'LScom|ExcellentCredit',
                hier1: 'LScom|GoodCredit'
            });
        },

        contactUs: function () {
            SC._t({
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|Questions' : 'LScom|Questions|QuestionsForm',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Questions'
            });
        },

        contactUsThankYou: function () {
            SC._t({
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|QuestionsThankYou' : 'LScom|Questions|QuestionsThankYou',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Questions'
            });
        },

        siteMap: function () {
            SC._t({
                pageName: 'LScom|SiteMap|MainPage',
                hier1: 'LScom|SiteMap'
            });
        },

        errorPage: function () {
            SC._t({
                pageName: 'LScom|ErrorPage|MainPage',
                hier1: 'LScom|ErrorPage'
            });
        },

        inTheNews: function () {
            SC._t({
                pageName: 'LScom|AboutUs|MediaRoom|InTheNews',
                hier1: 'LScom|AboutUs|MediaRoom'
            });
        },

        privacySecurity: function () {
            SC._t({
                pageName: 'LScom|Privacy&Security|Privacy&SecurityInfo',
                hier1: 'LScom|Privacy&Security'
            });
        },

        electronicDisclosure: function () {
            SC._t({
                pageName: 'LScom|StatementElectronicRecords|StatementElectronicRecordInfo',
                hier1: 'LScom|StatementElectronicRecords'
            });
        },

        signIn: function () {
            var o = { pageName: 'LScom|Customer|SignIn', hier1: 'LScom|Customer' };
            SC._t(o);
        },

        signedIn: function () {
            SC._t({
                eVar46: 'Lightstream',
                events: 'event2'
            });
        },

        rates: function (eVar, val) {
            var pageVars = {
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|Rates' : 'LScom|Rates&Terms|CurrentRates&Calculator',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Rates&Terms'
            };
            if (typeof eVar === 'string' && /^eVar/.test(eVar)) {
                pageVars[eVar] = val;
            }
            SC._t(pageVars);
        },

        rateMatch: function () {
            SC._t({
                pageName: 'LScom|RateMatch',
                hier1: 'LScom'
            });
        },

        accessibility: function () {
            SC._t({
                pageName: 'LScom|Accessibility ',
                hier1: 'LScom'
            });
        },

        accessibilityContactUs: function () {
            SC._t({
                linkTrackVars: 'eVar11,events',
                linkTrackEvents: 'event38',
                events: 'event38',
                eVar11: 'LScom|ADAContactForm'
            });
        },

        accessibilityContactUsSubmit: function () {
            SC._t({
                linkTrackVars: 'eVar11,events',
                linkTrackEvents: 'event5',
                events: 'event5',
                eVar11: 'LScom|ADAContactForm'
            });
        },

        faq: function () {
            var prefix = 'LScom|FAQ',
                hash = win.location.hash,
                topic = hash ? '|' + hash.split('#')[1] : '';

            SC._t({ pageName: prefix + topic, hier1: prefix });
        },

        vimeoVideo: function (adobeTrackingEvent) {

            /*data that will be sent to adobe*/
            var adobeTrackingData = {};

            adobeTrackingData.events = adobeTrackingEvent.event.name;
            adobeTrackingData[adobeTrackingEvent.property.name] = adobeTrackingEvent.property.value;
            adobeTrackingData[adobeTrackingEvent.variable.name] = adobeTrackingEvent.variable.value;

            if (console && JSON && JSON !== undefined) {
                console.log('SiteCatalyst video: ' + JSON.stringify({
                    event: adobeTrackingEvent,
                    trackingData: adobeTrackingData
                }, null, 4));
            }
            /*send tracking data*/
            SC._video(adobeTrackingData);
        },

        navbar: {
            applyNow: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Nav|ApplyButton',
                    events: 'event23'
                });
            },

            signIn: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Nav|SignInButton',
                    events: 'event23'
                });
            },

            links: function () {
                $(document).ready(function () {
                    var oldNavbarSelector = '.desktop-nav .mainmenu a:not(a[id="SignInNavLink"], a[id="ApplyNavLink"])';
                    var newNavbarSelector = '.desktop-nav .navbar-navigation-header-links a, .desktop-nav .navbar-menu a';

                    $(oldNavbarSelector + ', ' + newNavbarSelector).on('click', function (e) {
                        SC._button({
                            linkTrackVars: 'eVar45,events',
                            linkTrackEvents: 'event23',
                            eVar45: 'LScom|Nav|' + e.currentTarget.text.replace(new RegExp(' ', 'g'), ''),
                            events: 'event23'
                        });
                    });
                });
            }
        },

        jumboTiles: function () {
            SC._button({
                linkTrackVars: 'eVar45,events',
                linkTrackEvents: 'event21',
                eVar45: 'LScom|In-Page|LearnMoreButton',
                events: 'event21'
            });
        },

        banner: {
            applyNow: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Banner|ApplyButton',
                    events: 'event23'
                });
            },

            ratesAndTerms: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Banner|SeeRatesButton',
                    events: 'event23'
                });
            },

            benefits: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Banner|Benefits',
                    events: 'event23'
                });
            },

            lendingUncomplicated: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    events: 'event23',
                    eVar45: 'LScom|In-Page|GetStartedButton'
                });
            },

            notReadyToApply: function () {
                SC._t({
                    linkTrackVars: 'eVar11,events',
                    linkTrackEvents: 'event38',
                    events: 'event38',
                    eVar11: 'LScom|KeepInTouchForm'
                });
            }
        },

        tiles: {
            learnMore: function () {
                $(document).ready(function () {
                    $('.learnMoreButton').on('click', function (e) {
                        SC._button({
                            linkTrackVars: 'eVar45,events',
                            linkTrackEvents: 'event23',
                            eVar45: 'LScom|Home|TilesLearnMore',
                            events: 'event23'
                        });
                    });
                });
            },
            scroll: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Home|TilesViewMore',
                    events: 'event23'
                });
            }
        },

        rateCalculator: {
            rateBeat: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Rates&Terms|RateBeatProgramImage',
                    events: 'event23'
                });
            }
        },

        footer: {
            socialIcons: function (socialChannel) {
                $(document).ready(function () {
                    $('.social a').click(function (e) {
                        var socialChannel = $(e.currentTarget).data('tracking');

                        if (!socialChannel)
                            socialChannel = 'OopsForgotToSpecifyOne';

                        SC._button({
                            linkTrackVars: 'eVar45,events',
                            linkTrackEvents: 'event23',
                            eVar45: 'LScom|Social|' + socialChannel,
                            events: 'event23'
                        });
                    });
                });
            }
        },

        benefitIcons: function () {
            $(document).ready(function () {
                $('.homefeatures a').click(function (e) {
                    SC._toolusage({
                        linkTrackVars: 'eVar4,events',
                        linkTrackEvents: 'event23',
                        eVar4: 'Lightstream Benefits Icons',
                        events: 'event23'
                    });
                });
            });

        },

        progressBar: {
            loanInfo: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Application|ProgressBar|LoanInformation',
                    events: 'event23'
                });
            }, 
            personalInfo: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Application|ProgressBar|PersonalInformation',
                    events: 'event23'
                });
            }, 
            securityInfo: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Application|ProgressBar|SecurityInformation',
                    events: 'event23'
                });
            }, 
            confirmAndSubmit: function () {
                SC._button({
                    linkTrackVars: 'eVar45,events',
                    linkTrackEvents: 'event23',
                    eVar45: 'LScom|Application|ProgressBar|Confirm&Submit',
                    events: 'event23'
                });
            }
        }
    };

    // LOAN APPLICATION
    SC.apply = {
        // cookie names
        _ckUnique: 'SessionApplyCookie',
        _ckBasic: '__a_basic',
        _ckLoanInfo: '__a_loaninfo',
        _ckPersInfo: '__a_persinfo',
        _ckConfirm: '__a_confirm',
        _ckThankYou: '__a_thankyou',
        _ckSendLoan: '__a_perSendLoan',

        basic: function () {
            var o = {
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|Apply' : 'LScom|Application|BasicRequirements',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application'
            };

            SC._t(o);
        },

        basicClick: function () {
            var o = {
                products: ';AnythingLoan;;;;eVar21=InitialProduct',
                eVar27: 'AnythingLoan'
            };

            // set on first page visit
            if (!$.cookie(this._ckBasic)) {
                o.events = 'event6,event25, event96';
                $.cookie(this._ckBasic, true, { expires: 1 });
            }

            SC._t(o);
        },

        loanInfoPageLoad: function () {
            var o = {
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|LoanInfo' : 'LScom|Application|LoanInformation',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application',
                eVar27: 'AnythingLoan',
                events: 'event6, event96',
                products: ';AnythingLoan;;;;eVar21=InitialProduct'
            };
            SC._t(o);
        },

        /* helper function to set "s" object with loan information from cookie or object
    
        */
        setApplyLoanInfo: function (o, loanInfo) {
            $.cookie.json = true; // tells cookie plugin to treat values as valid json, will auto serialize/deserialize

            if (!loanInfo) {
                loanInfo = $.cookie(this._ckLoanInfo) || {}; // if no loan obj was passed in, try the cookie
            }

            var amount = loanInfo.amount || '',
                term = loanInfo.term || '',
                purpose = loanInfo.purpose || '',
                loanType = loanInfo.loanType || '';

            o.eVar30 = term;
            o.products = ';AnythingLoan;;;event18=' + amount + ';eVar30=' + term;
            o.eVar27 = 'AnythingLoan|' + purpose + '|' + loanType;
        },

        sendLoanInfo: function (amount, term, purpose, appType) {
            var o = {
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|LoanInfo' : 'LScom|Application|LoanInformation',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application',
                events: 'event18,event25'
            };
            this.setApplyLoanInfo(o, {
                amount: amount,
                term: term ? term + ' Months' : '',
                purpose: purpose,
                loanType: appType
            });
            //SC._sendLoan(o);
            //SC._tl(o);
        },

        // documentation calls for page tracking as well as loan info values when user submits/clicks next
        loanInfo: function (next, send) {
            $.cookie.json = true; // tells cookie plugin to treat values as valid json, will auto serialize/deserialize
            $.removeCookie(this._ckThankYou);

            var cookieOpts = { expires: 1, path: '/' },
                loanInfo = $.cookie(this._ckLoanInfo) || {},
                amount,
                term,
                o = {
                    pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|LoanInfo' : 'LScom|Application|LoanInformation',
                    hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application'
                };

            // set on first page visit
            if (!loanInfo.visited) {
                o.eVar27 = 'AnythingLoan';
                o.products = ';AnythingLoan;;;;eVar21=InitialProduct';
                o.events = 'event6, event96';
                loanInfo.visited = true;
                $.cookie(this._ckLoanInfo, loanInfo, cookieOpts);
            }

            // capture values so they can be transmitted on next page
            if (next) {
                amount = $('input[name$=uxLoanAmount]').val() || $('#LoanAmount').val();
                loanInfo.amount = amount ? parseFloat(amount.replace(/[\$\,]+/g, '')).toFixed(2) : '';

                term = $('input[name$=uxLoanTerm]').val() || $('#LoanTermMonths').val();
                loanInfo.term = term ? term + ' Months' : '';

                loanInfo.purpose = $('select[name$=uxPurposeOfLoan]').val() || $('#PurposeOfLoan').val();
                loanInfo.loanType = $('select[name$=uxApplicationType]').val() || $('#ApplicationType').val();

                $.cookie(this._ckLoanInfo, loanInfo, cookieOpts);
            } else if (send) { // tracks values stored in cookie
                this.setApplyLoanInfo(o, loanInfo);
                //SC._sendLoan(o);
                //SC._tl(o);
            } else { // basic page tracking

                if ($('.DESVALSummaryHeader').length > 0) {
                    o.prop18 = "Soft|AnyThingLoan|FormInputError";
                }

                SC._t(o);
            }
        },

        personalInfo: function () {
            $.cookie.json = true; // tells cookie plugin to treat values as valid json, will auto serialize/deserialize

            var loanInfo = $.cookie(this._ckLoanInfo) || {};

            if (loanInfo.amount && loanInfo.term && loanInfo.purpose && loanInfo.loanType) {
                var o = {
                    pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|PersonalInfo' : 'LScom|Application|PersonalInformation',
                    hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application',
                    eVar30: loanInfo.term,
                    products: ';AnythingLoan;;;event18=' + loanInfo.amount + ';eVar30=' + loanInfo.term,
                    eVar27: 'AnythingLoan|' + loanInfo.purpose + '|' + loanInfo.loanType

                };
                //only set the event the intial page load 
                if (!$.cookie(this._ckSendLoan)) {
                    o.events = 'event18, event25';
                    $.cookie(this._ckSendLoan, true, { expires: 1 });
                }

            }
            else {
                var o = {
                    pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|PersonalInfo' : 'LScom|Application|PersonalInformation',
                    hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application'
                };
            }

            // set on first page visit
            if (!$.cookie(this._ckPersInfo)) {
                $.cookie(this._ckPersInfo, true, { expires: 1 });
            }
            SC._t(o);
        },

        securityInfo: function () {
            SC._t({
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|SecurityInformation' : 'LScom|Application|SecurityInformation',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application'
            });
        },

        confirm: function () {
            var o = {
                pageName: SC.isAcctSvc ? 'LScom|Customer|AccountServices|Confirm&Submit' : 'LScom|Application|Confirm&Submit',
                hier1: SC.isAcctSvc ? 'LScom|Customer|AccountServices' : 'LScom|Application'
            };

            SC._t(o);
        },

        thankYou: function (sessionApplyCookieValue) {
            var o = {
                pageName: 'LScom|Application|ThankYou',
                hier1: 'LScom|Application',
                products: ';AnythingLoan',
                eVar38: sessionApplyCookieValue || '',
                transactionID: sessionApplyCookieValue || ''
            };

            // set on first page visit
            if (!$.cookie(this._ckThankYou)) {
                o.events = 'event7, event97 ';
                $.cookie(this._ckThankYou, true);
                SC._t(o);
            }

            // clean up cookies
            //$.removeCookie(this._ckUnique);
            $.removeCookie(this._ckBasic);
            $.removeCookie(this._ckLoanInfo);
            $.removeCookie(this._ckPersInfo);
            $.removeCookie(this._ckConfirm);
            $.removeCookie(this._ckSendLoan);
            //$.removeCookie(this._ckThankYou);
        }
    };

    // APPLICATION STATUS / PREFUNDING
    SC.appStatus = {
        counter: function () {
            /* NOTE: this is incomplete, refer to SEO build out doc
            "s.products="";AnythingLoan;;;event18=####;eVar30=see values above""
            s.eVar27=""see list below""
            s.eVar30=""see list below"""
            */

            SC._t({
                pageName: 'LScom|Customer|CounterStatus|ReviewCounterOffer',
                hier1: 'LScom|Customer|CounterStatus',
                events: 'event18,event48'
            });
        },

        counterV: function () {
            SC._t({ pageName: 'LScom|Customer|CounterStatus|CounterTermsVer', hier1: 'LScom|Customer|CounterStatus' });
        },

        changeLoanTerms: function () {
            SC._t({ pageName: 'LScom|Customer|PreFunding|ChangeLoanTerms', hier1: 'LScom|Customer|PreFunding' });
        },

        changeLoanTermOffer: function () {
            SC._t({ pageName: 'LScom|Customer|PreFunding|ChangeLoanTermOffer', hier1: 'LScom|Customer|PreFunding' });
        },

        cancelled: function () {
            SC._t({ pageName: 'LScom|Customer|CancelledApp|ClientRequest', hier1: 'LScom|Customer|CancelledApp', prop3: 'LightStream|CancelledApp' });
        },

        dupeAppCancelled: function () {
            SC._t({ pageName: 'LScom|Customer|CancelledApp|DuplicateApp', hier1: 'LScom|Customer|CancelledApp' });
        },

        declineDecision: function () {
            SC._t({ pageName: 'LScom|Customer|Decline|LoanAppDecision', hier1: 'LScom|Customer|Decline' });
        },

        declineNotice: function () {
            SC._t({ pageName: 'LScom|Customer|AccountServices|DeclineNotice', hier1: 'LScom|Customer|AccountServices' });
        },

        appsNoLongerAvail: function () {
            SC._t({ pageName: 'LScom|Customer|ErrorPage|AppsNoLongerAvail', hier1: 'LScom|Customer|ErrorPage', prop18: 'Soft|AnyThingLoan|AppNoLongerAvailable' });
        },

        fundingError: function () {
            SC._t({ pageName: 'LScom|Customer|ErrorPage|FundingError', hier1: 'LScom|Customer|ErrorPage', prop18: 'Soft|AnyThingLoan|FundingError' });
        },

        accessUnavailable: function () {
            SC._t({ pageName: 'LScom|Customer|ErrorPage|AccessUnavailable', hier1: 'LScom|Customer|ErrorPage', prop18: 'Soft|AnyThingLoan|AccessUnavailable' });
        },

        reviewCounterOffer: function (offered_amount, offered_term, offered_purpose, offered_type) {
            var amount = offered_amount ? parseFloat(offered_amount.replace(/[\$\,]+/g, '')).toFixed(2) : '',
                term = offered_term ? offered_term + ' Months' : '';

            SC._t({
                pageName: 'LScom|Customer|PreFunding|ReviewLoanTerms',
                hier1: 'LScom|Customer|PreFunding',
                products: ';AnythingLoan;;;event18=' + amount + ';eVar30=' + term,
                eVar27: 'AnythingLoan|' + offered_purpose + '|' + offered_type,
                eVar30: term,
                events: 'event18,event48'
            });
        },
    };

    // PREFUNDING 
    SC.prefunding = {
        signLoanAgreement: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|SignLoanAgreement',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        viewLoanAgreement: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ViewLoanAgreement',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        fundingInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|FundingInfo',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        rescheduleFundingDate: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|RescheduleFundingDate',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        changeLoanTermOffer: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ChangeLoanTermOffer',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        approvedNewLoanTerms: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ApprovedNewLoanTerms',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        requestedLoanTerms: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|RequestedLoanTerms',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        cancelNewLoanTermReq: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|CancelNewLoanTermReq',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        changeMonthlyPmtDate: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ChangeMonthlyPmtDate',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        changeCheckingActInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ChangeCheckingActInfo',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        cancelPendingLoan: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|CancelPendingLoan',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        nLTRPending: function () {
            // obsolete - use pending(currentStatus) instead
            SC._t({
                pageName: 'LScom|Customer|PreFunding|NLTRPending',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        verification: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|Verification',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        loanAppInProcess: function () {
            // obsolete - use pending(currentStatus) instead
            SC._t({
                pageName: 'LScom|Customer|PreFunding|LoanAppInProcess',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        loanAppPending: function () {
            // obsolete - use pending(currentStatus) instead
            SC._t({
                pageName: 'LScom|Customer|PreFunding|LoanAppPending',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        pending: function (currentStatus) {
            if (currentStatus === 'ApprovedNTLR') {
                SC._t({
                    pageName: 'LScom|Customer|PreFunding|NLTRPending',
                    hier1: 'LScom|Customer|PreFunding'
                });
            } else if (currentStatus === 'PendingQ') {
                SC._t({
                    pageName: 'LScom|Customer|PreFunding|LoanAppPending',
                    hier1: 'LScom|Customer|PreFunding'
                });
            } else {
                // else - pending or in process
                SC._t({
                    pageName: 'LScom|Customer|PreFunding|LoanAppInProcess',
                    hier1: 'LScom|Customer|PreFunding'
                });
            }
        },

        emailAddressInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|EmailAddressInfo',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        loanAppInfoRequest: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|LoanAppInfoRequest',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        uploadDocs: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|UploadDocs',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        faxCoverSheet: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|FaxCoverSheet',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        accountSetup: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|AccountSetup',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        changeLoanTermConfirm: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ChangeLoanTermConfirm',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        changeLoanTermThankYou: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ChangeLoanTermThankYou',
                hier1: 'LScom|Customer|PreFunding',
                prop3: 'LightStream|LoanTermChangeRequested'
            });
        },

        newLoanAgreement: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|NewLoanAgreement',
                hier1: 'LScom|Customer|PreFunding',
                prop3: 'LightStream|LoanTermChangeApproved'
            });
        },

        newLoanReqComp: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|NewLoanReqComp',
                hier1: 'LScom|Customer|PreFunding'
            });
        },

        reviewLoanTerms: function (input_amount, input_term, input_purpose, input_type) {
            var amount = input_amount ? parseFloat(input_amount.toString().replace(/[\$\,]+/g, '')).toFixed(2) : '',
                term = input_term ? input_term + ' Months' : '';

            SC._t({
                pageName: 'LScom|Customer|PreFunding|ReviewLoanTerms',
                hier1: 'LScom|Customer|PreFunding',
                products: ';AnythingLoan;;;event18=' + amount + ';eVar30=' + term,
                eVar27: 'AnythingLoan|' + input_purpose + '|' + input_type,
                eVar30: term,
                events: 'event18,event48'
            });
        },

        scheduledFundingInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|PreFunding|ScheduledFundingInfo',
                hier1: 'LScom|Customer|PreFunding',
                products: ';AnythingLoan',
                eVar9: 'LightStream|AnythingLoan',
                events: 'event8'
            });
        }
    };

    // ACCOUNT SERVICES
    SC.acctServices = {
        welcome: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Welcome',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        summary: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Summary',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        accounts: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Accounts',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        autoPayInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|AutoPayInfo',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        autoPayChangeAcctInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|AutoPayChangeAcctInfo',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        autoPayChangePmntAmt: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|AutoPayChangePmntAmt',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        contactUsAccountServices: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Questions',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        extraPmntAndPayoffs: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ExtraPmnt&Payoffs',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        extraPmntSchedule: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ExtraPmntSchedule',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        extraPmntScheduled: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ExtraPmntScheduled',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        extraPmntScheduledCancel: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ExtraPmntScheduledCancel',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        extraPmntCancel: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ExtraPmntCancel',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        payoffSchedule: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PayoffSchedule',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        payoffMailIn: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PayoffMailIn',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        payoffMailInInvoice: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PayoffMailInInvoice',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        nickNameAcct: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|NicknameAcct',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        documents: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Documents',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        enotices: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Enotices',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        declineNotice: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|DeclineNotice',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefHome: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|Profile',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefContactInfo: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefContactInfo',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefAcctLockEnabled: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefAcctLockEnabled',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefAcctLockDisabled: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefAcctLockDisabled',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefChangeUserID: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefChangeUserID',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefUserIDOption: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefUserIDOption',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefChangeUserIDConfirm: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefChangeUserIDConfirm',
                hier1: 'LScom|Customer|AccountServices',
                prop3: 'LightStream|ChangeUserID'
            });
        },

        prefChangePassword: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefChangePassword',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefChangePassConfirm: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefChangePassConfirm',
                hier1: 'LScom|Customer|AccountServices',
                prop3: 'LightStream|ChangePass'
            });
        },

        prefChangeSecurityAnswer: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefChangeSecurityAnswer',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        prefChangeSecurityAnsConfirm: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefChangeSecurityAnsConfirm',
                hier1: 'LScom|Customer|AccountServices',
                prop3: 'LightStream|ChangeSecurityAns'
            });
        },

        prefeChoices: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefeChoices',
                hier1: 'LScom|Customer|AccountServices',
                prop3: 'LightStream|ChangeSecurityAns'
            });
        },

        prefeChoicesConfirm: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|PrefeChoicesConfirm',
                hier1: 'LScom|Customer|AccountServices',
                prop3: 'LightStream|ChangePrefChoice'
            });
        },

        privacySecurity: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices',
                prop11: 'Customer',
                prop12: 'AccountServices',
                prop13: 'PrivacySecurity'
            });
        },

        rates: function (eVar, val) {
            var pageVars = {
                pageName: 'LScom|Customer|AccountServices|Rates',
                hier1: 'LScom|Customer|AccountServices'
            };
            if (typeof eVar === 'string' && /^eVar/.test(eVar)) {
                pageVars[eVar] = val;
            }
            SC._t(pageVars);
        },

        applicationReceived: function () {
            var sessionId = $.cookie(this._ckUnique);
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ApplicationReceived',
                hier1: 'LScom|Customer|AccountServices',
                products: ';AnythingLoan',
                transactionID: sessionId,
                eVar38: sessionId,
                events: 'event7, event97 '
            });
        },

        history: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|History',
                hier1: 'LScom|Customer|AccountServices'
            });
        },

        viewLoanAgreement: function () {
            SC._t({
                pageName: 'LScom|Customer|AccountServices|ViewLoanAgreement',
                hier1: 'LScom|Customer|AccountServices'
            });
        },
    };

    // NEEDS INITIALIZATION
    SC.main.navbar.links();
    SC.main.tiles.learnMore();
    SC.main.footer.socialIcons();
    SC.main.benefitIcons();
}(jQuery, window));

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
//# sourceMappingURL=data:application/json;charset=utf8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIlNlYXJjaFRlcm1zLmpzIiwianF1ZXJ5LmNvb2tpZS5qcyIsImNvZGVfdG9fcGFzdGUuanMiLCJtYXJrZXRpbmcuanMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FDdkJBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUM5RkE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUNsMUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBIiwiZmlsZSI6InNjcmlwdHMtbWFya2V0aW5nLmpzIiwic291cmNlc0NvbnRlbnQiOlsiLypqc2xpbnQgYnJvd3NlcjogdHJ1ZSovXHJcbi8qanNsaW50IHBsdXNwbHVzOiB0cnVlICovXHJcbi8qanNsaW50IHJlZ2V4cDogdHJ1ZSAqL1xyXG4vKmdsb2JhbCB1bmVzY2FwZSwgJCAqLyAvLyB0aGlzIGlzIHdpZGVseSBzdXBwb3J0ZWRcclxuJChmdW5jdGlvbiAoKSB7XHJcbiAgICAndXNlIHN0cmljdCc7XHJcbiAgICB2YXIgaSwgcXMsIHFzYSwgcXNpcCwgd29yZHN0cmluZywgcmVmID0gZG9jdW1lbnQucmVmZXJyZXI7XHJcbiAgICBpZiAocmVmLmluZGV4T2YoJz8nKSA9PT0gLTEpIHtcclxuICAgICAgICByZXR1cm47XHJcbiAgICB9XHJcbiAgICBxcyA9IHJlZi5zdWJzdHIocmVmLmluZGV4T2YoJz8nKSArIDEpO1xyXG4gICAgcXNhID0gcXMuc3BsaXQoJyYnKTtcclxuICAgIGZvciAoaSA9IDA7IGkgPCBxc2EubGVuZ3RoOyBpKyspIHtcclxuICAgICAgICBxc2lwID0gcXNhW2ldLnNwbGl0KCc9Jyk7XHJcbiAgICAgICAgaWYgKHFzaXAubGVuZ3RoID4gMSkge1xyXG4gICAgICAgICAgICBpZiAocXNpcFswXSA9PT0gJ3EnIHx8IHFzaXBbMF0gPT09ICdwJykgeyAvLyBxPSBmb3IgR29vZ2xlIC8gQmluZywgcD0gZm9yIFlhaG9vXHJcbiAgICAgICAgICAgICAgICB3b3Jkc3RyaW5nID0gZGVjb2RlVVJJKHFzaXBbMV0ucmVwbGFjZSgvXFwrL2csICcgJykpO1xyXG4gICAgICAgICAgICAgICAgJC5jb29raWUoJ1NlYXJjaFRlcm1zJywgd29yZHN0cmluZyk7XHJcbiAgICAgICAgICAgICAgICAkLmNvb2tpZSgnU2VhcmNoRW5naW5lJywgcmVmLm1hdGNoKC86XFwvXFwvKC5bXlxcL10rKS8pWzFdKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgIH1cclxufSk7XHJcbi8vIyBzb3VyY2VNYXBwaW5nVVJMPVNlYXJjaFRlcm1zLmpzLm1hcCIsIi8qIVxyXG4qIGpRdWVyeSBDb29raWUgUGx1Z2luIHYxLjMuMVxyXG4qIGh0dHBzOi8vZ2l0aHViLmNvbS9jYXJoYXJ0bC9qcXVlcnktY29va2llXHJcbipcclxuKiBDb3B5cmlnaHQgMjAxMyBLbGF1cyBIYXJ0bFxyXG4qIFJlbGVhc2VkIHVuZGVyIHRoZSBNSVQgbGljZW5zZVxyXG4qL1xyXG4oZnVuY3Rpb24gKGZhY3RvcnkpIHtcclxuICAgIGlmICh0eXBlb2YgZGVmaW5lID09PSAnZnVuY3Rpb24nICYmIGRlZmluZS5hbWQpIHtcclxuICAgICAgICAvLyBBTUQuIFJlZ2lzdGVyIGFzIGFub255bW91cyBtb2R1bGUuXHJcbiAgICAgICAgZGVmaW5lKFsnanF1ZXJ5J10sIGZhY3RvcnkpO1xyXG4gICAgfSBlbHNlIHtcclxuICAgICAgICAvLyBCcm93c2VyIGdsb2JhbHMuXHJcbiAgICAgICAgZmFjdG9yeShqUXVlcnkpO1xyXG4gICAgfVxyXG59IChmdW5jdGlvbiAoJCkge1xyXG5cclxuICAgIHZhciBwbHVzZXMgPSAvXFwrL2c7XHJcblxyXG4gICAgZnVuY3Rpb24gcmF3KHMpIHtcclxuICAgICAgICByZXR1cm4gcztcclxuICAgIH1cclxuXHJcbiAgICBmdW5jdGlvbiBkZWNvZGVkKHMpIHtcclxuICAgICAgICByZXR1cm4gZGVjb2RlVVJJQ29tcG9uZW50KHMucmVwbGFjZShwbHVzZXMsICcgJykpO1xyXG4gICAgfVxyXG5cclxuICAgIGZ1bmN0aW9uIGNvbnZlcnRlZChzKSB7XHJcbiAgICAgICAgaWYgKHMuaW5kZXhPZignXCInKSA9PT0gMCkge1xyXG4gICAgICAgICAgICAvLyBUaGlzIGlzIGEgcXVvdGVkIGNvb2tpZSBhcyBhY2NvcmRpbmcgdG8gUkZDMjA2OCwgdW5lc2NhcGVcclxuICAgICAgICAgICAgcyA9IHMuc2xpY2UoMSwgLTEpLnJlcGxhY2UoL1xcXFxcIi9nLCAnXCInKS5yZXBsYWNlKC9cXFxcXFxcXC9nLCAnXFxcXCcpO1xyXG4gICAgICAgIH1cclxuICAgICAgICB0cnkge1xyXG4gICAgICAgICAgICByZXR1cm4gY29uZmlnLmpzb24gPyBKU09OLnBhcnNlKHMpIDogcztcclxuICAgICAgICB9IGNhdGNoIChlcikgeyB9XHJcbiAgICB9XHJcblxyXG4gICAgdmFyIGNvbmZpZyA9ICQuY29va2llID0gZnVuY3Rpb24gKGtleSwgdmFsdWUsIG9wdGlvbnMpIHtcclxuXHJcbiAgICAgICAgLy8gd3JpdGVcclxuICAgICAgICBpZiAodmFsdWUgIT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICBvcHRpb25zID0gJC5leHRlbmQoe30sIGNvbmZpZy5kZWZhdWx0cywgb3B0aW9ucyk7XHJcblxyXG4gICAgICAgICAgICBpZiAodHlwZW9mIG9wdGlvbnMuZXhwaXJlcyA9PT0gJ251bWJlcicpIHtcclxuICAgICAgICAgICAgICAgIHZhciBkYXlzID0gb3B0aW9ucy5leHBpcmVzLCB0ID0gb3B0aW9ucy5leHBpcmVzID0gbmV3IERhdGUoKTtcclxuICAgICAgICAgICAgICAgIHQuc2V0RGF0ZSh0LmdldERhdGUoKSArIGRheXMpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICB2YWx1ZSA9IGNvbmZpZy5qc29uID8gSlNPTi5zdHJpbmdpZnkodmFsdWUpIDogU3RyaW5nKHZhbHVlKTtcclxuXHJcbiAgICAgICAgICAgIHJldHVybiAoZG9jdW1lbnQuY29va2llID0gW1xyXG5cdFx0XHRcdGNvbmZpZy5yYXcgPyBrZXkgOiBlbmNvZGVVUklDb21wb25lbnQoa2V5KSxcclxuXHRcdFx0XHQnPScsXHJcblx0XHRcdFx0Y29uZmlnLnJhdyA/IHZhbHVlIDogZW5jb2RlVVJJQ29tcG9uZW50KHZhbHVlKSxcclxuXHRcdFx0XHRvcHRpb25zLmV4cGlyZXMgPyAnOyBleHBpcmVzPScgKyBvcHRpb25zLmV4cGlyZXMudG9VVENTdHJpbmcoKSA6ICcnLCAvLyB1c2UgZXhwaXJlcyBhdHRyaWJ1dGUsIG1heC1hZ2UgaXMgbm90IHN1cHBvcnRlZCBieSBJRVxyXG5cdFx0XHRcdG9wdGlvbnMucGF0aCA/ICc7IHBhdGg9JyArIG9wdGlvbnMucGF0aCA6ICcnLFxyXG5cdFx0XHRcdG9wdGlvbnMuZG9tYWluID8gJzsgZG9tYWluPScgKyBvcHRpb25zLmRvbWFpbiA6ICcnLFxyXG5cdFx0XHRcdG9wdGlvbnMuc2VjdXJlID8gJzsgc2VjdXJlJyA6ICcnXHJcblx0XHRcdF0uam9pbignJykpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgLy8gcmVhZFxyXG4gICAgICAgIHZhciBkZWNvZGUgPSBjb25maWcucmF3ID8gcmF3IDogZGVjb2RlZDtcclxuICAgICAgICB2YXIgY29va2llcyA9IGRvY3VtZW50LmNvb2tpZS5zcGxpdCgnOyAnKTtcclxuICAgICAgICB2YXIgcmVzdWx0ID0ga2V5ID8gdW5kZWZpbmVkIDoge307XHJcbiAgICAgICAgZm9yICh2YXIgaSA9IDAsIGwgPSBjb29raWVzLmxlbmd0aDsgaSA8IGw7IGkrKykge1xyXG4gICAgICAgICAgICB2YXIgcGFydHMgPSBjb29raWVzW2ldLnNwbGl0KCc9Jyk7XHJcbiAgICAgICAgICAgIHZhciBuYW1lID0gZGVjb2RlKHBhcnRzLnNoaWZ0KCkpO1xyXG4gICAgICAgICAgICB2YXIgY29va2llID0gZGVjb2RlKHBhcnRzLmpvaW4oJz0nKSk7XHJcblxyXG4gICAgICAgICAgICBpZiAoa2V5ICYmIGtleSA9PT0gbmFtZSkge1xyXG4gICAgICAgICAgICAgICAgcmVzdWx0ID0gY29udmVydGVkKGNvb2tpZSk7XHJcbiAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgaWYgKCFrZXkpIHtcclxuICAgICAgICAgICAgICAgIHJlc3VsdFtuYW1lXSA9IGNvbnZlcnRlZChjb29raWUpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICByZXR1cm4gcmVzdWx0O1xyXG4gICAgfTtcclxuXHJcbiAgICBjb25maWcuZGVmYXVsdHMgPSB7fTtcclxuXHJcbiAgICAkLnJlbW92ZUNvb2tpZSA9IGZ1bmN0aW9uIChrZXksIG9wdGlvbnMpIHtcclxuICAgICAgICBpZiAoJC5jb29raWUoa2V5KSAhPT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgIC8vIE11c3Qgbm90IGFsdGVyIG9wdGlvbnMsIHRodXMgZXh0ZW5kaW5nIGEgZnJlc2ggb2JqZWN0Li4uXHJcbiAgICAgICAgICAgICQuY29va2llKGtleSwgJycsICQuZXh0ZW5kKHt9LCBvcHRpb25zLCB7IGV4cGlyZXM6IC0xIH0pKTtcclxuICAgICAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIHJldHVybiBmYWxzZTtcclxuICAgIH07XHJcblxyXG59KSk7IiwiLypqc2xpbnQgYnJvd3NlcjogdHJ1ZSovXHJcbi8qanNsaW50IG5vbWVuOiB0cnVlICovXHJcbi8qZ2xvYmFsICQsIGpRdWVyeSwgY29uc29sZSovXHJcblxyXG4oZnVuY3Rpb24gKCQsIHdpbikge1xyXG4gICAgJ3VzZSBzdHJpY3QnO1xyXG5cclxuICAgIC8vIHNpdGUgY2F0YWx5c3Qgb2JqZWN0LCBtdXN0IGV4aXN0IG9uIHBhZ2UgYmVmb3JlIHRoaXMgc2NyaXB0IGlzIGNhbGxlZFxyXG4gICAgdmFyIHMgPSB3aW4ucyxcclxuICAgICAgICBTQztcclxuXHJcbiAgICAvKiBoZWxwZXIgZnVuY3Rpb24gdGhhdCBjbGVhcnMgYW55IHByZXZpb3VzbHkgc2V0IHZhbHVlcyBhbmQgc2V0cyB0aGUgXCJzXCIgb2JqZWN0J3MgcHJvcGVydGllc1xyXG4gICAgXHJcbiAgICBleC5cclxuICAgIHZhciBvID0geyBwYWdlTmFtZTogJ0hvbWUnIH07IC8vIG9iamVjdCBsaXRlcmFsIG5vdGF0aW9uLCBzaG9ydCBoYW5kIGZvciB2YXIgbyA9IG5ldyBPYmplY3QoKTtcclxuICAgIHNldE9iaihvKTtcclxuICAgICovXHJcbiAgICBmdW5jdGlvbiBzZXRPYmoobykge1xyXG4gICAgICAgIC8vIHdoaXRlIGxpc3Qgb2YgYXBwcm92ZWQgcHJvcGVydGllcyB0aGUgXCJzXCIgb2JqZWN0IGNhcmVzIGFib3V0LCByZWZlciB0byBzaXRlIGNhdGFseXN0IGRvY3VtZW50YXRpb24gYW5kIHVwZGF0ZSBhcyBuZWVkZWRcclxuICAgICAgICB2YXIgcHJvcCwgcHJvcGVydHlXaGl0ZUxpc3QgPSAvcGFnZU5hbWV8c2VydmVyfGNoYW5uZWx8cGFnZVR5cGV8cHJvcFtcXGRdK3xjYW1wYWlnbnxzdGF0ZXx6aXB8ZXZlbnRzfHByb2R1Y3RzfHhhY3R8cHVyY2hhc2VJRHx0cmFuc2FjdGlvbklEfGVWYXJbXFxkXSt8aGllcltcXGRdKy87XHJcblxyXG4gICAgICAgIHMuY2xlYXJWYXJzKCk7XHJcblxyXG4gICAgICAgIC8vIHNldCBwcm9wZXJ0aWVzXHJcbiAgICAgICAgZm9yIChwcm9wIGluIG8pIHtcclxuICAgICAgICAgICAgaWYgKG8uaGFzT3duUHJvcGVydHkocHJvcCkpIHtcclxuICAgICAgICAgICAgICAgIGlmIChwcm9wZXJ0eVdoaXRlTGlzdC50ZXN0KHByb3ApKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgc1twcm9wXSA9IG9bcHJvcF07XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgZnVuY3Rpb24gc2V0VmFsdWVzKG8sIG5hbWUpIHtcclxuICAgICAgICAvL3NldE9iaihvKTtcclxuICAgICAgICBpZiAoY29uc29sZSAmJiBKU09OICYmIEpTT04gIT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICBjb25zb2xlLmxvZyhuYW1lICsgSlNPTi5zdHJpbmdpZnkobykpO1xyXG4gICAgICAgIH1cclxuICAgICAgICB3aW5kb3cuZGF0YUxheWVyID0gbztcclxuICAgIH1cclxuXHJcbiAgICAvLyBTQyBpcyBnbG9iYWwgb2JqZWN0IHRoYXQgd2lsbCBjb250YWluIHRyYWNrZXIgYW5kIGV2ZW50IGNhbGxzXHJcbiAgICBTQyA9IHdpbi5TQyA9IHtcclxuICAgICAgICBpc0FjY3RTdmM6IGZhbHNlLFxyXG5cclxuICAgICAgICAvLyBpbmNyZW1lbnRzIHBhZ2Ugdmlld1xyXG4gICAgICAgIF90OiBmdW5jdGlvbiAobykge1xyXG4gICAgICAgICAgICBzZXRWYWx1ZXMobywgXCJfdFwiKTtcclxuICAgICAgICAgICAgLy93YWl0cyBmb3IgRW5zaWdodGVuIGZpbGVzIHRvIGZpbmlzaCBsb2FkaW5nXHJcbiAgICAgICAgICAgIEJvb3RzdHJhcHBlci5iaW5kUGFnZVNwZWNpZmljQ29tcGxldGlvbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBCb290c3RyYXBwZXIuZW5zRXZlbnQudHJpZ2dlcihcIkxTUGFnZUxvYWRcIik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgLy8gZG9lcyBub3QgaW5jcmVtZW50IHBhZ2UgdmlldyBjb3VudCwgdXNlZCB0byB0cmFjayBjdXN0b20gZXZlbnRzXHJcbiAgICAgICAgX3RsOiBmdW5jdGlvbiAobykge1xyXG4gICAgICAgICAgICAvL3NldE9iaihvKTtcclxuICAgICAgICAgICAgaWYgKGNvbnNvbGUgJiYgSlNPTiAmJiBKU09OICE9PSB1bmRlZmluZWQpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUubG9nKCdTaXRlQ2F0YWx5c3QuX3RsOiAnICsgSlNPTi5zdHJpbmdpZnkobykpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHdpbmRvdy5kYXRhTGF5ZXIgPSBvO1xyXG4gICAgICAgICAgICAvL3MudGwoKTsgXHJcbiAgICAgICAgfSxcclxuICAgICAgICBfc2VuZExvYW46IGZ1bmN0aW9uIChvKSB7XHJcbiAgICAgICAgICAgIHNldFZhbHVlcyhvLCBcIl9zZW5kTG9hblwiKTtcclxuICAgICAgICAgICAgQm9vdHN0cmFwcGVyLmVuc0V2ZW50LnRyaWdnZXIoXCJMU1NlbmRMb2FuSW5mb1wiKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF9idXR0b246IGZ1bmN0aW9uIChvKSB7XHJcbiAgICAgICAgICAgIHNldFZhbHVlcyhvLCBcIl9idXR0b25cIik7XHJcbiAgICAgICAgICAgIEJvb3RzdHJhcHBlci5lbnNFdmVudC50cmlnZ2VyKFwiTFNCdXR0b25DbGlja1wiKTtcclxuICAgICAgICB9LFxyXG4gICAgICAgIF92aWRlbzogZnVuY3Rpb24gKG8pIHtcclxuICAgICAgICAgICAgc2V0VmFsdWVzKG8sIFwiX3ZpZGVvXCIpO1xyXG4gICAgICAgICAgICBCb290c3RyYXBwZXIuZW5zRXZlbnQudHJpZ2dlcihcIkxTVmlkZW9DYWxsXCIpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgX3Rvb2x1c2FnZTogZnVuY3Rpb24gKG8pIHtcclxuICAgICAgICAgICAgc2V0VmFsdWVzKG8sIFwiX3Rvb2x1c2FnZVwiKTtcclxuICAgICAgICAgICAgQm9vdHN0cmFwcGVyLmVuc0V2ZW50LnRyaWdnZXIoXCJMU1Rvb2xzVXNhZ2VcIik7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbiAgICAvLyBNQUlOIEhPTUUgUEFHRVxyXG4gICAgU0MubWFpbiA9IHtcclxuICAgICAgICBob21lUGFnZTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAvLyBzaW1wbGUgdHJhY2tlclxyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEhvbWUnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgaG9tZVJlZGVzaWduOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218SG9tZScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEhvbWUnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGF1dG9MYW5kaW5nOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218TGFuZGluZ1BhZ2V8QWxsQXV0b0xvYW5zfE1haW5QYWdlJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218TGFuZGluZ1BhZ2UnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGhvd0l0V29ya3M6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxBcHBsaWNhdGlvbnxIb3dJdFdvcmtzSW5mbycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEFwcGxpY2F0aW9uJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwbGFudGFUcmVlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218UGxhbnRBVHJlZXxQbGFudEFUcmVlSW5mbycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfFBsYW50QVRyZWUnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHByaXZhY3lQb2xpY3k6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxQcml2YWN5JlNlY3VyaXR5fFByaXZhY3lQb2xpY3knLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxQcml2YWN5JlNlY3VyaXR5J1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBvbmxpbmVQcml2YWN5UHJhY3RpY2VzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218UHJpdmFjeSZTZWN1cml0eXxPbmxpbmVQcml2YWN5UHJhY3RpY2VzJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218UHJpdmFjeSZTZWN1cml0eSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgc2VjdXJpdHlQb2xpY3k6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxQcml2YWN5JlNlY3VyaXR5fFNlY3VyaXR5UG9saWN5JyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218UHJpdmFjeSZTZWN1cml0eSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYWJvdXRVczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8TWlzc2lvbicsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEFib3V0VXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGxpY2Vuc2luZzogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8TGljZW5zaW5nJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218QWJvdXRVcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY3VzdG9tZXJHdWFyYW50ZWU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lckd1YXJhbnRlZXxDdXN0b21lckd1YXJhbnRlZUluZm8nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lckd1YXJhbnRlZSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcmF0aW5nc1Jldmlld3M6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxSZXZpZXdzJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218UmV2aWV3cydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgb3VyVGVhbTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8T3VyVGVhbScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEFib3V0VXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHRoZUFueXRoaW5nTG9hbjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8VGhlQW55dGhpbmdMb2FuJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218QWJvdXRVcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgd2hvV2VBcmU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxBYm91dFVzfFdob1dlQXJlJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218QWJvdXRVcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY3VzdG9tZXJUZXN0aW1vbmlhbHM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxBYm91dFVzfEN1c3RvbWVyVGVzdGltb25pYWxzJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218QWJvdXRVcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYWZmaWxpYXRlUHJvZ3JhbTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8QWZmaWxpYXRlUHJvZ3JhbScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEFib3V0VXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIG1lZGlhUm9vbTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8TWVkaWFSb29tJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218QWJvdXRVc3xNZWRpYVJvb20nXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHByZXNzUmVsZWFzZXM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxBYm91dFVzfE1lZGlhUm9vbXxQcmVzc1JlbGVhc2VzJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218QWJvdXRVc3xNZWRpYVJvb20nXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHByZXNzS2l0OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218QWJvdXRVc3xNZWRpYVJvb218UHJlc3NLaXQnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxBYm91dFVzfE1lZGlhUm9vbSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYnVzaW5lc3NQYXJ0bmVyczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8QnVzaW5lc3NQYXJ0bmVycycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEFib3V0VXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGxvd1JhdGVQcm9taXNlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218TG93UmF0ZVByb21pc2V8TG93UmF0ZVByb21pc2VJbmZvJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218TG93UmF0ZVByb21pc2UnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGV4Y2VsbGVudENyZWRpdDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEV4Y2VsbGVudENyZWRpdCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEV4Y2VsbGVudENyZWRpdCdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZ29vZENyZWRpdDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEV4Y2VsbGVudENyZWRpdCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEdvb2RDcmVkaXQnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGNvbnRhY3RVczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xRdWVzdGlvbnMnIDogJ0xTY29tfFF1ZXN0aW9uc3xRdWVzdGlvbnNGb3JtJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiBTQy5pc0FjY3RTdmMgPyAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyA6ICdMU2NvbXxRdWVzdGlvbnMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGNvbnRhY3RVc1RoYW5rWW91OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiBTQy5pc0FjY3RTdmMgPyAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFF1ZXN0aW9uc1RoYW5rWW91JyA6ICdMU2NvbXxRdWVzdGlvbnN8UXVlc3Rpb25zVGhhbmtZb3UnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnIDogJ0xTY29tfFF1ZXN0aW9ucydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgc2l0ZU1hcDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfFNpdGVNYXB8TWFpblBhZ2UnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxTaXRlTWFwJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBlcnJvclBhZ2U6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxFcnJvclBhZ2V8TWFpblBhZ2UnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxFcnJvclBhZ2UnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGluVGhlTmV3czogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEFib3V0VXN8TWVkaWFSb29tfEluVGhlTmV3cycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEFib3V0VXN8TWVkaWFSb29tJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwcml2YWN5U2VjdXJpdHk6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxQcml2YWN5JlNlY3VyaXR5fFByaXZhY3kmU2VjdXJpdHlJbmZvJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218UHJpdmFjeSZTZWN1cml0eSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZWxlY3Ryb25pY0Rpc2Nsb3N1cmU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxTdGF0ZW1lbnRFbGVjdHJvbmljUmVjb3Jkc3xTdGF0ZW1lbnRFbGVjdHJvbmljUmVjb3JkSW5mbycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfFN0YXRlbWVudEVsZWN0cm9uaWNSZWNvcmRzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBzaWduSW46IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIG8gPSB7IHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8U2lnbkluJywgaGllcjE6ICdMU2NvbXxDdXN0b21lcicgfTtcclxuICAgICAgICAgICAgU0MuX3Qobyk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgc2lnbmVkSW46IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgZVZhcjQ2OiAnTGlnaHRzdHJlYW0nLFxyXG4gICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQyJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICByYXRlczogZnVuY3Rpb24gKGVWYXIsIHZhbCkge1xyXG4gICAgICAgICAgICB2YXIgcGFnZVZhcnMgPSB7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xSYXRlcycgOiAnTFNjb218UmF0ZXMmVGVybXN8Q3VycmVudFJhdGVzJkNhbGN1bGF0b3InLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnIDogJ0xTY29tfFJhdGVzJlRlcm1zJ1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICBpZiAodHlwZW9mIGVWYXIgPT09ICdzdHJpbmcnICYmIC9eZVZhci8udGVzdChlVmFyKSkge1xyXG4gICAgICAgICAgICAgICAgcGFnZVZhcnNbZVZhcl0gPSB2YWw7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgU0MuX3QocGFnZVZhcnMpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHJhdGVNYXRjaDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfFJhdGVNYXRjaCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBhY2Nlc3NpYmlsaXR5OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218QWNjZXNzaWJpbGl0eSAnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYWNjZXNzaWJpbGl0eUNvbnRhY3RVczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBsaW5rVHJhY2tWYXJzOiAnZVZhcjExLGV2ZW50cycsXHJcbiAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDM4JyxcclxuICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MzgnLFxyXG4gICAgICAgICAgICAgICAgZVZhcjExOiAnTFNjb218QURBQ29udGFjdEZvcm0nXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGFjY2Vzc2liaWxpdHlDb250YWN0VXNTdWJtaXQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXIxMSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgbGlua1RyYWNrRXZlbnRzOiAnZXZlbnQ1JyxcclxuICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50NScsXHJcbiAgICAgICAgICAgICAgICBlVmFyMTE6ICdMU2NvbXxBREFDb250YWN0Rm9ybSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZmFxOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciBwcmVmaXggPSAnTFNjb218RkFRJyxcclxuICAgICAgICAgICAgICAgIGhhc2ggPSB3aW4ubG9jYXRpb24uaGFzaCxcclxuICAgICAgICAgICAgICAgIHRvcGljID0gaGFzaCA/ICd8JyArIGhhc2guc3BsaXQoJyMnKVsxXSA6ICcnO1xyXG5cclxuICAgICAgICAgICAgU0MuX3QoeyBwYWdlTmFtZTogcHJlZml4ICsgdG9waWMsIGhpZXIxOiBwcmVmaXggfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgdmltZW9WaWRlbzogZnVuY3Rpb24gKGFkb2JlVHJhY2tpbmdFdmVudCkge1xyXG5cclxuICAgICAgICAgICAgLypkYXRhIHRoYXQgd2lsbCBiZSBzZW50IHRvIGFkb2JlKi9cclxuICAgICAgICAgICAgdmFyIGFkb2JlVHJhY2tpbmdEYXRhID0ge307XHJcblxyXG4gICAgICAgICAgICBhZG9iZVRyYWNraW5nRGF0YS5ldmVudHMgPSBhZG9iZVRyYWNraW5nRXZlbnQuZXZlbnQubmFtZTtcclxuICAgICAgICAgICAgYWRvYmVUcmFja2luZ0RhdGFbYWRvYmVUcmFja2luZ0V2ZW50LnByb3BlcnR5Lm5hbWVdID0gYWRvYmVUcmFja2luZ0V2ZW50LnByb3BlcnR5LnZhbHVlO1xyXG4gICAgICAgICAgICBhZG9iZVRyYWNraW5nRGF0YVthZG9iZVRyYWNraW5nRXZlbnQudmFyaWFibGUubmFtZV0gPSBhZG9iZVRyYWNraW5nRXZlbnQudmFyaWFibGUudmFsdWU7XHJcblxyXG4gICAgICAgICAgICBpZiAoY29uc29sZSAmJiBKU09OICYmIEpTT04gIT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZS5sb2coJ1NpdGVDYXRhbHlzdCB2aWRlbzogJyArIEpTT04uc3RyaW5naWZ5KHtcclxuICAgICAgICAgICAgICAgICAgICBldmVudDogYWRvYmVUcmFja2luZ0V2ZW50LFxyXG4gICAgICAgICAgICAgICAgICAgIHRyYWNraW5nRGF0YTogYWRvYmVUcmFja2luZ0RhdGFcclxuICAgICAgICAgICAgICAgIH0sIG51bGwsIDQpKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAvKnNlbmQgdHJhY2tpbmcgZGF0YSovXHJcbiAgICAgICAgICAgIFNDLl92aWRlbyhhZG9iZVRyYWNraW5nRGF0YSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgbmF2YmFyOiB7XHJcbiAgICAgICAgICAgIGFwcGx5Tm93OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBTQy5fYnV0dG9uKHtcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tWYXJzOiAnZVZhcjQ1LGV2ZW50cycsXHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrRXZlbnRzOiAnZXZlbnQyMycsXHJcbiAgICAgICAgICAgICAgICAgICAgZVZhcjQ1OiAnTFNjb218TmF2fEFwcGx5QnV0dG9uJyxcclxuICAgICAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDIzJ1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH0sXHJcblxyXG4gICAgICAgICAgICBzaWduSW46IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja1ZhcnM6ICdlVmFyNDUsZXZlbnRzJyxcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxOYXZ8U2lnbkluQnV0dG9uJyxcclxuICAgICAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDIzJ1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH0sXHJcblxyXG4gICAgICAgICAgICBsaW5rczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgJChkb2N1bWVudCkucmVhZHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBvbGROYXZiYXJTZWxlY3RvciA9ICcuZGVza3RvcC1uYXYgLm1haW5tZW51IGE6bm90KGFbaWQ9XCJTaWduSW5OYXZMaW5rXCJdLCBhW2lkPVwiQXBwbHlOYXZMaW5rXCJdKSc7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG5ld05hdmJhclNlbGVjdG9yID0gJy5kZXNrdG9wLW5hdiAubmF2YmFyLW5hdmlnYXRpb24taGVhZGVyLWxpbmtzIGEsIC5kZXNrdG9wLW5hdiAubmF2YmFyLW1lbnUgYSc7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICQob2xkTmF2YmFyU2VsZWN0b3IgKyAnLCAnICsgbmV3TmF2YmFyU2VsZWN0b3IpLm9uKCdjbGljaycsIGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrRXZlbnRzOiAnZXZlbnQyMycsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxOYXZ8JyArIGUuY3VycmVudFRhcmdldC50ZXh0LnJlcGxhY2UobmV3IFJlZ0V4cCgnICcsICdnJyksICcnKSxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBqdW1ib1RpbGVzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgbGlua1RyYWNrRXZlbnRzOiAnZXZlbnQyMScsXHJcbiAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxJbi1QYWdlfExlYXJuTW9yZUJ1dHRvbicsXHJcbiAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDIxJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBiYW5uZXI6IHtcclxuICAgICAgICAgICAgYXBwbHlOb3c6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja1ZhcnM6ICdlVmFyNDUsZXZlbnRzJyxcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxCYW5uZXJ8QXBwbHlCdXR0b24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfSxcclxuXHJcbiAgICAgICAgICAgIHJhdGVzQW5kVGVybXM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja1ZhcnM6ICdlVmFyNDUsZXZlbnRzJyxcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxCYW5uZXJ8U2VlUmF0ZXNCdXR0b24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfSxcclxuXHJcbiAgICAgICAgICAgIGJlbmVmaXRzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBTQy5fYnV0dG9uKHtcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tWYXJzOiAnZVZhcjQ1LGV2ZW50cycsXHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrRXZlbnRzOiAnZXZlbnQyMycsXHJcbiAgICAgICAgICAgICAgICAgICAgZVZhcjQ1OiAnTFNjb218QmFubmVyfEJlbmVmaXRzJyxcclxuICAgICAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDIzJ1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH0sXHJcblxyXG4gICAgICAgICAgICBsZW5kaW5nVW5jb21wbGljYXRlZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgU0MuX2J1dHRvbih7XHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja0V2ZW50czogJ2V2ZW50MjMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGVWYXI0NTogJ0xTY29tfEluLVBhZ2V8R2V0U3RhcnRlZEJ1dHRvbidcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9LFxyXG5cclxuICAgICAgICAgICAgbm90UmVhZHlUb0FwcGx5OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXIxMSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja0V2ZW50czogJ2V2ZW50MzgnLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MzgnLFxyXG4gICAgICAgICAgICAgICAgICAgIGVWYXIxMTogJ0xTY29tfEtlZXBJblRvdWNoRm9ybSdcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgdGlsZXM6IHtcclxuICAgICAgICAgICAgbGVhcm5Nb3JlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAkKGRvY3VtZW50KS5yZWFkeShmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgJCgnLmxlYXJuTW9yZUJ1dHRvbicpLm9uKCdjbGljaycsIGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrRXZlbnRzOiAnZXZlbnQyMycsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxIb21lfFRpbGVzTGVhcm5Nb3JlJyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIHNjcm9sbDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgU0MuX2J1dHRvbih7XHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja0V2ZW50czogJ2V2ZW50MjMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGVWYXI0NTogJ0xTY29tfEhvbWV8VGlsZXNWaWV3TW9yZScsXHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQyMydcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcmF0ZUNhbGN1bGF0b3I6IHtcclxuICAgICAgICAgICAgcmF0ZUJlYXQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja1ZhcnM6ICdlVmFyNDUsZXZlbnRzJyxcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxSYXRlcyZUZXJtc3xSYXRlQmVhdFByb2dyYW1JbWFnZScsXHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQyMydcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZm9vdGVyOiB7XHJcbiAgICAgICAgICAgIHNvY2lhbEljb25zOiBmdW5jdGlvbiAoc29jaWFsQ2hhbm5lbCkge1xyXG4gICAgICAgICAgICAgICAgJChkb2N1bWVudCkucmVhZHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgICQoJy5zb2NpYWwgYScpLmNsaWNrKGZ1bmN0aW9uIChlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHZhciBzb2NpYWxDaGFubmVsID0gJChlLmN1cnJlbnRUYXJnZXQpLmRhdGEoJ3RyYWNraW5nJyk7XHJcblxyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAoIXNvY2lhbENoYW5uZWwpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzb2NpYWxDaGFubmVsID0gJ09vcHNGb3Jnb3RUb1NwZWNpZnlPbmUnO1xyXG5cclxuICAgICAgICAgICAgICAgICAgICAgICAgU0MuX2J1dHRvbih7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tWYXJzOiAnZVZhcjQ1LGV2ZW50cycsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGVWYXI0NTogJ0xTY29tfFNvY2lhbHwnICsgc29jaWFsQ2hhbm5lbCxcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBiZW5lZml0SWNvbnM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgJChkb2N1bWVudCkucmVhZHkoZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgJCgnLmhvbWVmZWF0dXJlcyBhJykuY2xpY2soZnVuY3Rpb24gKGUpIHtcclxuICAgICAgICAgICAgICAgICAgICBTQy5fdG9vbHVzYWdlKHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0LGV2ZW50cycsXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja0V2ZW50czogJ2V2ZW50MjMnLFxyXG4gICAgICAgICAgICAgICAgICAgICAgICBlVmFyNDogJ0xpZ2h0c3RyZWFtIEJlbmVmaXRzIEljb25zJyxcclxuICAgICAgICAgICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQyMydcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9KTtcclxuXHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJvZ3Jlc3NCYXI6IHtcclxuICAgICAgICAgICAgbG9hbkluZm86IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja1ZhcnM6ICdlVmFyNDUsZXZlbnRzJyxcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxBcHBsaWNhdGlvbnxQcm9ncmVzc0JhcnxMb2FuSW5mb3JtYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfSwgXHJcbiAgICAgICAgICAgIHBlcnNvbmFsSW5mbzogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgU0MuX2J1dHRvbih7XHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja0V2ZW50czogJ2V2ZW50MjMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGVWYXI0NTogJ0xTY29tfEFwcGxpY2F0aW9ufFByb2dyZXNzQmFyfFBlcnNvbmFsSW5mb3JtYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfSwgXHJcbiAgICAgICAgICAgIHNlY3VyaXR5SW5mbzogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgU0MuX2J1dHRvbih7XHJcbiAgICAgICAgICAgICAgICAgICAgbGlua1RyYWNrVmFyczogJ2VWYXI0NSxldmVudHMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja0V2ZW50czogJ2V2ZW50MjMnLFxyXG4gICAgICAgICAgICAgICAgICAgIGVWYXI0NTogJ0xTY29tfEFwcGxpY2F0aW9ufFByb2dyZXNzQmFyfFNlY3VyaXR5SW5mb3JtYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50MjMnXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfSwgXHJcbiAgICAgICAgICAgIGNvbmZpcm1BbmRTdWJtaXQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgIFNDLl9idXR0b24oe1xyXG4gICAgICAgICAgICAgICAgICAgIGxpbmtUcmFja1ZhcnM6ICdlVmFyNDUsZXZlbnRzJyxcclxuICAgICAgICAgICAgICAgICAgICBsaW5rVHJhY2tFdmVudHM6ICdldmVudDIzJyxcclxuICAgICAgICAgICAgICAgICAgICBlVmFyNDU6ICdMU2NvbXxBcHBsaWNhdGlvbnxQcm9ncmVzc0JhcnxDb25maXJtJlN1Ym1pdCcsXHJcbiAgICAgICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQyMydcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbiAgICAvLyBMT0FOIEFQUExJQ0FUSU9OXHJcbiAgICBTQy5hcHBseSA9IHtcclxuICAgICAgICAvLyBjb29raWUgbmFtZXNcclxuICAgICAgICBfY2tVbmlxdWU6ICdTZXNzaW9uQXBwbHlDb29raWUnLFxyXG4gICAgICAgIF9ja0Jhc2ljOiAnX19hX2Jhc2ljJyxcclxuICAgICAgICBfY2tMb2FuSW5mbzogJ19fYV9sb2FuaW5mbycsXHJcbiAgICAgICAgX2NrUGVyc0luZm86ICdfX2FfcGVyc2luZm8nLFxyXG4gICAgICAgIF9ja0NvbmZpcm06ICdfX2FfY29uZmlybScsXHJcbiAgICAgICAgX2NrVGhhbmtZb3U6ICdfX2FfdGhhbmt5b3UnLFxyXG4gICAgICAgIF9ja1NlbmRMb2FuOiAnX19hX3BlclNlbmRMb2FuJyxcclxuXHJcbiAgICAgICAgYmFzaWM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIG8gPSB7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xBcHBseScgOiAnTFNjb218QXBwbGljYXRpb258QmFzaWNSZXF1aXJlbWVudHMnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnIDogJ0xTY29tfEFwcGxpY2F0aW9uJ1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgU0MuX3Qobyk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYmFzaWNDbGljazogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICB2YXIgbyA9IHtcclxuICAgICAgICAgICAgICAgIHByb2R1Y3RzOiAnO0FueXRoaW5nTG9hbjs7OztlVmFyMjE9SW5pdGlhbFByb2R1Y3QnLFxyXG4gICAgICAgICAgICAgICAgZVZhcjI3OiAnQW55dGhpbmdMb2FuJ1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgLy8gc2V0IG9uIGZpcnN0IHBhZ2UgdmlzaXRcclxuICAgICAgICAgICAgaWYgKCEkLmNvb2tpZSh0aGlzLl9ja0Jhc2ljKSkge1xyXG4gICAgICAgICAgICAgICAgby5ldmVudHMgPSAnZXZlbnQ2LGV2ZW50MjUsIGV2ZW50OTYnO1xyXG4gICAgICAgICAgICAgICAgJC5jb29raWUodGhpcy5fY2tCYXNpYywgdHJ1ZSwgeyBleHBpcmVzOiAxIH0pO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICBTQy5fdChvKTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBsb2FuSW5mb1BhZ2VMb2FkOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIHZhciBvID0ge1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8TG9hbkluZm8nIDogJ0xTY29tfEFwcGxpY2F0aW9ufExvYW5JbmZvcm1hdGlvbicsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcycgOiAnTFNjb218QXBwbGljYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgZVZhcjI3OiAnQW55dGhpbmdMb2FuJyxcclxuICAgICAgICAgICAgICAgIGV2ZW50czogJ2V2ZW50NiwgZXZlbnQ5NicsXHJcbiAgICAgICAgICAgICAgICBwcm9kdWN0czogJztBbnl0aGluZ0xvYW47Ozs7ZVZhcjIxPUluaXRpYWxQcm9kdWN0J1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICBTQy5fdChvKTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICAvKiBoZWxwZXIgZnVuY3Rpb24gdG8gc2V0IFwic1wiIG9iamVjdCB3aXRoIGxvYW4gaW5mb3JtYXRpb24gZnJvbSBjb29raWUgb3Igb2JqZWN0XHJcbiAgICBcclxuICAgICAgICAqL1xyXG4gICAgICAgIHNldEFwcGx5TG9hbkluZm86IGZ1bmN0aW9uIChvLCBsb2FuSW5mbykge1xyXG4gICAgICAgICAgICAkLmNvb2tpZS5qc29uID0gdHJ1ZTsgLy8gdGVsbHMgY29va2llIHBsdWdpbiB0byB0cmVhdCB2YWx1ZXMgYXMgdmFsaWQganNvbiwgd2lsbCBhdXRvIHNlcmlhbGl6ZS9kZXNlcmlhbGl6ZVxyXG5cclxuICAgICAgICAgICAgaWYgKCFsb2FuSW5mbykge1xyXG4gICAgICAgICAgICAgICAgbG9hbkluZm8gPSAkLmNvb2tpZSh0aGlzLl9ja0xvYW5JbmZvKSB8fCB7fTsgLy8gaWYgbm8gbG9hbiBvYmogd2FzIHBhc3NlZCBpbiwgdHJ5IHRoZSBjb29raWVcclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgdmFyIGFtb3VudCA9IGxvYW5JbmZvLmFtb3VudCB8fCAnJyxcclxuICAgICAgICAgICAgICAgIHRlcm0gPSBsb2FuSW5mby50ZXJtIHx8ICcnLFxyXG4gICAgICAgICAgICAgICAgcHVycG9zZSA9IGxvYW5JbmZvLnB1cnBvc2UgfHwgJycsXHJcbiAgICAgICAgICAgICAgICBsb2FuVHlwZSA9IGxvYW5JbmZvLmxvYW5UeXBlIHx8ICcnO1xyXG5cclxuICAgICAgICAgICAgby5lVmFyMzAgPSB0ZXJtO1xyXG4gICAgICAgICAgICBvLnByb2R1Y3RzID0gJztBbnl0aGluZ0xvYW47OztldmVudDE4PScgKyBhbW91bnQgKyAnO2VWYXIzMD0nICsgdGVybTtcclxuICAgICAgICAgICAgby5lVmFyMjcgPSAnQW55dGhpbmdMb2FufCcgKyBwdXJwb3NlICsgJ3wnICsgbG9hblR5cGU7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgc2VuZExvYW5JbmZvOiBmdW5jdGlvbiAoYW1vdW50LCB0ZXJtLCBwdXJwb3NlLCBhcHBUeXBlKSB7XHJcbiAgICAgICAgICAgIHZhciBvID0ge1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8TG9hbkluZm8nIDogJ0xTY29tfEFwcGxpY2F0aW9ufExvYW5JbmZvcm1hdGlvbicsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcycgOiAnTFNjb218QXBwbGljYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQxOCxldmVudDI1J1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICB0aGlzLnNldEFwcGx5TG9hbkluZm8obywge1xyXG4gICAgICAgICAgICAgICAgYW1vdW50OiBhbW91bnQsXHJcbiAgICAgICAgICAgICAgICB0ZXJtOiB0ZXJtID8gdGVybSArICcgTW9udGhzJyA6ICcnLFxyXG4gICAgICAgICAgICAgICAgcHVycG9zZTogcHVycG9zZSxcclxuICAgICAgICAgICAgICAgIGxvYW5UeXBlOiBhcHBUeXBlXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAvL1NDLl9zZW5kTG9hbihvKTtcclxuICAgICAgICAgICAgLy9TQy5fdGwobyk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgLy8gZG9jdW1lbnRhdGlvbiBjYWxscyBmb3IgcGFnZSB0cmFja2luZyBhcyB3ZWxsIGFzIGxvYW4gaW5mbyB2YWx1ZXMgd2hlbiB1c2VyIHN1Ym1pdHMvY2xpY2tzIG5leHRcclxuICAgICAgICBsb2FuSW5mbzogZnVuY3Rpb24gKG5leHQsIHNlbmQpIHtcclxuICAgICAgICAgICAgJC5jb29raWUuanNvbiA9IHRydWU7IC8vIHRlbGxzIGNvb2tpZSBwbHVnaW4gdG8gdHJlYXQgdmFsdWVzIGFzIHZhbGlkIGpzb24sIHdpbGwgYXV0byBzZXJpYWxpemUvZGVzZXJpYWxpemVcclxuICAgICAgICAgICAgJC5yZW1vdmVDb29raWUodGhpcy5fY2tUaGFua1lvdSk7XHJcblxyXG4gICAgICAgICAgICB2YXIgY29va2llT3B0cyA9IHsgZXhwaXJlczogMSwgcGF0aDogJy8nIH0sXHJcbiAgICAgICAgICAgICAgICBsb2FuSW5mbyA9ICQuY29va2llKHRoaXMuX2NrTG9hbkluZm8pIHx8IHt9LFxyXG4gICAgICAgICAgICAgICAgYW1vdW50LFxyXG4gICAgICAgICAgICAgICAgdGVybSxcclxuICAgICAgICAgICAgICAgIG8gPSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcGFnZU5hbWU6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8TG9hbkluZm8nIDogJ0xTY29tfEFwcGxpY2F0aW9ufExvYW5JbmZvcm1hdGlvbicsXHJcbiAgICAgICAgICAgICAgICAgICAgaGllcjE6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnIDogJ0xTY29tfEFwcGxpY2F0aW9uJ1xyXG4gICAgICAgICAgICAgICAgfTtcclxuXHJcbiAgICAgICAgICAgIC8vIHNldCBvbiBmaXJzdCBwYWdlIHZpc2l0XHJcbiAgICAgICAgICAgIGlmICghbG9hbkluZm8udmlzaXRlZCkge1xyXG4gICAgICAgICAgICAgICAgby5lVmFyMjcgPSAnQW55dGhpbmdMb2FuJztcclxuICAgICAgICAgICAgICAgIG8ucHJvZHVjdHMgPSAnO0FueXRoaW5nTG9hbjs7OztlVmFyMjE9SW5pdGlhbFByb2R1Y3QnO1xyXG4gICAgICAgICAgICAgICAgby5ldmVudHMgPSAnZXZlbnQ2LCBldmVudDk2JztcclxuICAgICAgICAgICAgICAgIGxvYW5JbmZvLnZpc2l0ZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgJC5jb29raWUodGhpcy5fY2tMb2FuSW5mbywgbG9hbkluZm8sIGNvb2tpZU9wdHMpO1xyXG4gICAgICAgICAgICB9XHJcblxyXG4gICAgICAgICAgICAvLyBjYXB0dXJlIHZhbHVlcyBzbyB0aGV5IGNhbiBiZSB0cmFuc21pdHRlZCBvbiBuZXh0IHBhZ2VcclxuICAgICAgICAgICAgaWYgKG5leHQpIHtcclxuICAgICAgICAgICAgICAgIGFtb3VudCA9ICQoJ2lucHV0W25hbWUkPXV4TG9hbkFtb3VudF0nKS52YWwoKSB8fCAkKCcjTG9hbkFtb3VudCcpLnZhbCgpO1xyXG4gICAgICAgICAgICAgICAgbG9hbkluZm8uYW1vdW50ID0gYW1vdW50ID8gcGFyc2VGbG9hdChhbW91bnQucmVwbGFjZSgvW1xcJFxcLF0rL2csICcnKSkudG9GaXhlZCgyKSA6ICcnO1xyXG5cclxuICAgICAgICAgICAgICAgIHRlcm0gPSAkKCdpbnB1dFtuYW1lJD11eExvYW5UZXJtXScpLnZhbCgpIHx8ICQoJyNMb2FuVGVybU1vbnRocycpLnZhbCgpO1xyXG4gICAgICAgICAgICAgICAgbG9hbkluZm8udGVybSA9IHRlcm0gPyB0ZXJtICsgJyBNb250aHMnIDogJyc7XHJcblxyXG4gICAgICAgICAgICAgICAgbG9hbkluZm8ucHVycG9zZSA9ICQoJ3NlbGVjdFtuYW1lJD11eFB1cnBvc2VPZkxvYW5dJykudmFsKCkgfHwgJCgnI1B1cnBvc2VPZkxvYW4nKS52YWwoKTtcclxuICAgICAgICAgICAgICAgIGxvYW5JbmZvLmxvYW5UeXBlID0gJCgnc2VsZWN0W25hbWUkPXV4QXBwbGljYXRpb25UeXBlXScpLnZhbCgpIHx8ICQoJyNBcHBsaWNhdGlvblR5cGUnKS52YWwoKTtcclxuXHJcbiAgICAgICAgICAgICAgICAkLmNvb2tpZSh0aGlzLl9ja0xvYW5JbmZvLCBsb2FuSW5mbywgY29va2llT3B0cyk7XHJcbiAgICAgICAgICAgIH0gZWxzZSBpZiAoc2VuZCkgeyAvLyB0cmFja3MgdmFsdWVzIHN0b3JlZCBpbiBjb29raWVcclxuICAgICAgICAgICAgICAgIHRoaXMuc2V0QXBwbHlMb2FuSW5mbyhvLCBsb2FuSW5mbyk7XHJcbiAgICAgICAgICAgICAgICAvL1NDLl9zZW5kTG9hbihvKTtcclxuICAgICAgICAgICAgICAgIC8vU0MuX3RsKG8pO1xyXG4gICAgICAgICAgICB9IGVsc2UgeyAvLyBiYXNpYyBwYWdlIHRyYWNraW5nXHJcblxyXG4gICAgICAgICAgICAgICAgaWYgKCQoJy5ERVNWQUxTdW1tYXJ5SGVhZGVyJykubGVuZ3RoID4gMCkge1xyXG4gICAgICAgICAgICAgICAgICAgIG8ucHJvcDE4ID0gXCJTb2Z0fEFueVRoaW5nTG9hbnxGb3JtSW5wdXRFcnJvclwiO1xyXG4gICAgICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgICAgIFNDLl90KG8pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcGVyc29uYWxJbmZvOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICQuY29va2llLmpzb24gPSB0cnVlOyAvLyB0ZWxscyBjb29raWUgcGx1Z2luIHRvIHRyZWF0IHZhbHVlcyBhcyB2YWxpZCBqc29uLCB3aWxsIGF1dG8gc2VyaWFsaXplL2Rlc2VyaWFsaXplXHJcblxyXG4gICAgICAgICAgICB2YXIgbG9hbkluZm8gPSAkLmNvb2tpZSh0aGlzLl9ja0xvYW5JbmZvKSB8fCB7fTtcclxuXHJcbiAgICAgICAgICAgIGlmIChsb2FuSW5mby5hbW91bnQgJiYgbG9hbkluZm8udGVybSAmJiBsb2FuSW5mby5wdXJwb3NlICYmIGxvYW5JbmZvLmxvYW5UeXBlKSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgbyA9IHtcclxuICAgICAgICAgICAgICAgICAgICBwYWdlTmFtZTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQZXJzb25hbEluZm8nIDogJ0xTY29tfEFwcGxpY2F0aW9ufFBlcnNvbmFsSW5mb3JtYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGhpZXIxOiBTQy5pc0FjY3RTdmMgPyAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyA6ICdMU2NvbXxBcHBsaWNhdGlvbicsXHJcbiAgICAgICAgICAgICAgICAgICAgZVZhcjMwOiBsb2FuSW5mby50ZXJtLFxyXG4gICAgICAgICAgICAgICAgICAgIHByb2R1Y3RzOiAnO0FueXRoaW5nTG9hbjs7O2V2ZW50MTg9JyArIGxvYW5JbmZvLmFtb3VudCArICc7ZVZhcjMwPScgKyBsb2FuSW5mby50ZXJtLFxyXG4gICAgICAgICAgICAgICAgICAgIGVWYXIyNzogJ0FueXRoaW5nTG9hbnwnICsgbG9hbkluZm8ucHVycG9zZSArICd8JyArIGxvYW5JbmZvLmxvYW5UeXBlXHJcblxyXG4gICAgICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgICAgIC8vb25seSBzZXQgdGhlIGV2ZW50IHRoZSBpbnRpYWwgcGFnZSBsb2FkIFxyXG4gICAgICAgICAgICAgICAgaWYgKCEkLmNvb2tpZSh0aGlzLl9ja1NlbmRMb2FuKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIG8uZXZlbnRzID0gJ2V2ZW50MTgsIGV2ZW50MjUnO1xyXG4gICAgICAgICAgICAgICAgICAgICQuY29va2llKHRoaXMuX2NrU2VuZExvYW4sIHRydWUsIHsgZXhwaXJlczogMSB9KTtcclxuICAgICAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB2YXIgbyA9IHtcclxuICAgICAgICAgICAgICAgICAgICBwYWdlTmFtZTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQZXJzb25hbEluZm8nIDogJ0xTY29tfEFwcGxpY2F0aW9ufFBlcnNvbmFsSW5mb3JtYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgICAgIGhpZXIxOiBTQy5pc0FjY3RTdmMgPyAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyA6ICdMU2NvbXxBcHBsaWNhdGlvbidcclxuICAgICAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIC8vIHNldCBvbiBmaXJzdCBwYWdlIHZpc2l0XHJcbiAgICAgICAgICAgIGlmICghJC5jb29raWUodGhpcy5fY2tQZXJzSW5mbykpIHtcclxuICAgICAgICAgICAgICAgICQuY29va2llKHRoaXMuX2NrUGVyc0luZm8sIHRydWUsIHsgZXhwaXJlczogMSB9KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBTQy5fdChvKTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBzZWN1cml0eUluZm86IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8U2VjdXJpdHlJbmZvcm1hdGlvbicgOiAnTFNjb218QXBwbGljYXRpb258U2VjdXJpdHlJbmZvcm1hdGlvbicsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcycgOiAnTFNjb218QXBwbGljYXRpb24nXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGNvbmZpcm06IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIG8gPSB7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogU0MuaXNBY2N0U3ZjID8gJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xDb25maXJtJlN1Ym1pdCcgOiAnTFNjb218QXBwbGljYXRpb258Q29uZmlybSZTdWJtaXQnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6IFNDLmlzQWNjdFN2YyA/ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnIDogJ0xTY29tfEFwcGxpY2F0aW9uJ1xyXG4gICAgICAgICAgICB9O1xyXG5cclxuICAgICAgICAgICAgU0MuX3Qobyk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgdGhhbmtZb3U6IGZ1bmN0aW9uIChzZXNzaW9uQXBwbHlDb29raWVWYWx1ZSkge1xyXG4gICAgICAgICAgICB2YXIgbyA9IHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218QXBwbGljYXRpb258VGhhbmtZb3UnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxBcHBsaWNhdGlvbicsXHJcbiAgICAgICAgICAgICAgICBwcm9kdWN0czogJztBbnl0aGluZ0xvYW4nLFxyXG4gICAgICAgICAgICAgICAgZVZhcjM4OiBzZXNzaW9uQXBwbHlDb29raWVWYWx1ZSB8fCAnJyxcclxuICAgICAgICAgICAgICAgIHRyYW5zYWN0aW9uSUQ6IHNlc3Npb25BcHBseUNvb2tpZVZhbHVlIHx8ICcnXHJcbiAgICAgICAgICAgIH07XHJcblxyXG4gICAgICAgICAgICAvLyBzZXQgb24gZmlyc3QgcGFnZSB2aXNpdFxyXG4gICAgICAgICAgICBpZiAoISQuY29va2llKHRoaXMuX2NrVGhhbmtZb3UpKSB7XHJcbiAgICAgICAgICAgICAgICBvLmV2ZW50cyA9ICdldmVudDcsIGV2ZW50OTcgJztcclxuICAgICAgICAgICAgICAgICQuY29va2llKHRoaXMuX2NrVGhhbmtZb3UsIHRydWUpO1xyXG4gICAgICAgICAgICAgICAgU0MuX3Qobyk7XHJcbiAgICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAgIC8vIGNsZWFuIHVwIGNvb2tpZXNcclxuICAgICAgICAgICAgLy8kLnJlbW92ZUNvb2tpZSh0aGlzLl9ja1VuaXF1ZSk7XHJcbiAgICAgICAgICAgICQucmVtb3ZlQ29va2llKHRoaXMuX2NrQmFzaWMpO1xyXG4gICAgICAgICAgICAkLnJlbW92ZUNvb2tpZSh0aGlzLl9ja0xvYW5JbmZvKTtcclxuICAgICAgICAgICAgJC5yZW1vdmVDb29raWUodGhpcy5fY2tQZXJzSW5mbyk7XHJcbiAgICAgICAgICAgICQucmVtb3ZlQ29va2llKHRoaXMuX2NrQ29uZmlybSk7XHJcbiAgICAgICAgICAgICQucmVtb3ZlQ29va2llKHRoaXMuX2NrU2VuZExvYW4pO1xyXG4gICAgICAgICAgICAvLyQucmVtb3ZlQ29va2llKHRoaXMuX2NrVGhhbmtZb3UpO1xyXG4gICAgICAgIH1cclxuICAgIH07XHJcblxyXG4gICAgLy8gQVBQTElDQVRJT04gU1RBVFVTIC8gUFJFRlVORElOR1xyXG4gICAgU0MuYXBwU3RhdHVzID0ge1xyXG4gICAgICAgIGNvdW50ZXI6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgLyogTk9URTogdGhpcyBpcyBpbmNvbXBsZXRlLCByZWZlciB0byBTRU8gYnVpbGQgb3V0IGRvY1xyXG4gICAgICAgICAgICBcInMucHJvZHVjdHM9XCJcIjtBbnl0aGluZ0xvYW47OztldmVudDE4PSMjIyM7ZVZhcjMwPXNlZSB2YWx1ZXMgYWJvdmVcIlwiXHJcbiAgICAgICAgICAgIHMuZVZhcjI3PVwiXCJzZWUgbGlzdCBiZWxvd1wiXCJcclxuICAgICAgICAgICAgcy5lVmFyMzA9XCJcInNlZSBsaXN0IGJlbG93XCJcIlwiXHJcbiAgICAgICAgICAgICovXHJcblxyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfENvdW50ZXJTdGF0dXN8UmV2aWV3Q291bnRlck9mZmVyJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8Q291bnRlclN0YXR1cycsXHJcbiAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDE4LGV2ZW50NDgnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGNvdW50ZXJWOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHsgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxDb3VudGVyU3RhdHVzfENvdW50ZXJUZXJtc1ZlcicsIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8Q291bnRlclN0YXR1cycgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY2hhbmdlTG9hblRlcm1zOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHsgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfENoYW5nZUxvYW5UZXJtcycsIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZycgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY2hhbmdlTG9hblRlcm1PZmZlcjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7IHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xDaGFuZ2VMb2FuVGVybU9mZmVyJywgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJyB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBjYW5jZWxsZWQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3QoeyBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfENhbmNlbGxlZEFwcHxDbGllbnRSZXF1ZXN0JywgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxDYW5jZWxsZWRBcHAnLCBwcm9wMzogJ0xpZ2h0U3RyZWFtfENhbmNlbGxlZEFwcCcgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZHVwZUFwcENhbmNlbGxlZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7IHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8Q2FuY2VsbGVkQXBwfER1cGxpY2F0ZUFwcCcsIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8Q2FuY2VsbGVkQXBwJyB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBkZWNsaW5lRGVjaXNpb246IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3QoeyBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfERlY2xpbmV8TG9hbkFwcERlY2lzaW9uJywgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxEZWNsaW5lJyB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBkZWNsaW5lTm90aWNlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHsgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8RGVjbGluZU5vdGljZScsIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBhcHBzTm9Mb25nZXJBdmFpbDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7IHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8RXJyb3JQYWdlfEFwcHNOb0xvbmdlckF2YWlsJywgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxFcnJvclBhZ2UnLCBwcm9wMTg6ICdTb2Z0fEFueVRoaW5nTG9hbnxBcHBOb0xvbmdlckF2YWlsYWJsZScgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZnVuZGluZ0Vycm9yOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHsgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxFcnJvclBhZ2V8RnVuZGluZ0Vycm9yJywgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxFcnJvclBhZ2UnLCBwcm9wMTg6ICdTb2Z0fEFueVRoaW5nTG9hbnxGdW5kaW5nRXJyb3InIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGFjY2Vzc1VuYXZhaWxhYmxlOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHsgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxFcnJvclBhZ2V8QWNjZXNzVW5hdmFpbGFibGUnLCBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEVycm9yUGFnZScsIHByb3AxODogJ1NvZnR8QW55VGhpbmdMb2FufEFjY2Vzc1VuYXZhaWxhYmxlJyB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICByZXZpZXdDb3VudGVyT2ZmZXI6IGZ1bmN0aW9uIChvZmZlcmVkX2Ftb3VudCwgb2ZmZXJlZF90ZXJtLCBvZmZlcmVkX3B1cnBvc2UsIG9mZmVyZWRfdHlwZSkge1xyXG4gICAgICAgICAgICB2YXIgYW1vdW50ID0gb2ZmZXJlZF9hbW91bnQgPyBwYXJzZUZsb2F0KG9mZmVyZWRfYW1vdW50LnJlcGxhY2UoL1tcXCRcXCxdKy9nLCAnJykpLnRvRml4ZWQoMikgOiAnJyxcclxuICAgICAgICAgICAgICAgIHRlcm0gPSBvZmZlcmVkX3Rlcm0gPyBvZmZlcmVkX3Rlcm0gKyAnIE1vbnRocycgOiAnJztcclxuXHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xSZXZpZXdMb2FuVGVybXMnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJyxcclxuICAgICAgICAgICAgICAgIHByb2R1Y3RzOiAnO0FueXRoaW5nTG9hbjs7O2V2ZW50MTg9JyArIGFtb3VudCArICc7ZVZhcjMwPScgKyB0ZXJtLFxyXG4gICAgICAgICAgICAgICAgZVZhcjI3OiAnQW55dGhpbmdMb2FufCcgKyBvZmZlcmVkX3B1cnBvc2UgKyAnfCcgKyBvZmZlcmVkX3R5cGUsXHJcbiAgICAgICAgICAgICAgICBlVmFyMzA6IHRlcm0sXHJcbiAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDE4LGV2ZW50NDgnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcbiAgICB9O1xyXG5cclxuICAgIC8vIFBSRUZVTkRJTkcgXHJcbiAgICBTQy5wcmVmdW5kaW5nID0ge1xyXG4gICAgICAgIHNpZ25Mb2FuQWdyZWVtZW50OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xTaWduTG9hbkFncmVlbWVudCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHZpZXdMb2FuQWdyZWVtZW50OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xWaWV3TG9hbkFncmVlbWVudCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGZ1bmRpbmdJbmZvOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xGdW5kaW5nSW5mbycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHJlc2NoZWR1bGVGdW5kaW5nRGF0ZTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8UmVzY2hlZHVsZUZ1bmRpbmdEYXRlJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY2hhbmdlTG9hblRlcm1PZmZlcjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8Q2hhbmdlTG9hblRlcm1PZmZlcicsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGFwcHJvdmVkTmV3TG9hblRlcm1zOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xBcHByb3ZlZE5ld0xvYW5UZXJtcycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHJlcXVlc3RlZExvYW5UZXJtczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8UmVxdWVzdGVkTG9hblRlcm1zJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY2FuY2VsTmV3TG9hblRlcm1SZXE6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfENhbmNlbE5ld0xvYW5UZXJtUmVxJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY2hhbmdlTW9udGhseVBtdERhdGU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfENoYW5nZU1vbnRobHlQbXREYXRlJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgY2hhbmdlQ2hlY2tpbmdBY3RJbmZvOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xDaGFuZ2VDaGVja2luZ0FjdEluZm8nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBjYW5jZWxQZW5kaW5nTG9hbjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8Q2FuY2VsUGVuZGluZ0xvYW4nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBuTFRSUGVuZGluZzogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAvLyBvYnNvbGV0ZSAtIHVzZSBwZW5kaW5nKGN1cnJlbnRTdGF0dXMpIGluc3RlYWRcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfE5MVFJQZW5kaW5nJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgdmVyaWZpY2F0aW9uOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xWZXJpZmljYXRpb24nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBsb2FuQXBwSW5Qcm9jZXNzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIC8vIG9ic29sZXRlIC0gdXNlIHBlbmRpbmcoY3VycmVudFN0YXR1cykgaW5zdGVhZFxyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8TG9hbkFwcEluUHJvY2VzcycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGxvYW5BcHBQZW5kaW5nOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIC8vIG9ic29sZXRlIC0gdXNlIHBlbmRpbmcoY3VycmVudFN0YXR1cykgaW5zdGVhZFxyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8TG9hbkFwcFBlbmRpbmcnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwZW5kaW5nOiBmdW5jdGlvbiAoY3VycmVudFN0YXR1cykge1xyXG4gICAgICAgICAgICBpZiAoY3VycmVudFN0YXR1cyA9PT0gJ0FwcHJvdmVkTlRMUicpIHtcclxuICAgICAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8TkxUUlBlbmRpbmcnLFxyXG4gICAgICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9IGVsc2UgaWYgKGN1cnJlbnRTdGF0dXMgPT09ICdQZW5kaW5nUScpIHtcclxuICAgICAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8TG9hbkFwcFBlbmRpbmcnLFxyXG4gICAgICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgLy8gZWxzZSAtIHBlbmRpbmcgb3IgaW4gcHJvY2Vzc1xyXG4gICAgICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xMb2FuQXBwSW5Qcm9jZXNzJyxcclxuICAgICAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGVtYWlsQWRkcmVzc0luZm86IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfEVtYWlsQWRkcmVzc0luZm8nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBsb2FuQXBwSW5mb1JlcXVlc3Q6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfExvYW5BcHBJbmZvUmVxdWVzdCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHVwbG9hZERvY3M6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfFVwbG9hZERvY3MnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBmYXhDb3ZlclNoZWV0OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xGYXhDb3ZlclNoZWV0JyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYWNjb3VudFNldHVwOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZ3xBY2NvdW50U2V0dXAnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBjaGFuZ2VMb2FuVGVybUNvbmZpcm06IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfENoYW5nZUxvYW5UZXJtQ29uZmlybScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGNoYW5nZUxvYW5UZXJtVGhhbmtZb3U6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfENoYW5nZUxvYW5UZXJtVGhhbmtZb3UnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJyxcclxuICAgICAgICAgICAgICAgIHByb3AzOiAnTGlnaHRTdHJlYW18TG9hblRlcm1DaGFuZ2VSZXF1ZXN0ZWQnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIG5ld0xvYW5BZ3JlZW1lbnQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfE5ld0xvYW5BZ3JlZW1lbnQnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJyxcclxuICAgICAgICAgICAgICAgIHByb3AzOiAnTGlnaHRTdHJlYW18TG9hblRlcm1DaGFuZ2VBcHByb3ZlZCdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgbmV3TG9hblJlcUNvbXA6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfE5ld0xvYW5SZXFDb21wJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8UHJlRnVuZGluZydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcmV2aWV3TG9hblRlcm1zOiBmdW5jdGlvbiAoaW5wdXRfYW1vdW50LCBpbnB1dF90ZXJtLCBpbnB1dF9wdXJwb3NlLCBpbnB1dF90eXBlKSB7XHJcbiAgICAgICAgICAgIHZhciBhbW91bnQgPSBpbnB1dF9hbW91bnQgPyBwYXJzZUZsb2F0KGlucHV0X2Ftb3VudC50b1N0cmluZygpLnJlcGxhY2UoL1tcXCRcXCxdKy9nLCAnJykpLnRvRml4ZWQoMikgOiAnJyxcclxuICAgICAgICAgICAgICAgIHRlcm0gPSBpbnB1dF90ZXJtID8gaW5wdXRfdGVybSArICcgTW9udGhzJyA6ICcnO1xyXG5cclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nfFJldmlld0xvYW5UZXJtcycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmcnLFxyXG4gICAgICAgICAgICAgICAgcHJvZHVjdHM6ICc7QW55dGhpbmdMb2FuOzs7ZXZlbnQxOD0nICsgYW1vdW50ICsgJztlVmFyMzA9JyArIHRlcm0sXHJcbiAgICAgICAgICAgICAgICBlVmFyMjc6ICdBbnl0aGluZ0xvYW58JyArIGlucHV0X3B1cnBvc2UgKyAnfCcgKyBpbnB1dF90eXBlLFxyXG4gICAgICAgICAgICAgICAgZVZhcjMwOiB0ZXJtLFxyXG4gICAgICAgICAgICAgICAgZXZlbnRzOiAnZXZlbnQxOCxldmVudDQ4J1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBzY2hlZHVsZWRGdW5kaW5nSW5mbzogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfFByZUZ1bmRpbmd8U2NoZWR1bGVkRnVuZGluZ0luZm8nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxQcmVGdW5kaW5nJyxcclxuICAgICAgICAgICAgICAgIHByb2R1Y3RzOiAnO0FueXRoaW5nTG9hbicsXHJcbiAgICAgICAgICAgICAgICBlVmFyOTogJ0xpZ2h0U3RyZWFtfEFueXRoaW5nTG9hbicsXHJcbiAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDgnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuICAgIH07XHJcblxyXG4gICAgLy8gQUNDT1VOVCBTRVJWSUNFU1xyXG4gICAgU0MuYWNjdFNlcnZpY2VzID0ge1xyXG4gICAgICAgIHdlbGNvbWU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8V2VsY29tZScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgc3VtbWFyeTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xTdW1tYXJ5JyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBhY2NvdW50czogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xBY2NvdW50cycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgYXV0b1BheUluZm86IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8QXV0b1BheUluZm8nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGF1dG9QYXlDaGFuZ2VBY2N0SW5mbzogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xBdXRvUGF5Q2hhbmdlQWNjdEluZm8nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGF1dG9QYXlDaGFuZ2VQbW50QW10OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfEF1dG9QYXlDaGFuZ2VQbW50QW10JyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBjb250YWN0VXNBY2NvdW50U2VydmljZXM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8UXVlc3Rpb25zJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBleHRyYVBtbnRBbmRQYXlvZmZzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfEV4dHJhUG1udCZQYXlvZmZzJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBleHRyYVBtbnRTY2hlZHVsZTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xFeHRyYVBtbnRTY2hlZHVsZScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZXh0cmFQbW50U2NoZWR1bGVkOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfEV4dHJhUG1udFNjaGVkdWxlZCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZXh0cmFQbW50U2NoZWR1bGVkQ2FuY2VsOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfEV4dHJhUG1udFNjaGVkdWxlZENhbmNlbCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZXh0cmFQbW50Q2FuY2VsOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfEV4dHJhUG1udENhbmNlbCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcGF5b2ZmU2NoZWR1bGU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8UGF5b2ZmU2NoZWR1bGUnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHBheW9mZk1haWxJbjogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQYXlvZmZNYWlsSW4nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHBheW9mZk1haWxJbkludm9pY2U6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8UGF5b2ZmTWFpbEluSW52b2ljZScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgbmlja05hbWVBY2N0OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfE5pY2tuYW1lQWNjdCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZG9jdW1lbnRzOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfERvY3VtZW50cycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgZW5vdGljZXM6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8RW5vdGljZXMnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGRlY2xpbmVOb3RpY2U6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8RGVjbGluZU5vdGljZScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZkhvbWU6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8UHJvZmlsZScsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZkNvbnRhY3RJbmZvOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFByZWZDb250YWN0SW5mbycsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZkFjY3RMb2NrRW5hYmxlZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQcmVmQWNjdExvY2tFbmFibGVkJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwcmVmQWNjdExvY2tEaXNhYmxlZDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQcmVmQWNjdExvY2tEaXNhYmxlZCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZkNoYW5nZVVzZXJJRDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQcmVmQ2hhbmdlVXNlcklEJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwcmVmVXNlcklET3B0aW9uOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFByZWZVc2VySURPcHRpb24nLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHByZWZDaGFuZ2VVc2VySURDb25maXJtOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFByZWZDaGFuZ2VVc2VySURDb25maXJtJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyxcclxuICAgICAgICAgICAgICAgIHByb3AzOiAnTGlnaHRTdHJlYW18Q2hhbmdlVXNlcklEJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwcmVmQ2hhbmdlUGFzc3dvcmQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8UHJlZkNoYW5nZVBhc3N3b3JkJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBwcmVmQ2hhbmdlUGFzc0NvbmZpcm06IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8UHJlZkNoYW5nZVBhc3NDb25maXJtJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyxcclxuICAgICAgICAgICAgICAgIHByb3AzOiAnTGlnaHRTdHJlYW18Q2hhbmdlUGFzcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZkNoYW5nZVNlY3VyaXR5QW5zd2VyOiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFByZWZDaGFuZ2VTZWN1cml0eUFuc3dlcicsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZkNoYW5nZVNlY3VyaXR5QW5zQ29uZmlybTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQcmVmQ2hhbmdlU2VjdXJpdHlBbnNDb25maXJtJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyxcclxuICAgICAgICAgICAgICAgIHByb3AzOiAnTGlnaHRTdHJlYW18Q2hhbmdlU2VjdXJpdHlBbnMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHByZWZlQ2hvaWNlczogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQcmVmZUNob2ljZXMnLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnLFxyXG4gICAgICAgICAgICAgICAgcHJvcDM6ICdMaWdodFN0cmVhbXxDaGFuZ2VTZWN1cml0eUFucydcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJlZmVDaG9pY2VzQ29uZmlybTogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBTQy5fdCh7XHJcbiAgICAgICAgICAgICAgICBwYWdlTmFtZTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlc3xQcmVmZUNob2ljZXNDb25maXJtJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyxcclxuICAgICAgICAgICAgICAgIHByb3AzOiAnTGlnaHRTdHJlYW18Q2hhbmdlUHJlZkNob2ljZSdcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfSxcclxuXHJcbiAgICAgICAgcHJpdmFjeVNlY3VyaXR5OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJyxcclxuICAgICAgICAgICAgICAgIHByb3AxMTogJ0N1c3RvbWVyJyxcclxuICAgICAgICAgICAgICAgIHByb3AxMjogJ0FjY291bnRTZXJ2aWNlcycsXHJcbiAgICAgICAgICAgICAgICBwcm9wMTM6ICdQcml2YWN5U2VjdXJpdHknXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHJhdGVzOiBmdW5jdGlvbiAoZVZhciwgdmFsKSB7XHJcbiAgICAgICAgICAgIHZhciBwYWdlVmFycyA9IHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFJhdGVzJyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9O1xyXG4gICAgICAgICAgICBpZiAodHlwZW9mIGVWYXIgPT09ICdzdHJpbmcnICYmIC9eZVZhci8udGVzdChlVmFyKSkge1xyXG4gICAgICAgICAgICAgICAgcGFnZVZhcnNbZVZhcl0gPSB2YWw7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgU0MuX3QocGFnZVZhcnMpO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIGFwcGxpY2F0aW9uUmVjZWl2ZWQ6IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgdmFyIHNlc3Npb25JZCA9ICQuY29va2llKHRoaXMuX2NrVW5pcXVlKTtcclxuICAgICAgICAgICAgU0MuX3Qoe1xyXG4gICAgICAgICAgICAgICAgcGFnZU5hbWU6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXN8QXBwbGljYXRpb25SZWNlaXZlZCcsXHJcbiAgICAgICAgICAgICAgICBoaWVyMTogJ0xTY29tfEN1c3RvbWVyfEFjY291bnRTZXJ2aWNlcycsXHJcbiAgICAgICAgICAgICAgICBwcm9kdWN0czogJztBbnl0aGluZ0xvYW4nLFxyXG4gICAgICAgICAgICAgICAgdHJhbnNhY3Rpb25JRDogc2Vzc2lvbklkLFxyXG4gICAgICAgICAgICAgICAgZVZhcjM4OiBzZXNzaW9uSWQsXHJcbiAgICAgICAgICAgICAgICBldmVudHM6ICdldmVudDcsIGV2ZW50OTcgJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG5cclxuICAgICAgICBoaXN0b3J5OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfEhpc3RvcnknLFxyXG4gICAgICAgICAgICAgICAgaGllcjE6ICdMU2NvbXxDdXN0b21lcnxBY2NvdW50U2VydmljZXMnXHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0sXHJcblxyXG4gICAgICAgIHZpZXdMb2FuQWdyZWVtZW50OiBmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgIFNDLl90KHtcclxuICAgICAgICAgICAgICAgIHBhZ2VOYW1lOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzfFZpZXdMb2FuQWdyZWVtZW50JyxcclxuICAgICAgICAgICAgICAgIGhpZXIxOiAnTFNjb218Q3VzdG9tZXJ8QWNjb3VudFNlcnZpY2VzJ1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9LFxyXG4gICAgfTtcclxuXHJcbiAgICAvLyBORUVEUyBJTklUSUFMSVpBVElPTlxyXG4gICAgU0MubWFpbi5uYXZiYXIubGlua3MoKTtcclxuICAgIFNDLm1haW4udGlsZXMubGVhcm5Nb3JlKCk7XHJcbiAgICBTQy5tYWluLmZvb3Rlci5zb2NpYWxJY29ucygpO1xyXG4gICAgU0MubWFpbi5iZW5lZml0SWNvbnMoKTtcclxufShqUXVlcnksIHdpbmRvdykpO1xyXG4iLCIvLy8qZ2xvYmFscyBzLCB3aW5kb3csIGRvY3VtZW50ICovXHJcbi8vKGZ1bmN0aW9uICgpIHtcclxuLy8gICAgJ3VzZSBzdHJpY3QnO1xyXG5cclxuLy8gICAgLy8gZGlzYWJsZSBzdXBlci1zbG93IG9tbml0dXJlIGNsaWNrIHRyYWNraW5nIG9uIHNvbWUgbGlua3NcclxuLy8gICAgdmFyIG9sZCA9IHMubXI7XHJcbi8vICAgIHMubXIgPSBmdW5jdGlvbiAoc2VzcywgcSwgcnMsIHRhLCB1KSB7XHJcbi8vICAgICAgICBpZiAod2luZG93LmV2ZW50ICYmIHdpbmRvdy5ldmVudC50b0VsZW1lbnQgJiYgd2luZG93LmV2ZW50LnRvRWxlbWVudC5jbGFzc0xpc3QuY29udGFpbnMoXCJuby10cmFja1wiKSkge1xyXG4vLyAgICAgICAgICAgIHJldHVybjtcclxuLy8gICAgICAgIH1cclxuLy8gICAgICAgIG9sZC5jYWxsKHRoaXMsIHNlc3MsIHEsIHJzLCB0YSwgdSk7XHJcbi8vICAgIH07XHJcbi8vfSgpKTtcclxuXHJcblxyXG4vKmdsb2JhbCB0cmFja2luZyovXHJcbihmdW5jdGlvbiAoJCwgU0MpIHtcclxuICAgIHZhciBfc2V0dXBWaW1lb1RyYWNraW5nID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICQoJ2lmcmFtZVtzcmMqPVwicGxheWVyLnZpbWVvLmNvbVwiXScpLmVhY2goZnVuY3Rpb24gKGlkeCwgaWZyYW1lKSB7XHJcbiAgICAgICAgICAgIHZhciB2aW1lb1BsYXllciA9IG5ldyBWaW1lby5QbGF5ZXIoaWZyYW1lKTtcclxuXHJcbiAgICAgICAgICAgIHZhciB2aW1lb1RyYWNrZXIgPSBuZXcgJC5BZG9iZVZpbWVvVHJhY2tpbmcoe1xyXG4gICAgICAgICAgICAgICAgdmltZW9QbGF5ZXI6IHZpbWVvUGxheWVyXHJcbiAgICAgICAgICAgIH0pO1xyXG5cclxuICAgICAgICAgICAgdmltZW9UcmFja2VyLnZpZGVvU3RhcnRlZCgpLnRoZW4oU0MubWFpbi52aW1lb1ZpZGVvKTtcclxuICAgICAgICAgICAgdmltZW9UcmFja2VyLnZpZGVvMjVQZXJjZW50V2F0Y2hlZCgpLnRoZW4oU0MubWFpbi52aW1lb1ZpZGVvKTtcclxuICAgICAgICAgICAgdmltZW9UcmFja2VyLnZpZGVvNTBQZXJjZW50V2F0Y2hlZCgpLnRoZW4oU0MubWFpbi52aW1lb1ZpZGVvKTtcclxuICAgICAgICAgICAgdmltZW9UcmFja2VyLnZpZGVvNzVQZXJjZW50V2F0Y2hlZCgpLnRoZW4oU0MubWFpbi52aW1lb1ZpZGVvKTtcclxuICAgICAgICAgICAgdmltZW9UcmFja2VyLnZpZGVvQ29tcGxldGVkKCkudGhlbihTQy5tYWluLnZpbWVvVmlkZW8pO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfVxyXG5cclxuICAgIHZhciBfb25Eb2N1bWVudFJlYWR5ID0gZnVuY3Rpb24gKCkge1xyXG4gICAgICAgIGlmICh0eXBlb2YgVmltZW8gPT0gJ3VuZGVmaW5lZCcpXHJcbiAgICAgICAgICAgIHJldHVybjtcclxuXHJcbiAgICAgICAgX3NldHVwVmltZW9UcmFja2luZygpO1xyXG4gICAgfVxyXG5cclxuICAgIHZhciBfaW5pdCA9IGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAkKGRvY3VtZW50KS5yZWFkeShfb25Eb2N1bWVudFJlYWR5KTtcclxuICAgIH1cclxuXHJcbiAgICBfaW5pdCgpO1xyXG59KShqUXVlcnksIFNDKTsiXX0=
