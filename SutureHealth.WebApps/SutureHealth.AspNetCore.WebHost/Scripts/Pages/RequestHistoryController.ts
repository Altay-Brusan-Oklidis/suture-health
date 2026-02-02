interface Window {
    retryRequestHandler: Function;
    archiveRequestHandler: Function;
    autoCompleteFilterHandler: Function;
    dateRangeFilterHandler: Function;
    downloadRequestHandler: Function;
    docIdNumberFilterHandler: Function;
    fileRequestHandler: Function;
    gridChangeHandler: Function;
    gridExcelExportHandler: Function;
    gridDataBoundHandler: Function;
    gridShowHistoryHandler: Function;
    forgeryToken: Function;
    OnGridRequestStart: Function;
    OnGridRequestEnd: Function;
    resendRequestHandler: Function;
    resetFilterHandler: Function;
    retractRequestHandler: Function;
    includeOlderDocumentsHandler: Function;
    toggleMoreLess: Function;
    rowTooltipHandler: Function;
    PatientNameFilterHandler: Function;
}

interface JQuery {
    getKendoButton(): kendo.ui.Button;
    getKendoDatePicker(): kendo.ui.DatePicker;
    getKendoDialog(): kendo.ui.Dialog;
    getKendoDropDownList(): kendo.ui.DropDownList;
    getKendoGrid(): kendo.ui.Grid;
    getKendoToolBar(): kendo.ui.ToolBar;
    getKendoWindow(): kendo.ui.Window;
}

declare namespace kendo.ui {
    interface GridTemplateHandlerEvent {
        element: JQuery;
        dataSource: kendo.data.DataSource;
    }

    interface Grid {
        expandToFit(): void;
        fitColumns(parentColumn: any): void;
    }

    interface GridColumn {
        exportFormat: string;
    }
}

declare namespace SutureSign {
    interface RequestHistoryItem extends kendo.data.ObservableObject {
        FormId: number;
        SubmittedAt: (Date | undefined);
        EffectiveDate: (Date | undefined);
        TemplateId: number;
        Form: string;
        Type: string;
        StatusDate: (Date | undefined);
        Age: number;
        StatusCode: number;
        Status: string;
        PdfUrl: string;

        ArchivedString: string;
        FiledString: string;
        IsArchived: boolean;
        IsFiled: boolean;

        //public string LastModifiedBy { get; set; }
        LastModified: (Date | undefined);

        PatientId: number;
        PatientName: string;
        PatientBirthdate: string;
        MedicalRecordNumber: string;

        RejectionReason: string;

        SignerMemberId: number;
        SignerOrganizationId: number | undefined;
        SignerName: string;
        Practice: string;
        CollaboratorName: string;

        SubmitterOrganizationName: string;
        SubmitterOrganizationId: number;
        SubmitterOffice: string;
    }
}

kendo.ui.Grid.prototype.fitColumns = function (parentColumn) {
    var grid = this;
    var columns = grid.columns;

    if (parentColumn && parentColumn.columns) {
        columns = parentColumn.columns;
    }

    columns.forEach(function (col) {
        if (col.columns) {
            return grid.fitColumns(col);
        }
        grid.autoFitColumn(col);
    });

    grid.expandToFit();
};

kendo.ui.Grid.prototype.expandToFit = function () {
    var $gridHeaderTable = this.thead.closest('table');
    var gridDataWidth = $gridHeaderTable.width();
    var gridWrapperWidth = $gridHeaderTable.closest('.k-grid-header-wrap').innerWidth();

    // Since this is called after column auto-fit, reducing size
    // of columns would overflow data.
    if (!gridDataWidth || !gridWrapperWidth || gridDataWidth >= gridWrapperWidth) {
        return;
    }

    var $headerCols = $gridHeaderTable.find('colgroup > col');
    var $tableCols = this.table.find('colgroup > col');

    var sizeFactor = (gridWrapperWidth / gridDataWidth);
    $headerCols.add($tableCols).not('.k-group-col').each(function () {
        var currentWidth = $(this).width();
        if (currentWidth) {
            var newWidth = (currentWidth * sizeFactor);
            $(this).css({
                width: newWidth
            });
        }
    });
};

