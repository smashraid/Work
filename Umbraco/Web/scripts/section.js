function ResizeTab() {
    var tvHeight = $(window).height();   //getViewportHeight();
    if (document.location) {
        tvHeight = tvHeight - 10;
    }
    var tvWidth = $(window).width(); // getViewportWidth();
    if (document.location) {
        tvWidth = tvWidth - 20;
    }
    var tabviewHeight = tvHeight - 12;
    $('#tabSection123').height((tabviewHeight - 67));
    $('#tabSection123').width((tvWidth - 2));
}

$(document).ready(function () {
    ResizeTab();
    $(window).resize(function () {
        ResizeTab();
    });

    ko.bindingHandlers.date = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var jsonDate = valueAccessor();
            var value = new Date(parseInt(jsonDate.substr(6)));
            var ret = value.getMonth() + 1 + "/" + value.getDate() + "/" + value.getFullYear();
            element.innerHTML = ret;
        },
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        }
    };

    $.urlParam = function (name) {
        var results = new RegExp('[\\?&]' + name + '=([^&#]*)').exec(window.location.href);
        if (!results) {
            return 0;
        }
        return results[1] || 0;
    };

    var bootstrapButton = $.fn.button.noConflict();
    $.fn.bootstrapBtn = bootstrapButton;
});