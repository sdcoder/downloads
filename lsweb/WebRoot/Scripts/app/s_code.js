/* SunTrust s_code v5.4 last updated 11/27/2018 */
/* SiteCatalyst code version: AppMeasurement 2.8.1*/

/* SunTrust Report Selection Criteria. */

/* Note to future developers. 
 * 
 * Adobe sent this file with a bug. See changeset:
 * http://team03:8080/tfs/LightStream/LoanSystem/_versionControl/changeset/35840
 * 
 * If any future versions include the contentWindow is null issue, they have re-introduced it
 */
var s_account = "suntrustdev";

var s_d = document.domain;
if (s_d.indexOf("test-ice") > -1 || s_d.indexOf("onlinebanking-itca") > -1 || s_d.indexOf("clientcareforms-test") > -1 || s_d.indexOf("newaccount-itca") > -1 || s_d.indexOf("internetbankingsupport-test") > -1 || s_d.indexOf("newaccount-intg") > -1 || s_d.indexOf("newaccount-aitc") > -1 || s_d.indexOf("mobile-itca") > -1) { s_account = "suntrusttest" }

else if (s_d.indexOf("ice-qa") > -1 || s_d.indexOf("onlinebanking-prdr") > -1 || s_d.indexOf("newaccount-aprd") > -1 || s_d.indexOf("newaccount-prdr") > -1 || s_d.indexOf("clientcareforms-qa") > -1 || s_d.indexOf("internetbankingsupport-qa") > -1 || s_d.indexOf("mobile-prdr") > -1 || s_d.indexOf("mobile-aplt") > -1) { s_account = "suntrustqa" }

else if (s_d.indexOf("stcom-bau.suntrust.com") > -1 || s_d.indexOf("clientcareforms-dev") > -1 || s_d.indexOf("dev4a-ice") > -1 || s_d.indexOf("newaccount-dev1") > -1 || s_d.indexOf("newaccount-dev2") > -1 || s_d.indexOf("newaccount-dev3") > -1 || s_d.indexOf("newaccount-dev4") > -1 || s_d.indexOf("internetbankingsupport-dev") > -1 || s_d.indexOf("corp.suntrust.com") > -1 || s_d.indexOf("mobile-dev") > -1 || s_d.indexOf("newaccount-mnt1") > -1 || s_d.indexOf("newaccount-mnt2") > -1 || s_d.indexOf("newaccount-adv1") > -1 || s_d.indexOf("newaccount-adv2") > -1 || s_d.indexOf("mobile-adv") > -1) { s_account = "suntrustdev" }

else if (s_d.indexOf("suntrust.com") > -1 || s_d.indexOf("newaccount") > -1 || s_d.indexOf("newaccount-dr") > -1 || s_d.indexOf("suntrustenespanol.com") > -1) { s_account = "suntrustprod" }

/*Added 05-12-2015*/
else if (s_d.indexOf("lightstream.com") > -1) {
    s_account = /(dev|test|qa).lightstream.com/i.test(s_d) ? 'suntrustlightstreamdev,suntrustdev' : 'suntrustlightstreamprod,suntrustprod';
}

/*Added 09-24*/
if (s_d.indexOf("sso.ft.cashedge.com") > -1) { s_account = "suntrustprod" }
if (s_d.indexOf("fundstransfer.cashedge.com") > -1) { s_account = "suntrustprod" }
if (s_d.indexOf("transfers.fta.cashedge.com") > -1) { s_account = "suntrustprod" }
if (s_d.indexOf("popmoney.ft1.cashedge.com") > -1) { s_account = "suntrustprod" }

var s_i, s_isip = 1, s_ip = "0123456789."
for (var s_i = 0; s_i < s_d.length; s_i++) { if (s_ip.indexOf(s_d.charAt(s_i)) == -1) { s_isip = 0 } }
if (s_isip) { s_account = "suntrustdev" }

var s = s_gi(s_account);

if (typeof Visitor != 'undefined') {
    s.visitor = Visitor.getInstance("AA7A3BC75245B3BC0A490D4D@AdobeOrg");
}

/************************** CONFIG SECTION **************************/
/* You may add or alter any code config here. */
if (!s.charSet) s.charSet = "UTF-8"
/* Conversion Config */
s.currencyCode = "USD"
/* Link Tracking Config */
s.trackDownloadLinks = true
s.trackExternalLinks = true
s.trackInlineStats = true
s.linkDownloadFileTypes = "exe,zip,wav,mp3,mov,mpg,avi,wmv,doc,pdf,xls,ofx,qfx,csv"
s.linkInternalFilters = "javascript:,suntrust.com,liveperson.net,fiserv.com,cashedge.com,zag.com,digitas.com,lightstream.com"
s.linkLeaveQueryString = false
s.linkTrackVars = "prop6,prop7,prop8,prop50,eVar6,eVar7,eVar8,eVar10,contextData.EVENTS"
s.linkTrackEvents = "None"

/* WARNING: Changing any of the below variables will cause drastic
changes to how your visitor data is collected.  Changes should only be
made when instructed to do so by your account manager.*/
s.visitorNamespace = "suntrust"
s.trackingServer = "omni.suntrust.com"
s.trackingServerSecure = "somni.suntrust.com"


/********************************************************************
 *
 * Supporting functions that may be shared between plug-ins
 *
 *******************************************************************/

/*
 * Utility Function: vpr - set the variable vs with value v
 */
s.vpr = new Function("vs", "v",
    "if(typeof(v)!='undefined' && vs){var s=this; eval('s.'+vs+'=\"'+v+'\"')}");

/* Plugin Config Section */
//performance timing
s.ptf = true;

s.ActionDepthTest = true

s.usePlugins = true;
s.doPlugins = function (s) {

    //Performance Timing
    if (s.ptf === true) s.performanceTiming();
    s.ptf = false;

    /*Current Version of Omniture Collection Code - Updated Manually*/
    s.prop50 = "SunTrust s_code v5.4|Omniture Base Code AM " + s.version;
    s.events = s.events ? s.events : s.events = "";
    s.server = ("" + s.wd.location).toLowerCase();

    /*Checks for Presence of Visitor ID Service - writes result into Context Data Variable */
    s.contextData['vidAPICheck'] = (typeof (Visitor) != "undefined" ? "VisitorAPI Present" : "VisitorAPI Missing");

    /* This code will automatically set s.channel, s.eVar10,
    s.prop11, s.prop12, s.prop13, and s.prop14 by extracting
    the appropriate values out of the s.hier1 variable */
    if (s.pageName && s.hier1) {
        var s_hps = s.split(s.hier1, "|")
        s.channel = s_hps[0];
        if (s.channel) { s.eVar10 = "D=ch"; }
        if (s_hps[1]) { s.prop11 = s.channel + "|" + s_hps[1]; } else { if (!s.hps) { s.hps = s.channel; } s.prop11 = s.hps; }
        if (s_hps[2]) { s.prop12 = s.prop11 + "|" + s_hps[2]; } else { if (!s.hps) { s.hps = s.prop11; } s.prop12 = s.hps; }
        if (s_hps[3]) { s.prop13 = s.prop12 + "|" + s_hps[3]; } else { if (!s.hps) { s.hps = s.prop12; } s.prop13 = s.hps; }
        if (s_hps[4]) { s.prop14 = s.prop13 + "|" + s_hps[4]; } else { if (!s.hps) { s.hps = s.prop13; } s.prop14 = s.hps; }
        s.hps = "";
    }

    /*External Campaign variable setting - set this to the query parameter you use for external promotional traffic*/
    if (s.campaign) s.campaign = s.getValOnce(s.campaign, 'v0', 0);
    if (!s.campaign) s.campaign = s.getQueryParam('cid,lcid');
    
    /*Internal Promotion variable setting - set this to the query parameter you use for internal promotion traffic*/
    if (!s.eVar2) s.eVar2 = s.getQueryParam('icid');
    if (s.eVar2) s.eVar2 = s.getValOnce(s.eVar2, 'v2', 0);

    /*Capture Internal search phrase*/
    if (s.prop1) s.prop1 = s.prop1.toLowerCase();
    if (s.prop1) s.prop1 = s.getValOnce(s.prop1, 'c1', 0);
    if (s.prop1) {
        s.eVar1 = "D=c1";
        s.events = s.apl(s.events, "event1", ",", 2);
    }
    if (!s.prop1) { s.prop2 = ""; }

    /*Copy Self-service prop and set event*/
    if (s.prop3) {
        s.eVar3 = "D=c3";
        s.events = s.apl(s.events, "event3", ",", 2);
    }

    /*Copy Tool Usage prop and set event*/
    if (s.prop4) {
        s.eVar4 = "D=c4";
        s.events = s.apl(s.events, "event4", ",", 2);
    }

    /*Copy Internal Traffic prop*/
    if (s.prop6) {
        s.eVar6 = "D=c6";
    }

    /*Copy Site Error prop*/
    if (s.prop18) {
        s.eVar20 = "D=c18";
    }

    /* TimeParting Variables */
    s.eVar7 = s.prop7 = s.getTimeParting('n', '-5'); // Sets hour and day
    // s.eVar8 = s.prop8 = s.getTimeParting('d', '-5'); // Set day

    if (s.pageName) {
        if (s.linkType == "e") {
            s.prop15 = s.pageName;
            s.linkTrackVars = s.linkTrackVars + ",prop15";
            //s.linkTrackVars = s.apl(s.linkTrackVars, "prop15", ",", 2);
        }
    }

    if (s.ActionDepthTest) {
        s.pdvalue = s.getActionDepth("s_depth");
        if (s.pdvalue == 1) s.events = s.apl(s.events, 'event11', ',', 2);
        if (s.pdvalue == 2) s.events = s.apl(s.events, 'event12', ',', 2);
        s.ActionDepthTest = false;
    }

    /* Autofire the Location Search event */
    if (s.eVar15 || s.eVar17) {
        s.events = s.apl(s.events, 'event16', ',', 2);
    }
    if (s.eVar15) { s.eVar15 = s.eVar15.toLowerCase(); }
    if (s.eVar17) { s.eVar17 = s.eVar17.toLowerCase(); }

    /* Get Previous Page Name */
    if (s.pageName) {
        s.eVar19 = s.getPreviousValue(s.pageName, 'v19', 0);
    }

    /*Deduplicate High-Level Pathing*/
    if (s.prop19) {
        s.prop19 = s.getValOnce(s.prop19, 'c19', 0);
    }

    /*Setting getTimeToComplete plugin to measure time from campaign click-through to application completion*/
    if (s.events.indexOf('event15') > -1)
        s.eVar14 = 'start';
    if (s.events.indexOf('event7') > -1)
        s.eVar14 = 'stop';
    s.eVar14 = s.getTimeToComplete(s.eVar14, 'ttc', 0);

    if (s.inList('event2', s.events) || s.c_r('s_ev39')) { s.eVar39 = s.getAndPersistValue('c', 's_ev39', 1825); }
    else { s.eVar39 = "p"; }

    if (s.eVar5 || s.c_r('s_ev5')) {
        if (s.eVar5) {
            if (s.eVar5.length < 11) {
                var tmpdigitalid = "tmp";
                s.eVar5 = s.getAndPersistValue(tmpdigitalid, 's_ev5', 1825);
            } else {
                var digitalid = s.eVar5;
                s.eVar5 = s.getAndPersistValue(digitalid, 's_ev5', 1825);
            }

        } else {
            var digitalid = s.c_r('s_ev5');
            if (digitalid.length < 11) {
                var tmpid = "tmp";
                s.eVar5 = s.getAndPersistValue(tmpid, 's_ev5', 1825);
            } else {
                s.eVar5 = s.c_r('s_ev5');
            }

        };
        // Setting eVar5 as Customer ID in Marketing Cloud if defined
        if (typeof Visitor != 'undefined') {
            visitor.setCustomerIDs({
                "digitalid": {
                    "id": s.eVar5,
                    "authState": 1 //Visitor.AuthState.AUTHENTICATED
                }
            });
        }
    }
    //AAM ID Sync for Analytics ID - checks first for legacy ID (s_vi), uses Marketing Cloud ID if s_vi not present
    if (typeof Visitor != 'undefined') {
        if (visitor.getAnalyticsVisitorID()) {
            var sc = visitor.getAnalyticsVisitorID();
        }
        else { var sc = visitor.getMarketingCloudVisitorID(); }
    }
    //Comment out this statement for Ensighten to work
    if (sc) {
        if (typeof Visitor != 'undefined') {
            visitor.setCustomerIDs({
                "st_adobeanalytics": {
                    "id": sc,
                    "authState": 0  // Visitor.AuthState.UNKNOWN
                }
            });
        }
    }

    s.eVar40 = "+1";

    s.list2 = s.gatherIntPromo('icid', 1, /icid=[A-Za-z0-9_]+/g);  //Gets all instances of 'icid' query string parameter values from the page
    if (s.list2) { s.events = s.apl(s.events, "event53", ",", 2); } //if list2 is present, trigger event53 (internal promo impressions event)

    s.contextData['EVENTS'] = s.events ? s.events + ',' : '';

    /* Identify Percent of Page Viewed*/

    var ppvArray = s.getPercentPageViewed(s.pageName);
    if (ppvArray) {
        s.prop30 = ppvArray[0]; //contains the previous page name
        s.prop31 = ppvArray[1]; //contains the total percent viewed
        s.prop32 = ppvArray[2]; //contains the percent viewed on initial load
        s.prop33 = ppvArray[3]; //contains the total number of vertical pixels
    }
}

/************************** PLUGINS SECTION *************************/
/* You may insert any plugins you wish to use here.                 */

/*
* Utility Plugin - Compatibility Functions for AppMeasurement JS
*/
s.wd = window;
s.fl = new Function("x", "l", ""
    + "return x?(''+x).substring(0,l):x");
s.pt = new Function("x", "d", "f", "a", ""
    + "var s=this,t=x,z=0,y,r;while(t){y=t.indexOf(d);y=y<0?t.length:y;t=t"
    + ".substring(0,y);r=s[f](t,a);if(r)return r;z+=y+d.length;t=x.substri"
    + "ng(z,x.length);t=z<x.length?t:''}return'';");

/*
 * Cookie Combining Utility v.5
 */

if (!s.__ccucr) {
    s.c_rr = s.c_r;
    s.__ccucr = true;
    function c_r(k) {
        var s = this, d = new Date, v = s.c_rr(k), c = s.c_rspers(), i, m, e;
        if (v) return v; k = s.escape ? s.escape(k) : encodeURIComponent(k);
        i = c.indexOf(' ' + k + '='); c = i < 0 ? s.c_rr('s_sess') : c;
        i = c.indexOf(' ' + k + '='); m = i < 0 ? i : c.indexOf('|', i);
        e = i < 0 ? i : c.indexOf(';', i); m = m > 0 ? m : e;
        v = i < 0 ? '' : s.unescape ? s.unescape(c.substring(i + 2 + k.length, m < 0 ? c.length : m)) : decodeURIComponent(c.substring(i + 2 + k.length, m < 0 ? c.length : m));
        return v;
    }
    function c_rspers() {
        var s = this, cv = s.c_rr("s_pers"), date = new Date().getTime(), expd = null, cvarr = [], vcv = "";
        if (!cv) return vcv; cvarr = cv.split(";"); for (var i = 0, l = cvarr.length; i < l; i++) {
            expd = cvarr[i].match(/\|([0-9]+)$/);
            if (expd && parseInt(expd[1]) >= date) { vcv += cvarr[i] + ";"; }
        } return vcv;
    }
    s.c_rspers = c_rspers;
    s.c_r = s.cookieRead = c_r;
}
if (!s.__ccucw) {
    s.c_wr = s.c_w;
    s.__ccucw = true;
    function c_w(k, v, e) {
        var s = this, d = new Date, ht = 0, pn = 's_pers', sn = 's_sess', pc = 0, sc = 0, pv, sv, c, i, t, f;
        d.setTime(d.getTime() - 60000); if (s.c_rr(k)) s.c_wr(k, '', d); k = s.escape ? s.escape(k) : encodeURIComponent(k);
        pv = s.c_rspers(); i = pv.indexOf(' ' + k + '='); if (i > -1) { pv = pv.substring(0, i) + pv.substring(pv.indexOf(';', i) + 1); pc = 1; }
        sv = s.c_rr(sn); i = sv.indexOf(' ' + k + '='); if (i > -1) {
            sv = sv.substring(0, i) + sv.substring(sv.indexOf(';', i) + 1);
            sc = 1;
        } d = new Date; if (e) {
            if (e == 1) e = new Date, f = e.getYear(), e.setYear(f + 5 + (f < 1900 ? 1900 : 0));
            if (e.getTime() > d.getTime()) {
                pv += ' ' + k + '=' + (s.escape ? s.escape(v) : encodeURIComponent(v)) + '|' + e.getTime() + ';';
                pc = 1;
            }
        } else {
            sv += ' ' + k + '=' + (s.escape ? s.escape(v) : encodeURIComponent(v)) + ';';
            sc = 1;
        } sv = sv.replace(/%00/g, ''); pv = pv.replace(/%00/g, ''); if (sc) s.c_wr(sn, sv, 0);
        if (pc) {
            t = pv; while (t && t.indexOf(';') != -1) {
                var t1 = parseInt(t.substring(t.indexOf('|') + 1, t.indexOf(';')));
                t = t.substring(t.indexOf(';') + 1); ht = ht < t1 ? t1 : ht;
            } d.setTime(ht); s.c_wr(pn, pv, d);
        }
        return v == s.c_r(s.unescape ? s.unescape(k) : decodeURIComponent(k));
    }
    s.c_w = s.cookieWrite = c_w;
}
/*
* Plugin: getTimeToComplete 0.4 - return the time from start to stop
*/
s.getTimeToComplete = new Function("v", "cn", "e", ""
    + "var s=this,d=new Date,x=d,k;if(!s.ttcr){e=e?e:0;if(v=='start'||v=='"
    + "stop')s.ttcr=1;x.setTime(x.getTime()+e*86400000);if(v=='start'){s.c"
    + "_w(cn,d.getTime(),e?x:0);return '';}if(v=='stop'){k=s.c_r(cn);if(!s"
    + ".c_w(cn,'',d)||!k)return '';v=(d.getTime()-k)/1000;var td=86400,th="
    + "3600,tm=60,r=5,u,un;if(v>td){u=td;un='days';}else if(v>th){u=th;un="
    + "'hours';}else if(v>tm){r=2;u=tm;un='minutes';}else{r=.2;u=1;un='sec"
    + "onds';}v=v*r/u;return (Math.round(v)/r)+' '+un;}}return '';");
/*
* Plugin: getAndPersistValue 0.3 - get a value on every page
*/
s.getAndPersistValue = new Function("v", "c", "e", ""
    + "var s=this,a=new Date;e=e?e:0;a.setTime(a.getTime()+e*86400000);if("
    + "v)s.c_w(c,v,e?a:0);return s.c_r(c);");
/*
* Plugin: getPreviousValue_v1.0 - return previous value of designated
* variable (requires split utility) - commenting out - updated version added with performance timing below

s.getPreviousValue = new Function("v", "c", "el", ""
    + "var s=this,t=new Date,i,j,r='';t.setTime(t.getTime()+1800000);if(el"
    + "){if(s.events){i=s.split(el,',');j=s.split(s.events,',');for(x in i"
    + "){for(y in j){if(i[x]==j[y]){if(s.c_r(c)) r=s.c_r(c);v?s.c_w(c,v,t)"
    + ":s.c_w(c,'no value',t);return r}}}}}else{if(s.c_r(c)) r=s.c_r(c);v?"
    + "s.c_w(c,v,t):s.c_w(c,'no value',t);return r}");
*/

