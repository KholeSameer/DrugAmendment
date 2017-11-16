
    $(window).load(function () {
        GetClient();
    });

    $(window).bind("pageshow", function () {
        $('#criteriaType').val('');
    $('#criteriaType option').text("Select Criteria Type");
        $('#criteriaType option').attr('selected', 'selected');
    });

    $(document).ready(function () {

        $('#client').change(function () {
            var clientName = $('#client').val();
            if (clientName == '') {
                $('#criteriaType').val('');
                $('#criteriaType option').text("Select Criteria Type");
                $('#criteriaType option').attr('selected', 'selected');
            }
            else {
                GetCriteria(clientName);
            }

        });
    $('#activeDrugsBtn').click(function () {
            var clientName = $('#client').val();
            var criteriaType = $('#criteriaType').val();
            if (clientName != '' && criteriaType != '') {
        GetActiveDrugs(clientName, criteriaType);
    }
            else {
        alert("All Fields are Mandatory...!");
    }
        });
        $('#deleteDrugBtn').click(function () {
            var clientName = $('#client').val();
            var criteriaType = $('#criteriaType').val();
            if (clientName != '' && criteriaType != '') {
                var url = "/Home/DeleteDrugView?clientName=" + clientName + "&criteriaType=" + criteriaType;
                window.location.href = url;
            }
            else {
        alert("All Fields are Manadatory...!");
    }

        });
    });

    function GetClient() {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: ' /Home/PopulateClients',
            dataType: 'json',
            success: function (data) {
                var clientOptions;
                clientOptions = "<option value=''>Select Customer</option>";
                var json = JSON.stringify(data);
                $.each(data, function (i, json) {
                    var ClientNames = json.Text.substring(json.Text.indexOf(".") + 1);
                    ClientNames = ClientNames.charAt(0).toUpperCase() + ClientNames.slice(1);
                    clientOptions += "<option value='" + json.Text + "'>" + ClientNames + "</option>";
                });
                $('#client').html(clientOptions);
            },
            error: function (err) {
                alert('Something went wrong...!');
            }
        });
    }

    function GetCriteria(clientName) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: ' /Home/PopulateCriteriaType',
            data: { ClientName: clientName },
            dataType: 'json',
            success: function (data) {
                var criteriaTypeOptions;
                var json = JSON.stringify(data);
                $.each(data, function (i, json) {
                    criteriaTypeOptions += "<option value='" + json.Text + "'>" + json.Text + "</option>";
                });

                $('#criteriaType').html(criteriaTypeOptions);
            },
            error: function (err) {
                alert('Something went wrong...!');
            }
        });
    }

    function GetActiveDrugs(clientName, criteriaType) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: ' /Home/GetDrugList',
            data: { ClientName: clientName, CriteriaType: criteriaType },
            dataType: 'json',
            success: function (data) {
                var json = JSON.stringify(data);
                var tableRow = "";
                var dynamicID = clientName.substring(clientName.indexOf(".") + 1);
                var table = "<table id='DrugDetailsTable" + dynamicID + "'>";
                var tableHeader = "<thead><tr><th>Criteria</th><th>TermID</th><th>Modification Date</th><th>Creation Date</th></tr ></thead >";
                $.each(data, function (i, json) {
                    var date = null;
                    var CreationDate = null;
                    var ModificationDate = null;

                    if (json.CreationDate != null) {
                        date = new Date(parseInt(json.CreationDate.substr(6)));
                        CreationDate = date.getFullYear() + "-" +
                            ("0" + (date.getMonth() + 1)).slice(-2) + "-" +
                            ("0" + date.getDate()).slice(-2) + " " + date.getHours() + ":" +
                            date.getMinutes();
                    }
                    if (json.ModificationDate != null) {
                        date = new Date(parseInt(json.ModificationDate.substr(6)));
                        ModificationDate = date.getFullYear() + "-" +
                            ("0" + (date.getMonth() + 1)).slice(-2) + "-" +
                            ("0" + date.getDate()).slice(-2) + " " + date.getHours() + ":" +
                            date.getMinutes();
                    }

                    tableRow += '<tr><td>' + json.Criteria + '</td><td>' + json.TermID + '</td><td>' + ModificationDate + '</td><td>' + CreationDate + '</td></tr>';

                    //console.log(json.Criteria + "  " + json.TermID + "  " + ModificationDate + "  " + CreationDate);
                });
                var appendData = table + tableHeader + tableRow;
                appendData += "</table >";
                $('#DrugDetailsDiv').html(appendData);
                //Pagination
                $("#DrugDetailsTable" + dynamicID +"").DataTable();
            },
            error: function (err) {
                alert('Something went wrong...!');
            }
        });
    }

    function DeleteDrug(clientName, criteriaType) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: ' /Home/DeleteDrug',
            data: { ClientName: clientName, CriteriaType: criteriaType },
            dataType: 'json',
            success: function (data) {
                alert("Deleted Successfully...!");
            },
            error: function (err) {
                alert('Something went wrong...! Drug not deleted/Updated');
            }
        });
    }
 
