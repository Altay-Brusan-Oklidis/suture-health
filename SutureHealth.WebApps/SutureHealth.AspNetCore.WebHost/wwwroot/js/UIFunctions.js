var uiFunction = {
    OnFailure: OnFailure,
    showNotification: showNotification
};

var loader = $('.k-loading-panel');

if (typeof (Sys) !== "undefined") {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        loader.hide();
    });
}

function OnFailure(error) {
    console.log(error);
    showNotification('Something went wrong! please try later', 'error');
    loader.hide();
}

function onShow(e) {
    if (e.sender.getNotifications().length === 1) {
        var element = e.element.parent(),
            eWidth = element.width(),
            eHeight = element.height(),
            wWidth = $(window).width(),
            wHeight = $(window).height(),
            newTop, newLeft;

        newLeft = Math.floor(wWidth / 2 - eWidth / 2);
        //newTop = Math.floor(wHeight/2 - eHeight / 2);
        e.element.parent().css({ top: 0, left: newLeft });
    }
}

function showNotification(msg, type) {
    var staticNotification = $("#staticNotification").kendoNotification({
        show: onShow,
        button: true
    }).data("kendoNotification");

    staticNotification.show(msg, type ? type : "error");
    if (type !== 'error') {
        $('.k-notification-wrap').css('background-color', '#00B3B3');
        $('.k-notification-wrap').parent().css('border', '#999');
        $('.k-i-error').remove();
    } else {
        $('.k-notification-wrap').css('background-color', '#c22c27');
        $('.k-notification-wrap').parent().css('border', '#999');
    }
}