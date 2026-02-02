
querySelectorAllAsArray = function (selector, node) {
    if (!node) {
        node = document;
    }
    return Array.prototype.slice.call(node.querySelectorAll(selector));
}
function disableButtonOnClick(selector, replacementText) {
    document.querySelectorAll(selector)
        .forEach(function (node) {
            node.addEventListener('click', function (event) {
                event.preventDefault();
                var element = this;
                if (element.disabled == false) {
                    element.disabled = true;
                    if (element.form != null) {
                        element.form.submit();
                        element.form.disabled = true;
                    }
                }
                if (replacementText) {
                    node.innerText = replacementText;
                }
                return false;
            });
        });
}

const delayAsync = function (ms) { return new Promise(function (res) { setTimeout(res, ms) }) };


function postAjax(url, data, success) {
    var params = typeof data == 'string' ? data : Object.keys(data).map(
        function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]) }
    ).join('&');
    var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
    xhr.open('POST', url);
    xhr.onreadystatechange = function () {
        if (xhr.readyState > 3 && xhr.status == 200) {
            if (success != null) {
                success(xhr.responseText);
            }
        }
    };
    xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.send(params);
    return xhr;
}
