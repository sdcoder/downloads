$(document).ready(function () {
    $('.cta-outer-container').click(function () {
        var $this: any = $(this);

        if ($this.hasClass('expanded')) {
            $this.removeClass('expanded');
        } else {
            collapseAllDrawers();
            $this.addClass('expanded');
        }
    });

    function collapseAllDrawers() {
        $('.cta-outer-container').each(function (i, element) {
            $(element).removeClass('expanded');
        });
    }

    $(window).scroll(function () {
        $('.cta-outer-container').each(function (i, element) {
            let drawer = $(element);
            let drawerBottom = drawer.offset().top + drawer.height();
            let windowTop = $(window).scrollTop();

            if (drawerBottom <= windowTop + 50) {
                if (drawer.hasClass('expanded'))
                    drawer.removeClass('expanded');
            }
        });
    });
});