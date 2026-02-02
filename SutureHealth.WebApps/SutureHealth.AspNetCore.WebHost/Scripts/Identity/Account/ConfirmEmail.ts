(function ($) {
    $(function () {
        var config = $("script[data-role='ConfirmEmailDialog']");
        if (config != null) {
            var trigger = config.data("trigger");
            var dialogSelector = config.data("window");
            var formSelector = config.data("form");

            $(trigger).on("click", function () {
                var dialog = $(dialogSelector).data("kendoDialog");
                var submit: JQuery<HTMLFormElement> = $(formSelector);

                window.LoadingPanel.visible(true);

                if (submit != null) {
                    var payload = submit.serialize();
                    $.post(submit.prop("action"), payload)
                        .done(function (data: IdentityResult) {
                            if (data) {
                                if (data.Succeeded) {
                                    if (dialog != null) {
                                        dialog.open();
                                        dialog.center();
                                    }
                                }
                                else {

                                }
                            }
                        })
                        .fail(function () {

                        })
                        .always(function () {
                            window.LoadingPanel.visible(false);
                        });
                }
            });
        }
    });

    // Run right away

})(jQuery);
