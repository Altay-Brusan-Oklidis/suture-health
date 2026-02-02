"use strict";
(function () {
    window.forgotPasswordHandler = function (e) {
        var forgotBtn = e.target;
        var userNameElement = forgotBtn.dataset.inputControl;
        var validationElement = forgotBtn.dataset.validationFormFieldName;
        if (validationElement) {
            var validationFormValue = document.getElementsByName(validationElement);
            var validationControl = $(validationFormValue);
            if (userNameElement) {
                var userName = $(userNameElement);
                if (userName && userName.valid()) {
                    var form = $("<form></form>");
                    form.append(userName.clone());
                    form.append(validationControl.clone());
                    $.ajax({
                        url: forgotBtn.dataset.formAction,
                        type: forgotBtn.dataset.formMethod,
                        data: form.serialize()
                    })
                        .always(function (data) {
                        window.NotificationPanel.info(data.Message);
                        if (forgotBtn.dataset.validationMessage) {
                            var validationMessage = $(forgotBtn.dataset.validationMessage);
                        }
                    });
                }
            }
        }
        e.preventDefault();
    };
})();
