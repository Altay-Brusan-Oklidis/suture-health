(function () {
    $(document).on("click", "input[name='SocialSecurityNumberType']", function () {
        let button = $(this),
            maskWidget = $("#SocialSecurityNumber").data("kendoMaskedTextBox"),
            masks = {
                "Full": "000-00-0000",
                "Last4": "0000",
                "Unavailable": " "
            },
            selection = button.val();

        maskWidget.setOptions({ "mask": masks[selection] });

        if (selection == "Unavailable") {
            $("#SocialSecurityNumberDialog").data("kendoDialog").open();
        }
    });

    $(document).on("click", "#whyNeedInfoLink", function () {
        $("#OnePatientOneRecordDialog").data("kendoDialog").open();
    });

    window.DateOfBirthChanged = function () {
        if (this.value() !== null) {
            let value = (this.value() instanceof Date) ? this.value() : new Date(this.value());
            let dateFromEpoch = new Date(Date.now() - value);
            $("#DateOfBirthAge").text(isNaN(dateFromEpoch.getTime()) ? 0 : Math.abs(dateFromEpoch.getUTCFullYear() - 1970));
        }
    };

    window.ForceAddSimilarPatient = function () {
        $("input[name='ForceAddSimilarPatient']").attr("value", "true");
        $("#PatientForm").submit();
    };

    $(document).on("click", "#SocialSecurityNumberType :radio", function () {
        $("#PatientForm").validate().element("#SocialSecurityNumber");
    });

    $(document).on("click", "#Gender :radio", function () {
        $("#GenderValue").val($(this).val());
        $("#PatientForm").validate().element("#GenderValue");
    });

    $(document).on("click", "#medicare", function () {
        if (!$(this).prop("checked") && $("#medicareAdvantage").prop("checked")) {
            return false;
        }
    });

    $(document).on("change", "#payerMixColumn input[type='checkbox']", function () {
        if ($(this).prop('checked')) {
            switch (this.id) {
                case "unAvailable":
                    $("#payerMixColumn input[type='checkbox']:not(#unAvailable)").prop("checked", false).change();
                    $("#payerMixColumn input[type='text']").val("");
                    $(".hiddenInput").val("").hide();
                    break;
                case "medicareAdvantage":
                    $("#payerMixColumn #medicare").prop("checked", true).change();
                default:
                    $("#unAvailable").prop("checked", false);
                    $(this).closest(".form-check").children(".hiddenInput").show();
                    break;
            }
        } else {
            $(this).closest(".form-check").children(".hiddenInput").hide();
        }
    });

    $(document).ready(function () {
        $("#payerMixColumn input[type='checkbox']:checked").closest(".form-check").children(".hiddenInput").show();

        $("form#PatientForm").data("validator").settings.submitHandler = function (form) {
            LoadingPanel.loader.show();
            form.submit();
        };
    });
})();

// By default validator ignores hidden fields.
// change the setting here to ignore nothing
$.validator.setDefaults({ ignore: null });

// Expressive Annotations treats enums as numbers by default.
ea.settings.apply({ "enumsAsNumbers": false });