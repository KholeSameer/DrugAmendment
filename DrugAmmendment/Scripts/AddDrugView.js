
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
                            alert(err + " Something went wrong while fetching the data.");
                        }
                    });
                },
                minLength: 3
            });
        });
    }
    $("#criteria").on("keydown", function (e) {
        var char = $("#criteria").val().length;
        if (char == 0)
            return e.which !== 32;
    });
})
            