$(document).ready(function () {
    $('#divImg').on('click', '.closeBtn', function (event) {
        deleteAnnotation($(this).parents('.annotation'));
    });

    $('#divImg').on('click', 'img', function () {
        deselectAllAnnotations();
    });

    $('#divImg').on('mousedown', '.annotationDragged', function (event) {
        selectAnnotation($(this));
    });

    $("#divImg").data("document-changed", "false");
    $("#divImg").on("dragstop", ".annotationDragged", function () {
        calculateAllAnnotationCoordinates(true);
        $(document).trigger("AnnotationEditor:AnnotationMouseEnter", [$(this).attr("id")]);
    });
    $("#divImg").on("mouseenter", ".annotationDragged", function () {
        $(document).trigger("AnnotationEditor:AnnotationMouseEnter", [$(this).attr("id")]);
    });
    $("#divImg").on("mouseleave", ".annotationDragged", function () {
        $(document).trigger("AnnotationEditor:AnnotationMouseLeave", [$(this).attr("id")]);
    });

    $(document).on("OcrSearch:ResultFound", function (evt, bindingResult, otherResults) {
        var createHighlightElement = function (queryResult, backgroundColor) {
            return $("<div class=\"ocrResult\" style=\"position: absolute; top: " + (queryResult.TopPixel + 2) + "px; left: " + (queryResult.LeftPixel + 3) + "px; width: " + queryResult.WidthPixels + "px; height: " + queryResult.HeightPixels + "px;\"></div>")
                    .css("background-color", backgroundColor)
                    .css("outline", "2px solid #4c4c4c")
                    .css("opacity", "0.6");
        },
            pageContainer = $("#divImg"),
            page1 = $("#page1"),
            page = $("#page" + bindingResult.PageNumber),
            ocrHighlight = createHighlightElement(bindingResult, "yellow");

        $(".ocrResult").remove();
        page.append(ocrHighlight);

        for (var i = 0; i < otherResults.length; ++i) {
            var result = otherResults[i],
                element = createHighlightElement(result, "lightyellow");

            $("#page" + result.PageNumber).append(element);
        }

        pageContainer.animate({
            "scrollTop": (((ocrHighlight.offset().top - page1.offset().top) / page1.height()) * page1.height()) - (pageContainer.height() / 2)
        });
    });
    $(document).on("OcrSearch:Submit", function () {
        $(".ocrResult").remove();
    });
    $(document).on("Annotation:Changed", function (evt, data) {
        $("#" + data.DropId).data("search-text", data.SearchText);
        $("#" + data.DropId).data("match-all", data.MatchAll);
    });
    $(document).on("Annotation:Deleted", function (evt, dropId) {
        deleteAnnotation($("#" + dropId));
    })
    $(document).on("Annotation:Initialized", function (evt, annotations) {
        droppedAnnotations = [];

        for (var i = 0; i < annotations.length; ++i) {
            var annotation = annotations[i];

            droppedAnnotations.push({
                "type": function (type) {
                    switch (type) {
                        case "VisibleSignature":
                            return "signature";
                        case "DateSigned":
                            return "datetimeSigned";
                        default:
                            return "";
                    }
                }(annotation.Type),
                "left": annotation.LeftPixel,
                "top": annotation.TopPixel,
                "height": annotation.HeightPixels,
                "width": annotation.WidthPixels,
                "pageNumber": annotation.PageNumber,
                "searchText": annotation.SearchText,
                "matchAll": annotation.MatchAll
            });
        }

        populateAllAnnotations();
        $(document).trigger("AnnotationEditor:AnnotationsChanged", [droppedAnnotations]);
    });

    $("#divImg").droppable({
        over: function (event, ui) {
            var droppedElement = ui.helper;
            droppedElement.addClass('annotationDragging');
        },
        drop: function (event, ui) {
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

                coordinates.required = true;

                addCloneAnnotationToImage(clone, coordinates);
                selectAnnotation(clone);

                calculateAllAnnotationCoordinates(true);
            }
        }
    });

    makeAnnotationsInHeaderDraggable($("#signature"));
    makeAnnotationsInHeaderDraggable($("#datetimeSigned"));
});

var droppedAnnotations = [];

function deleteAnnotation(annotation) {
    annotation.remove();
    calculateAllAnnotationCoordinates(true);
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
}

function addCloneAnnotationToImage(clone, coordinates) {
    var type = coordinates.type;
    var pageContainerSelector = "#divImg #page" + coordinates.pageNumber;
    var id = findNextAvailableCloneId(type);

    clone.css('left', coordinates.left);
    clone.css('top', coordinates.top);
    if (type != 'checkBox') {
        clone.css('height', coordinates.height);
        clone.css('width', coordinates.width);
    }
    clone.css('position', 'absolute');

    clone.attr('id', id);
    coordinates.id = id;

    clone.data("search-text", coordinates.searchText);
    clone.data("match-all", coordinates.matchAll);
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

    $(pageContainerSelector).append(clone);
}

function calculateAllAnnotationCoordinates(triggerEvent) {
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
                    id: id,
                    left: parseInt($(annotation).css('left')),
                    top: parseInt($(annotation).css('top')),
                    height: $(annotation).height(),
                    width: $(annotation).width(),
                    pageHeight: page.height(),
                    pageWidth: page.width(),
                    pageNumber: p + 1,
                    searchText: $(annotation).data("search-text"),
                    matchAll: $(annotation).data("match-all")
                };

                removePaddingFromCoordinates(coordinates);
                droppedAnnotations.push(coordinates);
            }
        }
    }

    if (triggerEvent) {
        $(document).trigger("AnnotationEditor:AnnotationsChanged", [droppedAnnotations]);
    }
}

function getTypenameFromCloneId(id) {
    if (id && id.indexOf('Clone') > 0) {
        return id.substring(0, id.indexOf('Clone'));
    } else {
        return id;
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