(function (HeaderFunctions, $, undefined) {
    HeaderFunctions.NetworkBadge = function (options) {
        var opt = {
            "Url": "/Network/Badge",
            "BadgeSelector": "#badgeCount",
            "ButtonSelector": "#networkPage",
            "CallToActionSelector": "#networkInvitationCallToAction"
        };
        $.extend(opt, options);

        $.ajax({
            "type": "GET",
            "url": opt.Url,
            "success": function (data) {
                $(opt.BadgeSelector).text(data.RecentlyJoinedCount).toggle(data.RecentlyJoinedCount > 0);
                $(opt.ButtonSelector).attr("href", data.NetworkUrl);
                $(opt.CallToActionSelector).toggle(data.CallToActionEnabled);
                $(opt.ButtonSelector).toggle(data.NetworkNavigationEnabled);
            }
        });
    };

    HeaderFunctions.WarmEndpoint = function (options) {
        var opt = {
            "IgnoreUrls": []
        };
        $.extend(opt, options);

        if (Array.isArray(opt.IgnoreUrls)) {
            for (var path in opt.IgnoreUrls) {
                if (window.location.pathname.toLowerCase().indexOf(opt.IgnoreUrls[path].toLowerCase()) >= 0) {
                    return;
                }
            }
        }

        $.ajax({
            "type": "GET",
            "url": opt.Url
        });
    };

    HeaderFunctions.InboxBadge = function (options) {
        var opt = {
            "Url": "/Revenue/Badge",
            "BadgeSelector": "#requestCount",
            "ButtonSelector": "#inboxPage",
        };
        $.extend(opt, options);

        $.ajax({
            "type": "GET",
            "url": opt.Url,
            "success": function (data) {
                $(opt.BadgeSelector).text(data.PendingDocumentCount).toggle(data.PendingDocumentCount > 0);
                $("#requestCount .network-badge").show();
            }
        });
    }
})(window.HeaderFunctions = window.HeaderFunctions || {}, jQuery);