/******************************************* BEGIN CODE TO DEPLOY *******************************************/
/* Adobe Consulting Plugin: performanceTiming v.3BETA (Requires AppMeasurement, getPreviousValue recommended) */
s.performanceTiming = function (av) { var s = this; try { (av && (s.ptv = av), "undefined" !== typeof performance) ? (s.performanceClear(), performance.clearResourceTimings(), "" !== s.c_r("s_ptc") && s.performanceRead(), 0 === performance.timing.loadEventEnd) ? s.pi = setInterval(function () { s.performanceWriteFull() }, 250) : 0 < performance.timing.loadEventEnd && (s.ptc ? s.ptc === performance.timing.loadEventEnd && 1 === performance.getEntries().length && (s.pwp = setInterval(function () { s.performanceWritePart() }, 500)) : s.performanceWriteFull()) : "undefined" !== typeof s.linkType && "e" === s.linkType && s.performanceRead() } catch (c) { } };
s.performanceClear = function () { for (var s = this, b = "pt.rdr pt.apc pt.dns pt.tcp pt.req pt.rsp pt.prc pt.onl pt.tot pt.pfi pt.ppi".split(" "), a = 0; a < b.length; a++)s.contextData[b[a]] = ""; s[s.ptv] = "" };
s.performanceWriteFull = function () { var s = this; try { if (0 < performance.timing.loadEventEnd && (clearInterval(s.pi), "" === s.c_r("s_ptc"))) { var a = performance.timing, b = ""; b = "pt.rdr$" + s.performanceCheck(a.fetchStart, a.navigationStart); b += "^^pt.apc$" + s.performanceCheck(a.domainLookupStart, a.fetchStart); b += "^^pt.dns$" + s.performanceCheck(a.domainLookupEnd, a.domainLookupStart); b += "^^pt.tcp$" + s.performanceCheck(a.connectEnd, a.connectStart); b += "^^pt.req$" + s.performanceCheck(a.responseStart, a.connectEnd); b += "^^pt.rsp$" + s.performanceCheck(a.responseEnd, a.responseStart); b += "^^pt.prc$" + s.performanceCheck(a.loadEventStart, a.domLoading); b += "^^pt.onl$" + s.performanceCheck(a.loadEventEnd, a.loadEventStart); b += "^^pt.tot$" + s.performanceCheck(a.loadEventEnd, a.navigationStart); s.c_w("s_ptc", b + "^^pt.pfi$1"); s.ptv && s.performanceGetEntries() } } catch (c) { return } s.ptc = performance.timing.loadEventEnd };
s.performanceWritePart = function () { var s = this; 0 < performance.getEntries().length && (s.ppfe === performance.getEntries().length ? clearInterval(s.pwp) : s.ppfe = performance.getEntries().length); try { s.tempPe = ((performance.getEntries()[performance.getEntries().length - 1].responseEnd - performance.getEntries()[0].startTime) / 1E3).toFixed(2); s.ptv && s.performanceGetEntries(); var a = "pt.rsp$" + s.tempPe + "^^pt.tot$" + s.tempPe + "^^pt.ppi$1"; s.c_w("s_ptc", a) } catch (b) { } };
s.performanceGetEntries = function () { var s = this; if ("" === s.c_r("s_ptc")) try { if (s.c_w("s_ptc", s.tempPe), sessionStorage && "undefined" !== typeof s.ptv) { s.tempPe = ""; var b = performance.getEntries(), c = b.length, a; for (a = 0; a < c; a++)s.tempPe += "!", s.tempPe += -1 < b[a].name.indexOf("?") ? b[a].name.split("?")[0] : b[a].name, s.tempPe += "|" + (Math.round(b[a].startTime) / 1E3).toFixed(1) + "|" + (Math.round(b[a].duration) / 1E3).toFixed(1) + "|" + b[a].initiatorType; sessionStorage.setItem("s_pec", s.tempPe) } } catch (d) { } };
s.performanceCheck = function (a, b) { if (0 <= a && 0 <= b) return 6E4 > a - b && 0 <= a - b ? parseFloat((a - b) / 1E3).toFixed(2) : 600 };
s.performanceRead = function () { var c = this.c_r("s_ptc").split("^^"), d = c.length, a; 0 < performance.timing.loadEventEnd && clearInterval(this.pi); for (a = 0; a < d; a++) { var b = c[a].split("$"); this.contextData[b[0]] = b[1]; "pt.tot" === b[0] && (this._totalPageLoadTime = b[1]) } this.c_w("s_ptc", "", 0); "undefined" !== typeof this.ptv && ("undefined" !== typeof sessionStorage ? (this[this.ptv] = sessionStorage.getItem("s_pec"), sessionStorage.removeItem("s_pec")) : this[this.ptv] = "sessionStorage Unavailable") };

/* Adobe Consulting Plugin: getPreviousValue v2.0 (Requires AppMeasurement) */
s.getPreviousValue = function (vtc, cn, el) { var s = this, g = "", a = !0; cn = cn ? cn : "s_gpv"; if (el) { a = !1; el = el.split(","); for (var h = s.events ? s.events.split(",") : "", e = 0, k = el.length; e < k; e++) { for (var f = 0, l = h.length; f < l; f++)if (el[e] === h[f]) { a = !0; break } if (!0 === a) break } } !0 === a && (a = new Date, a.setTime(a.getTime() + 18E5), s.c_r(cn) && (g = s.c_r(cn)), vtc ? s.c_w(cn, vtc, a) : s.c_w(cn, "no previous value", a)); return g };
/******************************************** END CODE TO DEPLOY ********************************************/

/*
 *	getQueryParam v2.5 - H-code and AppMeasurement Compatible
 */
s.getQueryParam = new Function("p", "d", "u", "h", ""
    + "var s=this,v='',i,j,t;d=d?d:'';u=u?u:(s.pageURL?s.pageURL:(s.wd?s.w"
    + "d.location:window.location));while(p){i=p.indexOf(',');i=i<0?p.leng"
    + "th:i;t=s.p_gpv(p.substring(0,i),u+'',h);if(t){t=t.indexOf('#')>-1?t"
    + ".substring(0,t.indexOf('#')):t;}if(t)v+=v?d+t:t;p=p.substring(i==p."
    + "length?i:i+1)}return v");
s.p_gpv = new Function("k", "u", "h", ""
    + "var s=this,v='',q;j=h==1?'#':'?';i=u.indexOf(j);if(k&&i>-1){q=u.sub"
    + "string(i+1);v=s.pt(q,'&','p_gvf',k)}return v");
s.p_gvf = new Function("t", "k", ""
    + "if(t){var s=this,i=t.indexOf('='),p=i<0?t:t.substring(0,i),v=i<0?'T"
    + "rue':t.substring(i+1);if(p.toLowerCase()==k.toLowerCase())return s."
    + "epa?s.epa(v):s.unescape(v);}return''");


/*
* Plugin: getActionDepth v1.0 - Returns the current
* page number of the visit
*/
s.getActionDepth = new Function("c", ""
    + "var s=this,v=1,t=new Date;t.setTime(t.getTime()+1800000);"
    + "if(!s.c_r(c)){v=1}if(s.c_r(c)){v=s.c_r(c);v++}"
    + "if(!s.c_w(c,v,t)){s.c_w(c,v,0)}return v;");

/*
* Plugin: getTimeParting 3.4
*/
s.getTimeParting = new Function("h", "z", ""
    + "var s=this,od;od=new Date('1/1/2000');if(od.getDay()!=6||od.getMont"
    + "h()!=0){return'Data Not Available';}else{var H,M,D,U,ds,de,tm,da=['"
    + "Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturda"
    + "y'],d=new Date();z=z?z:0;z=parseFloat(z);if(s._tpDST){var dso=s._tp"
    + "DST[d.getFullYear()].split(/,/);ds=new Date(dso[0]+'/'+d.getFullYea"
    + "r());de=new Date(dso[1]+'/'+d.getFullYear());if(h=='n'&&d>ds&&d<de)"
    + "{z=z+1;}else if(h=='s'&&(d>de||d<ds)){z=z+1;}}d=d.getTime()+(d.getT"
    + "imezoneOffset()*60000);d=new Date(d+(3600000*z));H=d.getHours();M=d"
    + ".getMinutes();M=(M<10)?'0'+M:M;D=d.getDay();U=' AM';if(H>=12){U=' P"
    + "M';H=H-12;}if(H==0){H=12;}D=da[D];tm=H+':'+M+U;return(tm+'|'+D);}");
/*
* Utility: inList v1.0 - find out if a value is in a list
*/
s.inList = new Function("v", "l", "d", ""
    + "var s=this,ar=Array(),i=0,d=(d)?d:',';if(typeof(l)=='string'){if(s."
    + "split)ar=s.split(l,d);else if(l.split)ar=l.split(d);else return-1}e"
    + "lse ar=l;while(i<ar.length){if(v==ar[i])return true;i++}return fals"
    + "e;");
/*
* Plugin Utility: apl v1.1
*/
s.apl = new Function("L", "v", "d", "u", ""
    + "var s=this,m=0;if(!L)L='';if(u){var i,n,a=s.split(L,d);for(i=0;i<a."
    + "length;i++){n=a[i];m=m||(u==1?(n==v):(n.toLowerCase()==v.toLowerCas"
    + "e()));}}if(!m)L=L?L+d+v:v;return L");

/*
* Utility Function: split v1.5 - split a string (JS 1.0 compatible)
*/
s.split = new Function("l", "d", ""
    + "var i,x=0,a=new Array;while(l){i=l.indexOf(d);i=i>-1?i:l.length;a[x"
    + "++]=l.substring(0,i);l=l.substring(i+d.length);}return a");

/*
* Plugin Utility: Replace v1.0
*/
s.repl = new Function("x", "o", "n", ""
    + "var i=x.indexOf(o),l=n.length;while(x&&i>=0){x=x.substring(0,i)+n+x."
    + "substring(i+o.length);i=x.indexOf(o,i+l)}return x");

/*
* Plugin: getValOnce_v1.11
*/
s.getValOnce = new Function("v", "c", "e", "t", ""
    + "var s=this,a=new Date,v=v?v:'',c=c?c:'s_gvo',e=e?e:0,i=t=='m'?6000"
    + "0:86400000,k=s.c_r(c);if(v){a.setTime(a.getTime()+e*i);s.c_w(c,v,e"
    + "==0?0:a);}return v==k?'':v");

/*
* Utility s.gatherIntPromo v2.0 - Pass in query string value and
returns all instances of that value in a comma-delimited list
*/
s.gatherIntPromo = new Function("p", "m", "r", ""
    + "var s=this,a,d,e,i,l='',r=r?r:'';if(m=='0'){a=document.getElementsB"
    + "yTagName('a');for(i=0;i<=a.length-1;i++){if(a[i].href.indexOf(p)!=-"
    + "1){if(l==''){l=s.getURLVars(a[i].href)[p];}else{l=l+','+s.getURLVar"
    + "s(a[i].href)[p];}}}}else if(m=='1'){a=document.getElementsByTagName"
    + "('body')[0].innerHTML;e=a.match(r);if(e==null){return null;}else{d="
    + "s.eliminateDuplicates(e);for(i=0;i<=d.length-1;i++){if(d[i].indexOf"
    + "(p)!=-1){if(l==''){l=s.getURLVars(d[i])[p];}else{l=l+','+s.getURLVa"
    + "rs(d[i])[p];}}}}}return l;");
s.eliminateDuplicates = new Function("ar", ""
    + "var s=this,j,le=ar.length,ou=[],ob={};for(j=0;j<le;j++){ob[ar[j]]=0"
    + ";}for(j in ob){ou.push(j);}return ou;");
s.getURLVars = new Function("u", ""
    + "var s=this,k,uv=[],uh,h=u.slice(u.indexOf('?')+1).split('&');for(k="
    + "0;k<h.length;k++){uh=h[k].split('=');uv.push(uh[0]);uv[uh[0]]=uh[1]"
    + ";}return uv;");

/*
 * Partner Plugin: DFA Check 1.0 - Restrict DFA calls to once a visit, per report suite, per click
 * through. Used in conjunction with VISTA. Deduplicates SCM hits.
 */
s.partnerDFACheck = new Function("cfg", ""
    + "var s=this,c=cfg.visitCookie,src=cfg.clickThroughParam,scp=cfg.searchCenterParam,p=cfg.newRsidsProp,tv=cfg.tEvar,dl=',',cr,nc,q,g,gs,i,j,k,fnd,v=1,t=new Date,cn=0,ca=new Array,aa=new Array,cs=new A"
    + "rray;t.setTime(t.getTime()+1800000);cr=s.c_r(c);if(cr){v=0;}ca=s.split(cr,dl);if(s.un)aa=s.split(s.un,dl);else aa=s.split(s.account,dl);for(i=0;i<aa.length;i++){fnd = 0;for(j=0;j<ca.length;j++){if(aa[i] == ca[j]){fnd=1;}}if(!fnd){cs[cn"
    + "]=aa[i];cn++;}}if(cs.length){for(k=0;k<cs.length;k++){nc=(nc?nc+dl:'')+cs[k];}cr=(cr?cr+dl:'')+nc;s.vpr(p,nc);v=1;}if(s.wd)q=s.wd.location.search.toLowerCase();else q=s.w.location.search.toLowerCase();q=s.repl(q,'?','&');g=q.indexOf('&'+src.toLow"
    + "erCase()+'=');gs=(scp)?q.indexOf('&'+scp.toLowerCase()+'='):-1;if(g>-1){s.vpr(p,cr);v=1;}else if(gs>-1){v=0;s.vpr(tv,'SearchCenter Visitors');}if(!s.c_w(c,cr,t)){s.c_w(c,cr,0);}if(!s.c_r(c)){v=0;}r"
    + "eturn v>=1;");

/********************************************************************
 *
 * Supporting functions that may be shared between plug-ins
 *
 *******************************************************************/

/*
 * Utility Function: vpr - set the variable vs with value v
 */
s.vpr = new Function("vs", "v",
    "if(typeof(v)!='undefined' && vs){var s=this; eval('s.'+vs+'=\"'+v+'\"')}");

/*
 * Plugin Utility: Replace v1.0
 */
s.repl = new Function("x", "o", "n", ""
    + "var i=x.indexOf(o),l=n.length;while(x&&i>=0){x=x.substring(0,i)+n+x.substring(i+o.length);i=x.indexOf(o,i+l)}return x");


/************************** DFA VARIABLES **************************/
/* @TODO: Fill in these variables with the settings mapped in the
 * DFA wizard and that match your desired preferences. Some of the
 * variables are optional and have been labeled as such below.
 * @TODO: Comments should be removed in a production deployment. */
var dfaConfig = {
    CSID: '5934', // DFA Client Site ID
    SPOTID: '2409535', // DFA Spotlight ID
    tEvar: 'eVar49', // Transfer variable, typically the "View Through" eVar.
    errorEvar: 'eVar50', // DFA error tracking (optional)
    timeoutEvent: 'event22', // Tracks timeouts/empty responses (optional)
    requestURL: "http://fls.doubleclick.net/json?spot=[SPOTID]&src=[CSID]&var=[VAR]&host=integrate.112.2o7.net%2Fdfa_echo%3Fvar%3D[VAR]%26AQE%3D1%26A2S%3D1&ord=[RAND]", // the DFA request URL
    maxDelay: "3000", // The maximum time to wait for DFA servers to respond, in milliseconds.
    visitCookie: "s_dfa", // The name of the visitor cookie to use to restrict DFA calls to once per visit.
    clickThroughParam: "dfaid", // A query string paramter that will force the DFA call to occur.
    searchCenterParam: undefined, // SearchCenter identifier.
    newRsidsProp: undefined //"prop34" // Stores the new report suites that need the DFA tracking code. (optional)
};
/************************ END DFA Variables ************************/

s.maxDelay = dfaConfig.maxDelay;
s.loadModule("Integrate")
s.Integrate.onLoad = function (s, m) {
    var dfaCheck = s.partnerDFACheck(dfaConfig);
    if (dfaCheck) {
        s.Integrate.add("DFA");
        s.Integrate.DFA.tEvar = dfaConfig.tEvar;
        s.Integrate.DFA.errorEvar = dfaConfig.errorEvar;
        s.Integrate.DFA.timeoutEvent = dfaConfig.timeoutEvent;
        s.Integrate.DFA.CSID = dfaConfig.CSID;
        s.Integrate.DFA.SPOTID = dfaConfig.SPOTID;
        s.Integrate.DFA.get(dfaConfig.requestURL);
        s.Integrate.DFA.setVars = function (s, p) {
            if (window[p.VAR]) { // got a response
                if (!p.ec) { // no errors
                    s[p.tEvar] = "DFA-" + (p.lis ? p.lis : 0) + "-" + (p.lip ? p.lip : 0) + "-" + (p.lastimp ? p.lastimp : 0) + "-" + (p.lastimptime ? p.lastimptime : 0) + "-" + (p.lcs ? p.lcs : 0) + "-" + (p.lcp ? p.lcp : 0) + "-" + (p.lastclk ? p.lastclk : 0) + "-" + (p.lastclktime ? p.lastclktime : 0)
                } else if (p.errorEvar) { // got an error response, track
                    s[p.errorEvar] = p.ec;
                }
            } else if (p.timeoutEvent) { // empty response or timeout
                s.events = ((!s.events || s.events == '') ? '' : (s.events + ',')) + p.timeoutEvent; // timeout event
            }
        }
    }

}

/****************************** MODULES *****************************/
s.loadModule("Media")
s.Media.onLoad = function (s, m) {
    /*Configure Media Module Functions */
    s.Media.autoTrack = true;
    s.Media.trackVars = "events,prop34,eVar41,eVar42,eVar43";
    s.Media.trackEvents = "event41,event42,event43,event44,event45,event46,event47"
    s.Media.trackMilestones = "25,50,75";
    s.Media.playerName = "SunTrust Media Player";
    s.Media.segmentByMilestones = true;
    s.Media.trackUsingContextData = true;
    s.Media.contextDataMapping = {
        "a.media.name": "eVar41,prop34",
        "a.media.segment": "eVar42",
        "a.contentType": "eVar43",
        "a.media.timePlayed": "event41",
        "a.media.view": "event42",
        "a.media.segmentView": "event44",
        "a.media.complete": "event43",
        "a.media.milestones": {
            25: "event45",
            50: "event46",
            75: "event47"
        }
    }
};

