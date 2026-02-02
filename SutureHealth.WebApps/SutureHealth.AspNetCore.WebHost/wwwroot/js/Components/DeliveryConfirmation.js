"use strict";
var SutureHealth;
(function (SutureHealth) {
    var Components;
    (function (Components) {
        //class ConfirmationForm { }
        //class PromptForm { }
        //class VerificationForm { }
        var ViewState;
        (function (ViewState) {
            ViewState[ViewState["Prompt"] = 0] = "Prompt";
            ViewState[ViewState["Verification"] = 1] = "Verification";
            ViewState[ViewState["Confirmation"] = 2] = "Confirmation";
        })(ViewState || (ViewState = {}));
        var DeliveryConfirmationHandler = /** @class */ (function () {
            function DeliveryConfirmationHandler(element) {
                var _a;
                var callbackUrl = element.getAttribute("data-callback-url");
                if (callbackUrl === undefined || callbackUrl == null)
                    throw new DOMException("the viewcallbackurl must be defined");
                this.viewCallbackUrl = callbackUrl;
                this.viewState = ViewState[(_a = element.getAttribute('data-view-state')) !== null && _a !== void 0 ? _a : "Prompt"];
                this.wrapperElement = $(element);
                switch (this.viewState) {
                    case ViewState.Prompt:
                        this.initializePrompt(element);
                        break;
                }
                //    let verificationForm = $("form#verificationForm", this);
                //    $('#confirmationButton').one('click', function () {
                //        var payload = confirmationForm.serialize();
                //        $.post(verificationForm.prop("action"), payload)
                //            .done(function () {
                //            })
                //            .fail(function () {
                //            })
                //            .always(function () {
                //            });
                //    });
            }
            DeliveryConfirmationHandler.prototype.initializePrompt = function (element) {
                var handler = this;
                var form = $("form", element);
                var button = $("button.btn-primary", element);
                button.one('click', function () {
                    var payload = form.serialize();
                    $.post(form.prop("action"), payload)
                        .done(function () {
                        $.get(handler.viewCallbackUrl + "?viewMode=" + ViewState.Verification)
                            .done(function (html) {
                            var dom = $.parseHTML(html);
                            var content = $("div[data-handler='destinationConfirmationWrapper']", dom);
                            handler.wrapperElement.html(content.html());
                            handler.initializeVerification(element);
                        });
                    })
                        .fail(function () {
                    })
                        .always(function () {
                    });
                });
            };
            DeliveryConfirmationHandler.prototype.initializeVerification = function (element) {
                var handler = this;
                var form = $("form", element);
                var input = $("input", element);
                var button = $("button.btn-primary", element);
                input.pincodeInput({ inputs: 6, hidedigits: false });
                button.one('click', function () {
                    var payload = form.serialize();
                    $.post(form.prop("action"), payload)
                        .done(function () {
                        $.get(handler.viewCallbackUrl + "?viewMode=" + ViewState.Confirmation)
                            .done(function (html) {
                            var dom = $.parseHTML(html);
                            var content = $("div[data-handler='destinationConfirmationWrapper']", dom);
                            handler.wrapperElement.html(content.html());
                        });
                    })
                        .fail(function () {
                    })
                        .always(function () {
                    });
                });
            };
            return DeliveryConfirmationHandler;
        }());
        Components.DeliveryConfirmationHandler = DeliveryConfirmationHandler;
    })(Components = SutureHealth.Components || (SutureHealth.Components = {}));
})(SutureHealth || (SutureHealth = {}));
(function ($) {
    $(function () {
        $("div[data-handler='destinationConfirmationWrapper']").each(function (i, elem) {
            new SutureHealth.Components.DeliveryConfirmationHandler(elem);
        });
    });
    // Run right away
})(jQuery);
