
$(document).ready(function () {
    var isBrandname = $('#criteriaType').val();
    if (isBrandname == 'BrandName') {
    }
    else {
        $(function () {
            $("#criteria").autocomplete({
                source: function (request, response) {
                    var param = { criteria: $('#criteria').val(), delivery: $('#client').val(), criteriaType: $('#criteriaType').val() };
                    $.ajax({
                        url: "GetAutoTHSTerm",
                        data: JSON.stringify(param),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        dataFilter: function (data) { return data; },
                        success: function (data) {
                            response($.map(data, function (item) {
                                return {
                                    label: item,
                                    value: item
                                };
                            }));

                        },
                        error: function (err) {
                            alert(err + "   Some Error Message");
                        }
                    });
                },
                minLength: 3
            });
        });
    }
})
            