function AppMeasurement_Module_Media(q) {
    var b = this; b.s = q; q = window; q.s_c_in || (q.s_c_il = [], q.s_c_in = 0); b._il = q.s_c_il; b._in = q.s_c_in; b._il[b._in] = b; q.s_c_in++; b._c = "s_m"; b.list = []; b.open = function (d, c, e, k) {
        var f = {}, a = new Date, l = "", g; c || (c = -1); if (d && e) {
            b.list || (b.list = {}); b.list[d] && b.close(d); k && k.id && (l = k.id); if (l) for (g in b.list) !Object.prototype[g] && b.list[g] && b.list[g].R == l && b.close(b.list[g].name); f.name = d; f.length = c; f.offset = 0; f.e = 0; f.playerName = b.playerName ? b.playerName : e; f.R = l; f.C = 0; f.a = 0; f.timestamp =
                Math.floor(a.getTime() / 1E3); f.k = 0; f.u = f.timestamp; f.c = -1; f.n = ""; f.g = -1; f.D = 0; f.I = {}; f.G = 0; f.m = 0; f.f = ""; f.B = 0; f.L = 0; f.A = 0; f.F = 0; f.l = !1; f.v = ""; f.J = ""; f.K = 0; f.r = !1; f.H = ""; f.complete = 0; f.Q = 0; f.p = 0; f.q = 0; b.list[d] = f
        }
    }; b.openAd = function (d, c, e, k, f, a, l, g) { var h = {}; b.open(d, c, e, g); if (h = b.list[d]) h.l = !0, h.v = k, h.J = f, h.K = a, h.H = l }; b.M = function (d) { var c = b.list[d]; b.list[d] = 0; c && c.monitor && clearTimeout(c.monitor.interval) }; b.close = function (d) { b.i(d, 0, -1) }; b.play = function (d, c, e, k) {
        var f = b.i(d, 1, c, e, k); f && !f.monitor &&
            (f.monitor = {}, f.monitor.update = function () { 1 == f.k && b.i(f.name, 3, -1); f.monitor.interval = setTimeout(f.monitor.update, 1E3) }, f.monitor.update())
    }; b.click = function (d, c) { b.i(d, 7, c) }; b.complete = function (d, c) { b.i(d, 5, c) }; b.stop = function (d, c) { b.i(d, 2, c) }; b.track = function (d) { b.i(d, 4, -1) }; b.P = function (d, c) {
        var e = "a.media.", k = d.linkTrackVars, f = d.linkTrackEvents, a = "m_i", l, g = d.contextData, h; c.l && (e += "ad.", c.v && (g["a.media.name"] = c.v, g[e + "pod"] = c.J, g[e + "podPosition"] = c.K), c.G || (g[e + "CPM"] = c.H)); c.r && (g[e + "clicked"] =
            !0, c.r = !1); g["a.contentType"] = "video" + (c.l ? "Ad" : ""); g["a.media.channel"] = b.channel; g[e + "name"] = c.name; g[e + "playerName"] = c.playerName; 0 < c.length && (g[e + "length"] = c.length); g[e + "timePlayed"] = Math.floor(c.a); 0 < Math.floor(c.a) && (g[e + "timePlayed"] = Math.floor(c.a)); c.G || (g[e + "view"] = !0, a = "m_s", b.Heartbeat && b.Heartbeat.enabled && (a = c.l ? b.__primetime ? "mspa_s" : "msa_s" : b.__primetime ? "msp_s" : "ms_s"), c.G = 1); c.f && (g[e + "segmentNum"] = c.m, g[e + "segment"] = c.f, 0 < c.B && (g[e + "segmentLength"] = c.B), c.A && 0 < c.a && (g[e + "segmentView"] =
                !0)); !c.Q && c.complete && (g[e + "complete"] = !0, c.S = 1); 0 < c.p && (g[e + "milestone"] = c.p); 0 < c.q && (g[e + "offsetMilestone"] = c.q); if (k) for (h in g) Object.prototype[h] || (k += ",contextData." + h); l = g["a.contentType"]; d.pe = a; d.pev3 = l; var q, s; if (b.contextDataMapping) for (h in d.events2 || (d.events2 = ""), k && (k += ",events"), b.contextDataMapping) if (!Object.prototype[h]) {
                    a = h.length > e.length && h.substring(0, e.length) == e ? h.substring(e.length) : ""; l = b.contextDataMapping[h]; if ("string" == typeof l) for (q = l.split(","), s = 0; s < q.length; s++) l =
                        q[s], "a.contentType" == h ? (k && (k += "," + l), d[l] = g[h]) : "view" == a || "segmentView" == a || "clicked" == a || "complete" == a || "timePlayed" == a || "CPM" == a ? (f && (f += "," + l), "timePlayed" == a || "CPM" == a ? g[h] && (d.events2 += (d.events2 ? "," : "") + l + "=" + g[h]) : g[h] && (d.events2 += (d.events2 ? "," : "") + l)) : "segment" == a && g[h + "Num"] ? (k && (k += "," + l), d[l] = g[h + "Num"] + ":" + g[h]) : (k && (k += "," + l), d[l] = g[h]); else if ("milestones" == a || "offsetMilestones" == a) h = h.substring(0, h.length - 1), g[h] && b.contextDataMapping[h + "s"][g[h]] && (f && (f += "," + b.contextDataMapping[h +
                            "s"][g[h]]), d.events2 += (d.events2 ? "," : "") + b.contextDataMapping[h + "s"][g[h]]); g[h] && (g[h] = 0); "segment" == a && g[h + "Num"] && (g[h + "Num"] = 0)
                } d.linkTrackVars = k; d.linkTrackEvents = f
    }; b.i = function (d, c, e, k, f) {
        var a = {}, l = (new Date).getTime() / 1E3, g, h, q = b.trackVars, s = b.trackEvents, t = b.trackSeconds, u = b.trackMilestones, v = b.trackOffsetMilestones, w = b.segmentByMilestones, x = b.segmentByOffsetMilestones, p, n, r = 1, m = {}, y; b.channel || (b.channel = b.s.w.location.hostname); if (a = d && b.list && b.list[d] ? b.list[d] : 0) if (a.l && (t = b.adTrackSeconds,
            u = b.adTrackMilestones, v = b.adTrackOffsetMilestones, w = b.adSegmentByMilestones, x = b.adSegmentByOffsetMilestones), 0 > e && (e = 1 == a.k && 0 < a.u ? l - a.u + a.c : a.c), 0 < a.length && (e = e < a.length ? e : a.length), 0 > e && (e = 0), a.offset = e, 0 < a.length && (a.e = a.offset / a.length * 100, a.e = 100 < a.e ? 100 : a.e), 0 > a.c && (a.c = e), y = a.D, m.name = d, m.ad = a.l, m.length = a.length, m.openTime = new Date, m.openTime.setTime(1E3 * a.timestamp), m.offset = a.offset, m.percent = a.e, m.playerName = a.playerName, m.mediaEvent = 0 > a.g ? "OPEN" : 1 == c ? "PLAY" : 2 == c ? "STOP" : 3 == c ? "MONITOR" :
                4 == c ? "TRACK" : 5 == c ? "COMPLETE" : 7 == c ? "CLICK" : "CLOSE", 2 < c || c != a.k && (2 != c || 1 == a.k)) {
            f || (k = a.m, f = a.f); if (c) {
                1 == c && (a.c = e); if ((3 >= c || 5 <= c) && 0 <= a.g && (r = !1, q = s = "None", a.g != e)) {
                    h = a.g; h > e && (h = a.c, h > e && (h = e)); p = u ? u.split(",") : 0; if (0 < a.length && p && e >= h) for (n = 0; n < p.length; n++) (g = p[n] ? parseFloat("" + p[n]) : 0) && h / a.length * 100 < g && a.e >= g && (r = !0, n = p.length, m.mediaEvent = "MILESTONE", a.p = m.milestone = g); if ((p = v ? v.split(",") : 0) && e >= h) for (n = 0; n < p.length; n++) (g = p[n] ? parseFloat("" + p[n]) : 0) && h < g && e >= g && (r = !0, n = p.length, m.mediaEvent =
                        "OFFSET_MILESTONE", a.q = m.offsetMilestone = g)
                } if (a.L || !f) { if (w && u && 0 < a.length) { if (p = u.split(",")) for (p.push("100"), n = h = 0; n < p.length; n++) if (g = p[n] ? parseFloat("" + p[n]) : 0) a.e < g && (k = n + 1, f = "M:" + h + "-" + g, n = p.length), h = g } else if (x && v && (p = v.split(","))) for (p.push("" + (0 < a.length ? a.length : "E")), n = h = 0; n < p.length; n++) if ((g = p[n] ? parseFloat("" + p[n]) : 0) || "E" == p[n]) { if (e < g || "E" == p[n]) k = n + 1, f = "O:" + h + "-" + g, n = p.length; h = g } f && (a.L = !0) } (f || a.f) && f != a.f && (a.F = !0, a.f || (a.m = k, a.f = f), 0 <= a.g && (r = !0)); (2 <= c || 100 <= a.e) && a.c < e &&
                    (a.C += e - a.c, a.a += e - a.c); if (2 >= c || 3 == c && !a.k) a.n += (1 == c || 3 == c ? "S" : "E") + Math.floor(e), a.k = 3 == c ? 1 : c; !r && 0 <= a.g && 3 >= c && (t = t ? t : 0) && a.a >= t && (r = !0, m.mediaEvent = "SECONDS"); a.u = l; a.c = e
            } if (!c || 3 >= c && 100 <= a.e) 2 != a.k && (a.n += "E" + Math.floor(e)), c = 0, q = s = "None", m.mediaEvent = "CLOSE"; 7 == c && (r = m.clicked = a.r = !0); if (5 == c || b.completeByCloseOffset && (!c || 100 <= a.e) && 0 < a.length && e >= a.length - b.completeCloseOffsetThreshold) r = m.complete = a.complete = !0; l = m.mediaEvent; "MILESTONE" == l ? l += "_" + m.milestone : "OFFSET_MILESTONE" == l && (l +=
                "_" + m.offsetMilestone); a.I[l] ? m.eventFirstTime = !1 : (m.eventFirstTime = !0, a.I[l] = 1); m.event = m.mediaEvent; m.timePlayed = a.C; m.segmentNum = a.m; m.segment = a.f; m.segmentLength = a.B; b.monitor && 4 != c && b.monitor(b.s, m); b.Heartbeat && b.Heartbeat.enabled && 0 <= a.g && (r = !1); 0 == c && b.M(d); r && a.D == y && (d = { contextData: {} }, d.linkTrackVars = q, d.linkTrackEvents = s, d.linkTrackVars || (d.linkTrackVars = ""), d.linkTrackEvents || (d.linkTrackEvents = ""), b.P(d, a), d.linkTrackVars || (d["!linkTrackVars"] = 1), d.linkTrackEvents || (d["!linkTrackEvents"] =
                    1), b.s.track(d), a.F ? (a.m = k, a.f = f, a.A = !0, a.F = !1) : 0 < a.a && (a.A = !1), a.n = "", a.p = a.q = 0, a.a -= Math.floor(a.a), a.g = e, a.D++)
        } return a
    }; b.O = function (d, c, e, k, f) { var a = 0; if (d && (!b.autoTrackMediaLengthRequired || c && 0 < c)) { if (b.list && b.list[d]) a = 1; else if (1 == e || 3 == e) b.open(d, c, "HTML5 Video", f), a = 1; a && b.i(d, e, k, -1, 0) } }; b.attach = function (d) {
        var c, e, k; d && d.tagName && "VIDEO" == d.tagName.toUpperCase() && (b.o || (b.o = function (c, a, d) {
            var e, h; b.autoTrack && (e = c.currentSrc, (h = c.duration) || (h = -1), 0 > d && (d = c.currentTime), b.O(e, h, a,
                d, c))
        }), c = function () { b.o(d, 1, -1) }, e = function () { b.o(d, 1, -1) }, b.j(d, "play", c), b.j(d, "pause", e), b.j(d, "seeking", e), b.j(d, "seeked", c), b.j(d, "ended", function () { b.o(d, 0, -1) }), b.j(d, "timeupdate", c), k = function () { d.paused || d.ended || d.seeking || b.o(d, 3, -1); setTimeout(k, 1E3) }, k())
    }; b.j = function (b, c, e) { b.attachEvent ? b.attachEvent("on" + c, e) : b.addEventListener && b.addEventListener(c, e, !1) }; void 0 == b.completeByCloseOffset && (b.completeByCloseOffset = 1); void 0 == b.completeCloseOffsetThreshold && (b.completeCloseOffsetThreshold =
        1); b.Heartbeat = {}; b.N = function () { var d, c; if (b.autoTrack && (d = b.s.d.getElementsByTagName("VIDEO"))) for (c = 0; c < d.length; c++) b.attach(d[c]) }; b.j(q, "load", b.N)
}


/* 1.1.0.35 - Module: Integrate - Required for Genesis Integrations */
function AppMeasurement_Module_Integrate(s) {
    var m = this; m.s = s; var w = window; if (!w.s_c_in) w.s_c_il = [], w.s_c_in = 0; m._il = w.s_c_il; m._in = w.s_c_in; m._il[m._in] = m; w.s_c_in++; m._c = "s_m"; m.list = []; m.add = function (c, b) {
        var a; b || (b = "s_Integrate_" + c); w[b] || (w[b] = {}); a = m[c] = w[b]; a.a = c; a.e = m; a._c = 0; a._d = 0; a.disable == void 0 && (a.disable = 0); a.get = function (b, c) {
            var d = document, f = d.getElementsByTagName("HEAD"), g; if (!a.disable && (c || (v = "s_" + m._in + "_Integrate_" + a.a + "_get_" + a._c), a._c++ , a.VAR = v, a.CALLBACK = "s_c_il[" + m._in +
                "]." + a.a + ".callback", a.delay(), f = f && f.length > 0 ? f[0] : d.body)) try { g = d.createElement("SCRIPT"); g.type = "text/javascript"; g.setAttribute("async", "async"); g.src = m.c(a, b); if (b.indexOf("[CALLBACK]") < 0) g.onload = g.onreadystatechange = function () { a.callback(w[v]) }; f.firstChild ? f.insertBefore(g, f.firstChild) : f.appendChild(g) } catch (s) { }
        }; a.callback = function (b) { var m; if (b) for (m in b) Object.prototype[m] || (a[m] = b[m]); a.ready() }; a.beacon = function (b) {
            var c = "s_i_" + m._in + "_Integrate_" + a.a + "_" + a._c; if (!a.disable) a._c++ ,
                c = w[c] = new Image, c.src = m.c(a, b)
        }; a.script = function (b) { a.get(b, 1) }; a.delay = function () { a._d++ }; a.ready = function () { a._d--; a.disable || s.delayReady() }; m.list.push(c)
    }; m._g = function (c) { var b, a = (c ? "use" : "set") + "Vars"; for (c = 0; c < m.list.length; c++) if ((b = m[m.list[c]]) && !b.disable && b[a]) try { b[a](s, b) } catch (w) { } }; m._t = function () { m._g(1) }; m._d = function () { var c, b; for (c = 0; c < m.list.length; c++) if ((b = m[m.list[c]]) && !b.disable && b._d > 0) return 1; return 0 }; m.c = function (m, b) {
        var a, w, e, d; b.toLowerCase().substring(0, 4) != "http" &&
            (b = "http://" + b); s.ssl && (b = s.replace(b, "http:", "https:")); m.RAND = Math.floor(Math.random() * 1E13); for (a = 0; a >= 0;) a = b.indexOf("[", a), a >= 0 && (w = b.indexOf("]", a), w > a && (e = b.substring(a + 1, w), e.length > 2 && e.substring(0, 2) == "s." ? (d = s[e.substring(2)]) || (d = "") : (d = "" + m[e], d != m[e] && parseFloat(d) != m[e] && (e = 0)), e && (b = b.substring(0, a) + encodeURIComponent(d) + b.substring(w + 1)), a = w)); return b
    }
}

/*
 * Plugin: getPercentPageViewed 2.0 (Minified)
 */
s.handlePPVevents = function () {
    if (!s_c_il) return; for (var i = 0, scill = s_c_il.length; i < scill; i++) if (typeof s_c_il[i] != "undefined" && s_c_il[i]._c && s_c_il[i]._c == "s_c") { var s = s_c_il[i]; break } if (!s) return; if (!s.getPPVid) return; var dh = Math.max(Math.max(s.d.body.scrollHeight, s.d.documentElement.scrollHeight), Math.max(s.d.body.offsetHeight, s.d.documentElement.offsetHeight), Math.max(s.d.body.clientHeight, s.d.documentElement.clientHeight)), vph = window.innerHeight || (s.d.documentElement.clientHeight || s.d.body.clientHeight),
        st = window.pageYOffset || (window.document.documentElement.scrollTop || window.document.body.scrollTop), vh = st + vph, pv = Math.min(Math.round(vh / dh * 100), 100), c = ""; if (!s.c_r("tp") || decodeURIComponent(s.c_r("s_ppv").split(",")[0]) != s.getPPVid || s.ppvChange == "1" && (s.c_r("tp") && dh != s.c_r("tp"))) { s.c_w("tp", dh); s.c_w("s_ppv", "") } else c = s.c_r("s_ppv"); var a = c && c.indexOf(",") > -1 ? c.split(",", 4) : [], id = a.length > 0 ? a[0] : escape(s.getPPVid), cv = a.length > 1 ? parseInt(a[1]) : 0, p0 = a.length > 2 ? parseInt(a[2]) : pv, cy = a.length > 3 ? parseInt(a[3]) :
            0, cn = pv > 0 ? id + "," + (pv > cv ? pv : cv) + "," + p0 + "," + (vh > cy ? vh : cy) : ""; s.c_w("s_ppv", cn)
};
s.getPercentPageViewed = function (pid, change) {
    var s = this, ist = !s.getPPVid ? true : false; pid = pid ? pid : s.pageName ? s.pageName : document.location.href; s.ppvChange = change ? change : "1"; if (typeof s.linkType != "undefined" && s.linkType != "0" && s.linkType != "" && s.linkType != "e") return ""; var v = s.c_r("s_ppv"), a = v.indexOf(",") > -1 ? v.split(",", 4) : []; if (a && a.length < 4) { for (var i = 3; i > 0; i--) a[i] = i < a.length ? a[i - 1] : ""; a[0] = "" } if (a) a[0] = unescape(a[0]); if (!s.getPPVid || s.getPPVid != pid) { s.getPPVid = pid; s.c_w("s_ppv", escape(s.getPPVid)); s.handlePPVevents() } if (ist) if (window.addEventListener) {
        window.addEventListener("load",
            s.handlePPVevents, false); window.addEventListener("click", s.handlePPVevents, false); window.addEventListener("scroll", s.handlePPVevents, false); window.addEventListener("resize", s.handlePPVevents, false)
    } else if (window.attachEvent) { window.attachEvent("onload", s.handlePPVevents); window.attachEvent("onclick", s.handlePPVevents); window.attachEvent("onscroll", s.handlePPVevents); window.attachEvent("onresize", s.handlePPVevents) } return pid != "-" ? a : a[1]
};

/****************************** MODULES *****************************/

s.loadModule("AudienceManagement");

