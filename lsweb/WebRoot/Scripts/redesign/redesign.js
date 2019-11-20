function doHover(trueOrFalse) {
    if (trueOrFalse)
        $('#ghostBox').css('opacity', '.4');
    else
        $('#ghostBox').css('opacity', '.25');
}
$(document).ready(function () {
    var owlCards = $('#card-carousel').owlCarousel({
        responsiveClass: true,
        slideBy: 2,
        margin: 20,
        responsive: {
            0: {
                items: 1,
                slideBy: 1
            },
            376: {
                items: 2,
                slideBy: 2,
                margin: 40
            },
            480: {
                items: 3,
                slideBy: 3,
                margin: 30
            },
            726: {
                items: 5,
                slideBy: 3
            },
            1000: {
                items: 6
            },
            1200: {
                items: 7
            }
        }
    });
    $('#carouselRow').css('visibility', 'visible'); //because it looks ugly before owl does its thing
    // all the other (non-card) carousels...
    $('.owl-carousel').owlCarousel({
        autoHeight: true,
        items: 1,
        dots: true
    });
    $('#ratesAndTerms, #ratesAndTermsMobile').click(function () {
        $('html, body').animate({
            scrollTop: $("#LoanCalculator").offset().top - 100
        }, 1000);
    });
    $('#ctaBar').click(function (event) {
        event.stopPropagation();
        if ($('#ctaDescriptions').css('display') === 'block')
            $('#ctaDescriptions').hide();
        else {
            $('#ctaDescriptions').show();
            SC.main.banner.benefits();
        }
    });
    function updateCarouselRowHeight() {
        var w = $('.cardFront').width();
        $('#carouselRow').css({
            'height': w + 'px'
        });
    }
    owlCards.on('resized.owl.carousel', function (event) {
        updateCarouselRowHeight();
    });
    owlCards.on('changed.owl.carousel', function (event) {
        var index = event.item.index;
        var count = event.item.count;
        var size = event.page.size;
        if (index === 0)
            $('#carouselLeft > i').addClass('arrowDisabled');
        else
            $('#carouselLeft > i').removeClass('arrowDisabled');
        if (index === count - size)
            $('#carouselRight > i').addClass('arrowDisabled');
        else
            $('#carouselRight > i').removeClass('arrowDisabled');
    });
    updateCarouselRowHeight();
    $('.card').each(function () {
        $(this).on('pointerdown pointerover pointerenter touchstart touchmove', function () {
            $(this).trigger('mouseenter');
        });
    });
    $('#see-more-link').click(function () {
        $('#apr-abridged').css('display', 'none');
        $('#apr-unabridged').css('display', 'block');
    });
    $('#see-less-link').click(function () {
        $('#apr-abridged').css('display', 'block');
        $('#apr-unabridged').css('display', 'none');
    });
}); //document ready
//Allows the carousel pagination to be trigger via hover rather than having to click
if ($(document).arrive) {
    $(document).arrive(".owl-dot", function () {
        var dot = $(this);
        dot.mouseover(function () {
            $(this).click();
        });
    });
}
function doNext() {
    $('#card-carousel').trigger('next.owl.carousel');
}
function doPrevious() {
    $('#card-carousel').trigger('prev.owl.carousel');
}
var lastCtaClickedMobile = '';
function showCtaDescription(ctaId) {
    $('#ctaMobileWhyLightStream').hide();
    $('#ctaMobileGuarantee').hide();
    $('#ctaMobileRate').hide();
    if (lastCtaClickedMobile !== ctaId) {
        $('#' + ctaId).show();
        $('#ctaDescriptionsMobile').show();
        lastCtaClickedMobile = ctaId;
    }
    else {
        lastCtaClickedMobile = '';
        $('#ctaDescriptionsMobile').hide();
    }
}
$(window).ready(function () {
    showBannerContent();
});
$(window).resize(function () {
    showBannerContent();
});
//FOUC fix
function showBannerContent() {
    if ($(window).width() > 725) {
        var header = $('#header > div.headline.hide-for-small-only.home3Headline');
        var banner = $('#heroBanner');
        if (header.css('visibility') !== 'visible')
            header.css('visibility', 'visible');
        if (banner.css('visibility') !== 'visible')
            banner.css('visibility', 'visible');
    }
    else {
        var mobileHeader = $('#header > div.headline.show-for-small-only');
        var mobileBanner = $('#mobileHeroBanner');
        if (mobileHeader.css('visibility') !== 'visible')
            mobileHeader.css('visibility', 'visible');
        if (mobileBanner.css('visibility') !== 'visible')
            mobileBanner.css('visibility', 'visible');
    }
}
//# sourceMappingURL=redesign.js.map