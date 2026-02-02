class IdentityResult {
    Errors: (string[] | undefined);
    Succeeded: boolean = false;
}

(function ($) {
    $(function () {
        var dialog = $("script[data-role='ConfirmPhoneDialog']");
        if (dialog != null) {
            var trigger = dialog.data("trigger");
            var selector = dialog.data("window");

            $(trigger).on("click", function () {
                var windowKendo = $(selector).data("kendoWindow");
                if (windowKendo != null) {
                    var wizard = $("[data-role='wizard']", windowKendo.element).data("kendoWizard");
                    if (wizard != null)
                        wizard.select(0);
                    windowKendo.open();
                    windowKendo.center();
                }
            });
        }
    });

    // Run right away
})(jQuery);

function ConfirmPhoneWizardButtonClick(e: kendo.ui.ButtonClickEvent) {
    let button = e.sender;
    if (button != null) {
        var form: JQuery<HTMLFormElement> = $("form", button.element.parents(".k-wizard-step"));
        var wizard: kendo.ui.Wizard | undefined = button.element.parents("[data-role='wizard']").data("kendoWizard");

        if (form != null) {
            var validation = form.validate();
            if (validation.form()) {
                var payload = form.serialize();
                $.post(form.prop("action"), payload)
                    .done(function (data: IdentityResult) {
                        if (data) {
                            if (data.Succeeded) {
                                if (wizard) {
                                    wizard.next();
                                    $("form", wizard.element).each(function (i, elem: HTMLElement) {
                                        (elem as HTMLFormElement).reset();
                                    });
                                }
                                return;
                            }
                            else
                            {
                                var tokenInput = $("input", form).prop("name");
                                var errors: Record<string, string | number> = {};
                                errors[tokenInput] = "The confirmation code is incorrect.";
                                validation.showErrors(errors);
                            }
                        }
                    })
                    .fail(function () {
                        var tokenInput = $("input", form).prop("name");
                        var errors: Record<string, string | number> = {};
                        errors[tokenInput] = "The confirmation code is incorrect.";
                        validation.showErrors(errors);
                    })
                    .always(function () {

                    });
            }
        }
    }
}

function ConfirmPhoneWizardSelect(e: kendo.ui.WizardSelectEvent) {
    e.preventDefault();
}

function ConfirmPhoneWizardComplete(e: kendo.ui.WizardDoneEvent) {
    var windowKendo = e.sender.element.closest(".k-window-content").data("kendoWindow");
    windowKendo?.close();
    window.location.reload();
}