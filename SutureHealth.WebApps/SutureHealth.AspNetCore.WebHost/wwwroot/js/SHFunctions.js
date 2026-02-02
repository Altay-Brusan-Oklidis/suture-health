var SH;
(function (SH, undefined) {
    SH.GetValueOrDefault = function (value, defaultValue) {
        return (value == undefined || value == null) ? defaultValue : value;
    };

    SH.GetResultOrDefault = function (func, defaultValue) {
        if (typeof func !== 'function') {
            return defaultValue;
        }

        try {
            return SH.GetValueOrDefault(func(), defaultValue);
        }
        catch (error) {
            return defaultValue;
        }
    };
})(SH || (SH = {}));