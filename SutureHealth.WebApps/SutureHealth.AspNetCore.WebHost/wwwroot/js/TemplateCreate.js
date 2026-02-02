(function () {
    $(document).ready(function () {
        $("#PdfContents").rules("add", {
            "FileRequired": true
        });

        $("#CancelButton").click(function () {
            window.location.href = $(this).data("url");
        });
    });

    $.validator.setDefaults({
        "ignore": null
    });
    $.validator.addMethod("FileRequired", function (value, element) {
        return (typeof value === 'string' && value.length > 0) || $(element).data("kendoUpload").getFiles().length > 0;
    }, "REQUIRED");

    window.OfficeChanged = function () {
        let sendModelString = sessionStorage.getItem("RequestSendActiveModel");

        if (sendModelString) {
            let sendModel = JSON.parse(sendModelString);

            sendModel.OrganizationId = this.dataItem().OrganizationId

            sessionStorage.setItem("RequestSendActiveModel", JSON.stringify(sendModel));
        }

        $("#TemplateTypeId").data("kendoDropDownList").dataSource.fetch();
    };

    window.TemplateTypeChanged = function () {
        $("#InternalName").val(this.dataItem().ShortName);
    };

    window.TemplateTypeReadParameters = function () {
        return {
            "organizationId": $("#OrganizationId").val()
        };
    };

    window.TemplateTypeDataSourceChanged = function (e) {
        if (!e.action) {
            this.pushInsert(0, {
                "TemplateTypeId": null,
                "Category": "General",
                "Name": "-- Select Template Type --"
            });

            $("#TemplateTypeId").data("kendoDropDownList").value(null);
            $("#InternalName").val("");
        }
    };
})();