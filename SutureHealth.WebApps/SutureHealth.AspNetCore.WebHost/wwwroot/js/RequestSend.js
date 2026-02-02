(function () {
    var activeModel = {},
        submitImmediately = false,
        patientUpdateConfirmed = false,
        setSignerLocation = function (organizationId) {
            $(".request-send").hide();

            let collaboratorDropDown = $("#CollaboratorDropDownList").data("kendoDropDownList"),
                assistantDropDown = $("#AssistantDropDownList").data("kendoDropDownList");

            if (!collaboratorDropDown || !assistantDropDown) {
                return;
            }

            $("#SignerOrganizationId").valid();

            if (activeModel.SignerMemberId == null || !organizationId) {
                collaboratorDropDown.dataSource.data([]);
                assistantDropDown.dataSource.data([]);
                setFieldVisibility();

                return;
            }

            $.get({
                "url": "/Request/Send/Signer/" + activeModel.SignerMemberId + "/Location/" + organizationId,
                "success": function (data) {
                    if (data.HasCollaborators) {
                        data.Collaborators.unshift({ "MemberId": null, "Summary": "None - Send Direct To Signer" });
                        collaboratorDropDown.dataSource.data(data.Collaborators);

                        if (collaboratorDropDown.dataSource.get(activeModel.CollaboratorMemberId)) {
                            collaboratorDropDown.value(activeModel.CollaboratorMemberId);
                        }
                        else {
                            activeModel.CollaboratorMemberId = collaboratorDropDown.dataItem().MemberId;
                        }
                    }
                    else {
                        collaboratorDropDown.dataSource.data([]);
                        activeModel.CollaboratorMemberId = null;
                    }

                    if (data.HasAssistants) {
                        data.Assistants.unshift({ "MemberId": null, "Summary": "None - Signer and All Assistants" });
                        assistantDropDown.dataSource.data(data.Assistants);

                        if (assistantDropDown.dataSource.get(activeModel.AssistantMemberId)) {
                            assistantDropDown.value(activeModel.AssistantMemberId);
                        }
                        else {
                            activeModel.AssistantMemberId = assistantDropDown.dataItem().MemberId;
                        }
                    }
                    else {
                        assistantDropDown.dataSource.data([]);
                        activeModel.AssistantMemberId = null;
                    }

                    setFieldVisibility();
                },
                "error": function (result) {
                    collaboratorDropDown.dataSource.data([]);
                    assistantDropDown.dataSource.data([]);
                }
            });
        },
        setPostFields = function () {
            $("form#RequestSendForm").attr("action", "/Request/Send/Organization/" + activeModel.OrganizationId + "/Submit");
            $("#FromOfficeAutoComplete").val(activeModel.FromOfficeAutoComplete);
            $("#SignerAutoComplete").val(activeModel.SignerAutoComplete);
            $("#PatientAutoComplete").val(activeModel.PatientAutoComplete);
            $("#PrimaryDxCodeAutoComplete").val(activeModel.PrimaryDxCodeAutoComplete);
            $("#ClinicalDateDatePicker").val(activeModel.ClinicalDate ? new Date(activeModel.ClinicalDate).toLocaleDateString("en-us", { "month": "2-digit", "year": "numeric", "day": "2-digit" }) : null);
            $("#FromOfficeExpandSearch").prop("checked", activeModel.FromOfficeExpandSearch);
            $("#SignerExpandSearch").prop("checked", activeModel.SignerExpandSearch);
            $("#PreviewBeforeSending").prop("checked", activeModel.PreviewBeforeSending);

            for (var key in activeModel) {
                $("input[name='" + key + "']").attr("value", SH.GetValueOrDefault(activeModel[key], ""));
            }
        },
        setFieldVisibility = function () {
            let canSendRequests = SH.GetResultOrDefault(function () { return OfficeDataSource.get(activeModel.OrganizationId).CanSendRequests; }, false),
                showDiagnosisCode = SH.GetResultOrDefault(function () { return TemplateDataSource.get(activeModel.TemplateId).DiagnosisCodeAllowed; }, false),
                showFileInput = SH.GetResultOrDefault(function () { return TemplateDataSource.get(activeModel.TemplateId).PdfRequired; }, false),
                hasCollaborators = SH.GetResultOrDefault(function () { return $("#CollaboratorDropDownList").data("kendoDropDownList").dataSource.total(); }, 0) > 0,
                hasAssistants = SH.GetResultOrDefault(function () { return $("#AssistantDropDownList").data("kendoDropDownList").dataSource.total(); }, 0) > 0,
                allFields = $(".request-send");

            allFields.toggle(canSendRequests);

            allFields.filter(".request-send-collaborator").toggle(canSendRequests && hasCollaborators);
            allFields.filter(".request-send-assistant").toggle(canSendRequests && hasAssistants);
            allFields.filter(".request-send-diagnosis-code").toggle(canSendRequests && showDiagnosisCode);
            allFields.filter(".request-send-pdf").toggle(canSendRequests && showFileInput);
            allFields.filter(".request-send-no-send").toggle(!canSendRequests);

            toggleFormLoader(false);
        },
        saveState = function (onComplete) {
            sessionStorage.setItem("RequestSendActiveModel", JSON.stringify(activeModel, function (k, v) { return SH.GetValueOrDefault(v); }));

            if (onComplete) {
                onComplete();
            }
        },
        preSubmitVerification = function (onSuccess) {
            $.post({
                "url": "/Request/Send/Organization/" + activeModel.OrganizationId + "/PreSubmitVerification",
                "contentType": "application/json",
                "data": JSON.stringify({
                    "ClinicalDate": activeModel.ClinicalDate,
                    "PatientId": activeModel.PatientId,
                    "SignerMemberId": activeModel.SignerMemberId,
                    "TemplateId": activeModel.TemplateId
                }),
                "success": onSuccess
            });
        },
        toggleFormLoader = function (visible) {
            $("#RequestSendForm").toggle(!visible);
            $(".request-send-loader").toggle(visible);
        },
        setMaxUploadSizeByOrganizationId = function (organizationId) {
            let selectedOffice = OfficeDataSource.get(organizationId),
                uploadWidget = $("#PdfContents").data("kendoUpload"),
                displayText = $("#SystemSizeLimit");

            uploadWidget.setOptions({
                "validation": {
                    "allowedExtensions": [".pdf"],
                    "maxFileSize": selectedOffice.MaxUploadBytes
                }
            });
            displayText.text(selectedOffice.MaxUploadBytes / 1048576);
        };

    $(document).ready(function () {
        if (submitImmediately) {
            LoadingPanel.loader.show();
            saveState(function () {
                document.getElementById('RequestSendForm').submit();
            });
            return;
        }

        $("#PdfContents").rules("add", {
            "FileRequired": true
        });
        $("#DiagnosisCodeId").rules("add", {
            "DiagnosisCodeRequired": true
        });
        $("form#RequestSendForm").data("validator").settings.submitHandler = function (form) {
            if (OfficeDataSource.get(activeModel.OrganizationId).HasIncompleteProfile) {
                $("#CompanyProfileError").data("kendoDialog").open();
                return;
            }

            if (!OfficeDataSource.get(activeModel.OrganizationId).IsPayingClient && !SignerLocationDataSource.get(activeModel.SignerOrganizationId).IsPayingClient) {
                $("#FreeSignerError").data("kendoDialog").open();
                return;
            }

            LoadingPanel.loader.show();
            setPostFields();
            preSubmitVerification(function (data) {
                if (!patientUpdateConfirmed && data.PatientUpdateRequested) {
                    $("#UpdatePatientModalContainer").load("/Request/Send/Organization/" + activeModel.OrganizationId + "/Patient/" + activeModel.PatientId + "/Update", function () {
                        LoadingPanel.loader.hide();
                    });
                    return;
                }

                switch (data.DuplicateRequestRiskLevel) {
                    case 1:
                        LoadingPanel.loader.hide();
                        $("#DuplicateRequestWarning").data("kendoDialog").open();
                        break;
                    case 2:
                        LoadingPanel.loader.hide();
                        $("#DuplicateRequestError").data("kendoDialog").open();
                        break;
                    default:
                        saveState(function () {
                            form.submit();
                        });
                        break;
                }
            });
        };
    });

    $.validator.setDefaults({
        "ignore": null,
        "onfocusout": false,
        "onkeyup": false,
        "onclick": false
    });    // jQuery.Validate doesn't check hidden fields by default.
    $.validator.addMethod("FileRequired", function (value, element) {
        let isRequired = SH.GetResultOrDefault(function () { return TemplateDataSource.get(activeModel.TemplateId).PdfRequired; }, false),
            widget = $(element).data("kendoUpload");

        return !isRequired || !widget || widget.getFiles().length > 0;
    }, "A file upload is required for this template type.");
    $.validator.addMethod("DiagnosisCodeRequired", function (value, element) {
        let isRequired = SH.GetResultOrDefault(function () { return TemplateDataSource.get(activeModel.TemplateId).DiagnosisCodeRequired; }, false);

        return !isRequired || SH.GetResultOrDefault(function () { return $(element).attr("value").length; }, 0) > 0;
    }, "Diagnosis Code is required");

    $(document).on("click", "input#FromOfficeExpandSearch", function () {
        activeModel.FromOfficeExpandSearch = $(this).prop("checked");
    });

    $(document).on("click", "input#SignerExpandSearch", function () {
        activeModel.SignerExpandSearch = $(this).prop("checked");
    });

    $(document).on("click", "input#PreviewBeforeSending", function () {
        activeModel.PreviewBeforeSending = $(this).prop("checked");
    });

    $(document).on("click", "#TemplateHelp", function () {
        window.open("/pdf/AboutTemplates.pdf", "_blank").focus();
    });

    $(document).on("change", "#PatientAutoComplete", function () {
        $("#PatientAutoComplete").attr("title", $("#PatientAutoComplete").val());
    });

    $(document).on("change", "#PrimaryDxCodeAutoComplete", function () {
        $("#PrimaryDxCodeAutoComplete").attr("title", $("#PrimaryDxCodeAutoComplete").val());
    });

    $(document).on("click", "#ResetFields", function () {
        let officeChanged = false;

        for (var key in activeModel) {
            if (key == "OrganizationId") {
                let offices = OfficeDataSource.data(),
                    defaultSelection = (function () {
                        let organizationId = offices.length > 0 ? offices[0].OrganizationId : null;

                        for (var index in offices) {
                            if (offices[index].IsPrimary) {
                                organizationId = offices[index].OrganizationId;
                            }
                        }

                        return organizationId;
                    })();

                if (activeModel[key] != defaultSelection) {
                    activeModel[key] = defaultSelection;
                    officeChanged = true;
                }
            }
            else {
                activeModel[key] = null;
            }
        }

        setPostFields();
        $("#TemplateDropDownList").data("kendoDropDownList").value(null);
        $("#PdfContents").data("kendoUpload").removeAllFiles();
        if (officeChanged) {
            let officeDropDown = $("#OfficeDropDownList").data("kendoDropDownList");

            if (officeDropDown) {
                officeDropDown.value(activeModel.OrganizationId);
            }
            TemplateDataSource.fetch();
        }
        SignerLocationDataSource.fetch();
        $(".field-validation-error").html("");
    });

    $(document).on("click", "#AddNewPatient", function () {
        activeModel.PatientId = undefined;
        activeModel.PatientAutoComplete = undefined;

        saveState(function () {
            window.location.href = "/Organization/" + activeModel.OrganizationId + "/Patient/Create";
        });
    });

    window.OnRequestSendInitialize = function (model, overrideClientModel, submit) {
        let sessionModelString = sessionStorage.getItem("RequestSendActiveModel");

        submitImmediately = submit;
        $.extend(activeModel, model);
        if (sessionModelString) {
            let sessionModel = JSON.parse(sessionModelString);

            if (overrideClientModel) {
                // Do not override signer fields.
                $.extend(activeModel, {
                    "FromOfficeExpandSearch": sessionModel.FromOfficeExpandSearch,
                    "SignerExpandSearch": sessionModel.SignerExpandSearch,
                    "SignerMemberId": sessionModel.SignerMemberId,
                    "SignerAutoComplete": sessionModel.SignerAutoComplete,
                    "SignerOrganizationId": sessionModel.SignerOrganizationId,
                    "AssistantMemberId": sessionModel.AssistantMemberId,
                    "CollaboratorMemberId": sessionModel.CollaboratorMemberId
                });
            }
            else {
                $.extend(activeModel, sessionModel);
            }
        }
        if (submit) {
            activeModel.TemplateId = model.TemplateId;
        }
        sessionStorage.removeItem("RequestSendActiveModel");

        setPostFields();
    };

    window.FromOfficeDataSourceHandler = function (options) {
        if (options.data.filter.filters.length == 0) {
            options.success([]);
            return;
        }

        $.post({
            "url": "/Request/Send/OrganizationsForSurrogateSender",
            "data": JSON.stringify({
                "Contains": options.data.filter.filters[0].value,
                "ExpandSearch": activeModel.FromOfficeExpandSearch
            }),
            "contentType": "application/json",
            "success": function (data) {
                options.success(data);
            },
            "error": function (result) {
                options.error(result);
            }
        });
    }

    window.TemplateDataSourceHandler = function (options) {
        let organizationId = activeModel.OrganizationId;

        if (!organizationId) {
            options.success([]);

            return;
        }

        $.get({
            "url": "/Request/Send/Template/Organization/" + organizationId,
            "success": function (data) {
                let groups = [],
                    createNewTemplateOption = {
                        "TemplateId": 0,
                        "Summary": "-- Create Custom Template --",
                        "ClinicalDateLabel": "Effective Date",
                        "DiagnosisCodeAllowed": false,
                        "DiagnosisCodeRequired": false,
                        "PdfRequired": false,
                        "IsCreateNew": true
                    },
                    selectOption = {
                        "TemplateId": null,
                        "Summary": "-- Select One --",
                        "ClinicalDateLabel": "Effective Date",
                        "DiagnosisCodeAllowed": false,
                        "DiagnosisCodeRequired": false,
                        "PdfRequired": false,
                        "IsChooseOne": true
                    };

                data.OrganizationTemplates.unshift(selectOption);
                groups.push({
                    "value": data.OrganizationTemplateGroupName,
                    "items": data.OrganizationTemplates.concat([createNewTemplateOption])
                });

                if (data.HasStandardTemplates) {
                    groups.push({
                        "value": data.StandardTemplateGroupName,
                        "items": data.StandardTemplates
                    });
                }

                options.success(groups);
            },
            "error": function (result) {
                options.error(result);
            }
        });
    };

    window.SignerDataSourceHandler = function (options) {
        let stateOrProvince = OfficeDataSource.get(activeModel.OrganizationId).StateOrProvince,
            expandSearch = activeModel.SignerExpandSearch;

        if (options.data.filter.filters.length == 0) {
            options.success([]);
            return;
        }

        $.post({
            "url": "/Search/Signer/OrganizationMember",
            "data": JSON.stringify({
                "Search": options.data.filter.filters[0].value,
                "OrganizationStateOrProvinceFilter": expandSearch ? null : stateOrProvince,
                "Count": 30
            }),
            "contentType": "application/json",
            "success": function (data) {
                options.success(data.Signers);
            },
            "error": function (result) {
                options.error(result);
            }
        });
    };

    window.PatientDataSourceHandler = function (options) {
        let organizationId = activeModel.OrganizationId;

        if (options.data.filter.filters.length == 0) {
            options.success([]);
            return;
        }

        $.post({
            "url": "/Search/Patient",
            "data": JSON.stringify({
                "Search": options.data.filter.filters[0].value,
                "OrganizationId": organizationId,
                "Count": 30
            }),
            "contentType": "application/json",
            "success": function (data) {
                options.success(data.Patients);
            },
            "error": function (result) {
                options.error(result);
            }
        });
    };

    window.PrimaryDxCodeDataSourceHandler = function (options) {
        if (options.data.filter.filters.length == 0) {
            options.success([]);
            return;
        }

        $.post({
            "url": "/Search/DiagnosisCode",
            "data": JSON.stringify({
                "Search": options.data.filter.filters[0].value,
                "Count": 30
            }),
            "contentType": "application/json",
            "success": function (data) {
                options.success(data.DiagnosisCodes);
            },
            "error": function (result) {
                options.error(result);
            }
        });
    };

    window.FromOfficeSelected = function (e) {
        if (e.dataItem.OrganizationId === activeModel.OrganizationId)
            return;

        activeModel.OrganizationId = e.dataItem.OrganizationId;
        activeModel.FromOfficeAutoComplete = e.dataItem.Summary;
        TemplateDataSource.fetch();
        setFieldVisibility();
    }

    window.FromOfficeChanged = function (e) {
        if ($(this.element).val().length == 0) {
            activeModel.FromOfficeAutoComplete = null;
        } else {
            $(this.element).val(activeModel.SignerAutoComplete);
        }
    }

    window.OfficeChanged = function () {
        activeModel.OrganizationId = this.value();
        TemplateDataSource.fetch();
        setFieldVisibility();
    };

    window.OfficeDataBound = function () {
        this.value(activeModel.OrganizationId);
        setFieldVisibility();
    };

    window.SignerSelected = function (e) {
        if (e.dataItem.MemberId === activeModel.SignerMemberId) {
            return;
        }

        activeModel.SignerMemberId = e.dataItem.MemberId;
        activeModel.SignerOrganizationId = e.dataItem.OrganizationId;
        activeModel.SignerAutoComplete = e.dataItem.Summary;
        activeModel.CollaboratorMemberId = null;
        activeModel.AssistantMemberId = null;
        setPostFields();
        $("#SignerMemberId").valid();
        SignerLocationDataSource.fetch();
    };

    window.SignerChanged = function (e) {
        if ($(this.element).val().length == 0) {
            activeModel.SignerMemberId = null;
            activeModel.SignerAutoComplete = null;
            activeModel.SignerOrganizationId = null;
            activeModel.CollaboratorMemberId = null;
            activeModel.AssistantMemberId = null;
            setPostFields();
            $("#SignerMemberId").valid();
            SignerLocationDataSource.fetch();
        }
        else {
            $(this.element).val(activeModel.SignerAutoComplete);
        }
    };

    window.SignerLocationDataSourceHandler = function (options) {
        if (!activeModel.SignerMemberId) {
            options.success([]);

            return;
        }

        $.get({
            "url": "/Request/Send/Signer/" + activeModel.SignerMemberId,
            "success": function (data) {
                options.success(data.Locations);
            },
            "error": function (result) {
                activeModel.SignerMemberId = null;
                options.error(result);
            }
        });
    };

    window.SignerLocationDataBound = function () {
        let dataItem = SignerLocationDataSource.get(activeModel.SignerOrganizationId),
            selectedItem = this.dataItem();

        if (dataItem) {
            this.value(activeModel.SignerOrganizationId);
        }
        else if (selectedItem) {
            dataItem = selectedItem;
            activeModel.SignerOrganizationId = dataItem.OrganizationId;
            setPostFields();
        }
        else if (this.dataItem(0)) {
            dataItem = this.dataItem(0);
            activeModel.SignerOrganizationId = dataItem.OrganizationId;
            this.value(activeModel.SignerOrganizationId);
            setPostFields();
        }

        setSignerLocation(SH.GetResultOrDefault(function () { return dataItem.OrganizationId; }));
    }

    window.SignerLocationChanged = function () {
        let selectedItem = this.dataItem();

        activeModel.SignerOrganizationId = selectedItem.OrganizationId;
        setSignerLocation(selectedItem.OrganizationId);
        setPostFields();
    };

    window.TemplateDataBound = function () {
        $(".request-send").hide();

        let dataItem = TemplateDataSource.get(activeModel.TemplateId),
            selectedItem = this.dataItem();

        if (dataItem && dataItem.TemplateId != null) {
            this.value(dataItem.TemplateId);
            $("#ClinicalDateLabel").text(dataItem.ClinicalDateLabel);
        }
        else if (selectedItem) {
            activeModel.TemplateId = selectedItem.TemplateId;
            $("#ClinicalDateLabel").text(selectedItem.ClinicalDateLabel);
        }
        else {
            // Default value selection; subject to change.
            selectedItem = this.dataItem(0);
            // surrogate senders have nothing selected on init.
            if (selectedItem) {
                activeModel.TemplateId = selectedItem.TemplateId;
                this.value(selectedItem.TemplateId);
                $("#ClinicalDateLabel").text(selectedItem.ClinicalDateLabel);
            }
        }

        // surrogate senders have nothing selected on init.
        if (selectedItem) {
            setPostFields();
            setMaxUploadSizeByOrganizationId(activeModel.OrganizationId);
        }

        setFieldVisibility();
    };

    window.TemplateChanged = function () {
        $(".request-send").hide();

        let selectedItem = this.dataItem();

        if (selectedItem.IsCreateNew) {
            activeModel.TemplateId = null;
            toggleFormLoader(true);
            saveState(function () {
                window.location.href = "/Template/Create?organizationId=" + activeModel.OrganizationId;
            });
            return;
        }

        activeModel.TemplateId = selectedItem.TemplateId;
        $("#ClinicalDateLabel").text(selectedItem.ClinicalDateLabel);
        setPostFields();
        setFieldVisibility();
        $("#TemplateId").valid();
    };

    window.CollaboratorChanged = function () {
        activeModel.CollaboratorMemberId = this.value();
    };

    window.AssistantChanged = function () {
        activeModel.AssistantMemberId = this.value();
    };

    window.PatientSelected = function (e) {
        activeModel.PatientId = e.dataItem.PatientId;
        activeModel.PatientAutoComplete = e.dataItem.Summary;
        setPostFields();
        $("#PatientId").valid();
    };

    window.PatientChanged = function (e) {
        if ($(this.element).val().length == 0) {
            activeModel.PatientId = null;
            activeModel.PatientAutoComplete = null;
            setPostFields();
            $("#PatientId").valid();
        }
        else {
            $(this.element).val(activeModel.PatientAutoComplete);
        }
    };

    window.ClinicalDateChanged = function (e) {
        let alertWidget = $("#GenericDialog").data("kendoDialog"),
            selectedDate = (this.value() instanceof Date) ? this.value() : (this.value() ? new Date(this.value().replace(/[^0-9\/]+/g, "")) : null),
            pastDateThreshold = (function () {
                let date = new Date();

                date.setHours(0, 0, 0, 0);
                date.setDate(date.getDate() - 30);

                return date;
            })(),
            futureDateThreshold = (function () {
                let date = new Date();

                date.setHours(23, 59, 59, 999);

                return date;
            })();

            todayDateThreshold = (function () {
                let date = new Date();

                date.setHours(0, 0, 0, 0);

                return date;
            })();

        if (selectedDate && isNaN(selectedDate.getTime())) {
            selectedDate = null;
        }

        activeModel.ClinicalDate = selectedDate != null ? this.element.val() : null;
        $("#ClinicalDate").val(selectedDate != null ? this.element.val() : null);
        $("#ClinicalDate").valid();

        if (selectedDate != null && selectedDate < pastDateThreshold) {
            alertWidget.content("More than 30 days have passed since this date.").open();
        }
        if (selectedDate != null && selectedDate > futureDateThreshold) {
            alertWidget.content("You have chosen an effective date in the future.").open();
        }

        if (selectedDate != null && selectedDate.getTime() == todayDateThreshold.getTime()) {
            alertWidget.content("Confirm the date entered is the date listed on document.").open();
        }
    };

    window.PrimaryDxCodeSelected = function (e) {
        activeModel.DiagnosisCodeId = e.dataItem.DiagnosisCodeId;
        activeModel.PrimaryDxCodeAutoComplete = e.dataItem.Summary;
        setPostFields();
        $("#DiagnosisCodeId").valid();
    };

    window.PrimaryDxCodeChanged = function (e) {
        if ($(this.element).val().length == 0) {
            activeModel.DiagnosisCodeId = null;
            activeModel.PrimaryDxCodeAutoComplete = null;
            setPostFields();
            $("#DiagnosisCodeId").valid();
        }
        else {
            $(this.element).val(activeModel.PrimaryDxCodeAutoComplete);
        }
    };

    window.PdfContentsSelected = function () {
        $("[data-valmsg-for='PdfContents']").text("");
    };

    window.RedirectToCompanyProfile = function () {
        window.location.href = "/Organization/" + activeModel.OrganizationId;
    };

    window.DuplicateRiskConfirmation = function () {
        saveState(function () {
            document.getElementById('RequestSendForm').submit();
        });
    };

    window.UpdatePatientConfirm = function () {
        patientUpdateConfirmed = true;
        $("#UpdatePatientModal").data("kendoDialog").destroy();
        $("#RequestSendForm").submit();
    };

    window.UpdatePatientSubmit = function () {
        let gender = $("select[name='Gender']").val(),
            ssn = $("#SocialSecurityNumber").data("kendoMaskedTextBox"),
            ssnType = $("#SocialSecurityNumberType").data("kendoRadioGroup"),
            mbi = $("#MedicareMbi").data("kendoMaskedTextBox"),
            errorContainer = $("#UpdatePatientModalValidationErrors");

        LoadingPanel.loader.show();
        $.post({
            "url": "/Request/Send/Organization/" + activeModel.OrganizationId + "/Patient/" + activeModel.PatientId + "/Update",
            "contentType": "application/json",
            "data": JSON.stringify({
                "Gender": SH.GetValueOrDefault(gender, null),
                "SocialSecurityNumber": SH.GetResultOrDefault(function () { return ssn.raw(); }, null),
                "SocialSecurityNumberType": SH.GetResultOrDefault(function () { return ssnType.value(); }, null),
                "MedicareMbi": SH.GetResultOrDefault(function () { return mbi.raw(); }, null)
            }),
            "success": function (data) {
                if (data.Success) {
                    $("#UpdatePatientModal").data("kendoDialog").destroy();
                    patientUpdateConfirmed = true;
                    $("#RequestSendForm").submit();
                }
                else {
                    let errorList = $("<ul></ul>");

                    for (var i in data.Errors) {
                        errorList.append($("<li></li>").html(data.Errors[i]));
                    }

                    errorContainer.empty();
                    errorContainer.append(errorList);
                    LoadingPanel.loader.hide();
                }
            }
        });

        return false;
    };
})();