function AppMeasurement_Module_AudienceManagement(d) {
    var a = this; a.s = d; var b = window; b.s_c_in || (b.s_c_il = [], b.s_c_in = 0); a._il = b.s_c_il; a._in = b.s_c_in; a._il[a._in] = a; b.s_c_in++; a._c = "s_m"; a.setup = function (c) { b.DIL && c && (c.disableDefaultRequest = !0, c.disableScriptAttachment = !0, a.instance = b.DIL.create(c), a.tools = b.DIL.tools) }; a.isReady = function () { return a.instance ? !0 : !1 }; a.getEventCallConfigParams = function () {
        return a.instance && a.instance.api && a.instance.api.getEventCallConfigParams ? a.instance.api.getEventCallConfigParams() :
            {}
    }; a.passData = function (b) { a.instance && a.instance.api && a.instance.api.passData && a.instance.api.passData(b) }
}
"function" !== typeof window.DIL && (window.DIL = function (a, c) {
    var d = [], b, g; a !== Object(a) && (a = {}); var f, k, n, u, s, m, p, y, x, J, K, D; f = a.partner; k = a.containerNSID; n = !!a.disableDestinationPublishingIframe; u = a.iframeAkamaiHTTPS; s = a.mappings; m = a.uuidCookie; p = !0 === a.enableErrorReporting; y = a.visitorService; x = a.declaredId; J = !0 === a.removeFinishedScriptsAndCallbacks; K = !0 === a.delayAllUntilWindowLoad; D = !0 === a.disableIDSyncs; var L, M, N, G, E, O, P, Q; L = !0 === a.disableScriptAttachment; M = !0 === a.disableCORSFiring; N = !0 === a.disableDefaultRequest;
    G = a.afterResultForDefaultRequest; E = a.dpIframeSrc; O = !0 === a.testCORS; P = !0 === a.useJSONPOnly; Q = a.visitorConstructor; p && DIL.errorModule.activate(); var R = !0 === window._dil_unit_tests; (b = c) && d.push(b + ""); if (!f || "string" !== typeof f) return b = "DIL partner is invalid or not specified in initConfig", DIL.errorModule.handleError({ name: "error", message: b, filename: "dil.js" }), Error(b); b = "DIL containerNSID is invalid or not specified in initConfig, setting to default of 0"; if (k || "number" === typeof k) k = parseInt(k, 10),
        !isNaN(k) && 0 <= k && (b = ""); b && (k = 0, d.push(b), b = ""); g = DIL.getDil(f, k); if (g instanceof DIL && g.api.getPartner() === f && g.api.getContainerNSID() === k) return g; if (this instanceof DIL) DIL.registerDil(this, f, k); else return new DIL(a, "DIL was not instantiated with the 'new' operator, returning a valid instance with partner = " + f + " and containerNSID = " + k); var B = { IS_HTTPS: "https:" === document.location.protocol, POST_MESSAGE_ENABLED: !!window.postMessage, COOKIE_MAX_EXPIRATION_DATE: "Tue, 19 Jan 2038 03:14:07 UTC" }, H =
            { stuffed: {} }, l = {}, q = {
                firingQueue: [], fired: [], firing: !1, sent: [], errored: [], reservedKeys: { sids: !0, pdata: !0, logdata: !0, callback: !0, postCallbackFn: !0, useImageRequest: !0 }, callbackPrefix: "demdexRequestCallback", firstRequestHasFired: !1, useJSONP: !0, abortRequests: !1, num_of_jsonp_responses: 0, num_of_jsonp_errors: 0, num_of_cors_responses: 0, num_of_cors_errors: 0, corsErrorSources: [], num_of_img_responses: 0, num_of_img_errors: 0, toRemove: [], removed: [], readyToRemove: !1, platformParams: {
                    d_nsid: k + "", d_rtbd: "json", d_jsonv: DIL.jsonVersion +
                    "", d_dst: "1"
                }, nonModStatsParams: { d_rtbd: !0, d_dst: !0, d_cts: !0, d_rs: !0 }, modStatsParams: null, adms: {
                    TIME_TO_CATCH_ALL_REQUESTS_RELEASE: 2E3, calledBack: !1, mid: null, noVisitorAPI: !1, instance: null, releaseType: "no VisitorAPI", admsProcessingStarted: !1, process: function (e) {
                        try {
                            if (!this.admsProcessingStarted) {
                                this.admsProcessingStarted = !0; var t = this, a, h, b, d, c; if ("function" === typeof e && "function" === typeof e.getInstance) {
                                    if (y === Object(y) && (a = y.namespace) && "string" === typeof a) h = e.getInstance(a, { idSyncContainerID: k });
                                    else { this.releaseType = "no namespace"; this.releaseRequests(); return } if (h === Object(h) && "function" === typeof h.isAllowed && "function" === typeof h.getMarketingCloudVisitorID && "function" === typeof h.getCustomerIDs) {
                                        if (!h.isAllowed()) { this.releaseType = "VisitorAPI not allowed"; this.releaseRequests(); return } this.instance = h; b = function (e) { "VisitorAPI" !== t.releaseType && (t.mid = e, t.releaseType = "VisitorAPI", t.releaseRequests()) }; R && (d = y.server) && "string" === typeof d && (h.server = d); c = h.getMarketingCloudVisitorID(b); if ("string" ===
                                            typeof c && c.length) { b(c); return } setTimeout(function () { "VisitorAPI" !== t.releaseType && (t.releaseType = "timeout", t.releaseRequests()) }, this.TIME_TO_CATCH_ALL_REQUESTS_RELEASE); return
                                    } this.releaseType = "invalid instance"
                                } else this.noVisitorAPI = !0; this.releaseRequests()
                            }
                        } catch (f) { this.releaseRequests() }
                    }, releaseRequests: function () { this.calledBack = !0; q.registerRequest() }, getMarketingCloudVisitorID: function () { return this.instance ? this.instance.getMarketingCloudVisitorID() : null }, getMIDQueryString: function () {
                        var e =
                            w.isPopulatedString, t = this.getMarketingCloudVisitorID(); e(this.mid) && this.mid === t || (this.mid = t); return e(this.mid) ? "d_mid=" + this.mid + "&" : ""
                    }, getCustomerIDs: function () { return this.instance ? this.instance.getCustomerIDs() : null }, getCustomerIDsQueryString: function (e) { if (e === Object(e)) { var t = "", a = [], h = [], b, d; for (b in e) e.hasOwnProperty(b) && (h[0] = b, d = e[b], d === Object(d) && (h[1] = d.id || "", h[2] = d.authState || 0, a.push(h), h = [])); if (h = a.length) for (e = 0; e < h; e++) t += "&d_cid_ic=" + a[e].join("%01"); return t } return "" }
                },
                declaredId: {
                    declaredId: { init: null, request: null }, declaredIdCombos: {}, setDeclaredId: function (e, t) { var a = w.isPopulatedString, h = encodeURIComponent; if (e === Object(e) && a(t)) { var b = e.dpid, d = e.dpuuid, c = null; if (a(b) && a(d)) { c = h(b) + "$" + h(d); if (!0 === this.declaredIdCombos[c]) return "setDeclaredId: combo exists for type '" + t + "'"; this.declaredIdCombos[c] = !0; this.declaredId[t] = { dpid: b, dpuuid: d }; return "setDeclaredId: succeeded for type '" + t + "'" } } return "setDeclaredId: failed for type '" + t + "'" }, getDeclaredIdQueryString: function () {
                        var e =
                            this.declaredId.request, t = this.declaredId.init, a = ""; null !== e ? a = "&d_dpid=" + e.dpid + "&d_dpuuid=" + e.dpuuid : null !== t && (a = "&d_dpid=" + t.dpid + "&d_dpuuid=" + t.dpuuid); return a
                    }
                }, registerRequest: function (e) {
                    var a = this.firingQueue; e === Object(e) && a.push(e); this.firing || !a.length || K && !DIL.windowLoaded || !this.adms.calledBack || (e = a.shift(), e.src = e.src.replace(/demdex.net\/event\?d_nsid=/, "demdex.net/event?" + this.adms.getMIDQueryString() + "d_nsid="), w.isPopulatedString(e.corsPostData) && (e.corsPostData = e.corsPostData.replace(/^d_nsid=/,
                        this.adms.getMIDQueryString() + "d_nsid=")), C.fireRequest(e), this.firstRequestHasFired || "script" !== e.tag && "cors" !== e.tag || (this.firstRequestHasFired = !0))
                }, processVisitorAPI: function () { this.adms.process(Q || window.Visitor) }, requestRemoval: function (e) {
                    if (!J) return "removeFinishedScriptsAndCallbacks is not boolean true"; var a = this.toRemove, r, h; e === Object(e) && (r = e.script, h = e.callbackName, (r === Object(r) && "SCRIPT" === r.nodeName || "no script created" === r) && "string" === typeof h && h.length && a.push(e)); if (this.readyToRemove &&
                        a.length) { h = a.shift(); r = h.script; h = h.callbackName; "no script created" !== r ? (e = r.src, r.parentNode.removeChild(r)) : e = r; window[h] = null; try { delete window[h] } catch (b) { } this.removed.push({ scriptSrc: e, callbackName: h }); DIL.variables.scriptsRemoved.push(e); DIL.variables.callbacksRemoved.push(h); return this.requestRemoval() } return "requestRemoval() processed"
                }
            }; g = function () {
                var e = "http://fast.", a = "?d_nsid=" + k + "#" + encodeURIComponent(document.location.href); if ("string" === typeof E && E.length) return E + a; B.IS_HTTPS &&
                    (e = !0 === u ? "https://fast." : "https://"); return e + f + ".demdex.net/dest5.html" + a
            }; var z = {
                THROTTLE_START: 3E4, throttleTimerSet: !1, id: "destination_publishing_iframe_" + f + "_" + k, url: g(), iframe: null, iframeHasLoaded: !1, sendingMessages: !1, messages: [], messagesPosted: [], messageSendingInterval: B.POST_MESSAGE_ENABLED ? 15 : 100, ibsDeleted: [], jsonProcessed: [], newIframeCreated: null, iframeIdChanged: !1, originalIframeHasLoadedAlready: null, attachIframe: function () {
                    function e() {
                        h = document.createElement("iframe"); h.id = b.id; h.style.cssText =
                            "display: none; width: 0; height: 0;"; h.src = b.url; b.newIframeCreated = !0; a(); document.body.appendChild(h)
                    } function a() { v.addListener(h, "load", function () { h.className = "aamIframeLoaded"; b.iframeHasLoaded = !0; b.requestToProcess() }) } var b = this, h = document.getElementById(this.id); h ? "IFRAME" !== h.nodeName ? (this.id += "_2", this.iframeIdChanged = !0, e()) : (this.newIframeCreated = !1, "aamIframeLoaded" !== h.className ? (this.originalIframeHasLoadedAlready = !1, a()) : (this.iframeHasLoaded = this.originalIframeHasLoadedAlready =
                        !0, this.requestToProcess())) : e(); this.iframe = h
                }, requestToProcess: function (e, a) { var b = this; e && !w.isEmptyObject(e) && this.process(e, a); this.iframeHasLoaded && this.messages.length && !this.sendingMessages && (this.throttleTimerSet || (this.throttleTimerSet = !0, setTimeout(function () { b.messageSendingInterval = B.POST_MESSAGE_ENABLED ? 15 : 150 }, this.THROTTLE_START)), this.sendingMessages = !0, this.sendMessages()) }, process: function (e, a) {
                    var b = encodeURIComponent, h, d, c, f, g, k; a === Object(a) && (k = v.encodeAndBuildRequest(["", a.dpid ||
                        "", a.dpuuid || ""], ",")); if ((h = e.dests) && h instanceof Array && (d = h.length)) for (c = 0; c < d; c++) f = h[c], f = [b("dests"), b(f.id || ""), b(f.y || ""), b(f.c || "")], this.addMessage(f.join("|")); if ((h = e.ibs) && h instanceof Array && (d = h.length)) for (c = 0; c < d; c++) f = h[c], f = [b("ibs"), b(f.id || ""), b(f.tag || ""), v.encodeAndBuildRequest(f.url || [], ","), b(f.ttl || ""), "", k], this.addMessage(f.join("|")); if ((h = e.dpcalls) && h instanceof Array && (d = h.length)) for (c = 0; c < d; c++) f = h[c], g = f.callback || {}, g = [g.obj || "", g.fn || "", g.key || "", g.tag || "", g.url ||
                            ""], f = [b("dpm"), b(f.id || ""), b(f.tag || ""), v.encodeAndBuildRequest(f.url || [], ","), b(f.ttl || ""), v.encodeAndBuildRequest(g, ","), k], this.addMessage(f.join("|")); this.jsonProcessed.push(e)
                }, addMessage: function (e) { var a = encodeURIComponent, a = p ? a("---destpub-debug---") : a("---destpub---"); this.messages.push(a + e) }, sendMessages: function () {
                    var e = this, a; (this.messages.length && this.iframe) ? (a = this.messages.shift(), DIL.xd.postMessage(a, this.url, this.iframe.contentWindow), this.messagesPosted.push(a), setTimeout(function () { e.sendMessages() },
                        this.messageSendingInterval)) : this.sendingMessages = !1
                }
            }, I = {
                traits: function (e) { w.isValidPdata(e) && (l.sids instanceof Array || (l.sids = []), v.extendArray(l.sids, e)); return this }, pixels: function (e) { w.isValidPdata(e) && (l.pdata instanceof Array || (l.pdata = []), v.extendArray(l.pdata, e)); return this }, logs: function (e) { w.isValidLogdata(e) && (l.logdata !== Object(l.logdata) && (l.logdata = {}), v.extendObject(l.logdata, e)); return this }, customQueryParams: function (e) {
                    w.isEmptyObject(e) || v.extendObject(l, e, q.reservedKeys);
                    return this
                }, signals: function (e, a) { var b, h = e; if (!w.isEmptyObject(h)) { if (a && "string" === typeof a) for (b in h = {}, e) e.hasOwnProperty(b) && (h[a + b] = e[b]); v.extendObject(l, h, q.reservedKeys) } return this }, declaredId: function (e) { q.declaredId.setDeclaredId(e, "request"); return this }, result: function (e) { "function" === typeof e && (l.callback = e); return this }, afterResult: function (e) { "function" === typeof e && (l.postCallbackFn = e); return this }, useImageRequest: function () { l.useImageRequest = !0; return this }, clearData: function () {
                    l =
                        {}; return this
                }, submit: function () { C.submitRequest(l); l = {}; return this }, getPartner: function () { return f }, getContainerNSID: function () { return k }, getEventLog: function () { return d }, getState: function () { var e = {}, a = {}; v.extendObject(e, q, { callbackPrefix: !0, useJSONP: !0, registerRequest: !0 }); v.extendObject(a, z, { attachIframe: !0, requestToProcess: !0, process: !0, sendMessages: !0 }); return { pendingRequest: l, otherRequestInfo: e, destinationPublishingInfo: a } }, idSync: function (e) {
                    if (D) return "Error: id syncs have been disabled";
                    if (e !== Object(e) || "string" !== typeof e.dpid || !e.dpid.length) return "Error: config or config.dpid is empty"; if ("string" !== typeof e.url || !e.url.length) return "Error: config.url is empty"; var a = e.url, b = e.minutesToLive, h = encodeURIComponent, d, a = a.replace(/^https:/, "").replace(/^http:/, ""); if ("undefined" === typeof b) b = 20160; else if (b = parseInt(b, 10), isNaN(b) || 0 >= b) return "Error: config.minutesToLive needs to be a positive number"; d = v.encodeAndBuildRequest(["", e.dpid, e.dpuuid || ""], ","); e = ["ibs", h(e.dpid), "img",
                        h(a), b, "", d]; z.addMessage(e.join("|")); q.firstRequestHasFired && z.requestToProcess(); return "Successfully queued"
                }, aamIdSync: function (e) { if (D) return "Error: id syncs have been disabled"; if (e !== Object(e) || "string" !== typeof e.dpuuid || !e.dpuuid.length) return "Error: config or config.dpuuid is empty"; e.url = "//dpm.demdex.net/ibs:dpid=" + e.dpid + "&dpuuid=" + e.dpuuid; return this.idSync(e) }, passData: function (e) {
                    if (w.isEmptyObject(e)) return "Error: json is empty or not an object"; z.ibsDeleted.push(e.ibs); delete e.ibs;
                    C.defaultCallback(e); return e
                }, getPlatformParams: function () { return q.platformParams }, getEventCallConfigParams: function () { var e = q, a = e.modStatsParams, b = e.platformParams, h; if (!a) { a = {}; for (h in b) b.hasOwnProperty(h) && !e.nonModStatsParams[h] && (a[h.replace(/^d_/, "")] = b[h]); e.modStatsParams = a } return a }
            }, C = {
                corsMetadata: function () {
                    var e = "none", a = !0; "undefined" !== typeof XMLHttpRequest && XMLHttpRequest === Object(XMLHttpRequest) && ("withCredentials" in new XMLHttpRequest ? e = "XMLHttpRequest" : (new Function("/*@cc_on return /^10/.test(@_jscript_version) @*/"))() ?
                        e = "XMLHttpRequest" : "undefined" !== typeof XDomainRequest && XDomainRequest === Object(XDomainRequest) && (a = !1), 0 < Object.prototype.toString.call(window.HTMLElement).indexOf("Constructor") && (a = !1)); return { corsType: e, corsCookiesEnabled: a }
                }(), getCORSInstance: function () { return "none" === this.corsMetadata.corsType ? null : new window[this.corsMetadata.corsType] }, submitRequest: function (e) { q.registerRequest(C.createQueuedRequest(e)); return !0 }, createQueuedRequest: function (e) {
                    var a = q, b, h = e.callback, d = "img", c; if (!w.isEmptyObject(s)) {
                        var f,
                            g, m; for (f in s) s.hasOwnProperty(f) && (g = s[f], null != g && "" !== g && f in e && !(g in e || g in q.reservedKeys) && (m = e[f], null != m && "" !== m && (e[g] = m)))
                    } w.isValidPdata(e.sids) || (e.sids = []); w.isValidPdata(e.pdata) || (e.pdata = []); w.isValidLogdata(e.logdata) || (e.logdata = {}); e.logdataArray = v.convertObjectToKeyValuePairs(e.logdata, "=", !0); e.logdataArray.push("_ts=" + (new Date).getTime()); "function" !== typeof h && (h = this.defaultCallback); a.useJSONP = !0 !== e.useImageRequest; a.useJSONP && (d = "script", b = a.callbackPrefix + "_" + k + "_" +
                        (new Date).getTime()); a = this.makeRequestSrcData(e, b); !P && (c = this.getCORSInstance()) && a.truncated && (this.corsMetadata.corsCookiesEnabled || a.isDeclaredIdCall) && (d = "cors"); return { tag: d, src: a.src, corsSrc: a.corsSrc, internalCallbackName: b, callbackFn: h, postCallbackFn: e.postCallbackFn, useImageRequest: !!e.useImageRequest, requestData: e, corsInstance: c, corsPostData: a.corsPostData, hasCORSError: !1 }
                }, defaultCallback: function (e, a) {
                    var b, h, d, c, f, g, k, x, p; if ((b = e.stuff) && b instanceof Array && (h = b.length)) for (d = 0; d < h; d++) if ((c =
                        b[d]) && c === Object(c)) { f = c.cn; g = c.cv; k = c.ttl; if ("undefined" === typeof k || "" === k) k = Math.floor(v.getMaxCookieExpiresInMinutes() / 60 / 24); x = c.dmn || "." + document.domain.replace(/^www\./, ""); p = c.type; f && (g || "number" === typeof g) && ("var" !== p && (k = parseInt(k, 10)) && !isNaN(k) && v.setCookie(f, g, 1440 * k, "/", x, !1), H.stuffed[f] = g) } b = e.uuid; w.isPopulatedString(b) && !w.isEmptyObject(m) && (h = m.path, "string" === typeof h && h.length || (h = "/"), d = parseInt(m.days, 10), isNaN(d) && (d = 100), v.setCookie(m.name || "aam_did", b, 1440 * d, h, m.domain ||
                            "." + document.domain.replace(/^www\./, ""), !0 === m.secure)); n || q.abortRequests || z.requestToProcess(e, a)
                }, makeRequestSrcData: function (e, a) {
                    e.sids = w.removeEmptyArrayValues(e.sids || []); e.pdata = w.removeEmptyArrayValues(e.pdata || []); var b = q, d = b.platformParams, c = v.encodeAndBuildRequest(e.sids, ","), g = v.encodeAndBuildRequest(e.pdata, ","), m = (e.logdataArray || []).join("&"); delete e.logdataArray; var x = B.IS_HTTPS ? "https://" : "http://", p = b.declaredId.getDeclaredIdQueryString(), s = b.adms.instance ? b.adms.getCustomerIDsQueryString(b.adms.getCustomerIDs()) :
                        "", A; A = []; var l, n, u, y; for (l in e) if (!(l in b.reservedKeys) && e.hasOwnProperty(l)) if (n = e[l], l = encodeURIComponent(l), n instanceof Array) for (u = 0, y = n.length; u < y; u++) A.push(l + "=" + encodeURIComponent(n[u])); else A.push(l + "=" + encodeURIComponent(n)); A = A.length ? "&" + A.join("&") : ""; l = !1; c = "d_nsid=" + d.d_nsid + p + s + (c.length ? "&d_sid=" + c : "") + (g.length ? "&d_px=" + g : "") + (m.length ? "&d_ld=" + encodeURIComponent(m) : ""); d = "&d_rtbd=" + d.d_rtbd + "&d_jsonv=" + d.d_jsonv + "&d_dst=" + d.d_dst; x = x + f + ".demdex.net/event"; g = b = x + "?" + c + (b.useJSONP ?
                            d + "&d_cb=" + (a || "") : "") + A; 2048 < b.length && (b = b.substring(0, b.lastIndexOf("&")), l = !0); return { corsSrc: x + "?" + (O ? "testcors=1&d_nsid=" + k + "&" : "") + "_ts=" + (new Date).getTime(), src: b, originalSrc: g, truncated: l, corsPostData: c + d + A, isDeclaredIdCall: "" !== p }
                }, fireRequest: function (e) { if ("img" === e.tag) this.fireImage(e); else { var a = q.declaredId, a = a.declaredId.request || a.declaredId.init || {}, a = { dpid: a.dpid || "", dpuuid: a.dpuuid || "" }; "script" === e.tag ? this.fireScript(e, a) : "cors" === e.tag && this.fireCORS(e, a) } }, fireImage: function (e) {
                    var a =
                        q, c, h; a.abortRequests || (a.firing = !0, c = new Image(0, 0), a.sent.push(e), c.onload = function () { a.firing = !1; a.fired.push(e); a.num_of_img_responses++; a.registerRequest() }, h = function (c) { b = "imgAbortOrErrorHandler received the event of type " + c.type; d.push(b); a.abortRequests = !0; a.firing = !1; a.errored.push(e); a.num_of_img_errors++; a.registerRequest() }, c.addEventListener ? (c.addEventListener("error", h, !1), c.addEventListener("abort", h, !1)) : c.attachEvent && (c.attachEvent("onerror", h), c.attachEvent("onabort", h)), c.src =
                            e.src)
                }, fireScript: function (a, c) {
                    var g = this, h = q, k, m, x = a.src, p = a.postCallbackFn, l = "function" === typeof p, s = a.internalCallbackName; h.abortRequests || (h.firing = !0, window[s] = function (g) {
                        try { g !== Object(g) && (g = {}); D && (z.ibsDeleted.push(g.ibs), delete g.ibs); var k = a.callbackFn; h.firing = !1; h.fired.push(a); h.num_of_jsonp_responses++; k(g, c); l && p(g, c) } catch (r) {
                            r.message = "DIL jsonp callback caught error with message " + r.message; b = r.message; d.push(b); r.filename = r.filename || "dil.js"; r.partner = f; DIL.errorModule.handleError(r);
                            try { k({ error: r.name + "|" + r.message }, c), l && p({ error: r.name + "|" + r.message }, c) } catch (x) { }
                        } finally { h.requestRemoval({ script: m, callbackName: s }), h.registerRequest() }
                    }, L ? (h.firing = !1, h.requestRemoval({ script: "no script created", callbackName: s })) : (m = document.createElement("script"), m.addEventListener && m.addEventListener("error", function (c) { h.requestRemoval({ script: m, callbackName: s }); b = "jsonp script tag error listener received the event of type " + c.type + " with src " + x; g.handleScriptError(b, a) }, !1), m.type = "text/javascript",
                        m.src = x, k = DIL.variables.scriptNodeList[0], k.parentNode.insertBefore(m, k)), h.sent.push(a), h.declaredId.declaredId.request = null)
                }, fireCORS: function (a, c) {
                    function g(r) {
                        var m; try { if (m = JSON.parse(r), m !== Object(m)) { h.handleCORSError(a, c, "Response is not JSON"); return } } catch (p) { h.handleCORSError(a, c, "Error parsing response as JSON"); return } try { var x = a.callbackFn; k.firing = !1; k.fired.push(a); k.num_of_cors_responses++; x(m, c); n && s(m, c) } catch (l) {
                            l.message = "DIL handleCORSResponse caught error with message " + l.message;
                            b = l.message; d.push(b); l.filename = l.filename || "dil.js"; l.partner = f; DIL.errorModule.handleError(l); try { x({ error: l.name + "|" + l.message }, c), n && s({ error: l.name + "|" + l.message }, c) } catch (q) { }
                        } finally { k.registerRequest() }
                    } var h = this, k = q, m = this.corsMetadata.corsType, x = a.corsSrc, p = a.corsInstance, l = a.corsPostData, s = a.postCallbackFn, n = "function" === typeof s; if (!k.abortRequests) {
                        k.firing = !0; if (M) k.firing = !1; else try {
                            p.open("post", x, !0), "XMLHttpRequest" === m ? (p.withCredentials = !0, p.setRequestHeader("Content-Type",
                                "application/x-www-form-urlencoded"), p.onreadystatechange = function () { 4 === this.readyState && (200 === this.status ? g(this.responseText) : h.handleCORSError(a, c, "onreadystatechange")) }) : "XDomainRequest" === m && (p.onload = function () { g(this.responseText) }), p.onerror = function () { h.handleCORSError(a, c, "onerror") }, p.ontimeout = function () { h.handleCORSError(a, c, "ontimeout") }, p.send(l)
                        } catch (u) { this.handleCORSError(a, c, "try-catch") } k.sent.push(a); k.declaredId.declaredId.request = null
                    }
                }, handleCORSError: function (a, b, c) {
                    a.hasCORSError ||
                        (a.hasCORSError = !0, q.num_of_cors_errors++ , q.corsErrorSources.push(c), a.tag = "script", this.fireScript(a, b))
                }, handleScriptError: function (a, b) { q.num_of_jsonp_errors++; this.handleRequestError(a, b) }, handleRequestError: function (a, b) { var c = q; d.push(a); c.abortRequests = !0; c.firing = !1; c.errored.push(b); c.registerRequest() }
            }, w = {
                isValidPdata: function (a) { return a instanceof Array && this.removeEmptyArrayValues(a).length ? !0 : !1 }, isValidLogdata: function (a) { return !this.isEmptyObject(a) }, isEmptyObject: function (a) {
                    if (a !==
                        Object(a)) return !0; for (var b in a) if (a.hasOwnProperty(b)) return !1; return !0
                }, removeEmptyArrayValues: function (a) { for (var b = 0, c = a.length, d, f = [], b = 0; b < c; b++) d = a[b], "undefined" !== typeof d && null !== d && "" !== d && f.push(d); return f }, isPopulatedString: function (a) { return "string" === typeof a && a.length }
            }, v = {
                addListener: function () {
                    if (document.addEventListener) return function (a, b, c) { a.addEventListener(b, function (a) { "function" === typeof c && c(a) }, !1) }; if (document.attachEvent) return function (a, b, c) {
                        a.attachEvent("on" +
                            b, function (a) { "function" === typeof c && c(a) })
                    }
                }(), convertObjectToKeyValuePairs: function (a, b, c) { var d = [], f, g; b || (b = "="); for (f in a) a.hasOwnProperty(f) && (g = a[f], "undefined" !== typeof g && null !== g && "" !== g && d.push(f + b + (c ? encodeURIComponent(g) : g))); return d }, encodeAndBuildRequest: function (a, b) { return this.map(a, function (a) { return encodeURIComponent(a) }).join(b) }, map: function (a, b) {
                    if (Array.prototype.map) return a.map(b); if (void 0 === a || null === a) throw new TypeError; var c = Object(a), d = c.length >>> 0; if ("function" !==
                        typeof b) throw new TypeError; for (var f = Array(d), g = 0; g < d; g++) g in c && (f[g] = b.call(b, c[g], g, c)); return f
                }, filter: function (a, b) { if (!Array.prototype.filter) { if (void 0 === a || null === a) throw new TypeError; var c = Object(a), d = c.length >>> 0; if ("function" !== typeof b) throw new TypeError; for (var g = [], f = 0; f < d; f++) if (f in c) { var k = c[f]; b.call(b, k, f, c) && g.push(k) } return g } return a.filter(b) }, getCookie: function (a) {
                    a += "="; var b = document.cookie.split(";"), c, d, f; c = 0; for (d = b.length; c < d; c++) {
                        for (f = b[c]; " " === f.charAt(0);) f =
                            f.substring(1, f.length); if (0 === f.indexOf(a)) return decodeURIComponent(f.substring(a.length, f.length))
                    } return null
                }, setCookie: function (a, b, c, d, f, g) { var k = new Date; c && (c *= 6E4); document.cookie = a + "=" + encodeURIComponent(b) + (c ? ";expires=" + (new Date(k.getTime() + c)).toUTCString() : "") + (d ? ";path=" + d : "") + (f ? ";domain=" + f : "") + (g ? ";secure" : "") }, extendArray: function (a, b) { return a instanceof Array && b instanceof Array ? (Array.prototype.push.apply(a, b), !0) : !1 }, extendObject: function (a, b, c) {
                    var d; if (a === Object(a) && b ===
                        Object(b)) { for (d in b) !b.hasOwnProperty(d) || !w.isEmptyObject(c) && d in c || (a[d] = b[d]); return !0 } return !1
                }, getMaxCookieExpiresInMinutes: function () { return ((new Date(B.COOKIE_MAX_EXPIRATION_DATE)).getTime() - (new Date).getTime()) / 1E3 / 60 }
            }; "error" === f && 0 === k && v.addListener(window, "load", function () { DIL.windowLoaded = !0 }); var S = !1, F = function () { S || (S = !0, q.registerRequest(), U(), n || q.abortRequests || z.attachIframe(), q.readyToRemove = !0, q.requestRemoval()) }, U = function () {
                n || setTimeout(function () {
                    N || q.firstRequestHasFired ||
                        ("function" === typeof G ? I.afterResult(G).submit() : I.submit())
                }, DIL.constants.TIME_TO_DEFAULT_REQUEST)
            }, T = document; "error" !== f && (DIL.windowLoaded ? F() : "complete" !== T.readyState && "loaded" !== T.readyState ? v.addListener(window, "load", function () { DIL.windowLoaded = !0; F() }) : (DIL.windowLoaded = !0, F())); q.declaredId.setDeclaredId(x, "init"); q.processVisitorAPI(); this.api = I; this.getStuffedVariable = function (a) { var b = H.stuffed[a]; b || "number" === typeof b || (b = v.getCookie(a)) || "number" === typeof b || (b = ""); return b }; this.validators =
                w; this.helpers = v; this.constants = B; this.log = d; R && (this.pendingRequest = l, this.requestController = q, this.setDestinationPublishingUrl = g, this.destinationPublishing = z, this.requestProcs = C, this.variables = H, this.callWindowLoadFunctions = F)
}, function () { var a = document, c; null == a.readyState && a.addEventListener && (a.readyState = "loading", a.addEventListener("DOMContentLoaded", c = function () { a.removeEventListener("DOMContentLoaded", c, !1); a.readyState = "complete" }, !1)) }(), DIL.extendStaticPropertiesAndMethods = function (a) {
    var c;
    if (a === Object(a)) for (c in a) a.hasOwnProperty(c) && (this[c] = a[c])
}, DIL.extendStaticPropertiesAndMethods({
    version: "6.2", jsonVersion: 1, constants: { TIME_TO_DEFAULT_REQUEST: 50 }, variables: { scriptNodeList: document.getElementsByTagName("script"), scriptsRemoved: [], callbacksRemoved: [] }, windowLoaded: !1, dils: {}, isAddedPostWindowLoad: function (a) { this.windowLoaded = "function" === typeof a ? !!a() : "boolean" === typeof a ? a : !0 }, create: function (a) {
        try { return new DIL(a) } catch (c) {
            return (new Image(0, 0)).src = "http://error.demdex.net/event?d_nsid=0&d_px=14137&d_ld=name%3Derror%26filename%3Ddil.js%26partner%3Dno_partner%26message%3DError%2520in%2520attempt%2520to%2520create%2520DIL%2520instance%2520with%2520DIL.create()%26_ts%3D" +
                (new Date).getTime(), Error("Error in attempt to create DIL instance with DIL.create()")
        }
    }, registerDil: function (a, c, d) { c = c + "$" + d; c in this.dils || (this.dils[c] = a) }, getDil: function (a, c) { var d; "string" !== typeof a && (a = ""); c || (c = 0); d = a + "$" + c; return d in this.dils ? this.dils[d] : Error("The DIL instance with partner = " + a + " and containerNSID = " + c + " was not found") }, dexGetQSVars: function (a, c, d) { c = this.getDil(c, d); return c instanceof this ? c.getStuffedVariable(a) : "" }, xd: {
        postMessage: function (a, c, d) {
            var b = 1; c &&
                (window.postMessage ? d.postMessage(a, c.replace(/([^:]+:\/\/[^\/]+).*/, "$1")) : c && (d.location = c.replace(/#.*$/, "") + "#" + +new Date + b++ + "&" + a))
        }
    }
}), DIL.errorModule = function () {
    var a = DIL.create({ partner: "error", containerNSID: 0, disableDestinationPublishingIframe: !0 }), c = { harvestererror: 14138, destpuberror: 14139, dpmerror: 14140, generalerror: 14137, error: 14137, noerrortypedefined: 15021, evalerror: 15016, rangeerror: 15017, referenceerror: 15018, typeerror: 15019, urierror: 15020 }, d = !1; return {
        activate: function () { d = !0 }, handleError: function (b) {
            if (!d) return "DIL error module has not been activated";
            b !== Object(b) && (b = {}); var g = b.name ? (b.name + "").toLowerCase() : "", f = []; b = { name: g, filename: b.filename ? b.filename + "" : "", partner: b.partner ? b.partner + "" : "no_partner", site: b.site ? b.site + "" : document.location.href, message: b.message ? b.message + "" : "" }; f.push(g in c ? c[g] : c.noerrortypedefined); a.api.pixels(f).logs(b).useImageRequest().submit(); return "DIL error report sent"
        }, pixelMap: c
    }
}(), DIL.tools = {}, DIL.modules = {
    helpers: {
        handleModuleError: function (a, c, d) {
            var b = ""; c = c || "Error caught in DIL module/submodule: ";
            a === Object(a) ? b = c + (a.message || "err has no message") : (b = c + "err is not a valid object", a = {}); a.message = b; d instanceof DIL && (a.partner = d.api.getPartner()); DIL.errorModule.handleError(a); return this.errorMessage = b
        }
    }
});
DIL.tools.getSearchReferrer = function (a, c) {
    var d = DIL.getDil("error"), b = DIL.tools.decomposeURI(a || document.referrer), g = "", f = "", k = { queryParam: "q" }; return (g = d.helpers.filter([c === Object(c) ? c : {}, { hostPattern: /aol\./ }, { hostPattern: /ask\./ }, { hostPattern: /bing\./ }, { hostPattern: /google\./ }, { hostPattern: /yahoo\./, queryParam: "p" }], function (a) { return !(!a.hasOwnProperty("hostPattern") || !b.hostname.match(a.hostPattern)) }).shift()) ? {
        valid: !0, name: b.hostname, keywords: (d.helpers.extendObject(k, g), f = k.queryPattern ?
            (g = ("" + b.search).match(k.queryPattern)) ? g[1] : "" : b.uriParams[k.queryParam], decodeURIComponent(f || "").replace(/\+|%20/g, " "))
    } : { valid: !1, name: "", keywords: "" }
};
DIL.tools.decomposeURI = function (a) { var c = DIL.getDil("error"), d = document.createElement("a"); d.href = a || document.referrer; return { hash: d.hash, host: d.host.split(":").shift(), hostname: d.hostname, href: d.href, pathname: d.pathname.replace(/^\//, ""), protocol: d.protocol, search: d.search, uriParams: function (a, d) { c.helpers.map(d.split("&"), function (c) { c = c.split("="); a[c.shift()] = c.shift() }); return a }({}, d.search.replace(/^(\/|\?)?|\/$/g, "")) } };
DIL.tools.getMetaTags = function () { var a = {}, c = document.getElementsByTagName("meta"), d, b, g, f, k; d = 0; for (g = arguments.length; d < g; d++) if (f = arguments[d], null !== f) for (b = 0; b < c.length; b++) if (k = c[b], k.name === f) { a[f] = k.content; break } return a };
DIL.modules.siteCatalyst = {
    dil: null, handle: DIL.modules.helpers.handleModuleError, init: function (a, c, d, b) {
        try {
            var g = this, f = { name: "DIL Site Catalyst Module Error" }, k = function (a) { f.message = a; DIL.errorModule.handleError(f); return a }; this.options = b === Object(b) ? b : {}; this.dil = null; if (c instanceof DIL) this.dil = c; else return k("dilInstance is not a valid instance of DIL"); f.partner = c.api.getPartner(); if (a !== Object(a)) return k("siteCatalystReportingSuite is not an object"); window.AppMeasurement_Module_DIL = a.m_DIL =
                function (a) {
                    var b = "function" === typeof a.m_i ? a.m_i("DIL") : this; if (b !== Object(b)) return k("m is not an object"); b.trackVars = g.constructTrackVars(d); b.d = 0; b.s = a; b._t = function () {
                        var a, b, c = "," + this.trackVars + ",", d = this.s, f, s = []; f = []; var n = {}, u = !1; if (d !== Object(d)) return k("Error in m._t function: s is not an object"); if (this.d) {
                            if ("function" === typeof d.foreachVar) d.foreachVar(function (a, b) { "undefined" !== typeof b && (n[a] = b, u = !0) }, this.trackVars); else {
                                if (!(d.va_t instanceof Array)) return k("Error in m._t function: s.va_t is not an array");
                                if (d.lightProfileID) (a = d.lightTrackVars) && (a = "," + a + "," + d.vl_mr + ","); else if (d.pe || d.linkType) a = d.linkTrackVars, d.pe && (b = d.pe.substring(0, 1).toUpperCase() + d.pe.substring(1), d[b] && (a = d[b].trackVars)), a && (a = "," + a + "," + d.vl_l + "," + d.vl_l2 + ","); if (a) { b = 0; for (s = a.split(","); b < s.length; b++) 0 <= c.indexOf("," + s[b] + ",") && f.push(s[b]); f.length && (c = "," + f.join(",") + ",") } f = 0; for (b = d.va_t.length; f < b; f++) a = d.va_t[f], 0 <= c.indexOf("," + a + ",") && "undefined" !== typeof d[a] && null !== d[a] && "" !== d[a] && (n[a] = d[a], u = !0)
                            } g.includeContextData(d,
                                n).store_populated && (u = !0); u && this.d.api.signals(n, "c_").submit()
                        }
                    }
                }; a.loadModule("DIL"); a.DIL.d = c; return f.message ? f.message : "DIL.modules.siteCatalyst.init() completed with no errors"
        } catch (n) { return this.handle(n, "DIL.modules.siteCatalyst.init() caught error with message ", this.dil) }
    }, constructTrackVars: function (a) {
        var c = [], d, b, g, f, k; if (a === Object(a)) {
            d = a.names; if (d instanceof Array && (g = d.length)) for (b = 0; b < g; b++) f = d[b], "string" === typeof f && f.length && c.push(f); a = a.iteratedNames; if (a instanceof Array &&
                (g = a.length)) for (b = 0; b < g; b++) if (d = a[b], d === Object(d) && (f = d.name, k = parseInt(d.maxIndex, 10), "string" === typeof f && f.length && !isNaN(k) && 0 <= k)) for (d = 0; d <= k; d++) c.push(f + d); if (c.length) return c.join(",")
        } return this.constructTrackVars({ names: "pageName channel campaign products events pe pev1 pev2 pev3".split(" "), iteratedNames: [{ name: "prop", maxIndex: 75 }, { name: "eVar", maxIndex: 250 }] })
    }, includeContextData: function (a, c) {
        var d = {}, b = !1; if (a.contextData === Object(a.contextData)) {
            var g = a.contextData, f = this.options.replaceContextDataPeriodsWith,
                k = this.options.filterFromContextVariables, n = {}, u, s, m, p; "string" === typeof f && f.length || (f = "_"); if (k instanceof Array) for (u = 0, s = k.length; u < s; u++) m = k[u], this.dil.validators.isPopulatedString(m) && (n[m] = !0); for (p in g) !g.hasOwnProperty(p) || n[p] || !(k = g[p]) && "number" !== typeof k || (p = ("contextData." + p).replace(/\./g, f), c[p] = k, b = !0)
        } d.store_populated = b; return d
    }
};
DIL.modules.GA = {
    dil: null, arr: null, tv: null, errorMessage: "", defaultTrackVars: ["_setAccount", "_setCustomVar", "_addItem", "_addTrans", "_trackSocial"], defaultTrackVarsObj: null, signals: {}, hasSignals: !1, handle: DIL.modules.helpers.handleModuleError, init: function (a, c, d) {
        try {
            this.tv = this.arr = this.dil = null; this.errorMessage = ""; this.signals = {}; this.hasSignals = !1; var b = { name: "DIL GA Module Error" }, g = ""; c instanceof DIL ? (this.dil = c, b.partner = this.dil.api.getPartner()) : (g = "dilInstance is not a valid instance of DIL",
                b.message = g, DIL.errorModule.handleError(b)); a instanceof Array && a.length ? this.arr = a : (g = "gaArray is not an array or is empty", b.message = g, DIL.errorModule.handleError(b)); this.tv = this.constructTrackVars(d); this.errorMessage = g
        } catch (f) { this.handle(f, "DIL.modules.GA.init() caught error with message ", this.dil) } finally { return this }
    }, constructTrackVars: function (a) {
        var c = [], d, b, g, f; if (this.defaultTrackVarsObj !== Object(this.defaultTrackVarsObj)) {
            g = this.defaultTrackVars; f = {}; d = 0; for (b = g.length; d < b; d++) f[g[d]] =
                !0; this.defaultTrackVarsObj = f
        } else f = this.defaultTrackVarsObj; if (a === Object(a)) { a = a.names; if (a instanceof Array && (b = a.length)) for (d = 0; d < b; d++) g = a[d], "string" === typeof g && g.length && g in f && c.push(g); if (c.length) return c } return this.defaultTrackVars
    }, constructGAObj: function (a) {
        var c = {}; a = a instanceof Array ? a : this.arr; var d, b, g, f; d = 0; for (b = a.length; d < b; d++) g = a[d], g instanceof Array && g.length && (g = [], f = a[d], g instanceof Array && f instanceof Array && Array.prototype.push.apply(g, f), f = g.shift(), "string" ===
            typeof f && f.length && (c[f] instanceof Array || (c[f] = []), c[f].push(g))); return c
    }, addToSignals: function (a, c) { if ("string" !== typeof a || "" === a || null == c || "" === c) return !1; this.signals[a] instanceof Array || (this.signals[a] = []); this.signals[a].push(c); return this.hasSignals = !0 }, constructSignals: function () {
        var a = this.constructGAObj(), c = {
            _setAccount: function (a) { this.addToSignals("c_accountId", a) }, _setCustomVar: function (a, b, c) { "string" === typeof b && b.length && this.addToSignals("c_" + b, c) }, _addItem: function (a, b, c, d,
                f, g) { this.addToSignals("c_itemOrderId", a); this.addToSignals("c_itemSku", b); this.addToSignals("c_itemName", c); this.addToSignals("c_itemCategory", d); this.addToSignals("c_itemPrice", f); this.addToSignals("c_itemQuantity", g) }, _addTrans: function (a, b, c, d, f, g, k, n) {
                    this.addToSignals("c_transOrderId", a); this.addToSignals("c_transAffiliation", b); this.addToSignals("c_transTotal", c); this.addToSignals("c_transTax", d); this.addToSignals("c_transShipping", f); this.addToSignals("c_transCity", g); this.addToSignals("c_transState",
                        k); this.addToSignals("c_transCountry", n)
                }, _trackSocial: function (a, b, c, d) { this.addToSignals("c_socialNetwork", a); this.addToSignals("c_socialAction", b); this.addToSignals("c_socialTarget", c); this.addToSignals("c_socialPagePath", d) }
        }, d = this.tv, b, g, f, k, n, u; b = 0; for (g = d.length; b < g; b++) if (f = d[b], a.hasOwnProperty(f) && c.hasOwnProperty(f) && (u = a[f], u instanceof Array)) for (k = 0, n = u.length; k < n; k++) c[f].apply(this, u[k])
    }, submit: function () {
        try {
            if ("" !== this.errorMessage) return this.errorMessage; this.constructSignals();
            return this.hasSignals ? (this.dil.api.signals(this.signals).submit(), "Signals sent: " + this.dil.helpers.convertObjectToKeyValuePairs(this.signals, "=", !0) + this.dil.log) : "No signals present"
        } catch (a) { return this.handle(a, "DIL.modules.GA.submit() caught error with message ", this.dil) }
    }, Stuffer: {
        LIMIT: 5, dil: null, cookieName: null, delimiter: null, errorMessage: "", handle: DIL.modules.helpers.handleModuleError, callback: null, v: function () { return !1 }, init: function (a, c, d) {
            try {
                this.callback = this.dil = null, this.errorMessage =
                    "", a instanceof DIL ? (this.dil = a, this.v = this.dil.validators.isPopulatedString, this.cookieName = this.v(c) ? c : "aam_ga", this.delimiter = this.v(d) ? d : "|") : this.handle({ message: "dilInstance is not a valid instance of DIL" }, "DIL.modules.GA.Stuffer.init() error: ")
            } catch (b) { this.handle(b, "DIL.modules.GA.Stuffer.init() caught error with message ", this.dil) } finally { return this }
        }, process: function (a) {
            var c, d, b, g, f, k; k = !1; var n = 1; if (a === Object(a) && (c = a.stuff) && c instanceof Array && (d = c.length)) for (a = 0; a < d; a++) if ((b =
                c[a]) && b === Object(b) && (g = b.cn, f = b.cv, g === this.cookieName && this.v(f))) { k = !0; break } if (k) { c = f.split(this.delimiter); "undefined" === typeof window._gaq && (window._gaq = []); b = window._gaq; a = 0; for (d = c.length; a < d && !(k = c[a].split("="), f = k[0], k = k[1], this.v(f) && this.v(k) && b.push(["_setCustomVar", n++, f, k, 1]), n > this.LIMIT); a++); this.errorMessage = 1 < n ? "No errors - stuffing successful" : "No valid values to stuff" } else this.errorMessage = "Cookie name and value not found in json"; if ("function" === typeof this.callback) return this.callback()
        },
        submit: function () { try { var a = this; if ("" !== this.errorMessage) return this.errorMessage; this.dil.api.afterResult(function (c) { a.process(c) }).submit(); return "DIL.modules.GA.Stuffer.submit() successful" } catch (c) { return this.handle(c, "DIL.modules.GA.Stuffer.submit() caught error with message ", this.dil) } }
    }
};
DIL.modules.Peer39 = {
    aid: "", dil: null, optionals: null, errorMessage: "", calledBack: !1, script: null, scriptsSent: [], returnedData: [], handle: DIL.modules.helpers.handleModuleError, init: function (a, c, d) {
        try {
            this.dil = null; this.errorMessage = ""; this.calledBack = !1; this.optionals = d === Object(d) ? d : {}; d = { name: "DIL Peer39 Module Error" }; var b = [], g = ""; this.isSecurePageButNotEnabled(document.location.protocol) && (g = "Module has not been enabled for a secure page", b.push(g), d.message = g, DIL.errorModule.handleError(d)); c instanceof
                DIL ? (this.dil = c, d.partner = this.dil.api.getPartner()) : (g = "dilInstance is not a valid instance of DIL", b.push(g), d.message = g, DIL.errorModule.handleError(d)); "string" === typeof a && a.length ? this.aid = a : (g = "aid is not a string or is empty", b.push(g), d.message = g, DIL.errorModule.handleError(d)); this.errorMessage = b.join("\n")
        } catch (f) { this.handle(f, "DIL.modules.Peer39.init() caught error with message ", this.dil) } finally { return this }
    }, isSecurePageButNotEnabled: function (a) {
        return "https:" === a && !0 !== this.optionals.enableHTTPS ?
            !0 : !1
    }, constructSignals: function () { var a = this, c = this.constructScript(), d = DIL.variables.scriptNodeList[0]; window["afterFinished_" + this.aid] = function () { try { var b = a.processData(p39_KVP_Short("c_p", "|").split("|")); b.hasSignals && a.dil.api.signals(b.signals).submit() } catch (c) { } finally { a.calledBack = !0, "function" === typeof a.optionals.afterResult && a.optionals.afterResult() } }; d.parentNode.insertBefore(c, d); this.scriptsSent.push(c); return "Request sent to Peer39" }, processData: function (a) {
        var c, d, b, g, f = {}, k =
            !1; this.returnedData.push(a); if (a instanceof Array) for (c = 0, d = a.length; c < d; c++) b = a[c].split("="), g = b[0], b = b[1], g && isFinite(b) && !isNaN(parseInt(b, 10)) && (f[g] instanceof Array || (f[g] = []), f[g].push(b), k = !0); return { hasSignals: k, signals: f }
    }, constructScript: function () {
        var a = document.createElement("script"), c = this.optionals, d = c.scriptId, b = c.scriptSrc, c = c.scriptParams; a.id = "string" === typeof d && d.length ? d : "peer39ScriptLoader"; a.type = "text/javascript"; "string" === typeof b && b.length ? a.src = b : (a.src = (this.dil.constants.IS_HTTPS ?
            "https:" : "http:") + "//stags.peer39.net/" + this.aid + "/trg_" + this.aid + ".js", "string" === typeof c && c.length && (a.src += "?" + c)); return a
    }, submit: function () { try { return "" !== this.errorMessage ? this.errorMessage : this.constructSignals() } catch (a) { return this.handle(a, "DIL.modules.Peer39.submit() caught error with message ", this.dil) } }
};

s.AudienceManagement.setup({
    "partner": "suntrustbanksinc",
    "containerNSID": 0,
    "uuidCookie": {
        "name": "aam_uuid",
        "days": 30
    }
});

var sunDil = DIL.getDil("suntrustbanksinc", 0);



/*
 Start ActivityMap Module

 The following module enables ActivityMap tracking in Adobe Analytics. ActivityMap
 allows you to view data overlays on your links and content to understand how
 users engage with your web site. If you do not intend to use ActivityMap, you
 can remove the following block of code from your AppMeasurement.js file.
 Additional documentation on how to configure ActivityMap is available at:
 https://marketing.adobe.com/resources/help/en_US/analytics/activitymap/getting-started-admins.html
*/
function AppMeasurement_Module_ActivityMap(f) {
    function g(a, d) { var b, c, n; if (a && d && (b = e.c[d] || (e.c[d] = d.split(",")))) for (n = 0; n < b.length && (c = b[n++]);)if (-1 < a.indexOf(c)) return null; p = 1; return a } function q(a, d, b, c, e) {
        var g, h; if (a.dataset && (h = a.dataset[d])) g = h; else if (a.getAttribute) if (h = a.getAttribute("data-" + b)) g = h; else if (h = a.getAttribute(b)) g = h; if (!g && f.useForcedLinkTracking && e && (g = "", d = a.onclick ? "" + a.onclick : "")) {
            b = d.indexOf(c); var l, k; if (0 <= b) {
                for (b += 10; b < d.length && 0 <= "= \t\r\n".indexOf(d.charAt(b));)b++;
                if (b < d.length) { h = b; for (l = k = 0; h < d.length && (";" != d.charAt(h) || l);)l ? d.charAt(h) != l || k ? k = "\\" == d.charAt(h) ? !k : 0 : l = 0 : (l = d.charAt(h), '"' != l && "'" != l && (l = 0)), h++; if (d = d.substring(b, h)) a.e = new Function("s", "var e;try{s.w." + c + "=" + d + "}catch(e){}"), a.e(f) }
            }
        } return g || e && f.w[c]
    } function r(a, d, b) { var c; return (c = e[d](a, b)) && (p ? (p = 0, c) : g(k(c), e[d + "Exclusions"])) } function s(a, d, b) {
        var c; if (a && !(1 === (c = a.nodeType) && (c = a.nodeName) && (c = c.toUpperCase()) && t[c]) && (1 === a.nodeType && (c = a.nodeValue) && (d[d.length] = c), b.a ||
            b.t || b.s || !a.getAttribute || ((c = a.getAttribute("alt")) ? b.a = c : (c = a.getAttribute("title")) ? b.t = c : "IMG" == ("" + a.nodeName).toUpperCase() && (c = a.getAttribute("src") || a.src) && (b.s = c)), (c = a.childNodes) && c.length)) for (a = 0; a < c.length; a++)s(c[a], d, b)
    } function k(a) {
        if (null == a || void 0 == a) return a; try {
            return a.replace(RegExp("^[\\s\\n\\f\\r\\t\t-\r \u00a0\u1680\u180e\u2000-\u200a\u2028\u2029\u205f\u3000\ufeff]+", "mg"), "").replace(RegExp("[\\s\\n\\f\\r\\t\t-\r \u00a0\u1680\u180e\u2000-\u200a\u2028\u2029\u205f\u3000\ufeff]+$",
                "mg"), "").replace(RegExp("[\\s\\n\\f\\r\\t\t-\r \u00a0\u1680\u180e\u2000-\u200a\u2028\u2029\u205f\u3000\ufeff]{1,}", "mg"), " ").substring(0, 254)
        } catch (d) { }
    } var e = this; e.s = f; var m = window; m.s_c_in || (m.s_c_il = [], m.s_c_in = 0); e._il = m.s_c_il; e._in = m.s_c_in; e._il[e._in] = e; m.s_c_in++; e._c = "s_m"; e.c = {}; var p = 0, t = { SCRIPT: 1, STYLE: 1, LINK: 1, CANVAS: 1 }; e._g = function () {
        var a, d, b, c = f.contextData, e = f.linkObject; (a = f.pageName || f.pageURL) && (d = r(e, "link", f.linkName)) && (b = r(e, "region")) && (c["a.activitymap.page"] = a.substring(0,
            255), c["a.activitymap.link"] = 128 < d.length ? d.substring(0, 128) : d, c["a.activitymap.region"] = 127 < b.length ? b.substring(0, 127) : b, c["a.activitymap.pageIDType"] = f.pageName ? 1 : 0)
    }; e.link = function (a, d) {
        var b; if (d) b = g(k(d), e.linkExclusions); else if ((b = a) && !(b = q(a, "sObjectId", "s-object-id", "s_objectID", 1))) {
            var c, f; (f = g(k(a.innerText || a.textContent), e.linkExclusions)) || (s(a, c = [], b = { a: void 0, t: void 0, s: void 0 }), (f = g(k(c.join("")))) || (f = g(k(b.a ? b.a : b.t ? b.t : b.s ? b.s : void 0))) || !(c = (c = a.tagName) && c.toUpperCase ? c.toUpperCase() :
                "") || ("INPUT" == c || "SUBMIT" == c && a.value ? f = g(k(a.value)) : "IMAGE" == c && a.src && (f = g(k(a.src))))); b = f
        } return b
    }; e.region = function (a) { for (var d, b = e.regionIDAttribute || "id"; a && (a = a.parentNode);) { if (d = q(a, b, b, b)) return d; if ("BODY" == a.nodeName) return "BODY" } }
}


/* End ActivityMap Module */



/*
 

============== DO NOT ALTER ANYTHING BELOW THIS LINE ! ===============



AppMeasurement for JavaScript version: 2.8.1

Copyright 1996-2016 Adobe, Inc. All Rights Reserved

More info available at http://www.adobe.com/marketing-cloud.html


*/



function AppMeasurement(r) {
    var a = this; a.version = "2.8.1"; var k = window; k.s_c_in || (k.s_c_il = [], k.s_c_in = 0); a._il = k.s_c_il; a._in = k.s_c_in; a._il[a._in] = a; k.s_c_in++; a._c = "s_c"; var p = k.AppMeasurement.Xb; p || (p = null); var n = k, m, s; try { for (m = n.parent, s = n.location; m && m.location && s && "" + m.location != "" + s && n.location && "" + m.location != "" + n.location && m.location.host == s.host;)n = m, m = n.parent } catch (u) { } a.F = function (a) { try { console.log(a) } catch (b) { } }; a.Oa = function (a) { return "" + parseInt(a) == "" + a }; a.replace = function (a, b, d) {
        return !a ||
            0 > a.indexOf(b) ? a : a.split(b).join(d)
    }; a.escape = function (c) { var b, d; if (!c) return c; c = encodeURIComponent(c); for (b = 0; 7 > b; b++)d = "+~!*()'".substring(b, b + 1), 0 <= c.indexOf(d) && (c = a.replace(c, d, "%" + d.charCodeAt(0).toString(16).toUpperCase())); return c }; a.unescape = function (c) { if (!c) return c; c = 0 <= c.indexOf("+") ? a.replace(c, "+", " ") : c; try { return decodeURIComponent(c) } catch (b) { } return unescape(c) }; a.Fb = function () {
        var c = k.location.hostname, b = a.fpCookieDomainPeriods, d; b || (b = a.cookieDomainPeriods); if (c && !a.Ga && !/^[0-9.]+$/.test(c) &&
            (b = b ? parseInt(b) : 2, b = 2 < b ? b : 2, d = c.lastIndexOf("."), 0 <= d)) { for (; 0 <= d && 1 < b;)d = c.lastIndexOf(".", d - 1), b--; a.Ga = 0 < d ? c.substring(d) : c } return a.Ga
    }; a.c_r = a.cookieRead = function (c) { c = a.escape(c); var b = " " + a.d.cookie, d = b.indexOf(" " + c + "="), f = 0 > d ? d : b.indexOf(";", d); c = 0 > d ? "" : a.unescape(b.substring(d + 2 + c.length, 0 > f ? b.length : f)); return "[[B]]" != c ? c : "" }; a.c_w = a.cookieWrite = function (c, b, d) {
        var f = a.Fb(), e = a.cookieLifetime, g; b = "" + b; e = e ? ("" + e).toUpperCase() : ""; d && "SESSION" != e && "NONE" != e && ((g = "" != b ? parseInt(e ? e : 0) : -60) ?
            (d = new Date, d.setTime(d.getTime() + 1E3 * g)) : 1 == d && (d = new Date, g = d.getYear(), d.setYear(g + 5 + (1900 > g ? 1900 : 0)))); return c && "NONE" != e ? (a.d.cookie = a.escape(c) + "=" + a.escape("" != b ? b : "[[B]]") + "; path=/;" + (d && "SESSION" != e ? " expires=" + d.toUTCString() + ";" : "") + (f ? " domain=" + f + ";" : ""), a.cookieRead(c) == b) : 0
    }; a.Cb = function () { var c = a.Util.getIeVersion(); "number" === typeof c && 10 > c && (a.unsupportedBrowser = !0, a.rb(a, function () { })) }; a.rb = function (a, b) { for (var d in a) a.hasOwnProperty(d) && "function" === typeof a[d] && (a[d] = b) };
    a.L = []; a.ja = function (c, b, d) {
        if (a.Ha) return 0; a.maxDelay || (a.maxDelay = 250); var f = 0, e = (new Date).getTime() + a.maxDelay, g = a.d.visibilityState, h = ["webkitvisibilitychange", "visibilitychange"]; g || (g = a.d.webkitVisibilityState); if (g && "prerender" == g) { if (!a.ka) for (a.ka = 1, d = 0; d < h.length; d++)a.d.addEventListener(h[d], function () { var c = a.d.visibilityState; c || (c = a.d.webkitVisibilityState); "visible" == c && (a.ka = 0, a.delayReady()) }); f = 1; e = 0 } else d || a.p("_d") && (f = 1); f && (a.L.push({ m: c, a: b, t: e }), a.ka || setTimeout(a.delayReady,
            a.maxDelay)); return f
    }; a.delayReady = function () { var c = (new Date).getTime(), b = 0, d; for (a.p("_d") ? b = 1 : a.za(); 0 < a.L.length;) { d = a.L.shift(); if (b && !d.t && d.t > c) { a.L.unshift(d); setTimeout(a.delayReady, parseInt(a.maxDelay / 2)); break } a.Ha = 1; a[d.m].apply(a, d.a); a.Ha = 0 } }; a.setAccount = a.sa = function (c) {
        var b, d; if (!a.ja("setAccount", arguments)) if (a.account = c, a.allAccounts) for (b = a.allAccounts.concat(c.split(",")), a.allAccounts = [], b.sort(), d = 0; d < b.length; d++)0 != d && b[d - 1] == b[d] || a.allAccounts.push(b[d]); else a.allAccounts =
            c.split(",")
    }; a.foreachVar = function (c, b) { var d, f, e, g, h = ""; e = f = ""; if (a.lightProfileID) d = a.P, (h = a.lightTrackVars) && (h = "," + h + "," + a.oa.join(",") + ","); else { d = a.g; if (a.pe || a.linkType) h = a.linkTrackVars, f = a.linkTrackEvents, a.pe && (e = a.pe.substring(0, 1).toUpperCase() + a.pe.substring(1), a[e] && (h = a[e].Vb, f = a[e].Ub)); h && (h = "," + h + "," + a.H.join(",") + ","); f && h && (h += ",events,") } b && (b = "," + b + ","); for (f = 0; f < d.length; f++)e = d[f], (g = a[e]) && (!h || 0 <= h.indexOf("," + e + ",")) && (!b || 0 <= b.indexOf("," + e + ",")) && c(e, g) }; a.r = function (c,
        b, d, f, e) {
        var g = "", h, l, k, q, m = 0; "contextData" == c && (c = "c"); if (b) {
            for (h in b) if (!(Object.prototype[h] || e && h.substring(0, e.length) != e) && b[h] && (!d || 0 <= d.indexOf("," + (f ? f + "." : "") + h + ","))) {
                k = !1; if (m) for (l = 0; l < m.length; l++)h.substring(0, m[l].length) == m[l] && (k = !0); if (!k && ("" == g && (g += "&" + c + "."), l = b[h], e && (h = h.substring(e.length)), 0 < h.length)) if (k = h.indexOf("."), 0 < k) l = h.substring(0, k), k = (e ? e : "") + l + ".", m || (m = []), m.push(k), g += a.r(l, b, d, f, k); else if ("boolean" == typeof l && (l = l ? "true" : "false"), l) {
                    if ("retrieveLightData" ==
                        f && 0 > e.indexOf(".contextData.")) switch (k = h.substring(0, 4), q = h.substring(4), h) { case "transactionID": h = "xact"; break; case "channel": h = "ch"; break; case "campaign": h = "v0"; break; default: a.Oa(q) && ("prop" == k ? h = "c" + q : "eVar" == k ? h = "v" + q : "list" == k ? h = "l" + q : "hier" == k && (h = "h" + q, l = l.substring(0, 255))) }g += "&" + a.escape(h) + "=" + a.escape(l)
                }
            } "" != g && (g += "&." + c)
        } return g
    }; a.usePostbacks = 0; a.Ib = function () {
        var c = "", b, d, f, e, g, h, l, k, q = "", m = "", n = e = ""; if (a.lightProfileID) b = a.P, (q = a.lightTrackVars) && (q = "," + q + "," + a.oa.join(",") +
            ","); else { b = a.g; if (a.pe || a.linkType) q = a.linkTrackVars, m = a.linkTrackEvents, a.pe && (e = a.pe.substring(0, 1).toUpperCase() + a.pe.substring(1), a[e] && (q = a[e].Vb, m = a[e].Ub)); q && (q = "," + q + "," + a.H.join(",") + ","); m && (m = "," + m + ",", q && (q += ",events,")); a.events2 && (n += ("" != n ? "," : "") + a.events2) } if (a.visitor && a.visitor.getCustomerIDs) {
                e = p; if (g = a.visitor.getCustomerIDs()) for (d in g) Object.prototype[d] || (f = g[d], "object" == typeof f && (e || (e = {}), f.id && (e[d + ".id"] = f.id), f.authState && (e[d + ".as"] = f.authState))); e && (c += a.r("cid",
                    e))
            } a.AudienceManagement && a.AudienceManagement.isReady() && (c += a.r("d", a.AudienceManagement.getEventCallConfigParams())); for (d = 0; d < b.length; d++) {
                e = b[d]; g = a[e]; f = e.substring(0, 4); h = e.substring(4); g || ("events" == e && n ? (g = n, n = "") : "marketingCloudOrgID" == e && a.visitor && (g = a.visitor.marketingCloudOrgID)); if (g && (!q || 0 <= q.indexOf("," + e + ","))) {
                    switch (e) {
                        case "customerPerspective": e = "cp"; break; case "marketingCloudOrgID": e = "mcorgid"; break; case "supplementalDataID": e = "sdid"; break; case "timestamp": e = "ts"; break; case "dynamicVariablePrefix": e =
                            "D"; break; case "visitorID": e = "vid"; break; case "marketingCloudVisitorID": e = "mid"; break; case "analyticsVisitorID": e = "aid"; break; case "audienceManagerLocationHint": e = "aamlh"; break; case "audienceManagerBlob": e = "aamb"; break; case "authState": e = "as"; break; case "pageURL": e = "g"; 255 < g.length && (a.pageURLRest = g.substring(255), g = g.substring(0, 255)); break; case "pageURLRest": e = "-g"; break; case "referrer": e = "r"; break; case "vmk": case "visitorMigrationKey": e = "vmt"; break; case "visitorMigrationServer": e = "vmf"; a.ssl &&
                                a.visitorMigrationServerSecure && (g = ""); break; case "visitorMigrationServerSecure": e = "vmf"; !a.ssl && a.visitorMigrationServer && (g = ""); break; case "charSet": e = "ce"; break; case "visitorNamespace": e = "ns"; break; case "cookieDomainPeriods": e = "cdp"; break; case "cookieLifetime": e = "cl"; break; case "variableProvider": e = "vvp"; break; case "currencyCode": e = "cc"; break; case "channel": e = "ch"; break; case "transactionID": e = "xact"; break; case "campaign": e = "v0"; break; case "latitude": e = "lat"; break; case "longitude": e = "lon"; break;
                        case "resolution": e = "s"; break; case "colorDepth": e = "c"; break; case "javascriptVersion": e = "j"; break; case "javaEnabled": e = "v"; break; case "cookiesEnabled": e = "k"; break; case "browserWidth": e = "bw"; break; case "browserHeight": e = "bh"; break; case "connectionType": e = "ct"; break; case "homepage": e = "hp"; break; case "events": n && (g += ("" != g ? "," : "") + n); if (m) for (h = g.split(","), g = "", f = 0; f < h.length; f++)l = h[f], k = l.indexOf("="), 0 <= k && (l = l.substring(0, k)), k = l.indexOf(":"), 0 <= k && (l = l.substring(0, k)), 0 <= m.indexOf("," + l + ",") && (g +=
                            (g ? "," : "") + h[f]); break; case "events2": g = ""; break; case "contextData": c += a.r("c", a[e], q, e); g = ""; break; case "lightProfileID": e = "mtp"; break; case "lightStoreForSeconds": e = "mtss"; a.lightProfileID || (g = ""); break; case "lightIncrementBy": e = "mti"; a.lightProfileID || (g = ""); break; case "retrieveLightProfiles": e = "mtsr"; break; case "deleteLightProfiles": e = "mtsd"; break; case "retrieveLightData": a.retrieveLightProfiles && (c += a.r("mts", a[e], q, e)); g = ""; break; default: a.Oa(h) && ("prop" == f ? e = "c" + h : "eVar" == f ? e = "v" + h : "list" ==
                                f ? e = "l" + h : "hier" == f && (e = "h" + h, g = g.substring(0, 255)))
                    }g && (c += "&" + e + "=" + ("pev" != e.substring(0, 3) ? a.escape(g) : g))
                } "pev3" == e && a.e && (c += a.e)
            } a.na && (c += "&lrt=" + a.na, a.na = null); return c
    }; a.D = function (a) { var b = a.tagName; if ("undefined" != "" + a.$b || "undefined" != "" + a.Qb && "HTML" != ("" + a.Qb).toUpperCase()) return ""; b = b && b.toUpperCase ? b.toUpperCase() : ""; "SHAPE" == b && (b = ""); b && (("INPUT" == b || "BUTTON" == b) && a.type && a.type.toUpperCase ? b = a.type.toUpperCase() : !b && a.href && (b = "A")); return b }; a.Ka = function (a) {
        var b = k.location,
            d = a.href ? a.href : "", f, e, g; f = d.indexOf(":"); e = d.indexOf("?"); g = d.indexOf("/"); d && (0 > f || 0 <= e && f > e || 0 <= g && f > g) && (e = a.protocol && 1 < a.protocol.length ? a.protocol : b.protocol ? b.protocol : "", f = b.pathname.lastIndexOf("/"), d = (e ? e + "//" : "") + (a.host ? a.host : b.host ? b.host : "") + ("/" != d.substring(0, 1) ? b.pathname.substring(0, 0 > f ? 0 : f) + "/" : "") + d); return d
    }; a.M = function (c) {
        var b = a.D(c), d, f, e = "", g = 0; return b && (d = c.protocol, f = c.onclick, !c.href || "A" != b && "AREA" != b || f && d && !(0 > d.toLowerCase().indexOf("javascript")) ? f ? (e = a.replace(a.replace(a.replace(a.replace("" +
            f, "\r", ""), "\n", ""), "\t", ""), " ", ""), g = 2) : "INPUT" == b || "SUBMIT" == b ? (c.value ? e = c.value : c.innerText ? e = c.innerText : c.textContent && (e = c.textContent), g = 3) : "IMAGE" == b && c.src && (e = c.src) : e = a.Ka(c), e) ? { id: e.substring(0, 100), type: g } : 0
    }; a.Yb = function (c) { for (var b = a.D(c), d = a.M(c); c && !d && "BODY" != b;)if (c = c.parentElement ? c.parentElement : c.parentNode) b = a.D(c), d = a.M(c); d && "BODY" != b || (c = 0); c && (b = c.onclick ? "" + c.onclick : "", 0 <= b.indexOf(".tl(") || 0 <= b.indexOf(".trackLink(")) && (c = 0); return c }; a.Pb = function () {
        var c, b, d = a.linkObject,
            f = a.linkType, e = a.linkURL, g, h; a.pa = 1; d || (a.pa = 0, d = a.clickObject); if (d) { c = a.D(d); for (b = a.M(d); d && !b && "BODY" != c;)if (d = d.parentElement ? d.parentElement : d.parentNode) c = a.D(d), b = a.M(d); b && "BODY" != c || (d = 0); if (d && !a.linkObject) { var l = d.onclick ? "" + d.onclick : ""; if (0 <= l.indexOf(".tl(") || 0 <= l.indexOf(".trackLink(")) d = 0 } } else a.pa = 1; !e && d && (e = a.Ka(d)); e && !a.linkLeaveQueryString && (g = e.indexOf("?"), 0 <= g && (e = e.substring(0, g))); if (!f && e) {
                var m = 0, q = 0, n; if (a.trackDownloadLinks && a.linkDownloadFileTypes) for (l = e.toLowerCase(),
                    g = l.indexOf("?"), h = l.indexOf("#"), 0 <= g ? 0 <= h && h < g && (g = h) : g = h, 0 <= g && (l = l.substring(0, g)), g = a.linkDownloadFileTypes.toLowerCase().split(","), h = 0; h < g.length; h++)(n = g[h]) && l.substring(l.length - (n.length + 1)) == "." + n && (f = "d"); if (a.trackExternalLinks && !f && (l = e.toLowerCase(), a.Na(l) && (a.linkInternalFilters || (a.linkInternalFilters = k.location.hostname), g = 0, a.linkExternalFilters ? (g = a.linkExternalFilters.toLowerCase().split(","), m = 1) : a.linkInternalFilters && (g = a.linkInternalFilters.toLowerCase().split(",")), g))) {
                        for (h =
                            0; h < g.length; h++)n = g[h], 0 <= l.indexOf(n) && (q = 1); q ? m && (f = "e") : m || (f = "e")
                    }
            } a.linkObject = d; a.linkURL = e; a.linkType = f; if (a.trackClickMap || a.trackInlineStats) a.e = "", d && (f = a.pageName, e = 1, d = d.sourceIndex, f || (f = a.pageURL, e = 0), k.s_objectID && (b.id = k.s_objectID, d = b.type = 1), f && b && b.id && c && (a.e = "&pid=" + a.escape(f.substring(0, 255)) + (e ? "&pidt=" + e : "") + "&oid=" + a.escape(b.id.substring(0, 100)) + (b.type ? "&oidt=" + b.type : "") + "&ot=" + c + (d ? "&oi=" + d : "")))
    }; a.Jb = function () {
        var c = a.pa, b = a.linkType, d = a.linkURL, f = a.linkName; b && (d ||
            f) && (b = b.toLowerCase(), "d" != b && "e" != b && (b = "o"), a.pe = "lnk_" + b, a.pev1 = d ? a.escape(d) : "", a.pev2 = f ? a.escape(f) : "", c = 1); a.abort && (c = 0); if (a.trackClickMap || a.trackInlineStats || a.ActivityMap) {
                var b = {}, d = 0, e = a.cookieRead("s_sq"), g = e ? e.split("&") : 0, h, l, k, e = 0; if (g) for (h = 0; h < g.length; h++)l = g[h].split("="), f = a.unescape(l[0]).split(","), l = a.unescape(l[1]), b[l] = f; f = a.account.split(","); h = {}; for (k in a.contextData) k && !Object.prototype[k] && "a.activitymap." == k.substring(0, 14) && (h[k] = a.contextData[k], a.contextData[k] =
                    ""); a.e = a.r("c", h) + (a.e ? a.e : ""); if (c || a.e) {
                        c && !a.e && (e = 1); for (l in b) if (!Object.prototype[l]) for (k = 0; k < f.length; k++)for (e && (g = b[l].join(","), g == a.account && (a.e += ("&" != l.charAt(0) ? "&" : "") + l, b[l] = [], d = 1)), h = 0; h < b[l].length; h++)g = b[l][h], g == f[k] && (e && (a.e += "&u=" + a.escape(g) + ("&" != l.charAt(0) ? "&" : "") + l + "&u=0"), b[l].splice(h, 1), d = 1); c || (d = 1); if (d) {
                            e = ""; h = 2; !c && a.e && (e = a.escape(f.join(",")) + "=" + a.escape(a.e), h = 1); for (l in b) !Object.prototype[l] && 0 < h && 0 < b[l].length && (e += (e ? "&" : "") + a.escape(b[l].join(",")) +
                                "=" + a.escape(l), h--); a.cookieWrite("s_sq", e)
                        }
                    }
            } return c
    }; a.Kb = function () {
        if (!a.Tb) {
            var c = new Date, b = n.location, d, f, e = f = d = "", g = "", h = "", l = "1.2", k = a.cookieWrite("s_cc", "true", 0) ? "Y" : "N", m = "", p = ""; if (c.setUTCDate && (l = "1.3", (0).toPrecision && (l = "1.5", c = [], c.forEach))) { l = "1.6"; f = 0; d = {}; try { f = new Iterator(d), f.next && (l = "1.7", c.reduce && (l = "1.8", l.trim && (l = "1.8.1", Date.parse && (l = "1.8.2", Object.create && (l = "1.8.5"))))) } catch (r) { } } d = screen.width + "x" + screen.height; e = navigator.javaEnabled() ? "Y" : "N"; f = screen.pixelDepth ?
                screen.pixelDepth : screen.colorDepth; g = a.w.innerWidth ? a.w.innerWidth : a.d.documentElement.offsetWidth; h = a.w.innerHeight ? a.w.innerHeight : a.d.documentElement.offsetHeight; try { a.b.addBehavior("#default#homePage"), m = a.b.Zb(b) ? "Y" : "N" } catch (s) { } try { a.b.addBehavior("#default#clientCaps"), p = a.b.connectionType } catch (t) { } a.resolution = d; a.colorDepth = f; a.javascriptVersion = l; a.javaEnabled = e; a.cookiesEnabled = k; a.browserWidth = g; a.browserHeight = h; a.connectionType = p; a.homepage = m; a.Tb = 1
        }
    }; a.Q = {}; a.loadModule = function (c,
        b) { var d = a.Q[c]; if (!d) { d = k["AppMeasurement_Module_" + c] ? new k["AppMeasurement_Module_" + c](a) : {}; a.Q[c] = a[c] = d; d.kb = function () { return d.qb }; d.sb = function (b) { if (d.qb = b) a[c + "_onLoad"] = b, a.ja(c + "_onLoad", [a, d], 1) || b(a, d) }; try { Object.defineProperty ? Object.defineProperty(d, "onLoad", { get: d.kb, set: d.sb }) : d._olc = 1 } catch (f) { d._olc = 1 } } b && (a[c + "_onLoad"] = b, a.ja(c + "_onLoad", [a, d], 1) || b(a, d)) }; a.p = function (c) {
            var b, d; for (b in a.Q) if (!Object.prototype[b] && (d = a.Q[b]) && (d._olc && d.onLoad && (d._olc = 0, d.onLoad(a, d)), d[c] &&
                d[c]())) return 1; return 0
        }; a.Mb = function () { var c = Math.floor(1E13 * Math.random()), b = a.visitorSampling, d = a.visitorSamplingGroup, d = "s_vsn_" + (a.visitorNamespace ? a.visitorNamespace : a.account) + (d ? "_" + d : ""), f = a.cookieRead(d); if (b) { b *= 100; f && (f = parseInt(f)); if (!f) { if (!a.cookieWrite(d, c)) return 0; f = c } if (f % 1E4 > b) return 0 } return 1 }; a.R = function (c, b) {
            var d, f, e, g, h, l; for (d = 0; 2 > d; d++)for (f = 0 < d ? a.Ca : a.g, e = 0; e < f.length; e++)if (g = f[e], (h = c[g]) || c["!" + g]) {
                if (!b && ("contextData" == g || "retrieveLightData" == g) && a[g]) for (l in a[g]) h[l] ||
                    (h[l] = a[g][l]); a[g] = h
            }
        }; a.Ya = function (c, b) { var d, f, e, g; for (d = 0; 2 > d; d++)for (f = 0 < d ? a.Ca : a.g, e = 0; e < f.length; e++)g = f[e], c[g] = a[g], b || c[g] || (c["!" + g] = 1) }; a.Eb = function (a) {
            var b, d, f, e, g, h = 0, l, k = "", m = ""; if (a && 255 < a.length && (b = "" + a, d = b.indexOf("?"), 0 < d && (l = b.substring(d + 1), b = b.substring(0, d), e = b.toLowerCase(), f = 0, "http://" == e.substring(0, 7) ? f += 7 : "https://" == e.substring(0, 8) && (f += 8), d = e.indexOf("/", f), 0 < d && (e = e.substring(f, d), g = b.substring(d), b = b.substring(0, d), 0 <= e.indexOf("google") ? h = ",q,ie,start,search_key,word,kw,cd," :
                0 <= e.indexOf("yahoo.co") && (h = ",p,ei,"), h && l)))) { if ((a = l.split("&")) && 1 < a.length) { for (f = 0; f < a.length; f++)e = a[f], d = e.indexOf("="), 0 < d && 0 <= h.indexOf("," + e.substring(0, d) + ",") ? k += (k ? "&" : "") + e : m += (m ? "&" : "") + e; k && m ? l = k + "&" + m : m = "" } d = 253 - (l.length - m.length) - b.length; a = b + (0 < d ? g.substring(0, d) : "") + "?" + l } return a
        }; a.eb = function (c) {
            var b = a.d.visibilityState, d = ["webkitvisibilitychange", "visibilitychange"]; b || (b = a.d.webkitVisibilityState); if (b && "prerender" == b) {
                if (c) for (b = 0; b < d.length; b++)a.d.addEventListener(d[b],
                    function () { var b = a.d.visibilityState; b || (b = a.d.webkitVisibilityState); "visible" == b && c() }); return !1
            } return !0
        }; a.fa = !1; a.J = !1; a.ub = function () { a.J = !0; a.j() }; a.da = !1; a.V = !1; a.pb = function (c) { a.marketingCloudVisitorID = c; a.V = !0; a.j() }; a.ga = !1; a.W = !1; a.vb = function (c) { a.visitorOptedOut = c; a.W = !0; a.j() }; a.aa = !1; a.S = !1; a.$a = function (c) { a.analyticsVisitorID = c; a.S = !0; a.j() }; a.ca = !1; a.U = !1; a.bb = function (c) { a.audienceManagerLocationHint = c; a.U = !0; a.j() }; a.ba = !1; a.T = !1; a.ab = function (c) {
            a.audienceManagerBlob = c; a.T =
                !0; a.j()
        }; a.cb = function (c) { a.maxDelay || (a.maxDelay = 250); return a.p("_d") ? (c && setTimeout(function () { c() }, a.maxDelay), !1) : !0 }; a.ea = !1; a.I = !1; a.za = function () { a.I = !0; a.j() }; a.isReadyToTrack = function () {
            var c = !0, b = a.visitor, d, f, e; a.fa || a.J || (a.eb(a.ub) ? a.J = !0 : a.fa = !0); if (a.fa && !a.J) return !1; b && b.isAllowed() && (a.da || a.marketingCloudVisitorID || !b.getMarketingCloudVisitorID || (a.da = !0, a.marketingCloudVisitorID = b.getMarketingCloudVisitorID([a, a.pb]), a.marketingCloudVisitorID && (a.V = !0)), a.ga || a.visitorOptedOut ||
                !b.isOptedOut || (a.ga = !0, a.visitorOptedOut = b.isOptedOut([a, a.vb]), a.visitorOptedOut != p && (a.W = !0)), a.aa || a.analyticsVisitorID || !b.getAnalyticsVisitorID || (a.aa = !0, a.analyticsVisitorID = b.getAnalyticsVisitorID([a, a.$a]), a.analyticsVisitorID && (a.S = !0)), a.ca || a.audienceManagerLocationHint || !b.getAudienceManagerLocationHint || (a.ca = !0, a.audienceManagerLocationHint = b.getAudienceManagerLocationHint([a, a.bb]), a.audienceManagerLocationHint && (a.U = !0)), a.ba || a.audienceManagerBlob || !b.getAudienceManagerBlob || (a.ba =
                    !0, a.audienceManagerBlob = b.getAudienceManagerBlob([a, a.ab]), a.audienceManagerBlob && (a.T = !0)), c = a.da && !a.V && !a.marketingCloudVisitorID, b = a.aa && !a.S && !a.analyticsVisitorID, d = a.ca && !a.U && !a.audienceManagerLocationHint, f = a.ba && !a.T && !a.audienceManagerBlob, e = a.ga && !a.W, c = c || b || d || f || e ? !1 : !0); a.ea || a.I || (a.cb(a.za) ? a.I = !0 : a.ea = !0); a.ea && !a.I && (c = !1); return c
        }; a.o = p; a.u = 0; a.callbackWhenReadyToTrack = function (c, b, d) {
            var f; f = {}; f.zb = c; f.yb = b; f.wb = d; a.o == p && (a.o = []); a.o.push(f); 0 == a.u && (a.u = setInterval(a.j,
                100))
        }; a.j = function () { var c; if (a.isReadyToTrack() && (a.tb(), a.o != p)) for (; 0 < a.o.length;)c = a.o.shift(), c.yb.apply(c.zb, c.wb) }; a.tb = function () { a.u && (clearInterval(a.u), a.u = 0) }; a.mb = function (c) { var b, d, f = p, e = p; if (!a.isReadyToTrack()) { b = []; if (c != p) for (d in f = {}, c) f[d] = c[d]; e = {}; a.Ya(e, !0); b.push(f); b.push(e); a.callbackWhenReadyToTrack(a, a.track, b); return !0 } return !1 }; a.Gb = function () {
            var c = a.cookieRead("s_fid"), b = "", d = "", f; f = 8; var e = 4; if (!c || 0 > c.indexOf("-")) {
                for (c = 0; 16 > c; c++)f = Math.floor(Math.random() * f),
                    b += "0123456789ABCDEF".substring(f, f + 1), f = Math.floor(Math.random() * e), d += "0123456789ABCDEF".substring(f, f + 1), f = e = 16; c = b + "-" + d
            } a.cookieWrite("s_fid", c, 1) || (c = 0); return c
        }; a.t = a.track = function (c, b) {
            var d, f = new Date, e = "s" + Math.floor(f.getTime() / 108E5) % 10 + Math.floor(1E13 * Math.random()), g = f.getYear(), g = "t=" + a.escape(f.getDate() + "/" + f.getMonth() + "/" + (1900 > g ? g + 1900 : g) + " " + f.getHours() + ":" + f.getMinutes() + ":" + f.getSeconds() + " " + f.getDay() + " " + f.getTimezoneOffset()); a.visitor && a.visitor.getAuthState && (a.authState =
                a.visitor.getAuthState()); a.p("_s"); a.mb(c) || (b && a.R(b), c && (d = {}, a.Ya(d, 0), a.R(c)), a.Mb() && !a.visitorOptedOut && (a.analyticsVisitorID || a.marketingCloudVisitorID || (a.fid = a.Gb()), a.Pb(), a.usePlugins && a.doPlugins && a.doPlugins(a), a.account && (a.abort || (a.trackOffline && !a.timestamp && (a.timestamp = Math.floor(f.getTime() / 1E3)), f = k.location, a.pageURL || (a.pageURL = f.href ? f.href : f), a.referrer || a.Za || (f = a.Util.getQueryParam("adobe_mc_ref", null, null, !0), a.referrer = f || void 0 === f ? void 0 === f ? "" : f : n.document.referrer),
                    a.Za = 1, a.referrer = a.Eb(a.referrer), a.p("_g")), a.Jb() && !a.abort && (a.visitor && !a.supplementalDataID && a.visitor.getSupplementalDataID && (a.supplementalDataID = a.visitor.getSupplementalDataID("AppMeasurement:" + a._in, a.expectSupplementalData ? !1 : !0)), a.Kb(), g += a.Ib(), a.ob(e, g), a.p("_t"), a.referrer = ""))), c && a.R(d, 1)); a.abort = a.supplementalDataID = a.timestamp = a.pageURLRest = a.linkObject = a.clickObject = a.linkURL = a.linkName = a.linkType = k.s_objectID = a.pe = a.pev1 = a.pev2 = a.pev3 = a.e = a.lightProfileID = 0
        }; a.Ba = []; a.registerPreTrackCallback =
            function (c) { for (var b = [], d = 1; d < arguments.length; d++)b.push(arguments[d]); "function" == typeof c ? a.Ba.push([c, b]) : a.debugTracking && a.F("DEBUG: Non function type passed to registerPreTrackCallback") }; a.hb = function (c) { a.xa(a.Ba, c) }; a.Aa = []; a.registerPostTrackCallback = function (c) { for (var b = [], d = 1; d < arguments.length; d++)b.push(arguments[d]); "function" == typeof c ? a.Aa.push([c, b]) : a.debugTracking && a.F("DEBUG: Non function type passed to registerPostTrackCallback") }; a.gb = function (c) { a.xa(a.Aa, c) }; a.xa = function (c,
                b) { if ("object" == typeof c) for (var d = 0; d < c.length; d++) { var f = c[d][0], e = c[d][1]; e.unshift(b); if ("function" == typeof f) try { f.apply(null, e) } catch (g) { a.debugTracking && a.F(g.message) } } }; a.tl = a.trackLink = function (c, b, d, f, e) { a.linkObject = c; a.linkType = b; a.linkName = d; e && (a.l = c, a.A = e); return a.track(f) }; a.trackLight = function (c, b, d, f) { a.lightProfileID = c; a.lightStoreForSeconds = b; a.lightIncrementBy = d; return a.track(f) }; a.clearVars = function () {
                    var c, b; for (c = 0; c < a.g.length; c++)if (b = a.g[c], "prop" == b.substring(0, 4) ||
                        "eVar" == b.substring(0, 4) || "hier" == b.substring(0, 4) || "list" == b.substring(0, 4) || "channel" == b || "events" == b || "eventList" == b || "products" == b || "productList" == b || "purchaseID" == b || "transactionID" == b || "state" == b || "zip" == b || "campaign" == b) a[b] = void 0
                }; a.tagContainerMarker = ""; a.ob = function (c, b) { var d = a.ib() + "/" + c + "?AQB=1&ndh=1&pf=1&" + (a.ya() ? "callback=s_c_il[" + a._in + "].doPostbacks&et=1&" : "") + b + "&AQE=1"; a.hb(d); a.fb(d); a.X() }; a.ib = function () {
                    var c = a.jb(); return "http" + (a.ssl ? "s" : "") + "://" + c + "/b/ss/" + a.account + "/" +
                        (a.mobile ? "5." : "") + (a.ya() ? "10" : "1") + "/JS-" + a.version + (a.Sb ? "T" : "") + (a.tagContainerMarker ? "-" + a.tagContainerMarker : "")
                }; a.ya = function () { return a.AudienceManagement && a.AudienceManagement.isReady() || 0 != a.usePostbacks }; a.jb = function () { var c = a.dc, b = a.trackingServer; b ? a.trackingServerSecure && a.ssl && (b = a.trackingServerSecure) : (c = c ? ("" + c).toLowerCase() : "d1", "d1" == c ? c = "112" : "d2" == c && (c = "122"), b = a.lb() + "." + c + ".2o7.net"); return b }; a.lb = function () {
                    var c = a.visitorNamespace; c || (c = a.account.split(",")[0], c = c.replace(/[^0-9a-z]/gi,
                        "")); return c
                }; a.Xa = /{(%?)(.*?)(%?)}/; a.Wb = RegExp(a.Xa.source, "g"); a.Db = function (c) { if ("object" == typeof c.dests) for (var b = 0; b < c.dests.length; ++b) { var d = c.dests[b]; if ("string" == typeof d.c && "aa." == d.id.substr(0, 3)) for (var f = d.c.match(a.Wb), e = 0; e < f.length; ++e) { var g = f[e], h = g.match(a.Xa), k = ""; "%" == h[1] && "timezone_offset" == h[2] ? k = (new Date).getTimezoneOffset() : "%" == h[1] && "timestampz" == h[2] && (k = a.Hb()); d.c = d.c.replace(g, a.escape(k)) } } }; a.Hb = function () {
                    var c = new Date, b = new Date(6E4 * Math.abs(c.getTimezoneOffset()));
                    return a.k(4, c.getFullYear()) + "-" + a.k(2, c.getMonth() + 1) + "-" + a.k(2, c.getDate()) + "T" + a.k(2, c.getHours()) + ":" + a.k(2, c.getMinutes()) + ":" + a.k(2, c.getSeconds()) + (0 < c.getTimezoneOffset() ? "-" : "+") + a.k(2, b.getUTCHours()) + ":" + a.k(2, b.getUTCMinutes())
                }; a.k = function (a, b) { return (Array(a + 1).join(0) + b).slice(-a) }; a.ua = {}; a.doPostbacks = function (c) {
                    if ("object" == typeof c) if (a.Db(c), "object" == typeof a.AudienceManagement && "function" == typeof a.AudienceManagement.isReady && a.AudienceManagement.isReady() && "function" == typeof a.AudienceManagement.passData) a.AudienceManagement.passData(c);
                    else if ("object" == typeof c && "object" == typeof c.dests) for (var b = 0; b < c.dests.length; ++b) { var d = c.dests[b]; "object" == typeof d && "string" == typeof d.c && "string" == typeof d.id && "aa." == d.id.substr(0, 3) && (a.ua[d.id] = new Image, a.ua[d.id].alt = "", a.ua[d.id].src = d.c) }
                }; a.fb = function (c) { a.i || a.Lb(); a.i.push(c); a.ma = a.C(); a.Va() }; a.Lb = function () { a.i = a.Nb(); a.i || (a.i = []) }; a.Nb = function () { var c, b; if (a.ta()) { try { (b = k.localStorage.getItem(a.qa())) && (c = k.JSON.parse(b)) } catch (d) { } return c } }; a.ta = function () {
                    var c = !0; a.trackOffline &&
                        a.offlineFilename && k.localStorage && k.JSON || (c = !1); return c
                }; a.La = function () { var c = 0; a.i && (c = a.i.length); a.q && c++; return c }; a.X = function () { if (a.q && (a.B && a.B.complete && a.B.G && a.B.wa(), a.q)) return; a.Ma = p; if (a.ra) a.ma > a.O && a.Ta(a.i), a.va(500); else { var c = a.xb(); if (0 < c) a.va(c); else if (c = a.Ia()) a.q = 1, a.Ob(c), a.Rb(c) } }; a.va = function (c) { a.Ma || (c || (c = 0), a.Ma = setTimeout(a.X, c)) }; a.xb = function () {
                    var c; if (!a.trackOffline || 0 >= a.offlineThrottleDelay) return 0; c = a.C() - a.Ra; return a.offlineThrottleDelay < c ? 0 : a.offlineThrottleDelay -
                        c
                }; a.Ia = function () { if (0 < a.i.length) return a.i.shift() }; a.Ob = function (c) { if (a.debugTracking) { var b = "AppMeasurement Debug: " + c; c = c.split("&"); var d; for (d = 0; d < c.length; d++)b += "\n\t" + a.unescape(c[d]); a.F(b) } }; a.nb = function () { return a.marketingCloudVisitorID || a.analyticsVisitorID }; a.Z = !1; var t; try { t = JSON.parse('{"x":"y"}') } catch (w) { t = null } t && "y" == t.x ? (a.Z = !0, a.Y = function (a) { return JSON.parse(a) }) : k.$ && k.$.parseJSON ? (a.Y = function (a) { return k.$.parseJSON(a) }, a.Z = !0) : a.Y = function () { return null }; a.Rb = function (c) {
                    var b,
                        d, f; a.nb() && 2047 < c.length && ("undefined" != typeof XMLHttpRequest && (b = new XMLHttpRequest, "withCredentials" in b ? d = 1 : b = 0), b || "undefined" == typeof XDomainRequest || (b = new XDomainRequest, d = 2), b && (a.AudienceManagement && a.AudienceManagement.isReady() || 0 != a.usePostbacks) && (a.Z ? b.Da = !0 : b = 0)); !b && a.Wa && (c = c.substring(0, 2047)); !b && a.d.createElement && (0 != a.usePostbacks || a.AudienceManagement && a.AudienceManagement.isReady()) && (b = a.d.createElement("SCRIPT")) && "async" in b && ((f = (f = a.d.getElementsByTagName("HEAD")) && f[0] ?
                            f[0] : a.d.body) ? (b.type = "text/javascript", b.setAttribute("async", "async"), d = 3) : b = 0); b || (b = new Image, b.alt = "", b.abort || "undefined" === typeof k.InstallTrigger || (b.abort = function () { b.src = p })); b.Sa = Date.now(); b.Fa = function () { try { b.G && (clearTimeout(b.G), b.G = 0) } catch (a) { } }; b.onload = b.wa = function () { b.Sa && (a.na = Date.now() - b.Sa); a.gb(c); b.Fa(); a.Bb(); a.ha(); a.q = 0; a.X(); if (b.Da) { b.Da = !1; try { a.doPostbacks(a.Y(b.responseText)) } catch (d) { } } }; b.onabort = b.onerror = b.Ja = function () {
                                b.Fa(); (a.trackOffline || a.ra) && a.q &&
                                    a.i.unshift(a.Ab); a.q = 0; a.ma > a.O && a.Ta(a.i); a.ha(); a.va(500)
                            }; b.onreadystatechange = function () { 4 == b.readyState && (200 == b.status ? b.wa() : b.Ja()) }; a.Ra = a.C(); if (1 == d || 2 == d) { var e = c.indexOf("?"); f = c.substring(0, e); e = c.substring(e + 1); e = e.replace(/&callback=[a-zA-Z0-9_.\[\]]+/, ""); 1 == d ? (b.open("POST", f, !0), b.send(e)) : 2 == d && (b.open("POST", f), b.send(e)) } else if (b.src = c, 3 == d) { if (a.Pa) try { f.removeChild(a.Pa) } catch (g) { } f.firstChild ? f.insertBefore(b, f.firstChild) : f.appendChild(b); a.Pa = a.B } b.G = setTimeout(function () {
                                b.G &&
                                    (b.complete ? b.wa() : (a.trackOffline && b.abort && b.abort(), b.Ja()))
                            }, 5E3); a.Ab = c; a.B = k["s_i_" + a.replace(a.account, ",", "_")] = b; if (a.useForcedLinkTracking && a.K || a.A) a.forcedLinkTrackingTimeout || (a.forcedLinkTrackingTimeout = 250), a.ia = setTimeout(a.ha, a.forcedLinkTrackingTimeout)
                }; a.Bb = function () { if (a.ta() && !(a.Qa > a.O)) try { k.localStorage.removeItem(a.qa()), a.Qa = a.C() } catch (c) { } }; a.Ta = function (c) { if (a.ta()) { a.Va(); try { k.localStorage.setItem(a.qa(), k.JSON.stringify(c)), a.O = a.C() } catch (b) { } } }; a.Va = function () {
                    if (a.trackOffline) {
                        if (!a.offlineLimit ||
                            0 >= a.offlineLimit) a.offlineLimit = 10; for (; a.i.length > a.offlineLimit;)a.Ia()
                    }
                }; a.forceOffline = function () { a.ra = !0 }; a.forceOnline = function () { a.ra = !1 }; a.qa = function () { return a.offlineFilename + "-" + a.visitorNamespace + a.account }; a.C = function () { return (new Date).getTime() }; a.Na = function (a) { a = a.toLowerCase(); return 0 != a.indexOf("#") && 0 != a.indexOf("about:") && 0 != a.indexOf("opera:") && 0 != a.indexOf("javascript:") ? !0 : !1 }; a.setTagContainer = function (c) {
                    var b, d, f; a.Sb = c; for (b = 0; b < a._il.length; b++)if ((d = a._il[b]) && "s_l" ==
                        d._c && d.tagContainerName == c) { a.R(d); if (d.lmq) for (b = 0; b < d.lmq.length; b++)f = d.lmq[b], a.loadModule(f.n); if (d.ml) for (f in d.ml) if (a[f]) for (b in c = a[f], f = d.ml[f], f) !Object.prototype[b] && ("function" != typeof f[b] || 0 > ("" + f[b]).indexOf("s_c_il")) && (c[b] = f[b]); if (d.mmq) for (b = 0; b < d.mmq.length; b++)f = d.mmq[b], a[f.m] && (c = a[f.m], c[f.f] && "function" == typeof c[f.f] && (f.a ? c[f.f].apply(c, f.a) : c[f.f].apply(c))); if (d.tq) for (b = 0; b < d.tq.length; b++)a.track(d.tq[b]); d.s = a; break }
                }; a.Util = {
                    urlEncode: a.escape, urlDecode: a.unescape,
                    cookieRead: a.cookieRead, cookieWrite: a.cookieWrite, getQueryParam: function (c, b, d, f) { var e, g = ""; b || (b = a.pageURL ? a.pageURL : k.location); d = d ? d : "&"; if (!c || !b) return g; b = "" + b; e = b.indexOf("?"); if (0 > e) return g; b = d + b.substring(e + 1) + d; if (!f || !(0 <= b.indexOf(d + c + d) || 0 <= b.indexOf(d + c + "=" + d))) { e = b.indexOf("#"); 0 <= e && (b = b.substr(0, e) + d); e = b.indexOf(d + c + "="); if (0 > e) return g; b = b.substring(e + d.length + c.length + 1); e = b.indexOf(d); 0 <= e && (b = b.substring(0, e)); 0 < b.length && (g = a.unescape(b)); return g } }, getIeVersion: function () {
                        if (document.documentMode) return document.documentMode;
                        for (var a = 7; 4 < a; a--) { var b = document.createElement("div"); b.innerHTML = "\x3c!--[if IE " + a + "]><span></span><![endif]--\x3e"; if (b.getElementsByTagName("span").length) return a } return null
                    }
                }; a.H = "supplementalDataID timestamp dynamicVariablePrefix visitorID marketingCloudVisitorID analyticsVisitorID audienceManagerLocationHint authState fid vmk visitorMigrationKey visitorMigrationServer visitorMigrationServerSecure charSet visitorNamespace cookieDomainPeriods fpCookieDomainPeriods cookieLifetime pageName pageURL customerPerspective referrer contextData currencyCode lightProfileID lightStoreForSeconds lightIncrementBy retrieveLightProfiles deleteLightProfiles retrieveLightData".split(" ");
    a.g = a.H.concat("purchaseID variableProvider channel server pageType transactionID campaign state zip events events2 products audienceManagerBlob tnt".split(" ")); a.oa = "timestamp charSet visitorNamespace cookieDomainPeriods cookieLifetime contextData lightProfileID lightStoreForSeconds lightIncrementBy".split(" "); a.P = a.oa.slice(0); a.Ca = "account allAccounts debugTracking visitor visitorOptedOut trackOffline offlineLimit offlineThrottleDelay offlineFilename usePlugins doPlugins configURL visitorSampling visitorSamplingGroup linkObject clickObject linkURL linkName linkType trackDownloadLinks trackExternalLinks trackClickMap trackInlineStats linkLeaveQueryString linkTrackVars linkTrackEvents linkDownloadFileTypes linkExternalFilters linkInternalFilters useForcedLinkTracking forcedLinkTrackingTimeout trackingServer trackingServerSecure ssl abort mobile dc lightTrackVars maxDelay expectSupplementalData usePostbacks registerPreTrackCallback registerPostTrackCallback AudienceManagement".split(" ");
    for (m = 0; 250 >= m; m++)76 > m && (a.g.push("prop" + m), a.P.push("prop" + m)), a.g.push("eVar" + m), a.P.push("eVar" + m), 6 > m && a.g.push("hier" + m), 4 > m && a.g.push("list" + m); m = "pe pev1 pev2 pev3 latitude longitude resolution colorDepth javascriptVersion javaEnabled cookiesEnabled browserWidth browserHeight connectionType homepage pageURLRest marketingCloudOrgID".split(" "); a.g = a.g.concat(m); a.H = a.H.concat(m); a.ssl = 0 <= k.location.protocol.toLowerCase().indexOf("https"); a.charSet = "UTF-8"; a.contextData = {}; a.offlineThrottleDelay =
        0; a.offlineFilename = "AppMeasurement.offline"; a.Ra = 0; a.ma = 0; a.O = 0; a.Qa = 0; a.linkDownloadFileTypes = "exe,zip,wav,mp3,mov,mpg,avi,wmv,pdf,doc,docx,xls,xlsx,ppt,pptx"; a.w = k; a.d = k.document; try { if (a.Wa = !1, navigator) { var v = navigator.userAgent; if ("Microsoft Internet Explorer" == navigator.appName || 0 <= v.indexOf("MSIE ") || 0 <= v.indexOf("Trident/") && 0 <= v.indexOf("Windows NT 6")) a.Wa = !0 } } catch (x) { } a.ha = function () {
            a.ia && (k.clearTimeout(a.ia), a.ia = p); a.l && a.K && a.l.dispatchEvent(a.K); a.A && ("function" == typeof a.A ? a.A() :
                a.l && a.l.href && (a.d.location = a.l.href)); a.l = a.K = a.A = 0
        }; a.Ua = function () {
            a.b = a.d.body; a.b ? (a.v = function (c) {
                var b, d, f, e, g; if (!(a.d && a.d.getElementById("cppXYctnr") || c && c["s_fe_" + a._in])) {
                    if (a.Ea) if (a.useForcedLinkTracking) a.b.removeEventListener("click", a.v, !1); else { a.b.removeEventListener("click", a.v, !0); a.Ea = a.useForcedLinkTracking = 0; return } else a.useForcedLinkTracking = 0; a.clickObject = c.srcElement ? c.srcElement : c.target; try {
                        if (!a.clickObject || a.N && a.N == a.clickObject || !(a.clickObject.tagName || a.clickObject.parentElement ||
                            a.clickObject.parentNode)) a.clickObject = 0; else {
                            var h = a.N = a.clickObject; a.la && (clearTimeout(a.la), a.la = 0); a.la = setTimeout(function () { a.N == h && (a.N = 0) }, 1E4); f = a.La(); a.track(); if (f < a.La() && a.useForcedLinkTracking && c.target) {
                                for (e = c.target; e && e != a.b && "A" != e.tagName.toUpperCase() && "AREA" != e.tagName.toUpperCase();)e = e.parentNode; if (e && (g = e.href, a.Na(g) || (g = 0), d = e.target, c.target.dispatchEvent && g && (!d || "_self" == d || "_top" == d || "_parent" == d || k.name && d == k.name))) {
                                    try { b = a.d.createEvent("MouseEvents") } catch (l) {
                                        b =
                                            new k.MouseEvent
                                    } if (b) { try { b.initMouseEvent("click", c.bubbles, c.cancelable, c.view, c.detail, c.screenX, c.screenY, c.clientX, c.clientY, c.ctrlKey, c.altKey, c.shiftKey, c.metaKey, c.button, c.relatedTarget) } catch (m) { b = 0 } b && (b["s_fe_" + a._in] = b.s_fe = 1, c.stopPropagation(), c.stopImmediatePropagation && c.stopImmediatePropagation(), c.preventDefault(), a.l = c.target, a.K = b) }
                                }
                            }
                        }
                    } catch (n) { a.clickObject = 0 }
                }
            }, a.b && a.b.attachEvent ? a.b.attachEvent("onclick", a.v) : a.b && a.b.addEventListener && (navigator && (0 <= navigator.userAgent.indexOf("WebKit") &&
                a.d.createEvent || 0 <= navigator.userAgent.indexOf("Firefox/2") && k.MouseEvent) && (a.Ea = 1, a.useForcedLinkTracking = 1, a.b.addEventListener("click", a.v, !0)), a.b.addEventListener("click", a.v, !1))) : setTimeout(a.Ua, 30)
        }; a.Cb(); a.ac || (r ? a.setAccount(r) : a.F("Error, missing Report Suite ID in AppMeasurement initialization"), a.Ua(), a.loadModule("ActivityMap"))
}
function s_gi(r) { var a, k = window.s_c_il, p, n, m = r.split(","), s, u, t = 0; if (k) for (p = 0; !t && p < k.length;) { a = k[p]; if ("s_c" == a._c && (a.account || a.oun)) if (a.account && a.account == r) t = 1; else for (n = a.account ? a.account : a.oun, n = a.allAccounts ? a.allAccounts : n.split(","), s = 0; s < m.length; s++)for (u = 0; u < n.length; u++)m[s] == n[u] && (t = 1); p++ } t ? a.setAccount && a.setAccount(r) : a = new AppMeasurement(r); return a } AppMeasurement.getInstance = s_gi; window.s_objectID || (window.s_objectID = 0);
function s_pgicq() { var r = window, a = r.s_giq, k, p, n; if (a) for (k = 0; k < a.length; k++)p = a[k], n = s_gi(p.oun), n.setAccount(p.un), n.setTagContainer(p.tagContainerName); r.s_giq = 0 } s_pgicq();
