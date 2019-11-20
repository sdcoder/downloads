



$(function () {
    if (!$('body').data('PBI29082')) {
        //$('#rates').prepend('<div class="benefits-bar-wrapper"><div class="row benefits-bar"><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/Auto.png" class="auto" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR to 6.54%* APR with AutoPay Rates vary by loan purpose</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/kitchen.png" class="kitchen" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/debt.png" class="debt" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/swimmingpool.png" class="swimmingpool" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/swimmingpool.png" class="swimmingpool" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/wedding.png" class="wedding" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/timeshare.png" class="timeshare" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div><div class="card-grid" style="perspective: 200px; position: relative; transform-style: preserve-3d;"><div class="front" style="backface-visibility: hidden; transform-style: preserve-3d; position: absolute; z-index: 1; height: 100%; width: 100%; transition: all 0.5s ease-out; transform: rotateY(0deg);"><img src="/Content/images/tiles/boat.png" class="boat" /></div><div class="back" style="transform: rotateY(-180deg); z-index: 0; backface-visibility: hidden; transform-style: preserve-3d; position: absolute; height: 100%; width: 100%; transition: all 0.5s ease-out;"><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div></div></div>');

        //$('article.rate-calculator').prepend('<div class="benefits-bar-wrapper"><div class="row benefits-bar"><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/Auto.png" class="auto" /><p>Fixed rates as low as 1.99%* APR to 6.54%* APR with AutoPay Rates vary by loan purpose</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/kitchen.png" class="bank" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/debt.png" class="bank" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/swimmingpool.png" class="bank" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/swimmingpool.png" class="bank" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/wedding.png" class="bank" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div><div class="medium-4 columns"><img src="/Content/images/tiles/timeshare.png" class="nofees" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/images/tiles/boat.png" class="ratebeat" /><p>Fixed rates as low as 1.99%* APR with AutoPay</p></div></div></div>');
        //$('article.rate-calculator').prepend('<div class="benefits-bar-wrapper"><div class="row benefits-bar"><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29086/icon_bank_reverse.png" class="bank" /><p>Part of one of the nation\'s STRONGEST BANKS</p></div><div class="medium-4 columns"><img src="/Content/tnt/29086/icon_nofees_reverse.png" class="nofees" /><p>NO FEES,<br/> No home equity required</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29086/icon_ratebeat_reverse.png" class="ratebeat" /><p>WE\'LL BEAT ANY<br/>competitor\'s<br/>qualifying rate <sup>2</sup></p></div></div></div>');

        //$('.ratedetails').prepend($('header h4.subtitle.cms'));
        //$('.ratedetails').prepend($('header h2.cms'));

        //$('header img.hide-for-small.banner').attr('src', '/content/tnt/29086/desktop-header-wide.jpg').css('opacity', '100');
        //$('header img.show-for-small.banner').attr('src', '/content/tnt/29086/desktop-header-wide.jpg').css('opacity', '100');
        //$rateTable = $('article.rate-calculator div.medium-8.medium-pull-4.columns');
        //$rateTable.removeClass('medium-pull-4').removeClass('medium-8').addClass('medium-12');
        //$('.heroaction').find('h2.cms').text('Swimming Pool Loans');
        //$rateTable.find('section.row div.medium-9.columns').addClass('large-12').find('h1').text('Annual Percentage Rates (APR)');

        //
        //$('li[data-nav="sign-in"]').remove();

        //$('.featured_testimonial').remove();

        //$('<div class="row"><div class="columns apply-button small-8 medium-4 large-3"><a href="/apply" class="button" ng-click="applyFor(\'HomeImprovement\');">Apply Now</a></div></div>').insertAfter($('.terms-container'));

        //$('#RateTableRateBeatLogo')
        //    .removeClass('hide-for-small medium-3')
        //    .addClass('small-4 medium-2 large-2 end')
        //    .detach()
        //    .insertAfter($('.apply-button'));

        //$('#RateTableCurrentAPRRow')
        //    .removeClass('medium-9 large-12')
        //    .addClass('small-12 medium-12 large-12');

        //$('time').parent()
        //    .removeClass('medium-4')
        //    .addClass('time-container small-12 medium-12 large-12')
        //    .detach()
        //    .insertBefore($('#RateTableCurrentAPRRow'));

        //$('.important-legalcopy').parent()
        //    .removeClass('medium-12')
        //    .addClass('important-legal-copy-container small-12 medium-12 large-12');

        //$('#DisplayRate').text('1');


        var mq = window.matchMedia("(max-width: 767px)");
        if (mq.matches) {
            // window width is under 768px
            //
            //$('#show_menu').remove();

            //$('#DisplayRate').text('1').css('font-size', '10px').css('color', '#ABE2F7').css('top', '-15px');

            setTimeout(function () {
                $('table.loanrates tr')[2].click();
                $('table.loanrates tr')[3].click();
            }, 500);
        } else {
            mq = window.matchMedia("(max-width: 1000px)");
            if (mq.matches) {
                //$('#DisplayRate').text('1').css('font-size', '14px').css('color', '#ABE2F7').css('top', '-39px');
            } else {
                //$('#DisplayRate').text('1').css('font-size', '14px').css('color', '#ABE2F7').css('top', '-32px');
            }
        }
        //$('#AprDisclosure').html(function () {
        //    return $(this).html().replace('*', '<sup>1</sup>');
        //}).append('<br /><br /><p><sup>2</sup> You can fund your loan today if today is a banking business day, your application is approved, and you complete the following steps by 2:30 p.m. Eastern time: (1) review and electronically sign your loan agreement; (2) provide us with your funding preferences and relevant banking information; and (3) complete the final verification process.</p><p><sup>3</sup> After receiving your loan from us, if you are not completely satisfied with your experience, please contact us. We will email you a questionnaire so we can improve our services. When we receive your completed questionnaire, we will send you $100. Our guarantee expires 30 days after you receive your loan. We reserve the right to change or discontinue our guarantee at any time. Limited to one $100 payment per funded loan. LightStream and SunTrust teammates do not qualify for the Loan Experience Guarantee.</p>');
        //$('.heroaction.clearfix').show();
        //$('body').data('PBI18307', true);
        //$('.headerbar').removeClass('darktint');
        //$('.navbar').css('opacity', '100');
        //$('.mainmenu').parent().append('<div class="excellent-credit">For those with good to excellent credit.')

        //$rateGrid = $('#RateTable div.medium-12.columns.table');
        //$rateGrid.removeClass('medium-12').addClass('large-12').addClass('medium-8');

        //$loanTerms = $('div.medium-12.columns.terms-container');
        //$rateGrid.parent().append($loanTerms.removeClass('medium-12').addClass('medium-4').addClass('large-12'));

        //$('body').css('opacity', '100');
        //$('a[href="/partial/cms/excellent-credit"]').replaceWith('excellent credit').click(function () {
        //    return false
        //});

        // future design
        //if (!$('#RateTableCurrentAPRRow')) {
        //    $('#rates div.columns.medium-12:first').hide();
        //    $('#rates div.panel').removeClass('panel');
        //}
        //$('#rates a[href="/apply"]').hide();
        //$('.rate-calculator').append('<div class="row benefits-bar"><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29082/Rate_icon.png" class="rate" /><p>Get the low rate you deserve.Starting as low as 1.99%* APR with AutoPay when you purchase from a dealer, our fixed-rate used car loans feature flexible loan amounts from $5,000 to $100,000.</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29082/Car_icon.png" class="car" /><p>Be a cash buyer and get any car you want.LightStream deposits funds direclty to your account, which saves tiem at th dealer, and gives you the power to negotiate a better deal with any dealer or private seller.</p></div><div class="medium-4 columns"><img src="/Content/tnt/29082/signarticle_icon.png" class="signarticle" /><p>Same-day funding available3.With LightStream you\'re in control of the lending process. You choose your funding date. In fact, you can even have funds deposited in your account as soon as the same day you apply.</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29082/check_icon.png" class="check" /><p>We\'ll beat the lowest rate you can find2.Many lenders say they have low rates. How many will match the lowest documented APR you can find? We do, because we\'re truly confident that our rates are always competitive.</p></div></div></div>');

        //$('footer').prepend('<div class="get-started"><img src="/Content/tnt/29082/sky.jpg" class="sky" />Lending uncomplicated. Only from LightStream. You earned it. <div class="row"><div class="columns apply-button small-8 medium-4 large-3"><a href="/apply" class="button" ng-click="applyFor(\'HomeImprovement\');">Get Started</a></div></div></div><div class="row benefits-bar"><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29086/icon_bank_reverse.png" class="bank" /><p>Part of one of the nation\'s STRONGEST BANKS</p></div><div class="medium-4 columns"><img src="/Content/tnt/29086/icon_nofees_reverse.png" class="nofees" /><p>NO FEES,<br/> No home equity required</p></div><div class="medium-4 columns hide-for-small"><img src="/Content/tnt/29086/icon_ratebeat_reverse.png" class="ratebeat" /><p>WE\'LL BEAT ANY<br/>competitor\'s<br/>qualifying rate <sup>2</sup></p></div></div></div>');

        /////
        //$('.homefeatures.hide-for-small-only').find('div:eq(0)');
        //$('.homefeatures.hide-for-small-only').find('div:eq(0)').find('.hideie8').attr('src', '/Content/tnt/29082/Rate_icon.png');
        //$('.homefeatures.hide-for-small-only').find('div:eq(0)').find('.showie8').attr('src', '/Content/tnt/29082/Rate_icon.svg');
        //$('.homefeatures.hide-for-small-only').find('div:eq(0)').append('<p>Starting as low as 1.99%* APR with AutoPay when you purchase from a dealer, our fixed-rate used car loans feature flexible loan amounts from $5,000 to $100,000.</p>');

        //$('.homefeatures.hide-for-small-only').find('div:eq(1)');
        //$('.homefeatures.hide-for-small-only').find('div:eq(1)').find('.hideie8').attr('src', '/Content/tnt/29082/Car_icon.png');
        //$('.homefeatures.hide-for-small-only').find('div:eq(1)').find('.showie8').attr('src', '/Content/tnt/29082/Car_icon.svg');
        //$('.homefeatures.hide-for-small-only').find('div:eq(1)').append('<p>LightStream deposits funds direclty to your account, which saves tiem at th dealer, and gives you the power to negotiate a better deal with any dealer or private seller.</p>');

        //$('.homefeatures.hide-for-small-only').find('div:eq(2)');
        //$('.homefeatures.hide-for-small-only').find('div:eq(2)').find('.hideie8').attr('src', '/Content/tnt/29082/signarticle_icon.png');
        //$('.homefeatures.hide-for-small-only').find('div:eq(2)').find('.showie8').attr('src', '/Content/tnt/29082/signarticle_icon.svg');
        //$('.homefeatures.hide-for-small-only').find('div:eq(2)').append('<p>With LightStream you\'re in control of the lending process. You choose your funding date. In fact, you can even have funds deposited in your account as soon as the same day you apply.</p>');

        //$('.homefeatures.hide-for-small-only').find('div:eq(3)');
        //$('.homefeatures.hide-for-small-only').find('div:eq(3)').find('.hideie8').attr('src', '/Content/tnt/29082/check_icon.png');
        //$('.homefeatures.hide-for-small-only').find('div:eq(3)').find('.showie8').attr('src', '/Content/tnt/29082/check_icon.svg');
        //$('.homefeatures.hide-for-small-only').find('div:eq(3)').append('<p>Many lenders say they have low rates. How many will match the lowest documented APR you can find? We do, because we\'re truly confident that our rates are always competitive.</p>');

        $('.featured_testimonial');

        // Call Quovolver on the '.quotes' object
        $('.testimonials_customer').quovolver({
            children: 'li',
            transitionSpeed: 600,
            autoPlay: false,
            equalHeight: true,
            navPosition: 'below',
            navNum: true
        });

        // Call Quovolver on the '.quotes' object
        $('.testimonials_media').quovolver({
            children: 'li',
            transitionSpeed: 600,
            autoPlay: false,
            equalHeight: true,
            navPosition: 'below',
            navNum: true
        });


    }
});

