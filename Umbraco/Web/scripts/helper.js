(function ($) {
    $.fn.ajaxLoading = function (options) {
        var defaults = {
            backgroundColor: "#fff",
            opacity: 0.7,
            image: "/images/loading.gif"
        };
        var settings = $.extend({}, defaults, options);
        var overlay = $('<div></div>').css({
            'background-color': settings.backgroundColor,
            'opacity': settings.opacity,
            'width': $(this).width(),
            'height': $(this).height(),
            'position': 'absolute',
            'top': '0px',
            'left': '0px',
            'z-index': 99999
            //'display': 'none'
        }).addClass('ajaxloading').append($('<div></div>').css({
            'background-image': "url('" + settings.image + "')",
            'background-repeat': 'no-repeat',
            'background-position': 'center center',
            'background-color': 'transparent',
            'width': '100%',
            'height': '100%'
        }));
        return this.each(function () {
            // Plugin code would go here...
            $(this).append(overlay);
        });
    };

    $.fn.closeLoading = function() {
        if ($('.ajaxloading').length > 0) {
            $('.ajaxloading').hide('slow');
            $('.ajaxloading').remove();
        }
    };
})(jQuery);

