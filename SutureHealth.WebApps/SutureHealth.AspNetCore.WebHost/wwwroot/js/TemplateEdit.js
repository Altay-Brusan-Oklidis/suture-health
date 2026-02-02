(function () {
    var renderValidationErrors = function (errors) {
        let validationSummaryAlert = $("#ValidationSummary"),
            listElement = $("<ul></ul>");

        validationSummaryAlert.empty().hide();

        if (!errors || errors.length == 0) {
            return;
        }

        for (var i in errors) {
            listElement.append($("<li>" + errors[i] + "</li>"));
        }

        validationSummaryAlert.append(listElement).show();
    }

    $(document).ready(function () {
        LoadingPanel.loader.hide();

        $("#SaveButton").click(function () {
            let element = $(this),
                validationSummary = AnnotationEditor.Validate();

            renderValidationErrors();

            if (!validationSummary.Success) {
                renderValidationErrors(validationSummary.Errors);
                return;
            }

            LoadingPanel.loader.show();
            $.post({
                "url": element.attr("data-post-url"),
                "contentType": "application/json",
                "data": JSON.stringify({
                    "Annotations": AnnotationEditor.GetAnnotations()
                }),
                "success": function (response) {
                    let sendModelString = sessionStorage.getItem("RequestSendActiveModel");

                    if (!response.Success) {
                        LoadingPanel.loader.hide();
                        renderValidationErrors(response.Errors);
                        return;
                    }

                    if (sendModelString) {
                        let sendModel = JSON.parse(sendModelString);

                        sendModel.OrganizationId = element.attr("data-organization-id");
                        sendModel.TemplateId = element.attr("data-template-id");

                        sessionStorage.setItem("RequestSendActiveModel", JSON.stringify(sendModel));
                    }

                    window.location.href = element.attr("data-return-url");
                },
                "error": function () {
                    LoadingPanel.loader.hide();
                    renderValidationErrors([ "An unknown error occurred.  Please try again." ]);
                }
            });
        });

        $("#CancelButton").click(function () {
            window.location.href = $(this).attr("data-url");
        });
    });
})();