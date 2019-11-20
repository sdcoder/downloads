/*globals $, angular, Foundation, LightStreamCMS */
/*jslint browser:true */

(function ($, window, document) {
    'use strict';

    // initialize foundation layout / UI
    $(document).ready(function () {
        $(document).foundation();
    });

    // Mobile nav show / hide
    $("#show_how").click(function () {
        $("div.headertoggle_reveal").toggle();
        $("#show_how .fa").toggleClass("fa-rotate-180");
        return false;
    });

    $("#show_how_mobile").click(function () {
        $("div.headertoggle_reveal").toggle();

        $("#show_how_mobile .ls").toggleClass("ls-plus");
        $("#show_how_mobile .ls").toggleClass("ls-minus");

        return false;
    });

    $("#show_menu").click(function () {
        $("nav.mainmenu").toggle();
        return false;
    });

    $(".expand_hide").click(function () {
        $('div.mobileonlytoggle').toggle();
        $('div.toggle').toggle();
        $(".expand_hide .fa").toggleClass("fa-rotate-180");
        return false;
    });

    /**
        * TableSorter
        */

    // jQuery('.tablesorter').tablesorter();

    // Module for managing dynamic CMS content
    var LightStreamCMS = (function (my) {
        my.bindJumpLinks = function ($scope) {
            // jump page links
            $('a[data-jump=true]', $scope).bind('click', function (event) {
                event.preventDefault();
                var $self = $(this);
                $('#JumpPageModal').foundation('open');

                $('#JumpPageModalTarget').attr('href', $(this).attr('href'));
                $('#JumpPageModalTarget').bind('click', function () {
                    $('#JumpPageModal').foundation('close');
                    // support pop-up windows (like the Norton badge)
                    if ($self.data('popup')) {
                        window.open($(this).attr('href'),
                            $self.data('name'),
                            'width=' + $self.data('width') + ',height=' + $self.data('height') + ',location=yes,status=no,scrollbars=no,resizable=yes', true);
                        $('#JumpPageModalTarget').unbind('click');
                        return false;
                    }
                });
                return false;
            });
        };

        my.bindPopupLinks = function ($scope) {
            // replace old CMS customer guarantee with new modal
            $scope.find('a[href="/customer-guarantee"]').attr('data-popup', 'false').on('click', function () {
                $('#AjaxModal').load('/customer-guarantee', function () {
                    $('#AjaxModal').append('<button class="close-button" data-close aria-label="Close modal" type="button"><span aria-hidden="true" class="close-reveal-modal no-print">&times;</span></button>');

                    $('#AjaxModal').foundation('open');
                });

                return false;
            });

            // modal links
            $scope.on('click', 'a[data-open][href!=\\#], a[data-popup=true]', function (e) {
                var $modal;

                if (($(this).attr('href') || "").toLowerCase() === '/enotices/modal' || ($(this).data('href') || "").toLowerCase() === '/enotices/modal') {
                    $modal = $('#ENoticesModal');
                    angular.element($modal).scope().getENotices(false, $(this).data('suppress'));
                    $('#ENoticesModal').foundation('open');

                    return false;
                }

                // replace old CMS links
                if ($(this).attr('href') === '/faq/#ExcellentCredit') {
                    $(this).attr('href', '/partial/cms/excellent-credit');
                }
                if ($(this).attr('href') === '/excellent-substantial-credit') {
                    $(this).attr('href', '/partial/cms/excellent-credit');
                }
                if ($(this).attr('href') === '/good-credit') {
                    $(this).attr('href', '/partial/cms/good-credit');
                }
                if ($(this).attr('href') === '/electronic-disclosures') {
                    $(this).attr('href', '/partial/cms/electronic-disclosures');
                }
                if ($(this).attr('href') === '/Apply/FloridaDocumentaryStampTaxInfo.aspx') {
                    $(this).attr('href', '/partial/florida-doc-stamp-info');
                }

                if ($(this).attr('href') === '/modals/privacy' || $(this).attr('href') === '/partial/cms/electronic-disclosures') {
                    $modal = $('#DisclosureModal');

                    $modal.on('closed.zf.reveal', function () {
                        //var anchor = $modal.data('who-opened');

                        //if (anchor) {
                        //    $(anchor).focus();
                        //}
                    });

                    var that = this;
                    $modal.load($(this).attr('href'), function () {
                        // add close button
                        $modal.append('<button class="close-button" data-close aria-label="Close modal" type="button"><span aria-hidden="true" class="close-reveal-modal no-print">&times;</span></button>');

                        // show the dialog
                        $modal.foundation('open');
                        $modal.data("who-opened", that);

                        // hook up tabs
                        $(document).foundation();

                        // update print styles
                        $('body').children().not('#DisclosureModal').addClass('no-print');

                        // close event, and restore print styles
                        $modal.find('.close-reveal-modal').on('click', function () {
                            //$modal.foundation('open');

                            $('body').children().not('#DisclosureModal').removeClass('no-print');
                        });
                        
                    });
                } else {
                    $('#AjaxModal').on('closed.zf.reveal', function () {
                        //var anchor = $('#AjaxModal').data('who-opened');

                        //if (anchor) {
                        //    $(anchor).focus();
                        //}
                    });

                    var that = this;
                    if ($(this).attr('href')) {
                        $('#AjaxModal').load($(this).attr('href'), function (responseText) {
                            $('#AjaxModal').append('<button class="close-button" data-close aria-label="Close modal" type="button"><span aria-hidden="true" class="close-reveal-modal no-print" >&times;</span></button>');

                            $('#AjaxModal').find('a[href="/customer-testimonials"]:not([target=_blank])').attr('href', '/partials/customer-testimonials').attr('data-popup', 'true');

                            if (responseText.indexOf("&dagger;") >= 0 || responseText.indexOf("†") >= 0) {
                                if ($('#AjaxModal').find('#SameDayFundingDisclosure').length === 0) {
                                    $('<div>').load('/components/samedayfundingdisclosure', function () {
                                        $('#AjaxModal').append('<br/>');
                                        var html = $(this).html();
                                        $('#AjaxModal').append($('<div id="SameDayFundingDisclosure" tabindex="0">').css('font-size', '10pt').css('color', '#585858').css('margin-top', '1em').text(html));
                                    });
                                }
                            }

                            $('#AjaxModal').foundation('open');
                        });
                    }
                }

                if (!$(this).attr('href') || $(this).attr('href') === '#') {
                    return;
                }

                return false;
            });
        };

        my.populateCMSRates = function () {
            var $rate;
            // CMS - hook up rates
            if ($('#DisplayRate').length) {
                $rate = $('#DisplayRate');
                $("span[data-value='min-amount']").html($rate.data('min-amount'));
                $("span[data-value='max-amount']").html($rate.data('max-amount'));
                $("span[data-value='min-rate']").html($rate.data('min-rate'));
                $("span[data-value='max-rate']").html($rate.data('max-rate'));
                $("span[data-value='min-term']").html($rate.data('min-term'));
                $("span[data-value='max-term']").html($rate.data('max-term'));
                $("span[data-value='invoice-penalty']").html($rate.data('invoice-penalty'));
            }
        };

        my.displayDisclosures = function () {
            // disclosures
            if ($('sup:contains("&dagger;")').length || $('sup:contains("†")').length) {
                $('#SameDayFundingDisclosure').show().load('/components/samedayfundingdisclosure');
            }
        };

        my.initPage = function () {
            my.bindJumpLinks($('body'));
            my.bindPopupLinks($('body'));
            my.populateCMSRates();
            my.displayDisclosures();
        };

        return my;
    }(LightStreamCMS || {}));
    LightStreamCMS.initPage();

    /**
    * Accordian List
    */

    $('ul.accordian li a.toggle').click(function (e) {
        e.preventDefault();
        var $content,
            $parent = $(this).parent();
        $parent.toggleClass('active');
        $content = $parent.find('div.content');
        if (!$content.html()) {
            $content.html('<img src="/content/images/ajax-loader.gif" />');
            $content.load('/partials' + $(this).attr('href'), function () {
                LightStreamCMS.bindJumpLinks($content);
                LightStreamCMS.bindPopupLinks($content);
            });
        }

        if ($parent.hasClass('active')) {
            $content.slideDown(300);
        } else {
            $content.slideUp(300);
        }
    });

    $('ul.accordian li ul.toggle').click(function () {
        var $list = $(this);
        $list.toggleClass('expanded');
    });

    /*
        takes care of custom accordions with class of .accordian li
        example: accordion with landing page links on the bottom of the mobile home page 
    */
    $('ul.accordian li ul.toggle').on('keypress', function (e) {
        var charCode = e.charCode || e.keyCode;

        if (charCode == 13) //enter key pressed
            $(this).toggleClass('expanded');
    });

    /**
     * Workaround for https://github.com/zurb/foundation/issues/4543,
     * i.e. initialize (at least the tab library) again;
     * needed for Ajax-fetched content
     */
    $(document).on('opened', '[data-open]', function () {
        var $modal = $(this);
        if ($modal.data('video'))
        {
            setTimeout(function () {
                new Vimeo.Player($modal).play().then(function () {
                    console.log('Played');
                });
            }, 2000);

        }
        //Foundation.init($modal.context, 'tab');

        // and CMS
        LightStreamCMS.bindPopupLinks($(this));
        LightStreamCMS.bindJumpLinks($(this));
    });

    $(document).ready(function () {
        $(window).scroll(function () {
            /*check both html,body and body for cross-browser compatibility*/
            if ($('html,body').scrollTop() > 89 || $('body').scrollTop() > 89) {
                $('.desktop-nav .mainmenu .hide-for-small-only').addClass('remove').removeClass('show');
                $('.desktop-nav').addClass('affixed');
            } else {
                $('.desktop-nav .mainmenu .hide-for-small-only').addClass('show').removeClass('remove');
                $('.desktop-nav').removeClass('affixed');
            }
        });
    });

    // to "flash" or highlight a disclaimer.
    $('a[data-flash]').on('click', function () {
        var $target = $('#' + $(this).data('flash'));
        $target.css('box-sizing', 'border-box');
        $target.css('border', '1px solid red');
        return false;
    });

    $('a.toggle.init').click().removeClass('init');

    /*mobile nav and menu*/

    var MobileNavBar = function () {
        var self = this;

        var _isDesktopLastRowHidden = false;

        var _windowViewPort = function() {
            return $(window).innerHeight();
        }

        var _windowTopPosition = function() {
            return $('html,body').scrollTop() || $('body').scrollTop();
        }

        var _menuTopPosition = function () {
            return $('.mobile-menu').offset().top;
        }

        var _menuHeight = function(){
            return $('.mobile-menu').height();
        }

        var _navbarHeight = function(){
            return $('.mobile-nav .nav-bar').height();
        }

        var _onDesktopHideLastMenuRow = function () {
            if ($('html,body').scrollTop() > 89 || $('body').scrollTop() > 89) {
                $('.desktop-nav .mainmenu .hide-for-small-only').addClass('remove').removeClass('show');
                $('header').addClass('affixed');

                _isDesktopLastRowHidden = false;
            } else {
                if (_isDesktopLastRowHidden)
                    return;

                $('.desktop-nav .mainmenu .hide-for-small-only').addClass('show').removeClass('remove');
                $('header').removeClass('affixed');

                _isDesktopLastRowHidden = true;
            }
        }

        var _onMenuOutOfViewHide = function () {
            if (!$('body').hasClass('menu-opened'))
                return;

            //scrolling down
            if (_windowTopPosition() + _navbarHeight() - _menuTopPosition() > _menuHeight())
                _toggleMenu();

        }

        var _onScrollingUpWhileMenuOpen = function () {
            if (_windowTopPosition() + _navbarHeight() < _menuTopPosition())
                _toggleMenu();
        }

        var _onScroll = function () {
            _onDesktopHideLastMenuRow();
            //_onMenuOutOfViewHide();
            //_onScrollingUpWhileMenuOpen();
        }

        var _onWindowsResized = function () {
            //if ($('body').hasClass('menu-opened') && $(window).innerWidth() >= 726)
            //    _toggleMenu();
        }

        var _resetOpenMenu = function () {
            $('.fa-times').removeClass('hide');
            $('.fa-bars').addClass('hide');
            $('body').addClass('menu-opened');
            $('body').css('overflow', 'hidden');

            $('.mobile-menu').offset({ top: _windowTopPosition() + _navbarHeight() } as any);
        }

        var _resetClosedMenu = function () {
            $('.fa-times').addClass('hide');
            $('.fa-bars').removeClass('hide');
            $('body').removeClass('menu-opened');
            $('body').css('overflow', 'unset');
        }

        var _resetBackdrop = function () {
            if ($('.mobile-menu').css('display') === 'none')
                $('.backdrop').css('display', 'none');

            $('.backdrop').css('height', $(document).height());
        }

        var _toggleMenu = function () {
            _closeAccordions();

            $('.mobile-menu').toggle();
            $('.backdrop').toggle();

            _resetBackdrop();

            if ($('.mobile-menu').css('display') === 'block')
                _resetOpenMenu();
            else
                _resetClosedMenu();

            return false;
        }

        var _manuallyCloseMenu = function () {
            $('.mobile-menu').css('display', 'none');

            _closeAccordions();
            _resetClosedMenu();
            _resetBackdrop();
        }

        var _closeAccordions = function() {
            $('.mobile-menu .accordion').each(function (i, accordion) {
                var $acc = $(this);

                var $openSections = $acc.find('.accordion-item.is-active .accordion-content');

                $openSections.each(function (i, section) {
                    $acc.foundation('up', $(section));
                });
            });
        };

        var _onModalClosed = function () {
            var isWindowsPhone = /windows phone/i.test(navigator.userAgent.toLowerCase());

            if (isWindowsPhone)
                $('body').css('overflow', 'hidden');

            _manuallyCloseMenu();

            
            $('html,body').promise().then(function () {
                /*
                    use promise then instead of complete call back func because 
                    complete will be called twice per element in the jQuery selector
                */

                if (isWindowsPhone)
                    $('body').css('overflow', '');
            });

            return false;
        }

        var _onMenuBackdropClicked = function () {
            var isWindowsPhone = /windows phone/i.test(navigator.userAgent.toLowerCase());

            if (isWindowsPhone)
                $('body').css('overflow', 'hidden');

            _toggleMenu()

            $('html,body').animate({ scrollTop: 0, }, 'slow').promise().then(function () {
                if (isWindowsPhone)
                    $('body').css('overflow', '');
            });

            return false;
        }

        self.init = function () {

            if ($('.mobile-menu').length <= 0)
                return;

            $(document).ready(function () {
                $(window).scroll(_onScroll);
                $(window).resize(_onWindowsResized);
            });

            $('.mobile-nav .hamburger a').click(_toggleMenu);

            $('.backdrop.mobile-menu-backdrop').click(_onMenuBackdropClicked);

            $('#JumpPageModal').bind('closed', _onModalClosed);
        }
    }

    var NavBar = function () {
        var self = this;

        var _mobileNavBar = new MobileNavBar();

        self.init = function () {
            _mobileNavBar.init();
        };
    };

    new NavBar().init();

    //load href's via ajax
    $('div[data-load-href]').each(function (idx, element) {
        let $element = $(element)

        var href = $element.data("load-href");
        var target = $element.data("load-href-target");

        //load content onto target via ajax
        $.ajax({
            url: href,
            type: 'GET',
        }).done(function (data) {
            $('#' + target).html(data);
        });
    });

    //$('a[data-toggle][href=\\#]').click(function () {
    //    return false;
    //});
}(jQuery, window, document));