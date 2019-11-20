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
