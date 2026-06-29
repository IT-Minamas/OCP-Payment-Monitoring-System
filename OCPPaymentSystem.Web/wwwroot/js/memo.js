$(document).ready(function () {
    loadCompany();
    loadSupplier();
    loadMemo();
});

function loadCompany() {
    $.ajax({
        url: "/Memo/Company",
        type: "GET",
        success: function (data) {
            $("#Company").empty();
            $("#Company").append(
                "<option value=''>-- Select Company --</option>");

            $.each(data, function (i, item) {
                $("#Company").append(
                    "<option value='" +
                    item.code +
                    "'>" +
                    item.code +
                    " - " +
                    item.name +
                    "</option>");
            });
        }
    });
}

function loadSupplier() {
    $.get("/Memo/Supplier", function (data) {
        $("#Supplier").empty();
        $.each(data, function (i, x) {
            $("#Supplier").append(
                $("<option>")
                    .val(x.code)
                    .text(x.name)
            );
        });
    });
}

function loadMemo() {
    $.get("/Memo/MemoList", function (data) {
        $("#tblMemo").empty();
        $.each(data, function (i, x) {
            var tr = $("<tr>");

            tr.append("<td>" + x.memoNo + "</td>");
            tr.append("<td>" + x.companyCode + "</td>");
            tr.append("<td>" + x.supplierName + "</td>");

            tr.append("<td>" + formatDate(x.memoDate) + "</td>");
            tr.append("<td style='text-align:right'>" +
                Number(x.amount).toLocaleString() +
                "</td>");
            tr.append("<td>" + x.memoNo + "</td>");
            tr.append("<td>" + x.approvalStatus + "</td>");
            tr.attr("data-id", x.memoNo);

            $("#tblMemo").append(tr);
        });
    });
}

$(document).on("click",
    "#tblMemo tr",
    function () {
        loadDetail(
            $(this).attr("data-id"));
    });

function loadDetail(memoNo) {
    $.get(
        "/Memo/Detail",
        {
            memoNo: memoNo
        },

        function (x) {
            $("#MemoNo").val(x.memoNo);
            $("#Company").val(x.companyCode);
            $("#Supplier").val(x.supplierCode);

            let dt = new Date(x.date);         
            //$("#MemoDate").val(dt.toLocaleDateString("en-GB"));

            $("#Amount").val(x.amount);
            $("#Remarks").val(x.remarks);
            $("#Invoice").val(x.invoice);
            $("#BAP").val(x.bap);
            $("#FakturPajak").val(x.fakturPajak);
        });

//    loadAttachment();
}

$(document).on("click", "#btnNew", function () {

    clearForm();

});

function clearForm() {
    $("#MemoNo").val("");

    $("#Company").prop("selectedIndex", 0);

    $("#Supplier").prop("selectedIndex", 0);

    $("#MemoDate").val("");

    $("#Amount").val("");

    $("#Remarks").val("");

    $("#Invoice").val("");

    $("#BAP").val("");

    $("#FakturPajak").val("");
}

$(document).on("click",
    "#btnSave",
    function () {
        saveMemo();
    });

function saveMemo() {
    if ($("#Company").val() == "") {
        alert("Please select Company.");
        return;
    }

    if ($("#Supplier").val() == "") {
        alert("Please select Supplier.");
        return;
    }

    if ($("#MemoDate").val() == "") {
        alert("Please select Date.");
        return;
    }

    if ($("#Amount").val() == "") {
        alert("Amount cannot be empty.");
        return;
    }

    var data =
    {
        companyCode: $("#Company").val(),
        supplierCode: $("#Supplier").val(),
        memoDate: $("#MemoDate").val(),
        amount: $("#Amount").val(),
        remarks: $("#Remarks").val()
    };

    $.ajax({
        url: "/Memo/Save",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(data),

        success: function (result) {
            if (!result.success) {
                alert(result.message);

                return;
            }

            alert("Data saved successfully.");

            loadMemo();
            loadDetail($("#MemoNo").val());
        },

        error: function (xhr) {
            console.log(xhr);

            alert(
                "Status : " + xhr.status +
                "\n\n" +
                xhr.responseText
            );
        }
    });

}

$(document).on("click", "#btnInvoice", function () {

    if ($("#MemoNo").val() == "") {

        alert("Please save Memo first.");

        return;

    }

    $("#fileInvoice").click();

});

$(document).on("click", "#btnBAP", function () {

    if ($("#MemoNo").val() == "") {

        alert("Please save Memo first.");

        return;

    }

    $("#fileBAP").click();

});

$(document).on("click", "#btnFaktur", function () {

    if ($("#MemoNo").val() == "") {

        alert("Please save Memo first.");

        return;

    }

    $("#fileFaktur").click();

});

$("#fileInvoice").change(function () {

    uploadAttachment(
        "INVOICE",
        this.files[0]);

});

$("#fileBAP").change(function () {

    uploadAttachment(
        "BAP",
        this.files[0]);

});

$("#fileFaktur").change(function () {

    uploadAttachment(
        "FAKTURPAJAK",
        this.files[0]);

});

function uploadAttachment(
    documentType,
    file) {
    var form = new FormData();

    form.append(
        "MemoNo",
        $("#MemoNo").val());

    form.append(
        "DocumentType",
        documentType);

    form.append(
        "File",
        file);

    $.ajax({

        url: "/Memo/UploadAttachment",

        type: "POST",

        data: form,

        processData: false,

        contentType: false,

        success: function (result) {
            if (!result.success) {
                alert(result.message);

                return;
            }

            loadAttachment();

        }

    });

}

function loadAttachment() {
    $.get(

        "/Memo/AttachmentList",

        {
            memoNo: $("#MemoNo").val()
        },

        function (data) {
            $("#Invoice").val("");

            $("#BAP").val("");

            $("#FakturPajak").val("");

            $.each(data, function (i, x) {
                switch (x.documentType) {
                    case "INVOICE":

                        $("#Invoice")
                            .val(x.fileName);

                        break;

                    case "BAP":

                        $("#BAP")
                            .val(x.fileName);

                        break;

                    case "FAKTURPAJAK":

                        $("#FakturPajak")
                            .val(x.fileName);

                        break;
                }

            });

        });

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