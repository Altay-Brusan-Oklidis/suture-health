(function () {
    var droppedAnnotations = [],
        textAreaValidationEnabled = false;

    $(document).ready(function () {
        $("#divImg").droppable({
            over: function (event, ui) {
                var droppedElement = ui.helper;
                droppedElement.addClass('annotationDragging');
            },
            drop: function (event, ui) {
                var droppedArea = $(this);
                var droppedElement = ui.helper;
                droppedElement.removeClass('annotationDragging');
                var draggableElement = ui.draggable;
                if (droppedElement.hasClass('annotationDragged') === false) {
                    var clone = droppedElement.clone();
                    var coordinates = {
                        //left: draggableElement.offset().left - droppedArea.offset().left + parseInt(clone.css('left')),
                        //top: draggableElement.offset().top - droppedArea.offset().top + parseInt(clone.css('top')) + $('#divImg').scrollTop(),
                        height: draggableElement.height(),
                        width: draggableElement.width(),
                        type: draggableElement.attr('id'),
                    };

                    //Set the top/left coordinates based on the page the annotation is dropped on
                    var page = 0;
                    var annotationPage = undefined;
                    $('.documentPage').each(function () {
                        var documentPage = $(this);
                        if (documentPage.offset().top <= droppedElement.offset().top) {
                            page++;
                            annotationPage = documentPage;
                        }
                    });
                    coordinates.pageNumber = page;
                    coordinates.top = droppedElement.offset().top - annotationPage.offset().top;
                    coordinates.left = droppedElement.offset().left - annotationPage.offset().left;

                    if (coordinates.type === 'checkBox') {
                        coordinates.top += 19;
                        coordinates.left += 5;
                    }

                    coordinates.required = coordinates.type == 'textArea' && textAreaValidationEnabled;

                    addCloneAnnotationToImage(clone, coordinates);
                    selectAnnotation(clone);
                    droppedArea.trigger("Document:AnnotationAdded");
                }
            }
        });

        documentReady();
    });

    window.AnnotationEditor = {
        "Initialize": function (annotations, configuration) {
            droppedAnnotations = annotations;
            textAreaValidationEnabled = SH.GetResultOrDefault(function () { return configuration.TextAreaValidationEnabled; }, false);
        },
        "GetAnnotations": getDroppedAnnotations,
        "Validate": getValidationSummary
    }

    function documentReady() {
        var onDocumentEdited = function (event) {
            $("#divImg").data("document-changed", "true");
        };

        $('#divImg').on('click', '.closeBtn', function (event) {
            var closeButton = $(this);
            closeButton.parents('.annotation').remove();
        });

        $('#divImg').on('click', '.checkBox .box', function (event) {
            var checkBoxInput = $(this).parent().find('input')[0];
            setCheckMark($(this).parent(), !checkBoxInput.checked);
        });
        $('#divImg').on('click', 'img', function () {
            deselectAllAnnotations();
        });

        $('#divImg').on('keyup change', '.annotation textarea', function (event) {
            var textArea = $(this);

            resizeTextareaBasedOnContents(textArea, event.code);
            setTextAreaAnnotationInputState(textArea);
        });

        $('#divImg').on('mousedown', '.annotationDragged', function (event) {
            selectAnnotation($(this));
        });

        $("#divImg").data("document-changed", "false");
        $("#divImg")//.on("Document:AnnotationAdded", onDocumentEdited)
            .on("change", ".annotationDragged.textArea textarea", onDocumentEdited)
            .on("click", ".annotationDragged .box", onDocumentEdited)
            .on("click", ".closeBtn", function (event) {
                var parent = $(this).closest(".annotationDragged");

                if ((parent.hasClass("checkBox") && $("input[type='checkbox']", parent).prop("checked")) || (parent.hasClass("textArea") && $("textarea", parent).val() != "")) {
                    onDocumentEdited();
                }
            });

        makeAnnotationsInHeaderDraggable($("#signature"));
        makeAnnotationsInHeaderDraggable($("#datetimeSigned"));
        makeAnnotationsInHeaderDraggable($("#textArea"));
        makeAnnotationsInHeaderDraggable($("#checkBox"));
        populateAllAnnotations();
    }

    function setCheckMark(annotation, changeToChecked) {
        var checkBoxInput = annotation.find('input')[0];
        var box = annotation.find('.box');
        if (changeToChecked) {
            checkBoxInput.checked = true;
            box.addClass('checked');
        } else {
            checkBoxInput.checked = false;
            box.removeClass('checked');
        }
    }

    function selectAnnotation(annotation, scroll) {
        if (annotation.hasClass('editableContent') === false) {
            deselectAllAnnotations();
            return;
        }
        var editableAnnotation = annotation;
        if (editableAnnotation.hasClass('selected') === false) {
            deselectAllAnnotations();
            editableAnnotation.addClass('selected');
        }
        if (scroll) {
            var top = document.getElementById(annotation[0].id).offsetTop + document.getElementById(annotation[0].id).offsetParent;
            window.scrollTo(0, top);
            $('#' + annotation[0].id).find('textarea').focus();
        }
    }

    function deselectAllAnnotations() {
        $('.annotation.selected').removeClass('selected');
    }

    function makeAnnotationsInHeaderDraggable(element) {
        element.draggable({
            helper: 'clone',
            containment: '#divImg'
        });
        return element;
    }

    function setTextAreaAnnotationInputState(element) {
        element.parent(".annotationDragged").toggleClass("needsData", (element.val().trim() === ""));
    }

    function populateAllAnnotations() {
        $('#divImg .annotation').remove();

        droppedAnnotations.sort(function (a, b) {
            if (a.pageNumber == b.pageNumber) {
                return b.top - a.top;
            } else {
                return b.pageNumber - a.pageNumber;
            }
        });
        droppedAnnotations.forEach(function (annotation, index) {
            var draggableAnnotationInHeader = $('#' + annotation.type);
            if (draggableAnnotationInHeader) {
                var clone = draggableAnnotationInHeader.clone();
                adjustAnnotationCoordinatesForPadding(annotation);
                addCloneAnnotationToImage(clone, annotation);
            }
        });
    }
    function adjustAnnotationCoordinatesForPadding(coordinates) {
        if (coordinates.type === 'signature') {
            coordinates.top -= 6;
            coordinates.left -= 3;
            coordinates.height = 42;
        }
        if (coordinates.type === 'datetimeSigned') {
            coordinates.left += 3;
            //    coordinates.width -= 2;
            coordinates.top -= 2;
            //    coordinates.height -= 5;
        }
        if (coordinates.type === 'checkBox') {
            coordinates.left += 1;
            //    coordinates.width -= 1;
            coordinates.top -= 2;
            //    coordinates.height -= 1;
        }
        if (coordinates.type === 'textArea') {
            coordinates.left += 3;
            coordinates.width -= 2;
            coordinates.top -= 1;
            coordinates.height -= 1;
        }
    }
    function removePaddingFromCoordinates(coordinates) {
        if (coordinates.type === 'signature') {
            coordinates.top += 6;
            coordinates.left += 3;
            coordinates.height = 24;
        }
        if (coordinates.type === 'datetimeSigned') {
            coordinates.left -= 3;
            //    coordinates.width += 2;
            coordinates.top += 2;
            //    coordinates.height += 5;
        }
        if (coordinates.type === 'checkBox') {
            coordinates.left -= 1;
            //    coordinates.width += 1;
            coordinates.top += 2;
            //    coordinates.height += 1;
        }
        if (coordinates.type === 'textArea') {
            coordinates.left -= 3;
            coordinates.width += 2;
            coordinates.top += 1;
            coordinates.height += 1;
        }
    }

    function addCloneAnnotationToImage(clone, coordinates) {
        var type = coordinates.type;
        var pageContainerSelector = "#divImg #page" + coordinates.pageNumber;
        clone.css('left', coordinates.left);
        clone.css('top', coordinates.top);
        if (type != 'checkBox') {
            clone.css('height', coordinates.height);
            clone.css('width', coordinates.width);
        }
        clone.css('position', 'absolute');

        clone.attr('id', findNextAvailableCloneId(type));
        clone.attr('annotationId', coordinates.id);

        clone.addClass('annotationDragged');
        clone.draggable({
            helper: 'original',
            containment: pageContainerSelector
        });
        if (type === 'signature') {
            var resizableOptions = {
                handles: 'e',  // resize horizontally on the right side only
                maxWidth: 400,
                containment: "parent"
            };

            clone.resizable(resizableOptions);
        }

        if (type === 'textArea') {
            var resizableOptions = {
                handles: 'nw, sw, se',  // resize horizontally on the right side only
                minWidth: 10,
                maxWidth: 850,
                containment: "parent",
                stop: function (event, ui) {
                    var textarea = $(this).find('textArea');
                    textarea.scrollTop(textarea.height());
                    resizeTextareaBasedOnContents(textarea);
                }
            };
            clone.resizable(resizableOptions);

            if (coordinates.required) {
                $(clone).addClass('textarea-required');
                clone.attr("title", "Required");
            }
            else {
                clone.attr("title", "Optional");
            }
        }
        if (type === 'checkBox') {
            var checked = coordinates.value === 'true' ? true : false;
            setCheckMark(clone, checked);
        }
        $(pageContainerSelector).append(clone);
        enableInputOn(clone, 'textArea');
        enableInputOn(clone, 'input[type=checkbox]');
        if (type == 'textArea') {
            var textArea = clone.find('textArea');
            textArea.val(coordinates.value);
            resizeTextareaBasedOnContents(textArea);
            setTextAreaAnnotationInputState(clone.find("textArea"));
        }
    }

    function calculateAllAnnotationCoordinates() {
        deselectAllAnnotations();
        droppedAnnotations = [];
        var pages = $('.documentPage');
        for (var p = 0; p < pages.length; p++) {
            var page = $(pages[p]);
            var annotations = page.find('.annotationDragged');
            if (annotations && annotations.length > 0) {
                for (var i = 0; i < annotations.length; i++) {
                    var annotation = annotations[i];
                    var id = $(annotation).attr('id');
                    var coordinates = {
                        type: getTypenameFromCloneId(id),
                        id: $(annotation).attr('annotationid'),
                        left: parseInt($(annotation).css('left')),
                        top: parseInt($(annotation).css('top')),
                        height: $(annotation).height(),
                        width: $(annotation).width(),
                        pageHeight: page.height(),
                        pageNumber: p + 1,
                        value: getAnnotationValueFromCloneId(id),
                        required: $(annotation).hasClass('textarea-required')
                    };


                    removePaddingFromCoordinates(coordinates);
                    droppedAnnotations.push(coordinates);
                }
            }
        }
    }

    function getTypenameFromCloneId(id) {
        if (id && id.indexOf('Clone') > 0) {
            return id.substring(0, id.indexOf('Clone'));
        } else {
            return id;
        }
    }

    function getAnnotationValueFromCloneId(id) {
        if (id) {
            var type = getTypenameFromCloneId(id);
            var element = $("#" + id);

            if (element.length === 1) {
                switch (type) {
                    case "textArea":
                        return $("textarea", element).val();
                        break;
                    case "checkBox":
                        return $("input", element).is(":checked").toString();
                        break;
                }
            }
        }

        return "";
    }

    function resizeTextareaBasedOnContents(textarea, keyPressedCode) {
        textarea.val(textarea.val().substring(0, 2799));
        var container = textarea.parents().first();
        var maximumHeight = 500;
        textarea.selectionStart = textarea.selectionEnd = textarea.val().length;

        textarea.scrollTop(textarea.height());
        while (textarea.scrollTop() > 0 && textarea.val() != '') {
            var containerHeight = container.height()
            if (containerHeight >= maximumHeight) {
                container.css('height', maximumHeight);
                var currentTextAreaContent = textarea.val();
                textarea.val(currentTextAreaContent.substring(0, currentTextAreaContent.length - 10));
            }
            else {
                container.css('height', containerHeight + 2);
            }
        }
    }

    function enableInputOn(clone, type) {
        var control = clone.find(type);
        if (control.length > 0) {
            control.removeAttr('disabled');
            control.focus();
        }
    }

    function findNextAvailableCloneId(annotationType) {
        var idPrefix = annotationType + 'Clone';
        for (var j = 0; j < $('.' + annotationType).length; j++) {
            var potentialId = idPrefix + j.toString();
            if ($('#' + potentialId).length <= 0) {
                return potentialId;
            }
        }
    }

    function getDroppedAnnotations() {
        var data = [];
        droppedAnnotations.forEach(function (d) {
            data.push({
                "Type": (function () {
                    switch (d.type) {
                        case "signature":
                            return "VisibleSignature";
                        case "datetimeSigned":
                            return "DateSigned"
                        case "checkBox":
                            return "CheckBox"
                        case "textArea":
                            return "TextArea"
                    }
                })(),
                "AnnotationId": d.id,
                "Left": parseInt(d.left, 10),
                "Top": parseInt(d.top, 10),
                "Height": parseInt(d.height, 10),
                "Width": parseInt(d.width, 10),
                "PageNumber": parseInt(d.pageNumber, 10),
                "PageHeight": parseInt(d.pageHeight, 10),
                "Value": d.value,
                "Required": d.required
            });
        });

        return data;
    }

    function getValidationSummary(selectInvalidAnnotations) {
        var missingTextAreaValue = "One or more required fields are incomplete.";
        var missingSignature = "A document must contain at least one signature field.";
        var errors = [];

        calculateAllAnnotationCoordinates();

        if (textAreaValidationEnabled && droppedAnnotations.filter(function (annotation) { return (annotation.type == "textArea" && (annotation.required == true && (annotation.value == null || annotation.value.trim().length == 0))); }).length > 0) {
            errors.push(missingTextAreaValue);
        }

        if (droppedAnnotations.filter(function (annotation) { return (annotation.type == "signature"); }).length == 0) {
            errors.push(missingSignature);
        }

        if (selectInvalidAnnotations) {
            selectFirstInvalidAnnotation();
        }

        return {
            "Success": (errors.length == 0),
            "Errors": errors
        };
    }

    function selectFirstInvalidAnnotation() {
        var emptyAnnotations = droppedAnnotations.filter(function (annotation) { return (annotation.type == "textArea" && (annotation.required == true && (annotation.value == null || annotation.value.trim().length == 0))); });
        if (emptyAnnotations.length > 0) {
            emptyAnnotations.sort(function (a, b) {
                if (a.pageNumber == b.pageNumber) {
                    return a.top - b.top;
                } else {
                    return a.pageNumber - b.pageNumber;
                }
            })
            selectAnnotation($(".annotation[annotationid='" + emptyAnnotations[0].id + "']"), true);
        }
    }
})();