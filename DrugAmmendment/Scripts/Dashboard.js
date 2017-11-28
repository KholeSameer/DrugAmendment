
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
                GetDrugList(clientName, criteriaType);
    }
            else {
        alert("All Fields are Mandatory...!");
    }
        });

        $('#exportToExcel').click(function () {
            var clientName = $('#client').val();
            var criteriaType = $('#criteriaType').val();
            if (clientName != '' && criteriaType != '') {
                ExportFunction(clientName, criteriaType);
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

    function GetDrugList(clientName, criteriaType) {
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
                var tableHeader = "<thead><tr><th>Criteria</th><th>Modification Date</th><th>Creation Date</th><th>IsActive</th></tr ></thead >";
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

                    tableRow += '<tr><td>' + json.Criteria + '</td><td>' + ModificationDate + '</td><td>' + CreationDate + '</td><td>' + json.IsActive + '</td></tr>';

                    //console.log(json.CriteriaType + "  " + json.IsActive);
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
 
    function ExportFunction(clientName, criteriaType) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: ' /Home/GetActiveDrugList',
            data: { ClientName: clientName, CriteriaType: criteriaType },
            dataType: 'json',
            success: function (data) {
                var json = JSON.stringify(data);
                var dynamicFileName = clientName.substring(clientName.indexOf(".") + 1);
                dynamicFileName = dynamicFileName.charAt(0).toUpperCase() + dynamicFileName.slice(1);
                dynamicFileName += "_" + criteriaType + "_" + (new Date($.now())).toString().slice(0, -31).slice(4);
                JSONToCSVConvertor(data, dynamicFileName , true);

                //$.each(data, function (i, json) {
                //    console.log(json.Criteria);
                //});
            },
            error: function (err) {
                alert('Something went wrong...!');
            }
        });
    }



 function JSONToCSVConvertor(JSONData,title, ShowLabel) {
            var arrData = typeof JSONData != 'object' ? JSON.parse(JSONData) : JSONData;
            var CSV = '';
            if (ShowLabel) {
                var row = "";
                for (var index in arrData[0]) {
            row += index + ',';
        }
                row = row.slice(0, -1);
                CSV += row + '\r\n';
            }
            for (var i = 0; i < arrData.length; i++) {
                var row = "";
                for (var index in arrData[i]) {
                    var arrValue = arrData[i][index] == null ? "" : '="' + arrData[i][index] + '"';
                    row += arrValue + ',';
                }
                row.slice(0, row.length - 1);
                CSV += row + '\r\n';
            }
            if (CSV == '') {
            growl.error("Invalid data");
        return;
            }
            var fileName = title;
            if (msieversion()) {
                var IEwindow = window.open();
                IEwindow.document.write('sep=,\r\n' + CSV);
                IEwindow.document.close();
                IEwindow.document.execCommand('SaveAs', true, fileName + ".csv");
                IEwindow.close();
            } else {
                var uri = 'data:application/csv;charset=utf-8,' + escape(CSV);
                var link = document.createElement("a");
                link.href = uri;
                link.style = "visibility:hidden";
                link.download = fileName + ".csv";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        }
        function msieversion() {
            var ua = window.navigator.userAgent;
            var msie = ua.indexOf("MSIE ");
            if (msie != -1 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) // If Internet Explorer, return version number
            {
                return true;
            } else { // If another browser, 
                return false;
            }

        }
