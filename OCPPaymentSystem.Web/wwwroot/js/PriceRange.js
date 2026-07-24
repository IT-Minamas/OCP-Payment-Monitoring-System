$(document).ready(function () {

    loadCompany();

    loadPriceRange();

    $("#btnSearch").click(function () {
        loadPriceRange();
    });

});

function newData() {

    clearForm();

    $("#DetailMill").focus();

}

$(document).on(
    "click",
    "#tblPriceRange tbody tr",
    function () {

        loadDetail($(this).data("id"));

    });

$(document).on("click", "#btnNew", function () {

    clearForm();

});

$(document).on("click", "#btnSave", function () {

    savePriceRange();

});

$(document).on("click", "#btnDelete", function () {

    deletePriceRange();

});

$(document).on("click", "#Attachment", function () {

    if ($("#Attachment").val() == "")
        return;

    downloadAttachment();

});

$(document).on("click", "#btnAttachment", function () {

    if ($("#ID").val() == "") {

        alert("Please save Price Range first.");

        return;

    }

    $("#fileAttachment").click();

});

$("#fileAttachment").change(function () {

    uploadAttachment(this.files[0]);

});

function loadCompany() {
    $.ajax({
        url: "/PriceRange/Mill",
        type: "GET",

        success: function (data) {
            console.table(data);
            $("#Mill").empty();

            $("#Mill").append(
                "<option value=''>-- Select Mill --</option>");

            $.each(data, function (i, item) {

                $("#Mill").append(

                    "<option value='" +
                    item.code +
                    "'>" +
                    item.code +
                    " - " +
                    item.name +
                    "</option>");

            });

            $("#DetailMill").html(
                $("#Mill").html());

        }

    });

}

function loadPriceRange() {

    let request = {

        CompanyCode: "",

        MillCode: $("#Mill").val(),

        DateFrom:
            $("#DateFrom").val() == ""
                ? null
                : $("#DateFrom").val(),

        DateTo:
            $("#DateTo").val() == ""
                ? null
                : $("#DateTo").val()

    };

    $.ajax({

        url: "/PriceRange/PriceRangeList",

        type: "POST",

        contentType: "application/json",

        data: JSON.stringify(request),

        success: function (data) {

            $("#tblPriceRange tbody").empty();

            $.each(data, function (i, x) {

                var tr = $("<tr>");

                tr.attr("data-id", x.id);

                tr.append("<td>" + (i + 1) + "</td>");

                tr.append("<td>" + x.millCode + ' - ' + x.millName + "</td>");

                tr.append("<td>" + formatDate(x.dateFrom) + "</td>");

                tr.append("<td>" + formatDate(x.dateTo) + "</td>");

                tr.append("<td class='text-end'>" +
                    Number(x.priceFrom).toLocaleString() +
                    "</td>");

                tr.append("<td class='text-end'>" +
                    Number(x.priceTo).toLocaleString() +
                    "</td>");

                tr.append("<td>" +
                    (x.attachmentFileName ?? "") +
                    "</td>");

                $("#tblPriceRange tbody").append(tr);

            });

        },

        error: function (xhr) {

            alert(xhr.responseText);

        }

    });

}
function loadDetail(id) {

    $.get(

        "/PriceRange/Detail",

        {
            id: id
        },

        function (x) {

            $("#ID").val(x.id);

            $("#DetailMill").val(x.millCode);

            let dt = new Date(x.dateFrom);

            let month =
                String(dt.getMonth() + 1).padStart(2, '0');

            let day =
                String(dt.getDate()).padStart(2, '0');

            let year =
                dt.getFullYear();

            $("#DetailDateFrom").val(
                `${year}-${month}-${day}`);

            dt = new Date(x.dateTo);

            month =
                String(dt.getMonth() + 1).padStart(2, '0');

            day =
                String(dt.getDate()).padStart(2, '0');

            year =
                dt.getFullYear();

            $("#DetailDateTo").val(
                `${year}-${month}-${day}`);

            $("#PriceFrom").val(x.priceFrom);

            $("#PriceTo").val(x.priceTo);

            $("#Attachment").val(
                x.attachmentFileName);

        });

}

