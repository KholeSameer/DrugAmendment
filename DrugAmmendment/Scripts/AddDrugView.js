
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
                                console.log(data);
                                response($.map(data, function (item) {
                                    return {
                                        label: item,
                                        value: item
                                    };
                                }));

                            },
                            error: function (err) {
                                debugger;
                                alert(err + "   Some Error Message");
                            }
                        });
                    },
                    minLength: 3
                });
            });
