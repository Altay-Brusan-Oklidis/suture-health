interface Window {
    LoadingPanel: SH.UI.LoadingPanel;
    NotificationPanel: kendo.ui.Notification | undefined;
}

namespace SH.UI {
    export class LoadingPanel {
        loader: JQuery<HTMLElement> | undefined;

        constructor(selector: string) {
            this.loader = $(selector);
        }

        visible(state: boolean) {
            if (state ?? true)
                this.loader?.show();
            else
                this.loader?.hide();
        }
    }

    export class NotificationPanel {
        widget: kendo.ui.Notification | undefined;

        constructor(selector: string) {
            let options: kendo.ui.NotificationOptions = {
                button: true,
                show: function (e: kendo.ui.NotificationShowEvent) {
                    if (e.sender.getNotifications().length === 1) {
                        var element = e.element?.parent(),
                            //eHeight = element?.height(),
                            //wHeight = $(window).height(),
                            //newTop, 
                            eWidth = element?.width(),
                            wWidth = $(window).width(),
                            newLeft;

                        if (wWidth && eWidth && element) {
                            //newTop = Math.floor(wHeight/2 - eHeight / 2);
                            newLeft = Math.floor(wWidth / 2 - eWidth / 2);
                            element.css({ top: 0, left: newLeft });
                        }
                    }
                }
            };

            this.widget = $(selector).kendoNotification(options).data("kendoNotification");
        }
    }

    $(() => {
        window.LoadingPanel = new LoadingPanel("#default-loading-panel");
        window.NotificationPanel = new NotificationPanel("#default-notification-panel").widget;
    });

    export class SessionMonitor {
        notification: kendo.ui.Window | undefined;
        progress: kendo.ui.ProgressBar | undefined;
        cushion: number;
        enableUI: boolean = false;
        idleTimeout: number;
        lastPingTime: number;
        pingUrl: string;
        pinging: boolean = false;
        redirectUrl: string;
        sessionLength: number = 0;
        timeout: number = 0;
        timeoutStartTime: number | undefined;
        timer: number | undefined;
        warningTimeout: number;

        constructor(selector: string, redirectUrl: string, pingUrl: string, idleTimeout: number, warningTimeout: number, cushion: number, enableUI: boolean) {
            var me = this;

            function onTimerChange(this: kendo.ui.ProgressBar, e: kendo.ui.ProgressBarChangeEvent) {
                if (me.timeoutStartTime) {
                    var secondsRemaining = Math.floor((me.timeout - ((new Date()).getTime() - me.timeoutStartTime)) / 1000),
                        min = Math.floor(secondsRemaining / 60),
                        sec = secondsRemaining % 60,
                        text = "Remaining Time: " + (min > 0 ? min + " minutes " : "") + (secondsRemaining > 0 && sec < 10 ? "0" + sec : sec) + " seconds.";
                    if (this.progressStatus) {
                        this.progressStatus.text(text);
                    }
                    console.debug(text);
                }
            }

            if (enableUI) {
                this.notification = $(selector).kendoWindow({ visible: false }).data("kendoWindow");
                this.progress = $(".sh-progress", $(selector)).kendoProgressBar({
                    change: onTimerChange,
                    value: 100
                }).data("kendoProgressBar");
            }

            this.cushion = cushion || 4000;
            this.idleTimeout = idleTimeout || (15 * 60 * 1000);
            this.lastPingTime = 0;
            this.pingUrl = pingUrl;
            this.redirectUrl = redirectUrl;
            this.warningTimeout = warningTimeout || (45 * 1000);

            // If user interacts with browser, reset timeout.
            $(document).on("mousedown mouseup keydown keyup", "", this.resetTimeout.bind(this));
            $(window).resize(this.resetTimeout.bind(this));
            $(".btn.btn-primary", $(selector)).on("click", { redirectUrl: this.redirectUrl }, function (e) {
                (window.top) && (window.top.location.href = me.redirectUrl);
            });

            // Start fresh by reseting timeout.
            this.resetTimeout();
        }