(function () {
    window.archiveRequestHandler = (e: kendo.ui.ButtonClickEvent) => {
        var grid = e.sender.element.parents(".k-grid").getKendoGrid();
        var url = grid.element.data("archiving-serviceurl");
        var selectedRows = grid.select();
        if (selectedRows.length > 0) {
            var dialog = $("div#configurationDialog").kendoDialog({
                closable: true,
                modal: true,
                title: `Confirm Archiving Documents`,
                content: `<p>Only the signed or rejected items you selected will be archived.</p><p>Pending items can only be retracted.</p>`,
                actions: [
                    {
                        text: "Mark as Archived",
                        action: (e: kendo.ui.ButtonClickEvent) => {
                            var forms: number[] = [];
                            selectedRows.each((index: number, element: HTMLElement) => {
                                let historyItem = grid.dataItem(element) as SutureSign.RequestHistoryItem;
                                forms.push(historyItem.FormId);
                            });

                            var data = kendo.antiForgeryTokens();
                            data.requestIds = forms;
                            $.post(url, data)
                                .done(function (data, status: string, jqXHR: JQueryXHR) {
                                    grid.dataSource.read().then(function () {
                                        grid.refresh();
                                        grid.trigger("change");
                                    });
                                })
                                .fail(function (jqXHR: JQueryXHR, status: string) {
                                    alert("failed");
                                })
                        }
                    },
                    { text: "Cancel", primary: true, action: (e: kendo.ui.ButtonClickEvent) => { dialog.getKendoDialog().close(); } }
                ]
            });

            dialog.getKendoDialog().open();
        }
    };

    window.autoCompleteFilterHandler = (e: kendo.ui.GridTemplateHandlerEvent) => {
        let control = $(e.element);
        let template = kendo.template($("script#SignerAutoCompleteTemplate").html());
        let filterCell = control.parents(".k-filtercell");
        filterCell.empty();

        jQuery("#Physicians").kendoAutoComplete({
            "dataTextField": "ShortName",
            "placeholder": "Signer",
            "filter": "contains",
            "dataSource": {
                "type": "aspnetmvc-ajax",
                "transport": {
                    "read": {
                        "url": "/tracking?handler=PhysicianRead",
                        "data": window.forgeryToken,
                        "type": "POST"
                    }, "prefix": ""
                },
                "serverPaging": true,
                "serverSorting": true,
                "serverFiltering": true,
                "serverGrouping": true,
                "serverAggregates": true,
                "filter": [],
                "schema": {
                    "data": "Data",
                    "total": "Total",
                    "errors": "Errors"
                }
            }
        });
    };

    interface IFieldValues {
        fieldName: string,
        value: Date
    }

    window.docIdNumberFilterHandler = (e: kendo.ui.GridTemplateHandlerEvent, defaultValues: (IFieldValues[] | undefined)) => {
        e.element.kendoNumericTextBox({
            decimals: 0,
            format: "#########",
            spinners: false,
            min: 100000000,
            max: 999999999
        });
    };

    window.dateRangeFilterHandler = (e: kendo.ui.GridTemplateHandlerEvent, defaultValues: (IFieldValues[] | undefined)) => {
        let template = kendo.template($("script#dateRangeFilterTemplate").html());
        let control = $(e.element);
        let filterCell = control.parents(".k-filtercell");
        let fieldName = filterCell.data("field");

        filterCell.empty();
        let filterControls = filterCell.html(kendo.render(template, [{ filterField: fieldName }]));
        var dateInput = $("input", filterControls).kendoDatePicker({
            change: function (e: kendo.ui.DatePickerChangeEvent) {
                var grid = $(e.sender.element.data("filter-grid")).getKendoGrid();
                if (grid) {
                    // get the current list of filters, remove this one, and add it back if its not the default value;
                    var filters = grid.dataSource.filter().filters || [];
                    var operator = e.sender.element.data("filter-operator");

                    filters = removeFilter(filters, fieldName, operator);
                    if (e.sender.value() != null) {
                        filters.push({ field: fieldName, operator: operator, value: e.sender.value() });
                    }

                    e.sender.element.siblings(".k-select").find(".k-link.k-link-clear").toggle(e.sender.value() != null);
                    grid.dataSource.filter(filters);

                    // refresh the data
                    grid.dataSource.read();
                }
            }
        });

        var clearButton = '<span class="k-link k-link-clear" style="display:none;position:absolute;margin-right:25px;right:6px;" aria-label="Clear the DateTimePicker"><span unselectable="on" class="k-icon k-i-close" aria-controls="dtp_timeview"></span></span>';
        dateInput.each((index: number, element: HTMLElement) => {
            $(element).getKendoDatePicker().wrapper.find(".k-select").prepend(clearButton);
        });

        //if (defaultValues)
        //{
        //    defaultValues.forEach((v, i, a) => {
        //        var inputField = $(`#${fieldName}${v.fieldName}`);
        //        if (inputField) {
        //            inputField.val(kendo.toString(v.value, "d"));
        //            inputField.siblings(".k-select").find(".k-link.k-link-clear").toggle(inputField.val() != null);
        //            inputField.trigger("change");
        //        }
        //    });
        //}

        $(".k-link.k-link-clear").on("click", function (e: JQuery.ClickEvent) {
            e.stopPropagation();
            e.preventDefault();
            var dtp = $(e.target).closest(".k-datepicker").find("input[data-role='datepicker']").data("kendoDatePicker");
            if (dtp != null) {
                dtp.value("");
                dtp.trigger("change");
            }
        });
    };

    window.includeOlderDocumentsHandler = (e: any) => {
        let grid: kendo.ui.Grid = e.item.element.parents(".k-grid").getKendoGrid(),
            filters = grid.dataSource.filter()?.filters ?? [];

        removeFilter(filters, "SubmittedAtOneYear");
        if (!e.checked) {
            filters.push({ field: "SubmittedAtOneYear", operator: "gte", value: grid.element.data("one-year-ago") });
        }

        constrictFromDate(e.checked);
        grid.dataSource.filter(filters);
        grid.dataSource.read();
    };

    function constrictFromDate(checked: boolean)
    {
        let fromDatePicker = $("#SubmittedAtFromDatePicker");
        if (fromDatePicker) {
            let lastYear = new Date();
            lastYear.setFullYear(lastYear.getFullYear() - 1);
            let options: any = {
                min: (checked) ? new Date(2010, 0, 1) : lastYear
            };

            let datePicker = fromDatePicker.getKendoDatePicker();
            datePicker.setOptions(options);
        }
    }

    window.downloadRequestHandler = (e: kendo.ui.ButtonClickEvent) => {
        var grid = e.sender.element.parents(".k-grid").getKendoGrid();
        var url = grid.element.data("downloading-serviceurl");
        var selectedRows = grid.select();
        if (selectedRows.length > 0) {
            let form = $("<form action=\"" + url + "\" method=\"post\"></form>"),
                tokens = kendo.antiForgeryTokens();

            for (let key in tokens) {
                form.append("<input name=\"" + key + "\" type=\"hidden\" value=\"" + tokens[key] + "\" />");
            }
            selectedRows.each((index: number, element: HTMLElement) => {
                form.append("<input name=\"requestIds[" + index + "]\" type=\"hidden\" value=\"" + (grid.dataItem(element) as SutureSign.RequestHistoryItem).FormId + "\" />");
            });

            $(document.body).append(form);
            form.submit();
        }
    };

    window.fileRequestHandler = (e: kendo.ui.ButtonClickEvent) => {
        var grid = e.sender.element.parents(".k-grid").getKendoGrid();
        var url = grid.element.data("filing-serviceurl");
        var selectedRows = grid.select();
        if (selectedRows.length > 0) {
            var dialog = $("div#configurationDialog").kendoDialog({
                closable: true,
                modal: true,
                title: `Confirm Filing of Documents`,
                content: `<p>Only the signed or rejected items you selected will be filed.</p><p>Pending items can only be retracted.</p>`,
                actions: [
                    {
                        text: "Mark as Filed",
                        action: (e: kendo.ui.ButtonClickEvent) => {
                            var forms: number[] = [];
                            selectedRows.each((index: number, element: HTMLElement) => {
                                let historyItem = grid.dataItem(element) as SutureSign.RequestHistoryItem;
                                forms.push(historyItem.FormId);
                            });

                            var data = kendo.antiForgeryTokens();
                            data.requestIds = forms;
                            $.post(url, data)
                                .done(function (data, status: string, jqXHR: JQueryXHR) {
                                    grid.dataSource.read().then(function () {
                                        grid.refresh();
                                        grid.trigger("change");
                                    });
                                })
                                .fail(function (jqXHR: JQueryXHR, status: string) {
                                    alert("failed");
                                })
                        }
                    },
                    { text: "Cancel", primary: true, action: (e: kendo.ui.ButtonClickEvent) => { dialog.getKendoDialog().close(); } }
                ]
            });

            dialog.getKendoDialog().open();
        }
    };

    window.forgeryToken = () => {
        return kendo.antiForgeryTokens();
    };

    window.gridChangeHandler = (e: kendo.ui.GridChangeEvent) => {
        let pending = false;
        let rejected = false;
        let retracted = false;
        let signed = false;
        let download = false;

        let selectedRows = e.sender.select();
        if (selectedRows.length > 0) {
            download = true;

            for (var i = 0; i < selectedRows.length; i++) {
                var dataItem = e.sender.dataItem(selectedRows[i]) as SutureSign.RequestHistoryItem;
                download = download && dataItem.PdfUrl != null;
                var statusCode = dataItem.get("RequestStatus");
                switch (statusCode) {
                    case 1:
                        signed = true;
                        break;
                    case 2:
                        rejected = true;
                        break;
                    case 3:
                        retracted = true;
                        break;
                    default:
                        pending = true;
                        break;
                }
            }
        }

        var toolbar = $(e.sender.element.data("toolbar-selector")).getKendoToolBar();

        toolbar.enable(`#${toolbar.element.data("archive-selector")}`, signed || rejected);
        toolbar.enable(`#${toolbar.element.data("download-selector")}`, download);
        toolbar.enable(`#${toolbar.element.data("filed-selector")}`, signed || rejected);
        toolbar.enable(`#${toolbar.element.data("resend-selector")}`, rejected || retracted);
        toolbar.enable(`#${toolbar.element.data("retract-selector")}`, pending);
    };

    window.gridExcelExportHandler = (e: kendo.ui.GridExcelExportEvent) => {
        var sheet = e.workbook?.sheets[0];
        if (sheet) {
            var grid = e.sender;
            var fields = grid?.dataSource?.options?.schema?.model.fields;
            var dateCells: { field: string | undefined, format: string | undefined; index: number; }[] = [];

            // get the date columns from the datasource
            // only check visible columns
            var visibleColumns = grid.columns.filter(function (col) { return col.hidden !== true });
            visibleColumns.forEach(function (col, colIndex) {
                var fieldName = col.field as string;
                // find matching model
                var match = fields[fieldName];
                // determine if this is a date column that will need a date/time format
                if (match && match.type === 'date') {
                    // give each column a format from the grid settings or a default format
                    dateCells.push({
                        field: col.field,
                        format: col.exportFormat ? col.exportFormat : "MM/dd/yyyy",
                        index: colIndex
                    });
                }
            });

            if (sheet.rows) {
                for (var rowIndex = 1; rowIndex < sheet.rows.length; rowIndex++) {
                    var row = sheet.rows[rowIndex];
                    if (row && row.cells) {
                        // apply the format to the columns found
                        for (var cellIndex = 0; cellIndex < dateCells.length; cellIndex++) {
                            let dateCell = dateCells[cellIndex];
                            row.cells[dateCell.index - 2].format = dateCell.format;
                        }
                    }
                }
            }
        }
    }

    window.gridDataBoundHandler = (e: kendo.ui.GridDataBoundEvent) => {
        var grid = e.sender;
        var kendoGrid = $(e.sender.element);
        var currentStatus = $(kendoGrid.data("status-filter-dropdown")).getKendoDropDownList()?.value();
        var isSender = kendoGrid.data("issender");

        if (isSender) {
            $.each(e.sender.dataSource.data(), function (i, row) {
                let element;
                if (row.get("Status") === "Rejected") {
                    element = $('tr[data-uid="' + row.uid + '"] ', e.sender.element);
                    element.addClass("colored-row-red");
                }
                else if (row.get("Status") === "Pending") {
                    element = $('tr[data-uid="' + row.uid + '"] ', e.sender.element);
                    element.addClass("colored-row-yellow");
                }
            });
        }

        // handle rejection reason
        $(".rejectionReason").each(function () {
            let reasonText = $(this).text();
            if (reasonText.length > 10) {
                $(this).html("<div class='showHideRejectionReason'>" + reasonText + "</div><a href='javascript:void(0)' onclick='toggleMoreLess(this)'>... More &or;</a>");
            }
        });

        if (isSender === true) {
            var processedColumn = grid.columns.filter(function (c, i) { return c.field == "RequestStatusDate" });
            if (processedColumn.length == 1) {
                grid.reorderColumn(16, processedColumn[0]);
            }

            var signerColumn = grid.columns.filter(function (c, i) { return c.field == "SignerName" });
            if (signerColumn.length == 1) {
                grid.reorderColumn(8, signerColumn[0]);
            }
        }

        if (currentStatus != "" && currentStatus != "2")
            grid.hideColumn("RejectionReason");
        else {
            grid.showColumn("RejectionReason");
        }

        grid.expandToFit();

        $(e.sender.element).find('[data-role="autocomplete"]').each(function () {
            $(this).data("kendoAutoComplete")?.setOptions({ autoWidth: true })
        });
    };

    window.toggleMoreLess = function (anchorClicked: Element) {
        var rejectionReason = $(anchorClicked).prev("div.showHideRejectionReason").text();
        if ($(anchorClicked).is(':contains("More")')) {
            $(anchorClicked).html("... Less &and;");
            $('<tr class="rejectionReasonRow"><td colspan="16" align="right">' + rejectionReason + '</td></tr>').insertAfter($(anchorClicked).closest("tr"));
        } else {
            $(anchorClicked).html("... More &or;");
            $(anchorClicked).closest("tr").next("tr.rejectionReasonRow").remove();
        }
    }

    var historyTemplate = kendo.template($("#historyTemplate").html());

    window.gridShowHistoryHandler = function (this: kendo.ui.Grid, e: JQueryMouseEventObject) {
        e.preventDefault();

        var url = this.element.data("history-serviceurl");
        var row = $(e.currentTarget).closest("tr").get(0);
        if (row != null) {
            var dataItem = this.dataItem(row) as SutureSign.RequestHistoryItem;
            var data = kendo.antiForgeryTokens();
            data.requestId = dataItem.FormId;
            $.post(url, data)
                .done(function (data, status: string, jqXHR: JQueryXHR) {
                    var viewModel = kendo.observable({
                        historyData: data
                    });

                    let win = $("#RequestHistory").getKendoWindow();
                    win.title(`History for DocID : ${dataItem.FormId}`)
                        .content(historyTemplate(viewModel))
                        .center()
                        .open();

                    $(".k-window-footer").remove();
                    $('<div class="k-window-footer"><button type="button" class="k-button close-button">Close</button></div>').insertAfter($("#RequestHistory"));

                    $(".close-button").click(function () {
                        win.close();
                    });
                })
                .fail(function (jqXHR: JQueryXHR, status: string) {
                    kendo.alert(`History for DocID : ${dataItem.FormId} is not available at this time. Please refresh the page or try again later.`);
                });
        }
    };

    window.retryRequestHandler = (e: kendo.ui.ButtonClickEvent) => {
        $.post("/request/retry")
            .done(function (data, status: string, jqXHR: JQueryXHR) {
                kendo.alert(`Retry fax documents succssfully.`);
            })
            .fail(function (jqXHR: JQueryXHR, status: string) {
                kendo.alert('Failed to retry fax documents');
            })
    }

    window.resendRequestHandler = (e: kendo.ui.ButtonClickEvent) => {
        var grid = e.sender.element.parents(".k-grid").getKendoGrid();
        var url = grid.element.data("resending-serviceurl");
        var selectedRows = grid.select();
        if (selectedRows.length > 0) {
            var dialog = $("div#configurationDialog").kendoDialog({
                closable: true,
                modal: true,
                title: `Confirm Resend`,
                content: "<p>Only the rejected/retracted items you selected will be resent</p>",
                actions: [
                    {
                        text: "Resend",
                        action: (e: kendo.ui.ButtonClickEvent) => {
                            var forms: number[] = [];
                            selectedRows.each((index: number, element: HTMLElement) => {
                                let historyItem = grid.dataItem(element) as SutureSign.RequestHistoryItem;
                                forms.push(historyItem.FormId);
                            });

                            var data = kendo.antiForgeryTokens();
                            data.requestIds = forms;
                            $.post(url, data)
                                .done(function (data, status: string, jqXHR: JQueryXHR) {
                                    grid.dataSource.read().then(function () {
                                        grid.refresh();
                                    });
                                })
                                .fail(function (jqXHR: JQueryXHR, status: string) {
                                    alert("failed");
                                })
                        }
                    },
                    { text: "Cancel", primary: true, action: (e: kendo.ui.ButtonClickEvent) => { dialog.getKendoDialog().close(); } }
                ]
            });

            dialog.getKendoDialog().open();
        }
    };

    window.retractRequestHandler = (e: kendo.ui.ButtonClickEvent) => {
        var grid = e.sender.element.parents(".k-grid").getKendoGrid();
        var url = grid.element.data("retracting-serviceurl");
        var selectedRows = grid.select();
        if (selectedRows.length > 0) {
            var dialog = $("div#configurationDialog").kendoDialog({
                closable: true,
                modal: true,
                title: `Confirm Retract`,
                content: "<p>Only the pending items you selected will be retracted and removed from the intended signer's inbox.</p><p>Signed or rejected items can only be archived.</p>",
                actions: [
                    {
                        text: "Retract",
                        action: (e: kendo.ui.ButtonClickEvent) => {
                            var forms: number[] = [];
                            selectedRows.each((index: number, element: HTMLElement) => {
                                let historyItem = grid.dataItem(element) as SutureSign.RequestHistoryItem;
                                forms.push(historyItem.FormId);
                            });

                            var data = kendo.antiForgeryTokens();
                            data.requestIds = forms;
                            $.post(url, data)
                                .done(function (data, status: string, jqXHR: JQueryXHR) {
                                    // kendo.alert(`${forms.length} items were withdrawn from the provider's inbox.`);
                                    grid.dataSource.read().then(function () {
                                        grid.refresh();
                                    });
                                })
                                .fail(function (jqXHR: JQueryXHR, status: string) {
                                    alert("failed");
                                })
                        }
                    },
                    { text: "Cancel", primary: true, action: (e: kendo.ui.ButtonClickEvent) => { dialog.getKendoDialog().close(); } }
                ]
            });

            dialog.getKendoDialog().open();
        }
    };

    let initialDataload = true;
    window.OnGridRequestStart = (e: kendo.data.DataSourceRequestStartEvent) => {
        if (!initialDataload) {
            // $("[data_filter_grid].k-dropdown").getKendoDropDownList().enable(false);

        }
    }

    window.OnGridRequestEnd = (e: kendo.data.DataSourceRequestEndEvent) => {
        if (initialDataload) {
            window.LoadingPanel.visible(false);
            initialDataload = false;
        }
        else {
            // $("[data_filter_grid]").getKendoDropDownList().enable(true);

        }
    }

    window.resetFilterHandler = (e: kendo.ui.ButtonClickEvent) => {
        window.top?.location.reload();
    };

    window.rowTooltipHandler = (e: kendo.ui.TooltipRequestStartEvent) => {
        let rowUid = $(e.target![0]).attr("data-uid")!,
            grid = $(e.sender.element[0]).getKendoGrid();

        return (grid.dataSource.getByUid(rowUid) as any).RowTooltip;
    };

    window.PatientNameFilterHandler = (e: kendo.ui.GridTemplateHandlerEvent) => {
        e.element.kendoComboBox({
            "dataSource": e.dataSource,
            "filter": "contains",
            "autoBind": false,
            "valuePrimitive": true,
            "minLength": 3,
            "enforceMinLength": true,
            "dataValueField": "PatientId",
            "dataTextField": "SearchSummary",
            "syncValueAndText": false,
            "clearButton": false
            });
    };

    function removeFilter(filter: any, searchFor: string, operator: (null | string) = null): any {
        if (filter == null)
            return [];

        for (var x = 0; x < filter.length; x++) {
            if (filter[x].filters != null && filter[x].filters.length >= 0) {
                if (filter[x].filters.length == 0) {
                    filter.splice(x, 1);
                    return removeFilter(filter, searchFor, operator);
                }
                filter[x].filters = removeFilter(filter[x].filters, searchFor, operator);
                if (filter[x].filters.length == 0)
                    filter.splice(x, 1);
            }
            else {
                if (filter[x].field == searchFor && (operator == null || filter[x].operator == operator)) {
                    filter.splice(x, 1);
                    return removeFilter(filter, searchFor, operator);
                }
            }
        }

        return filter;
    };

    interface IHash {
        [key: string]: (JQuery.jqXHR | undefined);
    }

    let currentRequests: IHash = {};
    $.ajaxPrefilter(function (options: JQueryAjaxSettings, originalOptions, jqXHR) {
        if (currentRequests) {
            if (options && options.url) {
                var url = options.url;
                var currentRequest = currentRequests[url];
                if (currentRequest) {
                    currentRequest.abort();
                }
                currentRequests[url] = jqXHR;
            }
        }
    });

    $(() => {
        constrictFromDate(false);

        $("input[data-filter-grid][data-filter-field][data-filter-default]").each(function (index, element) {
            $(element).getKendoDropDownList().bind("change", function (e: kendo.ui.DropDownListChangeEvent) {
                let grid = $(e.sender.element.attr("data-filter-grid")!).getKendoGrid();
                let fieldName = e.sender.element.attr("data-filter-field")!;
                if (grid) {
                    // get the current list of filters, remove this one, and add it back if its not hte default value;
                    let filters = grid.dataSource.filter()?.filters ?? [];
                    filters = removeFilter(filters, fieldName);
                    let value = e.sender.value();
                    if (value) {
                        filters.push({ field: fieldName, operator: "eq", value: value.length > 0 ? value : null });
                    }

                    grid.dataSource.filter(filters);
                }
            });
        });
    });
})();