//$(document).ready(function () {
//    /* The following code is executed once the DOM is loaded */

//    $('.sponsorFlip').bind("click", function () {

//        // $(this) point to the clicked .sponsorFlip element (caching it in elem for speed):

//        var elem = $(this);

//        // data('flipped') is a flag we set when we flip the element:

//        if (elem.data('flipped')) {
//            // If the element has already been flipped, use the revertFlip method
//            // defined by the plug-in to revert to the default state automatically:

//            elem.revertFlip();

//            // Unsetting the flag:
//            elem.data('flipped', false)
//        }
//        else {
//            // Using the flip method defined by the plugin:

//            elem.flip({
//                direction: 'lr',
//                speed: 750,
//                onBefore: function () {
//                    // Insert the contents of the .sponsorData div (hidden
//                    // from view with display:none) into the clicked
//                    // .sponsorFlip div before the flipping animation starts:

//                    elem.html(elem.siblings('.sponsorData').html());
//                }
//            });

//            // Setting the flag:
//            elem.data('flipped', true);
//        }
//    });

//});

///
//    eval(function (p, a, c, k, e, r) { e = function (c) { return (c < a ? '' : e(parseInt(c / a))) + ((c = c % a) > 35 ? String.fromCharCode(c + 29) : c.toString(36)) }; if (!''.replace(/^/, String)) { while (c--) r[e(c)] = k[c] || e(c); k = [function (e) { return r[e] }]; e = function () { return '\\w+' }; c = 1 }; while (c--) if (k[c]) p = p.replace(new RegExp('\\b' + e(c) + '\\b', 'g'), k[c]); return p }('(5($){5 H(a){a.1D.1f[a.1E]=1F(a.1G,10)+a.1H}6 j=5(a){1I({1J:"1g.Z.1K 1L 1M",1N:a})};6 k=5(){7(/*@1O!@*/11&&(1P 1Q.1h.1f.1R==="1S"))};6 l={1T:[0,4,4],1U:[1i,4,4],1V:[1j,1j,1W],1X:[0,0,0],1Y:[0,0,4],1Z:[1k,1l,1l],20:[0,4,4],21:[0,0,A],22:[0,A,A],23:[12,12,12],24:[0,13,0],26:[27,28,1m],29:[A,0,A],2a:[2b,1m,2c],2d:[4,1n,0],2e:[2f,2g,2h],2i:[A,0,0],2j:[2k,2l,2m],2n:[2o,0,R],2p:[4,0,4],2q:[4,2r,0],2s:[0,t,0],2t:[2u,0,2v],2w:[1i,1o,1n],2x:[2y,2z,1o],2A:[1p,4,4],2B:[1q,2C,1q],2D:[R,R,R],2E:[4,2F,2G],2H:[4,4,1p],2I:[0,4,0],2J:[4,0,4],2K:[t,0,0],2L:[0,0,t],2M:[t,t,0],2N:[4,1k,0],2O:[4,S,2P],2Q:[t,0,t],2R:[t,0,t],2S:[4,0,0],2T:[S,S,S],2U:[4,4,4],2V:[4,4,0],9:[4,4,4]};6 m=5(a){T(a&&a.1r("#")==-1&&a.1r("(")==-1){7"2W("+l[a].2X()+")"}2Y{7 a}};$.2Z($.30.31,{u:H,v:H,w:H,x:H});$.1s.32=5(){7 U.1t(5(){6 a=$(U);a.Z(a.B(\'1u\'))})};$.1s.Z=5(i){7 U.1t(5(){6 c=$(U),3,$8,C,14,15,16=k();T(c.B(\'V\')){7 11}6 e={I:(5(a){33(a){W"X":7"Y";W"Y":7"X";W"17":7"18";W"18":7"17";34:7"Y"}})(i.I),y:m(i.D)||"#E",D:m(i.y)||c.z("19-D"),1v:c.J(),F:i.F||1w,K:i.K||5(){},L:i.L||5(){},M:i.M||5(){}};c.B(\'1u\',e).B(\'V\',1).B(\'35\',e);3={s:c.s(),n:c.n(),y:m(i.y)||c.z("19-D"),1x:c.z("36-37")||"38",I:i.I||"X",G:m(i.D)||"#E",F:i.F||1w,o:c.1y().o,p:c.1y().p,1z:i.1v||39,9:"9",1a:i.1a||11,K:i.K||5(){},L:i.L||5(){},M:i.M||5(){}};16&&(3.9="#3a");$8=c.z("1b","3b").8(3c).B(\'V\',1).3d("1h").J("").z({1b:"1A",3e:"3f",p:3.p,o:3.o,3g:0,3h:3i});6 f=5(){7{1B:3.9,1x:0,3j:0,u:0,w:0,x:0,v:0,N:3.9,O:3.9,P:3.9,Q:3.9,19:"3k",3l:\'3m\',n:0,s:0}};6 g=5(){6 a=(3.n/13)*25;6 b=f();b.s=3.s;7{"q":b,"1c":{u:0,w:a,x:a,v:0,N:\'#E\',O:\'#E\',o:(3.o+(3.n/2)),p:(3.p-a)},"r":{v:0,u:0,w:0,x:0,N:3.9,O:3.9,o:3.o,p:3.p}}};6 h=5(){6 a=(3.n/13)*25;6 b=f();b.n=3.n;7{"q":b,"1c":{u:a,w:0,x:0,v:a,P:\'#E\',Q:\'#E\',o:3.o-a,p:3.p+(3.s/2)},"r":{u:0,w:0,x:0,v:0,P:3.9,Q:3.9,o:3.o,p:3.p}}};14={"X":5(){6 d=g();d.q.u=3.n;d.q.N=3.y;d.r.v=3.n;d.r.O=3.G;7 d},"Y":5(){6 d=g();d.q.v=3.n;d.q.O=3.y;d.r.u=3.n;d.r.N=3.G;7 d},"17":5(){6 d=h();d.q.w=3.s;d.q.P=3.y;d.r.x=3.s;d.r.Q=3.G;7 d},"18":5(){6 d=h();d.q.x=3.s;d.q.Q=3.y;d.r.w=3.s;d.r.P=3.G;7 d}};C=14[3.I]();16&&(C.q.3n="3o(D="+3.9+")");15=5(){6 a=3.1z;7 a&&a.1g?a.J():a};$8.1d(5(){3.K($8,c);$8.J(\'\').z(C.q);$8.1e()});$8.1C(C.1c,3.F);$8.1d(5(){3.M($8,c);$8.1e()});$8.1C(C.r,3.F);$8.1d(5(){T(!3.1a){c.z({1B:3.G})}c.z({1b:"1A"});6 a=15();T(a){c.J(a)}$8.3p();3.L($8,c);c.3q(\'V\');$8.1e()})})}})(3r);', 62, 214, '|||flipObj|255|function|var|return|clone|transparent||||||||||||||height|top|left|start|second|width|128|borderTopWidth|borderBottomWidth|borderLeftWidth|borderRightWidth|bgColor|css|139|data|dirOption|color|999|speed|toColor|int_prop|direction|html|onBefore|onEnd|onAnimation|borderTopColor|borderBottomColor|borderLeftColor|borderRightColor|211|192|if|this|flipLock|case|tb|bt|flip||false|169|100|dirOptions|newContent|ie6|lr|rl|background|dontChangeColor|visibility|first|queue|dequeue|style|jquery|body|240|245|165|42|107|140|230|224|144|indexOf|fn|each|flipRevertedSettings|content|500|fontSize|offset|target|visible|backgroundColor|animate|elem|prop|parseInt|now|unit|throw|name|js|plugin|error|message|cc_on|typeof|document|maxHeight|undefined|aqua|azure|beige|220|black|blue|brown|cyan|darkblue|darkcyan|darkgrey|darkgreen||darkkhaki|189|183|darkmagenta|darkolivegreen|85|47|darkorange|darkorchid|153|50|204|darkred|darksalmon|233|150|122|darkviolet|148|fuchsia|gold|215|green|indigo|75|130|khaki|lightblue|173|216|lightcyan|lightgreen|238|lightgrey|lightpink|182|193|lightyellow|lime|magenta|maroon|navy|olive|orange|pink|203|purple|violet|red|silver|white|yellow|rgb|toString|else|extend|fx|step|revertFlip|switch|default|flipSettings|font|size|12px|null|123456|hidden|true|appendTo|position|absolute|margin|zIndex|9999|lineHeight|none|borderStyle|solid|filter|chroma|remove|removeData|jQuery'.split('|'), 0, {}))


///