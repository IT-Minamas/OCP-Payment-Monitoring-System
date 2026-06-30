$(document).ready(function () {
    loadCompany();
    loadSupplier();
    loadMemo();

    if (currentUser.approvalLevel == 0) {
        submissionMode();
    } else {
        approvalMode();
    }
});

$(document).on("click", "#tblMemo tr", function () {
        loadDetail(
            $(this).attr("data-id"));
    });

$(document).on("click","#btnSave", function () {
        saveMemo();
    });

$("#btnDelete").click(function () {
    DeleteMemo();
});

$("#btnSubmit").click(function () {
    SubmitMemo();
});

$("#btnApprove").click(function () {
    approveMemo();
});

$(document).on("click", "#btnNew", function () {
    clearForm();
});

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

function DeleteMemo() {

    if ($("#MemoNo").val() == "") {
        alert("No Memo selected.");
        return;
    }

    if (!confirm("Delete this Memo ?"))
        return;

    $.ajax({
        url: "/Memo/Delete",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MemoNo: $("#MemoNo").val()
        }),

        success: function (result) {
            if (!result.success) {
                alert(result.message);
                return;
            }

            alert("Memo deleted successfully.");
            NewMemo();
            loadMemo();
            submissionMode();
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });

}

function NewMemo() {
    clearForm();
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
            $("#MillCode").val(x.millCode);

            let dt = new Date(x.memoDate);
            let month = String(dt.getMonth() + 1).padStart(2, '0');
            let day = String(dt.getDate()).padStart(2, '0');
            let year = dt.getFullYear();

            $("#MemoDate").val(`${year}-${month}-${day}`);
            $("#Amount").val(x.amount);
            $("#Remarks").val(x.remarks);
            $("#Invoice").val(x.invoice);
            $("#BAP").val(x.bap);
            $("#FakturPajak").val(x.fakturPajak);
            $("#ApprovalLevel").val(x.ApprovalLevel);
            if (x.approvalLevel == 0) {
                submissionMode();
            } else {
                approvalMode();
            }
        });

//    loadAttachment();
}
function approveMemo() {
    if ($("#MemoNo").val() == "") {
        alert("No memo selected.");
        return;
    }

    if (!confirm("Approve this memo?"))
        return;

    $.ajax({
        url: "/Memo/Approve",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            memoNo: $("#MemoNo").val()
        }),

        success: function (result) {
            if (!result.success) {
                alert(result.message);
                return;
            }

            alert("Memo approved.");
            loadDetail($("#MemoNo").val());
            loadMemo();
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}

function approvalMode() {
    $("#Company").prop("disabled", true);
    $("#Supplier").prop("disabled", true);
    $("#MemoDate").prop("disabled", true);
    $("#Amount").prop("disabled", true);
    $("#Remarks").prop("disabled", true);

    /*
    $("#btnNew").prop("disabled", false);
    $("#btnSave").prop("disabled", false);
    $("#btnSubmit").prop("disabled", false);
    $("#btnDelete").prop("disabled", false);
    $("#btnApprove").prop("disabled", true);
    $("#btnReject").prop("disabled", true);
    */

    $("#btnNew").hide();
    $("#btnSave").hide();
    $("#btnDelete").hide();
    $("#btnSubmit").hide();

    $("#btnApprove").show();
    $("#btnReject").show();
}
function submissionMode() {
    $("#Company").prop("disabled", false);
    $("#Supplier").prop("disabled", false);
    $("#MemoDate").prop("disabled", false);
    $("#Amount").prop("disabled", false);
    $("#Remarks").prop("disabled", false);

    /*
    $("#btnNew").prop("disabled", true);
    $("#btnSave").prop("disabled", true);
    $("#btnSubmit").prop("disabled", true);
    $("#btnDelete").prop("disabled", true);
    $("#btnApprove").prop("disabled", false);
    $("#btnReject").prop("disabled", false);
    */

    $("#btnNew").show();
    $("#btnSave").show();
    $("#btnDelete").show();
    $("#btnSubmit").show();

    $("#btnApprove").hide();
    $("#btnReject").hide();
}

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
        remarks: $("#Remarks").val(),
        memoNo: $("#MemoNo").val(),
        millCode: $("#MillCode").val()
    };

    let url =
        $("#MemoNo").val() == ""
            ? "/Memo/Save"
            : "/Memo/Update";

    $.ajax({
        url: url,
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

function SubmitMemo() {
    if ($("#txtMemoNo").val() == "") {
        alert("Please save Memo first.");
        return;
    }

    if (!confirm("Submit this Memo for Approval?"))
        return;

    $.ajax({
        url: "/Memo/Submit",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MemoNo: $("#MemoNo").val()
        }),

        success: function (result) {
            if (result.success) {
                alert("Memo submitted successfully.");
                loadDetail($("#MemoNo").val());
                loadMemo();
            }
            else {
                alert(result.message);
            }
        },

        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}