function savePriceRange() {

    if ($("#DetailMill").val() == "") {
        alert("Please select Mill.");
        return;
    }

    if ($("#DetailDateFrom").val() == "") {
        alert("Please select Date From.");
        return;
    }

    if ($("#DetailDateTo").val() == "") {
        alert("Please select Date To.");
        return;
    }

    if ($("#PriceFrom").val() == "") {
        alert("Please input Price Range From.");
        return;
    }

    if ($("#PriceTo").val() == "") {
        alert("Please input Price Range To.");
        return;
    }

    var data = {
        ID: $("#ID").val() == "" ? 0 : parseInt($("#ID").val()),
        MillCode: $("#DetailMill").val(),
        DateFrom: $("#DetailDateFrom").val(),
        DateTo: $("#DetailDateTo").val(),
        PriceFrom: parseFloat($("#PriceFrom").val()),
        PriceTo: parseFloat($("#PriceTo").val())
    };

    let url =
        $("#ID").val() == ""
            ? "/PriceRange/Save"
            : "/PriceRange/Update";

    //alert(url);
    console.table(data);
    //return;
    console.log(JSON.stringify(data));

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        processData: false,

        success: function (result) {

            if (!result.success) {

                alert(result.message);

                return;

            }

            if ($("#ID").val() == "")
                $("#ID").val(result.data);

            if ($("#fileAttachment")[0].files.length > 0) {

                uploadAttachment(
                    $("#fileAttachment")[0].files[0]);

                return;

            }

            loadPriceRange();

            clearForm();

            alert("Data saved successfully.");


        },

        error: function (xhr) {

            alert(xhr.responseText);

        }

    });

}

function deletePriceRange() {

    if ($("#ID").val() == "") {

        alert("No Price Range selected.");

        return;

    }

    if (!confirm("Delete this Price Range ?"))
        return;

    $.ajax({

        url: "/PriceRange/Delete",

        type: "POST",

        contentType: "application/json",

        data: JSON.stringify({

            ID: $("#ID").val()

        }),

        success: function (result) {

            if (!result.success) {

                alert(result.message);

                return;

            }

            alert("Price Range deleted.");

            clearForm();

            loadPriceRange();

        },

        error: function (xhr) {

            alert(xhr.responseText);

        }

    });

}

function uploadAttachment(file) {

    if (file == null)
        return;

    let data = new FormData();

    data.append(
        "id",
        $("#ID").val());

    data.append(
        "file",
        file,
        file.name);

    $.ajax({

        url: "/PriceRange/UploadAttachment",

        type: "POST",

        data: data,

        processData: false,

        contentType: false,

        success: function (result) {

            if (!result.success) {

                alert(result.message);

                return;

            }

            loadPriceRange();

            //loadDetail($("#ID").val());
            clearForm();

            alert("Attachment uploaded.");

        },

        error: function (xhr) {

            alert(xhr.responseText);

        }

    });

}

function downloadAttachment() {

    if ($("#ID").val() == "")
        return;

    window.open(

        "/PriceRange/DownloadAttachment?id="

        + $("#ID").val(),

        "_blank");

}

function clearForm() {

    $("#ID").val("");

    $("#DetailMill").prop("selectedIndex", 0);

    $("#DetailDateFrom").val("");

    $("#DetailDateTo").val("");

    $("#PriceFrom").val("");

    $("#PriceTo").val("");

    $("#Attachment").val("");

    $("#fileAttachment").val("");

}

function formatDate(value) {

    if (!value)
        return "";

    let dt = new Date(value);

    const month =
        [
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        ];

    return ("0" + dt.getDate()).slice(-2)
        + "-"
        + month[dt.getMonth()]
        + "-"
        + dt.getFullYear();

}