        resetTimeout() {
            var me: SessionMonitor = this;
            var now: number = new Date().getTime();
            var cacheTime: number = (me.lastPingTime + 300000);

            // Reset timeout by "pinging" server.
            if (!me.pinging && (now > cacheTime)) {
                me.lastPingTime = now;
                me.pinging = true;
                $.ajax({
                    type: "POST",
                    dataType: "json",
                    url: this.pingUrl,
                    xhrFields: { withCredentials: true }
                })
                    .done(function (result) {
                        if (me.timer) {
                            // Stop countdown.
                            window.clearTimeout(me.timer);
                        }

                        if (me.notification) {
                            // timer is reset so don't show the window if its there?
                            me.notification.close();
                        }

                        // Subract time it took to do the ping from
                        // the returned timeout and a little bit of 
                        // cushion so that client will be logged out 
                        // just before timeout has expired.
                        me.timeoutStartTime = now;
                        me.timeout = result.timeout - (me.timeoutStartTime - me.lastPingTime) - me.cushion;
                        me.pinging = false;

                        me.timeout = (me.timeout > me.idleTimeout) ? me.idleTimeout : me.timeout;
                        me.timer = window.setTimeout(me.tick.bind(me), me.timeout - me.warningTimeout);
                    })
                    .fail(function (result) {
                        (window.top) && (window.top.location.href = me.redirectUrl);
                    });
            }
            else {
                if (me.timer) {
                    // Stop countdown.
                    window.clearTimeout(me.timer);
                }

                if (me.notification) {
                    // timer is reset so don't show the window if its there?
                    me.notification.close();
                }

                var idleTimeout = (new Date()).getTime() + me.idleTimeout;
                me.timeout = (me.timeout > idleTimeout) ? idleTimeout : me.timeout;
                me.timer = window.setTimeout(me.tick.bind(me), me.timeout);
            }
        }

        tick() {
            this.tock();
            // once the styling is fixed this is whats needed to bring the UI back to life.
            if (!this.notification) return;
            if (this.timeout && this.timeoutStartTime) {
                this.progress?.value(100);
                this.notification.center().open();
            }
        }

        tock() {
            if (this.timeout && this.timeoutStartTime) {
                var secondsRemaining = Math.floor((this.timeout - ((new Date()).getTime() - this.timeoutStartTime)) / 1000),
                    percentageRemaining = Math.floor(secondsRemaining / (this.warningTimeout / 1000) * 100),
                    min = Math.floor(secondsRemaining / 60),
                    sec = secondsRemaining % 60,
                    text = "Remaining Time: " + (min > 0 ? min + " minutes " : "") + (secondsRemaining > 0 && sec < 10 ? "0" + sec : sec) + " seconds.";

                // once the styling is fixed this is whats needed to bring the UI back to life.
                if (this.progress) {
                    this.progress?.value(percentageRemaining);
                }

                // If timeout hasn't expired, continue countdown.
                if (secondsRemaining > 0) {
                    this.timer = window.setTimeout(this.tock.bind(this), 750);
                }
                // Else redirect to login.
                else {
                    if (this.notification) {
                        this.notification.close();
                    }
                    (window.top) && (window.top.location.href = this.redirectUrl);
                }
            }
        }
    }

    export class ConfirmButton {
        constructor(element: HTMLElement) {

        }
    }

    export class InputFormGroup {
        constructor(element: HTMLElement) {
            var clearButton = $(".ui-clear-button", element).first();
            var inputControl = $("input.form-control", element).first();

            if (clearButton && inputControl) {
                clearButton.on("click", (evt) => {
                    inputControl.val("");
                });
            }
        }
    }

    export class AjaxFormElement {
        constructor(element: HTMLElement) {
            $(element).on("click", (evt) => {
                evt.preventDefault();

                let actionButton = evt.target as HTMLButtonElement;
                let inputElements = actionButton.dataset.inputSelector;
                let validationElement = actionButton.dataset.validationFormFieldName;

                let inputControls = inputElements ? $(inputElements) : null;
                let validationFormValue = validationElement ? document.getElementsByName(validationElement) : null;
                let validationControl = validationFormValue ? $(validationFormValue) : null;

                if (inputControls && inputControls.valid()) {
                    let form = $("<form></form>");
                    if (validationControl) {
                        form.append(validationControl.clone())
                    }

                    inputControls.each((i, elem) => {
                        form.append($(elem).clone());
                    });

                    window.LoadingPanel.loader?.show();
                    $.ajax({
                        url: actionButton.dataset.formAction,
                        type: actionButton.dataset.formMethod,
                        data: form.serialize()
                    })
                        .done(function (data) {
                            if (actionButton.dataset.done) {
                                let fn = new Function("data", actionButton.dataset.done);
                                fn(data);
                            }
                        })
                        .fail(function () {
                            if (actionButton.dataset.fail) {
                                let fn = new Function(actionButton.dataset.fail);
                                fn();
                            }
                        })
                        .always(function (data) {
                            window.LoadingPanel.loader?.hide();
                            if (actionButton.dataset.always) {
                                let fn = new Function("data", actionButton.dataset.always);
                                fn(data);
                            }
                        });
                }
            });
        }
    }
}

$(() => {
    var buttons: JQuery<HTMLElement> = $(".input-form-group");
    if (buttons) {
        buttons.each((i, elem) => {
            new SH.UI.InputFormGroup(elem);
        });
    }

    buttons = $(".input-confirmation-button");
    if (buttons) {
        buttons.each((i, elem) => {
            new SH.UI.ConfirmButton(elem);
        });
    }

    var ajaxforms: JQuery<HTMLElement> = $("[data-role=form]");
    if (ajaxforms) {
        ajaxforms.each((i, elem) => {
            new SH.UI.AjaxFormElement(elem);
        });
    }

});